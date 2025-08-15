using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // TextMeshProを使用するために追加
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

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

    private GameState m_currentGameState = GameState.Gameplay;
    private int m_currentGemCount = 0;
    public System.Action<int> OnGemCountChanged;

    [Header("UI要素 (Inspectorで設定)")]
    [SerializeField] private GameObject m_permanentUICanvas;
    [SerializeField] private UnityEngine.UI.Image m_globalFadePanelImage;
    [SerializeField] private CanvasGroup m_globalFadeCanvasGroup;
    [SerializeField] private EventSystem m_permanentEventSystem;
    [SerializeField] private ScoreDisplay m_scoreDisplay;
    [SerializeField] private GameObject m_scorePanel;
    // TextMeshProUGUI型に変更
    [SerializeField] private TMPro.TextMeshProUGUI m_timeLimitText;

    [Header("シーン名 (Build Settingsに登録必須)")]
    [SerializeField] private string m_titleSceneName = "TitleScene";
    [SerializeField] private string m_bootstrapSceneName = "Bootstrap";
    [SerializeField] private string m_stageSelectSceneName = "StageSelect";
    [SerializeField] private string m_gameOverSceneName = "GameOverScene";
    [SerializeField] private string m_clearSceneName = "ClearScene";

    [SerializeField] private string[] m_stageSceneNames = { "Demo_tileset", "Demo_tileset2", "Demo_tileset3" };
    private int m_currentStageIndex = 0;

    private Coroutine m_fadeCoroutine;
    private UnityEngine.AsyncOperation m_asyncLoadOperation;
    private GameObject m_globalFadeCanvasInstance;
    public bool m_isGlobalTransitioning = false;
    [SerializeField] private float m_fadeDuration = 1.0f;
    private const string STAGE_CLEAR_KEY_PREFIX = "StageClear_";

    [Header("Time Limit Settings")]
    [Tooltip("Enable time limit for gameplay scenes.")]
    [SerializeField] private bool m_isTimeLimited = false;
    [Tooltip("Total time in seconds for the time limit.")]
    [SerializeField] private float m_timeLimitSeconds = 60.0f;

    [Header("BGM Management")]
    [Tooltip("シーン名とBGMのAudioClipをマッピングします。")]
    [SerializeField] private List<SceneBGMData> m_sceneBGMList;
    private Dictionary<string, AudioClip> m_sceneBGMMap = new Dictionary<string, AudioClip>();

    private float m_currentTime;
    private AudioSource m_audioSource;

    [System.Serializable]
    public class SceneBGMData
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    public GameState GetCurrentGameState() => m_currentGameState;
    public void SetState(GameState newState) => m_currentGameState = newState;
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

    public void SetGameOverStateImmediately()
    {
        if (m_currentGameState == GameState.GameOver) return;
        Time.timeScale = 0;
        SetState(GameState.GameOver);
        if (m_isGlobalTransitioning) return;
        LoadSceneWithFade(m_gameOverSceneName);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            CheckUIReferences();

            m_audioSource = GetComponent<AudioSource>();
            if (m_audioSource == null)
            {
                m_audioSource = gameObject.AddComponent<AudioSource>();
            }
            m_audioSource.loop = true;

            // シーン名とBGMのAudioClipを辞書に変換
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
            else
            {
                Debug.LogError("GameManager: Awake - GlobalFadeCanvasGroupが設定されていません。", this);
            }
            if (m_globalFadePanelImage == null)
            {
                Debug.LogError("GameManager: Awake - GlobalFadePanelImageが設定されていません。", this);
            }

            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName == m_bootstrapSceneName)
            {
                SceneManager.LoadScene(m_titleSceneName);
                SetState(GameState.Title);
            }
            else if (currentSceneName == m_titleSceneName)
            {
                SetState(GameState.Title);
                if (m_globalFadeCanvasGroup != null)
                {
                    m_globalFadeCanvasGroup.alpha = 0f;
                    m_globalFadeCanvasGroup.blocksRaycasts = false;
                    m_globalFadeCanvasGroup.interactable = false;
                }
            }
            else if (currentSceneName.StartsWith("Demo_tileset") || currentSceneName.StartsWith("Stage"))
            {
                SetState(GameState.Gameplay);
                UpdatePermanentUIForScene(currentSceneName);
                if (m_globalFadeCanvasGroup != null)
                {
                    m_globalFadeCanvasGroup.alpha = 0f;
                    m_globalFadeCanvasGroup.blocksRaycasts = false;
                    m_globalFadeCanvasGroup.interactable = false;
                }
            }
            else
            {
                Debug.LogWarning($"GameManager: 未知のシーン'{currentSceneName}'から起動しました。デフォルトのゲーム状態をGameplayに設定。", this);
                SetState(GameState.Gameplay);
                if (m_globalFadeCanvasGroup != null)
                {
                    m_globalFadeCanvasGroup.alpha = 0f;
                    m_globalFadeCanvasGroup.blocksRaycasts = false;
                    m_globalFadeCanvasGroup.interactable = false;
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // ゲームプレイ中で、時間制限が有効な場合
        if (m_isTimeLimited && m_currentGameState == GameState.Gameplay)
        {
            m_currentTime -= Time.deltaTime;

            // UIに残り時間を表示
            if (m_timeLimitText != null)
            {
                // 残り時間を整数に変換して表示
                m_timeLimitText.text = "Time: " + Mathf.CeilToInt(m_currentTime).ToString();

                // 時間が少なくなったらテキストの色を変えるなど視覚的な演出も可能
                if (m_currentTime <= 10f)
                {
                    m_timeLimitText.color = Color.red;
                }
                else
                {
                    m_timeLimitText.color = Color.white;
                }
            }

            // 時間切れになったらゲームオーバー
            if (m_currentTime <= 0)
            {
                m_currentTime = 0;
                GameOver();
            }
        }
    }

    private void CheckUIReferences()
    {
        if (m_permanentUICanvas == null) Debug.LogError("Permanent UICanvasが割り当てられていません。", this);
        if (m_globalFadePanelImage == null) Debug.LogError("Global Fade Panel Imageが割り当てられていません。", this);
        if (m_globalFadeCanvasGroup == null) Debug.LogError("Global Fade Canvas Groupが割り当てられていません。", this);
        if (m_permanentEventSystem == null) Debug.LogError("Permanent Event Systemが割り当てられていません。", this);
        if (m_scoreDisplay == null) Debug.LogError("Score Displayが割り当てられていません。", this);
        if (m_scorePanel == null) Debug.LogError("Score Panelが割り当てられていません。", this);
        if (m_timeLimitText == null) Debug.LogWarning("Time Limit Textが割り当てられていません。時間制限UIは表示されません。", this); // 新しい警告
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnDestroy()
    {
        if (instance == this) instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1;
        if (m_globalFadeCanvasInstance != null && !m_globalFadeCanvasInstance.activeSelf) m_globalFadeCanvasInstance.SetActive(true);
        if (m_globalFadePanelImage != null && !m_globalFadePanelImage.gameObject.activeSelf) m_globalFadePanelImage.gameObject.SetActive(true);
        if (mode == LoadSceneMode.Single)
        {
            bool shouldStartFadeIn = !(scene.name == m_titleSceneName && !m_isGlobalTransitioning) && scene.name != m_bootstrapSceneName;
            if (shouldStartFadeIn)
            {
                StartFadeIn();
            }
            else
            {
                if (m_globalFadeCanvasGroup != null)
                {
                    m_globalFadeCanvasGroup.alpha = 0f;
                    m_globalFadeCanvasGroup.blocksRaycasts = false;
                    m_globalFadeCanvasGroup.interactable = false;
                }
            }
        }
        UpdatePermanentUIForScene(scene.name);

        if (m_sceneBGMMap.ContainsKey(scene.name))
        {
            PlayBGM(m_sceneBGMMap[scene.name]);
        }
        else
        {
            PlayBGM(null);
        }

        if (scene.name == m_titleSceneName)
        {
            SetState(GameState.Title);
            ResetGameData();
        }
        else if (scene.name == m_stageSelectSceneName)
        {
            SetState(GameState.StageSelect);
        }
        else if (scene.name == m_gameOverSceneName)
        {
            SetState(GameState.GameOver);
        }
        else if (scene.name == m_clearSceneName)
        {
            SetState(GameState.StageClear);
        }
        else if (scene.name.StartsWith("Demo_tileset") || scene.name.StartsWith("Stage"))
        {
            SetState(GameState.Gameplay);
            InitializeGameplayState();
        }

        m_isGlobalTransitioning = false;
    }

    private void InitializeGameplayState()
    {
        if (m_isTimeLimited)
        {
            m_currentTime = m_timeLimitSeconds;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (m_audioSource == null)
        {
            Debug.LogWarning("GameManager: AudioSourceが割り当てられていません。", this);
            return;
        }

        if (m_audioSource.isPlaying)
        {
            m_audioSource.Stop();
        }

        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }
    }

    private void UpdatePermanentUIForScene(string sceneName)
    {
        bool isPermanentUIActive = sceneName.StartsWith("Demo_tileset") || sceneName.StartsWith("Stage");
        if (sceneName == m_bootstrapSceneName || sceneName == m_titleSceneName ||
            sceneName == m_gameOverSceneName || sceneName == m_clearSceneName ||
            sceneName == m_stageSelectSceneName)
        {
            isPermanentUIActive = false;
        }
        if (m_permanentUICanvas != null) m_permanentUICanvas.SetActive(isPermanentUIActive);
        else Debug.LogWarning("GameManager: Permanent UI Canvas (ScorePanelなど) が割り当てられていません。", this);
        bool isGameplayScene = sceneName.StartsWith("Demo_tileset") || sceneName.StartsWith("Stage");
        SetScoreUIActive(isGameplayScene);
        SetTimeLimitUIActive(isGameplayScene && m_isTimeLimited);
    }

    private void SetTimeLimitUIActive(bool isActive)
    {
        if (m_timeLimitText != null)
        {
            m_timeLimitText.gameObject.SetActive(isActive);
        }
    }

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

    private IEnumerator FadeOutAndLoadScene(string sceneName, float duration, System.Action onFadeOutComplete)
    {
        if (m_globalFadeCanvasGroup != null)
        {
            m_globalFadeCanvasGroup.blocksRaycasts = true;
            m_globalFadeCanvasGroup.interactable = true;
        }
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, duration, () =>
        {
            onFadeOutComplete?.Invoke();
            m_asyncLoadOperation = SceneManager.LoadSceneAsync(sceneName);
            m_asyncLoadOperation.allowSceneActivation = false;
        }));

        while (!m_asyncLoadOperation.isDone)
        {
            if (m_asyncLoadOperation.progress >= 0.9f)
            {
                if (m_globalFadeCanvasGroup.alpha >= 0.99f)
                {
                    m_asyncLoadOperation.allowSceneActivation = true;
                }
            }
            yield return null;
        }
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
            float progress = timer / duration;
            m_globalFadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            m_globalFadePanelImage.color = new Color(0f, 0f, 0f, m_globalFadePanelImage.color.a);
            yield return null;
        }
        m_globalFadeCanvasGroup.alpha = endAlpha;
        m_globalFadePanelImage.color = new Color(0f, 0f, 0f, m_globalFadePanelImage.color.a);
        onComplete?.Invoke();
    }

    private void StartFadeIn()
    {
        if (m_fadeCoroutine != null) StopCoroutine(m_fadeCoroutine);
        if (m_globalFadeCanvasInstance != null && !m_globalFadeCanvasInstance.activeSelf) m_globalFadeCanvasInstance.SetActive(true);
        if (m_globalFadePanelImage != null && !m_globalFadePanelImage.gameObject.activeSelf) m_globalFadePanelImage.gameObject.SetActive(true);
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

    public void GameOver()
    {
        if (m_currentGameState == GameState.GameOver || m_isGlobalTransitioning)
        {
            Debug.LogWarning("GameManager: 既にゲームオーバー状態か遷移中のため、GameOver処理をスキップしました。", this);
            return;
        }
        SetState(GameState.GameOver);
        PlayBGM(null);
        LoadSceneWithFade(m_gameOverSceneName);
    }

    public void GameClear()
    {
        if (m_currentGameState == GameState.StageClear || m_isGlobalTransitioning)
        {
            Debug.LogWarning("GameManager: 既にゲームクリア状態か遷移中のため、GameClear処理をスキップしました。", this);
            return;
        }
        SetState(GameState.StageClear);
        PlayBGM(null);
        LoadSceneWithFade(m_clearSceneName);
    }

    public void SetStageClear(int stageIndex)
    {
        PlayerPrefs.SetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 1);
        PlayerPrefs.Save();
        Debug.Log($"ステージ{stageIndex + 1}をクリアしました。");
    }

    public bool IsStageClear(int stageIndex) => PlayerPrefs.GetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 0) == 1;
    public void ClearAllStageData()
    {
        for (int i = 0; i < m_stageSceneNames.Length; i++)
        {
            PlayerPrefs.DeleteKey(STAGE_CLEAR_KEY_PREFIX + i);
        }
        PlayerPrefs.Save();
        Debug.Log("すべてのステージクリアデータを削除しました。");
    }

    public void ResetGameData()
    {
        m_currentGemCount = 0;
        OnGemCountChanged?.Invoke(m_currentGemCount);
        m_currentTime = m_timeLimitSeconds;
    }

    public void SetScoreUIActive(bool isActive)
    {
        if (m_scorePanel != null)
        {
            if (m_scorePanel.activeSelf != isActive)
            {
                m_scorePanel.SetActive(isActive);
            }
        }
        else
        {
            Debug.LogWarning("GameManager: ScorePanelが割り当てられていないため、SetScoreUIActiveをスキップしました。", this);
        }
    }
}
