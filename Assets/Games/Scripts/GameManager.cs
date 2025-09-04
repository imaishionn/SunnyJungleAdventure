using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の進行状況、状態、データ、UI、BGMなどを管理するシングルトンクラス。
/// シーンをまたいで存在し、ゲームの中心的な制御を担います。
/// </summary>
public class GameManager : MonoBehaviour
{
    // ====================================================================================================
    // #region: シングルトン
    // ====================================================================================================
    // 外部からアクセスするためのインスタンス。
    public static GameManager Instance { get; private set; }

    // ====================================================================================================
    // #region: メンバ変数（インスペクターから設定）
    // ====================================================================================================
    [Header("UI要素")]
    [Tooltip("シーン遷移時に破壊されない永続的なUIキャンバス")]
    [SerializeField] private GameObject m_permanentUICanvas;
    [Tooltip("シーンフェードに使用するキャンバスグループ")]
    [SerializeField] private CanvasGroup m_globalFadeCanvasGroup;
    [Tooltip("シーンフェードに使用するパネルのImageコンポーネント")]
    [SerializeField] private UnityEngine.UI.Image m_globalFadePanelImage;
    [Tooltip("永続的なEventSystem")]
    [SerializeField] private UnityEngine.EventSystems.EventSystem m_permanentEventSystem;
    [Tooltip("スコア表示を管理するクラス (必要に応じて)")]
    [SerializeField] private ScoreDisplay m_scoreDisplay;
    [Tooltip("スコアパネルのGameObject")]
    [SerializeField] private GameObject m_scorePanel;
    [Tooltip("時間制限を表示するTextMeshProのテキスト")]
    [SerializeField] private TMPro.TextMeshProUGUI m_timeLimitText;
    [Tooltip("モバイル操作用のUIキャンバス")]
    [SerializeField] private GameObject m_mobileControlCanvas;

    [Header("シーン名 (Build Settingsに登録必須)")]
    [Tooltip("タイトルシーンの名前")]
    [SerializeField] private string m_titleSceneName = "TitleScene";
    [Tooltip("最初にロードされる起動用シーンの名前")]
    [SerializeField] private string m_bootstrapSceneName = "Bootstrap";
    [Tooltip("ステージ選択シーンの名前")]
    [SerializeField] private string m_stageSelectSceneName = "StageSelect";
    [Tooltip("ゲームオーバーシーンの名前")]
    [SerializeField] private string m_gameOverSceneName = "GameOverScene";
    [Tooltip("ゲームクリアシーンの名前")]
    [SerializeField] private string m_clearSceneName = "ClearScene";
    [Tooltip("ステージシーンの配列")]
    [SerializeField] private string[] m_stageSceneNames = { "Demo_tileset", "Demo_tileset2", "Demo_tileset3", "Stage1Scene", "Stage2Scene", "Stage3Scene", "OpenCampus" };
    [Tooltip("スコア表示シーンの名前")]
    [SerializeField] private string m_scoreSceneName = "ScoreScene";

    [Header("時間制限設定")]
    [Tooltip("ゲームプレイシーンで時間制限を有効にするか")]
    [SerializeField] private bool m_isTimeLimited = false;
    [Tooltip("時間制限の合計時間 (秒)")]
    [SerializeField] private float m_timeLimitSeconds = 60.0f;

    [Header("BGM管理")]
    [Tooltip("シーン名とBGMのAudioClipをマッピングします。")]
    [SerializeField] private List<SceneBGMData> m_sceneBGMList;
    [Tooltip("BGMの初期音量 (0.0fから1.0f)")]
    [Range(0f, 1f)]
    [SerializeField] private float m_initialBGMVolume = 0.5f;

    // ====================================================================================================
    // #region: メンバ変数（スクリプト内部で管理）
    // ====================================================================================================
    private GameState m_currentGameState = GameState.Gameplay;
    private int m_currentGemCount = 0;
    private int m_currentStageIndex = 0;
    private float m_currentTime;
    private Coroutine m_fadeCoroutine;
    private AsyncOperation m_asyncLoadOperation;
    private GameObject m_globalFadeCanvasInstance;
    private bool m_isGlobalTransitioning = false; // シーン遷移中かどうかのフラグ
    private AudioSource m_audioSource;
    private PlayerMove m_player;
    private Dictionary<string, AudioClip> m_sceneBGMMap = new Dictionary<string, AudioClip>();
    private bool m_isInitialStartup = true; // アプリ起動直後かどうかのフラグ

