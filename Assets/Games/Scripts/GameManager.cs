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
    // ====================================================================================================
    // #region: シングルトン
    // ====================================================================================================
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
    [SerializeField] private string[] m_stageSceneNames = { "Demo_tileset", "Demo_tileset2", "Demo_tileset3", "Stage1Scene", "Stage2Scene", "Stage3Scene", "For open campus" };
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
    private UnityEngine.AsyncOperation m_asyncLoadOperation;
    private GameObject m_globalFadeCanvasInstance;
    private bool m_isGlobalTransitioning = false;
    private AudioSource m_audioSource;
    private PlayerMove m_player;
    private Dictionary<string, AudioClip> m_sceneBGMMap = new Dictionary<string, AudioClip>();
    private bool m_isInitialStartup = true;

    private const string STAGE_CLEAR_KEY_PREFIX = "StageClear_";

    // ====================================================================================================
    // #region: 列挙型とクラス
    // ====================================================================================================
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
    public float BGMVolume
    {
        get => m_audioSource != null ? m_audioSource.volume : 0f;
        set => m_audioSource.volume = Mathf.Clamp01(value);
    }
    public int finalScore { get; private set; }
    public float finalTime { get; private set; }
    public int currentGemCount => m_currentGemCount;
    public string[] stageSceneNames => m_stageSceneNames;
    public int currentStageIndex
    {
        get => m_currentStageIndex;
        set => m_currentStageIndex = value;
    }
    public string TitleSceneName => m_titleSceneName;
    public string StageSelectSceneName => m_stageSelectSceneName;
    public float fadeDuration => m_fadeDuration;
    [SerializeField] private float m_fadeDuration = 1.0f;
    public System.Action<int> OnGemCountChanged;

    // ====================================================================================================
    // #region: MonoBehaviour ライフサイクル
    // ====================================================================================================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnityEngine.Debug.Log("GameManagerインスタンスが作成され、DontDestroyOnLoadに設定されました。", this);

            CheckUIReferences();
            InitializeAudioSource();
            InitializeBGMMap();
            InitializeFadeCanvas();

            // 初回起動時のシーンに応じて状態設定
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (m_isInitialStartup && (currentSceneName == m_bootstrapSceneName || currentSceneName == "FirstScene"))
            {
                // 初回起動時のみフェードをせずにシーンをロード
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
            UnityEngine.Debug.LogWarning("GameManagerのインスタンスが既に存在するため、このオブジェクトは破棄されました。", this);
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (m_isTimeLimited && m_currentGameState == GameState.Gameplay)
        {
            m_currentTime -= Time.deltaTime;
            UpdateTimeDisplay();

            if (m_currentTime <= 0)
            {
                m_currentTime = 0;
                GameOver();
            }
        }
    }

    // ====================================================================================================
    // #region: パブリックメソッド
    // ====================================================================================================
    public GameState GetCurrentGameState() => m_currentGameState;
    public void SetState(GameState newState)
    {
        m_currentGameState = newState;
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
    public void SetGameOverStateImmediately()
    {
        if (m_currentGameState == GameState.GameOver) return;
        SetState(GameState.GameOver);
        if (m_isGlobalTransitioning) return;
        LoadSceneWithFade(m_gameOverSceneName);
    }
    public void RestartGame()
    {
        SetState(GameState.Gameplay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
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
            UnityEngine.Debug.LogError("GameManager: 再プレイするステージが見つかりません。ステージ選択へ戻ります。", this);
            LoadSceneWithFade(m_stageSelectSceneName);
        }
    }
    public void SetBGMVolume(float volume) => BGMVolume = volume;
    public PlayerMove GetPlayerMove() => m_player;
    public void LoadSceneWithFade(string sceneName, float duration = 1.0f, System.Action onFadeOutComplete = null)
    {
        if (m_isGlobalTransitioning) return;
        if (m_globalFadePanelImage == null || m_globalFadeCanvasGroup == null)
        {
            UnityEngine.Debug.LogError("GameManager: フェードに必要なUI要素が割り当てられていません！フェードなしでロードします。", this);
            SceneManager.LoadScene(sceneName);
            return;
        }
        m_isGlobalTransitioning = true;
        m_globalFadeCanvasInstance.SetActive(true);
        m_globalFadePanelImage.gameObject.SetActive(true);

        m_fadeCoroutine = StartCoroutine(FadeOutAndLoadScene(sceneName, duration, onFadeOutComplete));
    }
    public void GameOver()
    {
        if (m_currentGameState == GameState.GameOver || m_isGlobalTransitioning)
        {
            UnityEngine.Debug.LogWarning("GameManager: 既にゲームオーバー状態か、シーン遷移中のため、GameOver処理をスキップしました。", this);
            return;
        }
        SetState(GameState.GameOver);
        PlayBGM(null);
        SaveCurrentStageIndex();
        ResetGameData();
        LoadSceneWithFade(m_gameOverSceneName);
    }
    public void GameClear()
    {
        if (m_currentGameState == GameState.StageClear || m_isGlobalTransitioning)
        {
            UnityEngine.Debug.LogWarning("GameManager: 既にゲームクリア状態か、シーン遷移中のため、GameClear処理をスキップしました。", this);
            return;
        }
        SetState(GameState.StageClear);
        PlayBGM(null);
        finalScore = m_currentGemCount;
        finalTime = m_currentTime;
        LoadSceneWithFade(m_clearSceneName);
    }
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
            UnityEngine.Debug.Log("すべてのステージをクリアしました！タイトルシーンに戻ります。");
            LoadSceneWithFade(m_titleSceneName);
        }
    }
    public void SetStageClear(int stageIndex)
    {
        PlayerPrefs.SetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 1);
        PlayerPrefs.Save();
        UnityEngine.Debug.Log($"ステージ{stageIndex + 1}をクリアしました。");
    }
    public bool IsStageClear(int stageIndex) => PlayerPrefs.GetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 0) == 1;
    public void ClearAllStageData()
    {
        for (int i = 0; i < m_stageSceneNames.Length; i++)
        {
            PlayerPrefs.DeleteKey(STAGE_CLEAR_KEY_PREFIX + i);
        }
        PlayerPrefs.Save();
        UnityEngine.Debug.Log("すべてのステージクリアデータを削除しました。");
    }
    public void ResetGameData()
    {
        m_currentGemCount = 0;
        OnGemCountChanged?.Invoke(m_currentGemCount);
        m_currentTime = m_timeLimitSeconds;
    }

    // ====================================================================================================
    // #region: プライベートメソッド（ヘルパー）
    // ====================================================================================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetGameStateByScene(scene.name);
        UpdateSceneReferences(scene.name);
        PlayBGMForScene(scene.name);

        // 初回ロード時のみフェードを無効にする
        if (m_isInitialStartup)
        {
            // フェード状態をリセットして、即座に画面を表示する
            if (m_globalFadeCanvasGroup != null)
            {
                m_globalFadeCanvasGroup.alpha = 0f;
                m_globalFadeCanvasGroup.blocksRaycasts = false;
                m_globalFadeCanvasGroup.interactable = false;
            }
            m_isInitialStartup = false; // 初回ロードフラグを解除
        }
        else
        {
            // 2回目以降のロードでは通常通りフェードインを行う
            if (mode == LoadSceneMode.Single)
            {
                StartFadeIn();
            }
        }
        m_isGlobalTransitioning = false;
    }
    private void UpdateSceneReferences(string sceneName)
    {
        bool isGameplay = IsGameplayScene(sceneName);
        m_player = isGameplay ? FindObjectOfType<PlayerMove>() : null;
        if (m_player != null && m_mobileControlCanvas != null)
        {
            var joystick = m_mobileControlCanvas.transform.Find("JoystickBase")?.GetComponent<VirtualJoystick>();
            var jumpButton = m_mobileControlCanvas.transform.Find("JumpButton")?.GetComponent<JumpButtonController>();
            m_player.SetMobileControls(joystick, jumpButton);

            // 追加: ジャンプボタンにプレイヤーの参照を渡す
            if (jumpButton != null)
            {
                jumpButton.SetPlayerMove(m_player);
            }
        }
        UpdatePermanentUIForScene(sceneName);
    }
    private bool IsGameplayScene(string sceneName)
    {
        return (sceneName == "Stage1Scene" ||
            sceneName == "Stage2Scene" ||
            sceneName == "Stage3Scene" ||
            sceneName == "For open campus");
    }
    private void SetGameStateByScene(string sceneName)
    {
        if (sceneName == m_titleSceneName || sceneName == "FirstScene")
        {
            SetState(GameState.Title);
            ResetGameData();
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
            InitializeGameplayState();
        }
        else
        {
            UnityEngine.Debug.LogWarning($"GameManager: 未知のシーン'{sceneName}'から起動しました。デフォルトのゲーム状態をGameplayに設定。", this);
            SetState(GameState.Gameplay);
        }
    }
    private void InitializeGameplayState() => m_currentTime = m_timeLimitSeconds;
    private void UpdateTimeDisplay()
    {
        if (m_timeLimitText != null)
        {
            m_timeLimitText.text = ": " + Mathf.CeilToInt(m_currentTime).ToString();
            m_timeLimitText.color = m_currentTime <= 10f ? Color.red : Color.white;
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
        if (m_audioSource.clip == clip && m_audioSource.isPlaying) return;
        m_audioSource.Stop();
        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.volume = m_initialBGMVolume;
            m_audioSource.Play();
        }
    }
    private void PlayBGMForScene(string sceneName)
    {
        if (m_sceneBGMMap.ContainsKey(sceneName))
        {
            PlayBGM(m_sceneBGMMap[sceneName]);
        }
        else
        {
            PlayBGM(null);
        }
    }
    private void UpdatePermanentUIForScene(string sceneName)
    {
        bool isGameplay = IsGameplayScene(sceneName);
        if (m_permanentUICanvas != null) m_permanentUICanvas.SetActive(isGameplay);
        if (m_scorePanel != null) m_scorePanel.SetActive(isGameplay);
        if (m_timeLimitText != null) m_timeLimitText.gameObject.SetActive(isGameplay && m_isTimeLimited);
        if (m_mobileControlCanvas != null) m_mobileControlCanvas.SetActive(isGameplay && UnityEngine.Application.isMobilePlatform);
    }
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
        while (m_asyncLoadOperation.progress < 0.9f) yield return null;
        while (m_globalFadeCanvasGroup.alpha < 0.99f) yield return null;
        m_asyncLoadOperation.allowSceneActivation = true;
    }
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
            m_globalFadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        m_globalFadeCanvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }
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
    private void InitializeAudioSource()
    {
        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource == null) m_audioSource = gameObject.AddComponent<AudioSource>();
        m_audioSource.loop = true;
        m_audioSource.volume = m_initialBGMVolume;
    }
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