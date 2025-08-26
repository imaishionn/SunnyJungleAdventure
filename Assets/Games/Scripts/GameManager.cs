using System.Collections;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ゲーム全体の進行状況、状態、データ、UI、BGMなどを管理するシングルトンクラス。
/// シーンをまたいで存在し、ゲームの中心的な制御を担います。
/// </summary>
public class GameManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static GameManager instance { get; private set; }

    // ゲームの状態を定義する列挙型
    public enum GameState
    {
        Title,       // タイトル画面
        Gameplay,    // ゲームプレイ中
        StageClear,  // ステージクリア画面
        GameOver,    // ゲームオーバー画面
        Pause,       // 一時停止中
        Cutscene,    // カットシーン再生中
        StageSelect  // ステージ選択画面
    }

    // ゲームの現在の状態
    private GameState m_currentGameState = GameState.Gameplay;

    // ゲームプレイデータ
    private int m_currentGemCount = 0;
    public System.Action<int> OnGemCountChanged; // ジェムカウント変更時に発火するイベント

    // 最終スコアと最終時間を保存する変数
    public int finalScore { get; private set; }
    public float finalTime { get; private set; }

    // BGMの音量を調整するプロパティ (UI用)
    public float BGMVolume
    {
        get => m_audioSource != null ? m_audioSource.volume : 0f;
        set
        {
            if (m_audioSource != null)
            {
                m_audioSource.volume = Mathf.Clamp01(value);
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するUI要素
    // ----------------------------------------------------------------------------------------------------
    [Header("UI要素")]
    [Tooltip("シーン遷移時に破壊されない永続的なUIキャンバス")]
    [SerializeField] private GameObject m_permanentUICanvas;
    [Tooltip("シーンフェードに使用するパネルのImageコンポーネント")]
    [SerializeField] private UnityEngine.UI.Image m_globalFadePanelImage;
    [Tooltip("シーンフェードに使用するキャンバスグループ")]
    [SerializeField] private CanvasGroup m_globalFadeCanvasGroup;
    [Tooltip("永続的なEventSystem")]
    [SerializeField] private EventSystem m_permanentEventSystem;
    [Tooltip("スコア表示を管理するクラス (必要に応じて)")]
    [SerializeField] private ScoreDisplay m_scoreDisplay;
    [Tooltip("スコアパネルのGameObject")]
    [SerializeField] private GameObject m_scorePanel;
    [Tooltip("時間制限を表示するTextMeshProのテキスト")]
    [SerializeField] private TMPro.TextMeshProUGUI m_timeLimitText;

    // ----------------------------------------------------------------------------------------------------
    // シーン管理
    // ----------------------------------------------------------------------------------------------------
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
    [SerializeField] private string[] m_stageSceneNames = { "Demo_tileset", "Demo_tileset2", "Demo_tileset3", "Stage1Scene", "Stage2Scene", "Stage3Scene", "For open campus" };
    [Tooltip("スコア表示シーンの名前")]
    [SerializeField] private string m_scoreSceneName = "ScoreScene";

    // 現在のステージインデックス
    private int m_currentStageIndex = 0;

    // シーンフェード関連
    private Coroutine m_fadeCoroutine;
    private UnityEngine.AsyncOperation m_asyncLoadOperation;
    private GameObject m_globalFadeCanvasInstance;
    [Tooltip("シーン遷移中かどうか")]
    public bool m_isGlobalTransitioning = false;
    [Tooltip("フェードにかかる時間 (秒)")]
    [SerializeField] private float m_fadeDuration = 1.0f;
    private const string STAGE_CLEAR_KEY_PREFIX = "StageClear_";

    // ----------------------------------------------------------------------------------------------------
    // 時間制限設定
    // ----------------------------------------------------------------------------------------------------
    [Header("時間制限設定")]
    [Tooltip("ゲームプレイシーンで時間制限を有効にするか")]
    [SerializeField] private bool m_isTimeLimited = false;
    [Tooltip("時間制限の合計時間 (秒)")]
    [SerializeField] private float m_timeLimitSeconds = 60.0f;

    // ----------------------------------------------------------------------------------------------------
    // BGM管理
    // ----------------------------------------------------------------------------------------------------
    [Header("BGM管理")]
    [Tooltip("シーン名とBGMのAudioClipをマッピングします。")]
    [SerializeField] private List<SceneBGMData> m_sceneBGMList;

    // CS0305エラーを修正: Dictionaryに型引数を追加
    private Dictionary<string, AudioClip> m_sceneBGMMap = new Dictionary<string, AudioClip>();

    [Tooltip("BGMの初期音量 (0.0fから1.0f)")]
    [Range(0f, 1f)]
    [SerializeField] private float m_initialBGMVolume = 0.5f;

    // 時間制限の現在時間
    private float m_currentTime;
    // BGM再生用AudioSource
    private AudioSource m_audioSource;

    /// <summary>
    /// シーンとBGMのデータを格納するシリアライズ可能なクラス
    /// </summary>
    [System.Serializable]
    public class SceneBGMData
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    // ----------------------------------------------------------------------------------------------------
    // プロパティとパブリックメソッド
    // ----------------------------------------------------------------------------------------------------
    public GameState GetCurrentGameState() => m_currentGameState;
    public void SetState(GameState newState)
    {
        m_currentGameState = newState;
        // ゲームの状態に応じてTime.timeScaleを設定
        switch (newState)
        {
            case GameState.Gameplay:
            case GameState.Cutscene:
                Time.timeScale = 1;
                break;
            case GameState.Pause:
            case GameState.GameOver:
            case GameState.StageClear:
                Time.timeScale = 0;
                break;
            default:
                Time.timeScale = 1;
                break;
        }
    }
    public void AddGem(int amount)
    {
        m_currentGemCount += amount;
        OnGemCountChanged?.Invoke(m_currentGemCount);
    }
    public int currentGemCount => m_currentGemCount;
    public string[] stageSceneNames => m_stageSceneNames;
    public int currentStageIndex
    {
        get => m_currentStageIndex;
        set => m_currentStageIndex = value;
    }
    public string TitleSceneName => m_titleSceneName;
    public string StageSelectSceneName => m_stageSelectSceneName;

    /// <summary>
    /// 即座にゲームオーバー状態に設定し、ゲームオーバーシーンに遷移します。
    /// </summary>
    public void SetGameOverStateImmediately()
    {
        if (m_currentGameState == GameState.GameOver) return;
        SetState(GameState.GameOver);
        if (m_isGlobalTransitioning) return;
        LoadSceneWithFade(m_gameOverSceneName);
    }

    /// <summary>
    /// 現在のシーンを再ロードしてゲームを再開します。
    /// </summary>
    public void RestartGame()
    {
        SetState(GameState.Gameplay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 最後にプレイしたステージを再ロードしてゲームを再開します。
    /// </summary>
    public void RetryLastStage()
    {
        if (m_isGlobalTransitioning) return;

        SetState(GameState.Gameplay);

        if (m_currentStageIndex >= 0 && m_currentStageIndex < m_stageSceneNames.Length)
        {
            string sceneName = m_stageSceneNames[m_currentStageIndex];
            LoadSceneWithFade(sceneName);
        }
        else
        {
            Debug.LogError("GameManager: 再プレイするステージが見つかりません。ステージ選択へ戻ります。", this);
            LoadSceneWithFade(m_stageSelectSceneName);
        }
    }

    /// <summary>
    /// BGMの音量を設定します。
    /// </summary>
    /// <param name="volume">設定する音量 (0.0fから1.0f)</param>
    public void SetBGMVolume(float volume)
    {
        BGMVolume = volume;
    }

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManagerインスタンスが作成され、DontDestroyOnLoadに設定されました。", this);

            CheckUIReferences();

            m_audioSource = GetComponent<AudioSource>();
            if (m_audioSource == null)
            {
                m_audioSource = gameObject.AddComponent<AudioSource>();
            }
            m_audioSource.loop = true;
            m_audioSource.volume = m_initialBGMVolume;

            foreach (var data in m_sceneBGMList)
            {
                if (!m_sceneBGMMap.ContainsKey(data.sceneName))
                {
                    m_sceneBGMMap.Add(data.sceneName, data.bgmClip);
                }
            }

            if (m_globalFadeCanvasGroup != null)
            {
                m_globalFadeCanvasInstance = m_globalFadeCanvasGroup.gameObject;
                m_globalFadeCanvasGroup.alpha = 0f;
                m_globalFadeCanvasGroup.blocksRaycasts = false;
                m_globalFadeCanvasGroup.interactable = false;
            }

            // 初回起動時のシーンに応じた状態設定
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName == m_bootstrapSceneName || currentSceneName == "FirstScene")
            {
                SceneManager.LoadScene(m_titleSceneName);
            }
        }
        else
        {
            Debug.LogWarning("GameManagerのインスタンスが既に存在するため、このオブジェクトは破棄されました。", this);
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnDestroy()
    {
        if (instance == this) instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (m_isTimeLimited && m_currentGameState == GameState.Gameplay)
        {
            m_currentTime -= Time.deltaTime;

            if (m_timeLimitText != null)
            {
                m_timeLimitText.text = "Time: " + Mathf.CeilToInt(m_currentTime).ToString();
                m_timeLimitText.color = m_currentTime <= 10f ? Color.red : Color.white;
            }

            if (m_currentTime <= 0)
            {
                m_currentTime = 0;
                GameOver();
            }
        }
    }

    /// <summary>
    /// シーンがロードされたときに呼び出されるイベントハンドラ
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 永続UIの表示をシーン名に基づいて切り替える
        UpdatePermanentUIForScene(scene.name);

        // シーンに応じたBGMを再生
        if (m_sceneBGMMap.ContainsKey(scene.name))
        {
            PlayBGM(m_sceneBGMMap[scene.name]);
        }
        else
        {
            PlayBGM(null);
        }

        // シーン名に応じてゲーム状態を設定
        SetGameStateByScene(scene.name);

        // フェードイン処理
        if (mode == LoadSceneMode.Single)
        {
            StartFadeIn();
        }

        m_isGlobalTransitioning = false;
    }

    /// <summary>
    /// シーン名に基づいてゲーム状態を設定します。
    /// </summary>
    private void SetGameStateByScene(string sceneName)
    {
        // FirstSceneがロードされたらタイトル状態に設定
        if (sceneName == "FirstScene")
        {
            SetState(GameState.Title);
            ResetGameData();
            return;
        }

        if (sceneName == m_titleSceneName)
        {
            SetState(GameState.Title);
            ResetGameData();
        }
        else if (sceneName == m_stageSelectSceneName || sceneName.Contains("StageSelect")) // StageSelect2Sceneにも対応
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
        // "For open campus" をゲームプレイシーンとして追加
        else if (sceneName.StartsWith("Demo_tileset") || sceneName.StartsWith("Stage") || sceneName == "For open campus")
        {
            SetState(GameState.Gameplay);
            InitializeGameplayState();
        }
        else
        {
            Debug.LogWarning($"GameManager: 未知のシーン'{sceneName}'から起動しました。デフォルトのゲーム状態をGameplayに設定。", this);
            SetState(GameState.Gameplay);
        }
    }

    /// <summary>
    /// ゲームプレイ状態の初期化
    /// </summary>
    private void InitializeGameplayState()
    {
        if (m_isTimeLimited)
        {
            m_currentTime = m_timeLimitSeconds;
        }
    }

    /// <summary>
    /// BGMを再生します。clipがnullの場合は停止します。
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (m_audioSource == null)
        {
            Debug.LogWarning("GameManager: BGM再生用のAudioSourceが割り当てられていません。", this);
            return;
        }

        if (m_audioSource.clip == clip && m_audioSource.isPlaying)
        {
            return;
        }

        if (m_audioSource.isPlaying)
        {
            m_audioSource.Stop();
        }

        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.volume = m_initialBGMVolume;
            m_audioSource.Play();
        }
    }

    /// <summary>
    /// 現在のシーンに応じて永続的なUIの表示を切り替えます。
    /// </summary>
    private void UpdatePermanentUIForScene(string sceneName)
    {
        // "For open campus" をゲームプレイシーンとして追加
        bool isGameplayScene = sceneName.StartsWith("Demo_tileset") || sceneName.StartsWith("Stage") || sceneName == "For open campus";

        // Canvas全体とスコアパネルの表示を制御
        bool showPermanentCanvas = isGameplayScene;

        // StageSelectシーン名がStageSelectで終わるものすべてを非表示の対象とする
        if (sceneName.Contains("StageSelect") || sceneName == m_titleSceneName ||
            sceneName == m_gameOverSceneName || sceneName == m_clearSceneName ||
            sceneName == m_scoreSceneName || sceneName == m_bootstrapSceneName || sceneName == "FirstScene")
        {
            showPermanentCanvas = false;
        }

        if (m_permanentUICanvas != null) m_permanentUICanvas.SetActive(showPermanentCanvas);
        if (m_scorePanel != null) m_scorePanel.SetActive(showPermanentCanvas);
        if (m_timeLimitText != null) m_timeLimitText.gameObject.SetActive(showPermanentCanvas && m_isTimeLimited);
    }

    /// <summary>
    /// シーンをフェードアウトしながらロードします。
    /// </summary>
    public void LoadSceneWithFade(string sceneName, float duration = 1.0f, System.Action onFadeOutComplete = null)
    {
        if (m_isGlobalTransitioning) return;
        if (m_globalFadePanelImage == null || m_globalFadeCanvasGroup == null)
        {
            Debug.LogError("GameManager: フェードに必要なUI要素が割り当てられていません！フェードなしでロードします。", this);
            SceneManager.LoadScene(sceneName);
            return;
        }

        m_isGlobalTransitioning = true;
        if (m_globalFadeCanvasInstance != null && !m_globalFadeCanvasInstance.activeSelf) m_globalFadeCanvasInstance.SetActive(true);
        if (m_globalFadePanelImage != null && !m_globalFadePanelImage.gameObject.activeSelf) m_globalFadePanelImage.gameObject.SetActive(true);

        m_fadeCoroutine = StartCoroutine(FadeOutAndLoadScene(sceneName, duration, onFadeOutComplete));
    }

    /// <summary>
    /// フェードアウトしてシーンをロードするコルーチン
    /// </summary>
    private IEnumerator FadeOutAndLoadScene(string sceneName, float duration, System.Action onFadeOutComplete)
    {
        if (m_globalFadeCanvasGroup != null)
        {
            m_globalFadeCanvasGroup.blocksRaycasts = true;
            m_globalFadeCanvasGroup.interactable = true;
        }

        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, duration, onFadeOutComplete));

        m_asyncLoadOperation = SceneManager.LoadSceneAsync(sceneName);
        m_asyncLoadOperation.allowSceneActivation = false;

        while (m_asyncLoadOperation.progress < 0.9f)
        {
            yield return null;
        }

        // ロードが90%完了したら、フェードアウトが完了しているか確認し、シーンを有効化
        while (m_globalFadeCanvasGroup.alpha < 0.99f)
        {
            yield return null;
        }
        m_asyncLoadOperation.allowSceneActivation = true;
    }

    /// <summary>
    /// CanvasGroupのアルファ値を滑らかに変更するコルーチン
    /// </summary>
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
            float progress = timer / duration;
            m_globalFadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            yield return null;
        }
        m_globalFadeCanvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }

    /// <summary>
    /// シーンロード後のフェードインを開始します。
    /// </summary>
    private void StartFadeIn()
    {
        if (m_fadeCoroutine != null) StopCoroutine(m_fadeCoroutine);
        if (m_globalFadeCanvasInstance != null) m_globalFadeCanvasInstance.SetActive(true);
        if (m_globalFadePanelImage != null) m_globalFadePanelImage.gameObject.SetActive(true);

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

    /// <summary>
    /// ゲームオーバー処理を実行します。
    /// </summary>
    public void GameOver()
    {
        if (m_currentGameState == GameState.GameOver || m_isGlobalTransitioning)
        {
            Debug.LogWarning("GameManager: 既にゲームオーバー状態か、シーン遷移中のため、GameOver処理をスキップしました。", this);
            return;
        }
        SetState(GameState.GameOver);
        PlayBGM(null);

        string currentSceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < m_stageSceneNames.Length; i++)
        {
            if (m_stageSceneNames[i] == currentSceneName)
            {
                m_currentStageIndex = i;
                break;
            }
        }

        ResetGameData();
        LoadSceneWithFade(m_gameOverSceneName);
    }

    /// <summary>
    /// ゲームクリア処理を実行します。
    /// </summary>
    public void GameClear()
    {
        if (m_currentGameState == GameState.StageClear || m_isGlobalTransitioning)
        {
            Debug.LogWarning("GameManager: 既にゲームクリア状態か、シーン遷移中のため、GameClear処理をスキップしました。", this);
            return;
        }
        SetState(GameState.StageClear);
        PlayBGM(null);

        finalScore = m_currentGemCount;
        finalTime = m_currentTime;

        LoadSceneWithFade(m_clearSceneName);
    }

    /// <summary>
    /// 次のステージへ遷移します。
    /// </summary>
    public void GoToNextStage()
    {
        if (m_isGlobalTransitioning) return;

        m_currentStageIndex++;

        if (m_currentStageIndex < m_stageSceneNames.Length)
        {
            LoadSceneWithFade(m_stageSceneNames[m_currentStageIndex]);
        }
        else
        {
            Debug.Log("すべてのステージをクリアしました！タイトルシーンに戻ります。");
            LoadSceneWithFade(m_titleSceneName);
        }
    }

    /// <summary>
    /// 指定したステージのクリア状態を保存します。
    /// </summary>
    public void SetStageClear(int stageIndex)
    {
        PlayerPrefs.SetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 1);
        PlayerPrefs.Save();
        Debug.Log($"ステージ{stageIndex + 1}をクリアしました。");
    }

    /// <summary>
    /// 指定したステージがクリア済みかどうかを判定します。
    /// </summary>
    public bool IsStageClear(int stageIndex) => PlayerPrefs.GetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 0) == 1;

    /// <summary>
    /// 保存されているすべてのステージクリアデータを削除します。(デバッグ用)
    /// </summary>
    public void ClearAllStageData()
    {
        for (int i = 0; i < m_stageSceneNames.Length; i++)
        {
            PlayerPrefs.DeleteKey(STAGE_CLEAR_KEY_PREFIX + i);
        }
        PlayerPrefs.Save();
        Debug.Log("すべてのステージクリアデータを削除しました。");
    }

    /// <summary>
    /// ゲームデータを初期状態にリセットします。
    /// </summary>
    public void ResetGameData()
    {
        m_currentGemCount = 0;
        OnGemCountChanged?.Invoke(m_currentGemCount);
        m_currentTime = m_timeLimitSeconds;
    }

    /// <summary>
    /// 必要なUI参照が設定されているかチェックし、ログに警告を出力します。
    /// </summary>
    private void CheckUIReferences()
    {
        if (m_permanentUICanvas == null) Debug.LogError("GameManager: Permanent UICanvasが割り当てられていません。", this);
        if (m_globalFadePanelImage == null) Debug.LogError("GameManager: Global Fade Panel Imageが割り当てられていません。", this);
        if (m_globalFadeCanvasGroup == null) Debug.LogError("GameManager: Global Fade Canvas Groupが割り当てられていません。", this);
        if (m_permanentEventSystem == null) Debug.LogError("GameManager: Permanent Event Systemが割り当てられていません。", this);
        if (m_scoreDisplay == null) Debug.LogError("GameManager: Score Displayが割り当てられていません。", this);
        if (m_scorePanel == null) Debug.LogError("GameManager: Score Panelが割り当てられていません。", this);
        if (m_timeLimitText == null) Debug.LogWarning("GameManager: Time Limit Textが割り当てられていません。時間制限UIは表示されません。", this);
    }
}