    private const string STAGE_CLEAR_KEY_PREFIX = "StageClear_";

    // ====================================================================================================
    // #region: 列挙型とクラス
    // ====================================================================================================
    // ゲームの状態を定義する列挙型
    public enum GameState
    {
        Title,
        Gameplay,
        StageClear,
        GameOver,
        Pause,
        Cutscene,
        StageSelect
    }

    [System.Serializable]
    public class SceneBGMData
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    // ====================================================================================================
    // #region: プロパティとパブリックイベント
    // ====================================================================================================
    // BGMの音量を設定・取得
    public float BGMVolume
    {
        get => m_audioSource != null ? m_audioSource.volume : 0f;
        set => m_audioSource.volume = Mathf.Clamp01(value);
    }
    // ゲームクリア時の最終スコア
    public int finalScore { get; private set; }
    // ゲームクリア時の最終時間
    public float finalTime { get; private set; }
    // 現在のジェム（収集アイテム）数
    public int currentGemCount => m_currentGemCount;
    // ステージシーン名の配列
    public string[] stageSceneNames => m_stageSceneNames;
    // 現在のステージのインデックス
    public int currentStageIndex
    {
        get => m_currentStageIndex;
        set => m_currentStageIndex = value;
    }
    // タイトルシーン名
    public string TitleSceneName => m_titleSceneName;
    // ステージ選択シーン名
    public string StageSelectSceneName => m_stageSelectSceneName;
    // フェードにかかる時間
    public float fadeDuration => m_fadeDuration;
    [SerializeField] private float m_fadeDuration = 1.0f;
    // ジェムの数が変更されたときに発火するイベント
    public System.Action<int> OnGemCountChanged;

    // ====================================================================================================
    // #region: MonoBehaviour ライフサイクル
    // ====================================================================================================
    // オブジェクト生成時に一度だけ呼ばれる
    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄されないようにする
            UnityEngine.Debug.Log("GameManagerインスタンスが作成され、DontDestroyOnLoadに設定されました。", this);

            // 初期化処理をまとめて実行
            CheckUIReferences();
            InitializeAudioSource();
            InitializeBGMMap();
            InitializeFadeCanvas();

            // 初回起動時のシーンに応じて状態を設定し、タイトルシーンへロードする
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (m_isInitialStartup && (currentSceneName == m_bootstrapSceneName || currentSceneName == "FirstScene"))
            {
                SceneManager.LoadScene(m_titleSceneName);
            }
            else
            {
                // それ以外の場合は通常通りフェードを伴ってロード
                LoadSceneWithFade(m_titleSceneName);
            }
        }
        else
        {
            // 既にインスタンスが存在する場合は、重複を避けるために自身を破棄
            UnityEngine.Debug.LogWarning("GameManagerのインスタンスが既に存在するため、このオブジェクトは破棄されました。", this);
            Destroy(gameObject);
        }
    }

    // スクリプトが有効になったときに呼ばれる
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded; // シーンロード完了時のイベントにメソッドを登録
    // スクリプトが無効になったときに呼ばれる
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded; // シーンロード完了時のイベントからメソッドを削除
    // オブジェクトが破棄されるときに呼ばれる
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded; // イベントハンドラを必ず削除する
    }

    // 毎フレーム呼ばれる
    private void Update()
    {
        // 時間制限が有効で、ゲームプレイ状態の場合のみ、時間を減らす
        if (m_isTimeLimited && m_currentGameState == GameState.Gameplay)
        {
            m_currentTime -= Time.deltaTime;
            UpdateTimeDisplay(); // 時間表示を更新

            if (m_currentTime <= 0)
            {
                m_currentTime = 0;
                GameOver(); // 時間切れでゲームオーバー処理を呼び出す
            }
        }
    }

    // ====================================================================================================
    // #region: パブリックメソッド
    // ====================================================================================================
    // 現在のゲーム状態を取得する
    public GameState GetCurrentGameState() => m_currentGameState;
    // ゲーム状態を設定し、Time.timeScaleを制御する
    public void SetState(GameState newState)
    {
        m_currentGameState = newState;
        switch (newState)
        {
            case GameState.Gameplay:
            case GameState.Cutscene:
                Time.timeScale = 1; // ゲームを進行させる
                break;
            case GameState.Pause:
            case GameState.GameOver:
            case GameState.StageClear:
                Time.timeScale = 0; // ゲームを一時停止する
                break;
            default:
                Time.timeScale = 1; // その他の状態では通常通り進行
                break;
        }
    }
    // ジェムの数を増やす
    public void AddGem(int amount)
    {
        m_currentGemCount += amount;
        OnGemCountChanged?.Invoke(m_currentGemCount); // イベントを発火させてUIなどを更新
    }
    // 即座にゲームオーバー状態に設定し、シーン遷移を始める
    public void SetGameOverStateImmediately()
    {
        if (m_currentGameState == GameState.GameOver) return;
        SetState(GameState.GameOver);
        if (m_isGlobalTransitioning) return;
        LoadSceneWithFade(m_gameOverSceneName);
    }
    // 現在のシーンをリロードしてゲームを再開する
    public void RestartGame()
    {
        SetState(GameState.Gameplay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // 前のステージを再プレイする
    public void RetryLastStage()
    {
        if (m_isGlobalTransitioning) return;
        SetState(GameState.Gameplay);
        if (m_currentStageIndex >= 0 && m_currentStageIndex < m_stageSceneNames.Length)
        {
            string sceneName = m_stageSceneNames[m_currentStageIndex];
            LoadSceneWithFade(sceneName); // フェードアウトを伴ってシーンをロード
        }
        else
        {
            UnityEngine.Debug.LogError("GameManager: 再プレイするステージが見つかりません。ステージ選択へ戻ります。", this);
            LoadSceneWithFade(m_stageSelectSceneName);
        }
    }
    // BGMの音量を設定する
    public void SetBGMVolume(float volume) => BGMVolume = volume;
    // PlayerMoveコンポーネントを取得する
    public PlayerMove GetPlayerMove() => m_player;
    // フェードアウトしてから指定のシーンをロードする
    public void LoadSceneWithFade(string sceneName, float duration = 1.0f, System.Action onFadeOutComplete = null)
    {
        if (m_isGlobalTransitioning) return;
        if (m_globalFadePanelImage == null || m_globalFadeCanvasGroup == null)
        {
            UnityEngine.Debug.LogError("GameManager: フェードに必要なUI要素が割り当てられていません！フェードなしでロードします。", this);
            SceneManager.LoadScene(sceneName);
            return;
        }
        m_isGlobalTransitioning = true; // 遷移中フラグを立てる
        m_globalFadeCanvasInstance.SetActive(true);
        m_globalFadePanelImage.gameObject.SetActive(true);

        m_fadeCoroutine = StartCoroutine(FadeOutAndLoadScene(sceneName, duration, onFadeOutComplete));
    }
    // ゲームオーバー処理を実行する
    public void GameOver()
    {
        if (m_currentGameState == GameState.GameOver || m_isGlobalTransitioning)
        {
            UnityEngine.Debug.LogWarning("GameManager: 既にゲームオーバー状態か、シーン遷移中のため、GameOver処理をスキップしました。", this);
            return;
        }
        SetState(GameState.GameOver);
        PlayBGM(null); // BGMを停止
        SaveCurrentStageIndex(); // 現在のステージインデックスを保存
        ResetGameData(); // ゲームデータをリセット
        LoadSceneWithFade(m_gameOverSceneName); // ゲームオーバーシーンへ遷移
    }
    // ゲームクリア処理を実行する
    public void GameClear()
    {
        if (m_currentGameState == GameState.StageClear || m_isGlobalTransitioning)
        {
            UnityEngine.Debug.LogWarning("GameManager: 既にゲームクリア状態か、シーン遷移中のため、GameClear処理をスキップしました。", this);
            return;
        }
        SetState(GameState.StageClear);
        PlayBGM(null); // BGMを停止
        finalScore = m_currentGemCount; // 最終スコアを保存
        finalTime = m_currentTime; // 最終時間を保存
        LoadSceneWithFade(m_clearSceneName); // ゲームクリアシーンへ遷移
    }
    // 次のステージへ進む
    public void GoToNextStage()
    {
        if (m_isGlobalTransitioning) return;
        m_currentStageIndex++; // ステージインデックスをインクリメント
        if (m_currentStageIndex < m_stageSceneNames.Length)
        {
            LoadSceneWithFade(m_stageSceneNames[m_currentStageIndex]); // 次のステージをロード
        }
        else
        {
            UnityEngine.Debug.Log("すべてのステージをクリアしました！タイトルシーンに戻ります。");
            LoadSceneWithFade(m_titleSceneName); // 全ステージクリアでタイトルへ戻る
        }
    }
    // ステージクリア情報をPlayerPrefsに保存する
    public void SetStageClear(int stageIndex)
    {
        PlayerPrefs.SetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 1);
        PlayerPrefs.Save();
        UnityEngine.Debug.Log($"ステージ{stageIndex + 1}をクリアしました。");
    }
    // 指定したステージがクリア済みかどうかをチェックする
    public bool IsStageClear(int stageIndex) => PlayerPrefs.GetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 0) == 1;
    // すべてのステージクリアデータを削除する（デバッグ用など）
    public void ClearAllStageData()
    {
        for (int i = 0; i < m_stageSceneNames.Length; i++)
        {
            PlayerPrefs.DeleteKey(STAGE_CLEAR_KEY_PREFIX + i);
        }
        PlayerPrefs.Save();
        UnityEngine.Debug.Log("すべてのステージクリアデータを削除しました。");
    }
    // ゲームデータを初期状態にリセットする
    public void ResetGameData()
    {
        m_currentGemCount = 0;
        OnGemCountChanged?.Invoke(m_currentGemCount);
        m_currentTime = m_timeLimitSeconds;
    }

    // ====================================================================================================
    // #region: プライベートメソッド（ヘルパー）
    // ====================================================================================================
    // シーンのロードが完了したときに呼ばれるイベントハンドラ
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetGameStateByScene(scene.name); // シーン名に基づいてゲーム状態を設定
        UpdateSceneReferences(scene.name); // シーン内の必要なコンポーネントへの参照を更新
        PlayBGMForScene(scene.name); // シーンに応じたBGMを再生

        // 初回起動時とそれ以降でフェードインの挙動を分ける
        if (m_isInitialStartup)
        {
            if (m_globalFadeCanvasGroup != null)
            {
                m_globalFadeCanvasGroup.alpha = 0f;
                m_globalFadeCanvasGroup.blocksRaycasts = false;
                m_globalFadeCanvasGroup.interactable = false;
            }
            m_isInitialStartup = false;
        }
        else
        {
            if (mode == LoadSceneMode.Single)
            {
                StartFadeIn(); // フェードインを開始
            }
        }
        m_isGlobalTransitioning = false; // 遷移中フラグを解除
    }
    // シーンごとの必要なコンポーネントへの参照を更新する
    private void UpdateSceneReferences(string sceneName)
    {
        bool isGameplay = IsGameplayScene(sceneName);
        m_player = isGameplay ? FindObjectOfType<PlayerMove>() : null;
        if (m_player != null && m_mobileControlCanvas != null)
        {
            var joystick = m_mobileControlCanvas.transform.Find("JoystickBase")?.GetComponent<VirtualJoystick>();
            var jumpButton = m_mobileControlCanvas.transform.Find("JumpButton")?.GetComponent<JumpButtonController>();
            m_player.SetMobileControls(joystick, jumpButton);
            if (jumpButton != null)
            {
                jumpButton.SetPlayerMove(m_player);
            }
        }
        UpdatePermanentUIForScene(sceneName); // シーンに応じた永続UIの表示/非表示を切り替え
    }
    // 指定されたシーンがゲームプレイシーンかどうかを判定する
    private bool IsGameplayScene(string sceneName)
    {
        // ここにゲームプレイシーンの条件を記述
        return (sceneName == "Stage1Scene" ||
      sceneName == "Stage2Scene" ||
      sceneName == "Stage3Scene" ||
      sceneName == "OpenCampus");
    }
    // シーン名に基づいてゲーム状態を設定する
    private void SetGameStateByScene(string sceneName)
    {
        if (sceneName == m_titleSceneName || sceneName == "FirstScene")
        {
            SetState(GameState.Title);
            ResetGameData(); // タイトルに戻った際にデータをリセット
        }
        else if (sceneName == m_stageSelectSceneName || sceneName.Contains("StageSelect"))
        {
            SetState(GameState.StageSelect);
        }
        else if (sceneName == m_gameOverSceneName)
        {
            SetState(GameState.GameOver);
        }
        else if (sceneName == m_clearSceneName || sceneName == m_scoreSceneName)
        {
            SetState(GameState.StageClear);
        }
        else if (IsGameplayScene(sceneName))
        {
            SetState(GameState.Gameplay);
            InitializeGameplayState(); // ゲームプレイ状態の初期化
        }
        else
        {
            UnityEngine.Debug.LogWarning($"GameManager: 未知のシーン'{sceneName}'から起動しました。デフォルトのゲーム状態をGameplayに設定。", this);
            SetState(GameState.Gameplay);
        }
    }
    // ゲームプレイ状態の変数を初期化する
    private void InitializeGameplayState() => m_currentTime = m_timeLimitSeconds;
    // 時間表示UIを更新する
    private void UpdateTimeDisplay()
    {
        if (m_timeLimitText != null)
        {
            m_timeLimitText.text = ": " + Mathf.CeilToInt(m_currentTime).ToString();
            m_timeLimitText.color = m_currentTime <= 10f ? Color.red : Color.white; // 残り時間が少なくなると赤色に
        }
    }

    /// <summary>
    /// BGMを再生します。clipがnullの場合は停止します。
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (m_audioSource == null)
        {
            UnityEngine.Debug.LogWarning("GameManager: BGM再生用のAudioSourceが割り当てられていません。", this);
            return;
        }
        if (m_audioSource.clip == clip && m_audioSource.isPlaying) return; // 同じBGMが再生中なら何もしない
        m_audioSource.Stop();
        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.volume = m_initialBGMVolume;
            m_audioSource.Play();
        }
    }
    // シーン名に応じてBGMを再生する
    private void PlayBGMForScene(string sceneName)
    {
        if (m_sceneBGMMap.ContainsKey(sceneName))
        {
            PlayBGM(m_sceneBGMMap[sceneName]);
        }
        else
        {
            PlayBGM(null); // 対応するBGMがなければ停止
        }
    }
    // シーンに応じて永続UIの表示/非表示を切り替える
    private void UpdatePermanentUIForScene(string sceneName)
    {
        bool isGameplay = IsGameplayScene(sceneName);
        if (m_permanentUICanvas != null) m_permanentUICanvas.SetActive(isGameplay);
        if (m_scorePanel != null) m_scorePanel.SetActive(isGameplay);
        if (m_timeLimitText != null) m_timeLimitText.gameObject.SetActive(isGameplay && m_isTimeLimited);
        // モバイルプラットフォームの場合のみモバイルUIを表示
        if (m_mobileControlCanvas != null) m_mobileControlCanvas.SetActive(isGameplay && UnityEngine.Application.isMobilePlatform);
    }
    // フェードアウトとシーンの非同期ロードをコルーチンで実行する
    private IEnumerator FadeOutAndLoadScene(string sceneName, float duration, System.Action onFadeOutComplete)
    {
        if (m_globalFadeCanvasGroup != null)
        {
            m_globalFadeCanvasGroup.blocksRaycasts = true;
            m_globalFadeCanvasGroup.interactable = true;
        }
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, duration, onFadeOutComplete)); // フェードアウト
        m_asyncLoadOperation = SceneManager.LoadSceneAsync(sceneName); // シーンを非同期でロード開始
        m_asyncLoadOperation.allowSceneActivation = false; // ロード完了後もすぐには切り替えない
        while (m_asyncLoadOperation.progress < 0.9f) yield return null; // ロードの進捗が0.9fになるまで待つ
        while (m_globalFadeCanvasGroup.alpha < 0.99f) yield return null; // フェードアウトがほぼ完了するまで待つ
        m_asyncLoadOperation.allowSceneActivation = true; // シーンの切り替えを許可
    }
    // CanvasGroupのアルファ値を時間で変化させるコルーチン
    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        if (m_globalFadeCanvasGroup == null || m_globalFadePanelImage == null)
        {
            onComplete?.Invoke();
            yield break;
        }
        m_globalFadeCanvasGroup.alpha = startAlpha;
        m_globalFadePanelImage.color = new Color(0f, 0f, 0f, m_globalFadePanelImage.color.a);
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            m_globalFadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration); // 線形補間
            yield return null;
        }
        m_globalFadeCanvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }
    // フェードインを開始する
    private void StartFadeIn()
    {
        if (m_fadeCoroutine != null) StopCoroutine(m_fadeCoroutine);
        m_globalFadeCanvasInstance.SetActive(true);
        m_globalFadePanelImage.gameObject.SetActive(true);
        if (m_globalFadeCanvasGroup != null)
        {
            m_globalFadeCanvasGroup.blocksRaycasts = true;
            m_globalFadeCanvasGroup.interactable = true;
        }
        m_fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, 0f, m_fadeDuration, () =>
        {
            if (m_globalFadeCanvasGroup != null)
            {
                m_globalFadeCanvasGroup.alpha = 0f;
                m_globalFadeCanvasGroup.blocksRaycasts = false;
                m_globalFadeCanvasGroup.interactable = false;
            }
        }));
    }
    // インスペクターからの参照が正しく設定されているかチェックする
    private void CheckUIReferences()
    {
        if (m_permanentUICanvas == null) UnityEngine.Debug.LogError("GameManager: Permanent UICanvasが割り当てられていません。", this);
        if (m_globalFadePanelImage == null) UnityEngine.Debug.LogError("GameManager: Global Fade Panel Imageが割り当てられていません。", this);
        if (m_globalFadeCanvasGroup == null) UnityEngine.Debug.LogError("GameManager: Global Fade Canvas Groupが割り当てられていません。", this);
        if (m_permanentEventSystem == null) UnityEngine.Debug.LogError("GameManager: Permanent Event Systemが割り当てられていません。", this);
        if (m_scoreDisplay == null) UnityEngine.Debug.LogError("GameManager: Score Displayが割り当てられていません。", this);
        if (m_scorePanel == null) UnityEngine.Debug.LogError("GameManager: Score Panelが割り当てられていません。", this);
        if (m_timeLimitText == null) UnityEngine.Debug.LogWarning("GameManager: Time Limit Textが割り当てられていません。時間制限UIは表示されません。", this);
        if (m_mobileControlCanvas == null) UnityEngine.Debug.LogError("GameManager: Mobile Control Canvasが割り当てられていません。", this);
    }
    // BGM用のAudioSourceコンポーネントを初期化する
    private void InitializeAudioSource()
    {
        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource == null) m_audioSource = gameObject.AddComponent<AudioSource>();
        m_audioSource.loop = true; // ループ再生を有効に
        m_audioSource.volume = m_initialBGMVolume;
    }
    // BGMデータを辞書（Dictionary）に変換して検索を高速化する
    private void InitializeBGMMap()
    {
        foreach (var data in m_sceneBGMList)
        {
            if (!m_sceneBGMMap.ContainsKey(data.sceneName))
            {
                m_sceneBGMMap.Add(data.sceneName, data.bgmClip);
            }
        }
    }
    // フェードキャンバスを初期化する
    private void InitializeFadeCanvas()
    {
        if (m_globalFadeCanvasGroup != null)
        {
            m_globalFadeCanvasInstance = m_globalFadeCanvasGroup.gameObject;
            m_globalFadeCanvasGroup.alpha = 0f;
            m_globalFadeCanvasGroup.blocksRaycasts = false;
            m_globalFadeCanvasGroup.interactable = false;
        }
    }
    // 現在のステージインデックスを保存する
    private void SaveCurrentStageIndex()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < m_stageSceneNames.Length; i++)
        {
            if (m_stageSceneNames[i] == currentSceneName)
            {
                m_currentStageIndex = i;
                break;
            }
        }
    }
}