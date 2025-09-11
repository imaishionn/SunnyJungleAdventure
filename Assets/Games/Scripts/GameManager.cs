using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ゲーム全体の進行状況、状態、データ、UI、BGMなどを管理するシングルトンクラス。
/// シーンをまたいで存在し、ゲームの中心的な制御を担います。
/// </summary>
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("GameManager"), SerializeField]
    private GameObject _permanentUICanvas;
    [SerializeField]
    private CanvasGroup _globalFadeCanvasGroup;
    [SerializeField]
    private UnityEngine.UI.Image _globalFadePanelImage;
    [SerializeField]
    private UnityEngine.EventSystems.EventSystem _permanentEventSystem;
    [SerializeField]
    private ScoreDisplay _scoreDisplay;
    [SerializeField]
    private GameObject _scorePanel;
    [SerializeField]
    private TMPro.TextMeshProUGUI _timeLimitText;
    [SerializeField]
    private GameObject _mobileControlCanvas;

    [Header("Sceneの名前"), SerializeField]
    private string _gameOverSceneName = "GameOverScene";
    [SerializeField]
    private string _clearSceneName = "ClearScene";
    [SerializeField]
    private string _scoreSceneName = "ResultScene";

    [Header("時間制限"), SerializeField]
    private bool _isTimeLimited = false;
    [SerializeField]
    private float _timeLimitSeconds = 60.0f;
    [SerializeField]
    private float _fadeDuration = 1.0f;

    [Header("BGM"), SerializeField]
    private List<SceneBGMData> _sceneBGMList;
    [Range(0f, 1f), SerializeField]
    private float _initialBGMVolume = 0.5f;

    // シーン名の定数
    [Header("ステージシーンの設定")]
    public string[] StageSceneNames;
    public readonly string TitleSceneName = "TitleScene";
    public readonly string StageSelectSceneName = "StageSelectScene";
    public readonly string StageSelect2SceneName = "StageSelect2Scene";

    public enum GameState {
        Title,
        Gameplay,
        StageClear,
        GameOver,
        Pause,
        Cutscene,
        StageSelect
    }

    [System.Serializable]
    public class SceneBGMData {
        public string sceneName;
        public AudioClip bgmClip;
    }

    // プロパティ
    public float BGMVolume {
        get => _audioSource != null ? _audioSource.volume : 0f;
        set {
            if (_audioSource != null) {
                _audioSource.volume = Mathf.Clamp01(value);
            }
        }
    }
    public int FinalScore { get; private set; }
    public float FinalTime { get; private set; }
    public int CurrentGemCount { get; private set; }
    public int CurrentStageIndex { get; set; }
    public string CurrentSceneName { get; private set; }

    // イベント
    public System.Action<int> OnGemCountChanged;

    private GameState _currentGameState = GameState.Gameplay;
    private float _currentTime;
    private Coroutine _fadeCoroutine;
    private bool _isGlobalTransitioning = false;
    private AudioSource _audioSource;
    private readonly Dictionary<string, AudioClip> _sceneBGMMap = new();

    private const string STAGE_CLEAR_KEY_PREFIX = "StageClear_";
    private const string STAGE_SCORE_KEY_PREFIX = "StageScore_";

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManagerインスタンスを作成し、DontDestroyOnLoadに設定しました。", this);

            InitializeAudioSource();
            InitializeBGMMap();
            InitializeFadeCanvas();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else {
            Debug.LogWarning("重複したGameManagerインスタンスが見つかりました。このオブジェクトを破棄します。", this);
            Destroy(gameObject);
        }
    }

    private void Start() {
        if (SceneManager.GetActiveScene().name == "FirstScene") {
            SceneManager.LoadScene(TitleSceneName);
        }
    }

    private void OnDestroy() {
        if (Instance == this) {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void Update() {
        if (_isTimeLimited && _currentGameState == GameState.Gameplay) {
            _currentTime -= Time.deltaTime;
            UpdateTimeDisplay();
            if (_currentTime <= 0) {
                _currentTime = 0;
                GameOver();
            }
        }
    }

    public GameState GetCurrentGameState() => _currentGameState;

    public void SetState(GameState newState) {
        _currentGameState = newState;
        Time.timeScale = newState switch {
            GameState.Gameplay or GameState.Cutscene => 1,
            GameState.Pause or GameState.GameOver or GameState.StageClear => 0,
            _ => (float)1,
        };
    }

    public void AddScore(int amount) {
        CurrentGemCount += amount;
        OnGemCountChanged?.Invoke(CurrentGemCount);
    }

    public void LoadSceneWithFade(string sceneName, float duration = -1f, System.Action onFadeOutComplete = null) {
        if (_isGlobalTransitioning) {
            return;
        }
        _isGlobalTransitioning = true;

        float finalDuration = (duration > 0) ? duration : _fadeDuration;

        if (_globalFadeCanvasGroup == null) {
            Debug.LogError("GameManager: フェード用Canvas Groupが割り当てられていません。フェードなしでシーンをロードします。", this);
            SceneManager.LoadScene(sceneName);
            return;
        }
        if (_fadeCoroutine != null) {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeOutAndLoadScene(sceneName, finalDuration, onFadeOutComplete));
    }

    public void GameOver() {
        if (_currentGameState == GameState.GameOver || _isGlobalTransitioning) {
            return;
        }

        // ★修正: ゲームオーバー時のシーン名を保存
        CurrentSceneName = SceneManager.GetActiveScene().name;

        FinalScore = CurrentGemCount;
        FinalTime = _currentTime;
        SetState(GameState.GameOver);

        PlayBGM(null);
        ResetGameData();
        LoadSceneWithFade(_gameOverSceneName);
    }

    public void GameClear() {
        if (_currentGameState == GameState.StageClear || _isGlobalTransitioning) {
            return;
        }
        FinalScore = CurrentGemCount;
        FinalTime = _currentTime;
        SetState(GameState.StageClear);

        PlayBGM(null);
        SaveStageScore(CurrentStageIndex, FinalScore);
        SetStageClear(CurrentStageIndex);
        LoadSceneWithFade(_clearSceneName);
    }

    public void RetryLastStage() {
        if (_isGlobalTransitioning) {
            return;
        }
        SetState(GameState.Gameplay);
        if (!string.IsNullOrEmpty(CurrentSceneName)) {
            LoadSceneWithFade(CurrentSceneName);
        }
        else {
            Debug.LogError("GameManager: ステージをリトライできません。タイトルへ戻ります。", this);
            LoadSceneWithFade(TitleSceneName);
        }
    }

    public void GoToNextStage() {
        if (_isGlobalTransitioning) {
            return;
        }
        CurrentStageIndex++;
        if (CurrentStageIndex < StageSceneNames.Length) {
            LoadSceneWithFade(StageSceneNames[CurrentStageIndex]);
        }
        else {
            Debug.Log("すべてのステージをクリアしました！タイトルシーンへ戻ります。");
            LoadSceneWithFade(TitleSceneName);
        }
    }

    public void SetStageClear(int stageIndex) {
        if (stageIndex >= 0 && stageIndex < StageSceneNames.Length) {
            PlayerPrefs.SetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 1);
            PlayerPrefs.Save();
            Debug.Log($"ステージ {stageIndex + 1} をクリアしました。");
        }
        else {
            Debug.LogWarning($"無効なステージインデックス {stageIndex} です。クリア情報は保存されません。");
        }
    }

    public bool IsStageClear(int stageIndex) => PlayerPrefs.GetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex, 0) == 1;

    public void SaveStageScore(int stageIndex, int score) {
        if (stageIndex >= 0 && stageIndex < StageSceneNames.Length) {
            PlayerPrefs.SetInt(STAGE_SCORE_KEY_PREFIX + stageIndex, score);
            PlayerPrefs.Save();
        }
    }

    public int GetStageScore(int stageIndex) => PlayerPrefs.GetInt(STAGE_SCORE_KEY_PREFIX + stageIndex, 0);

    public void ClearAllStageData() {
        for (int i = 0; i < StageSceneNames.Length; i++) {
            PlayerPrefs.DeleteKey(STAGE_CLEAR_KEY_PREFIX + i);
            PlayerPrefs.DeleteKey(STAGE_SCORE_KEY_PREFIX + i);
        }
        PlayerPrefs.Save();
        Debug.Log("すべてのステージクリア・スコアデータを削除しました。");
    }

    public void ResetGameData() {
        CurrentGemCount = 0;
        OnGemCountChanged?.Invoke(CurrentGemCount);
        _currentTime = _isTimeLimited ? _timeLimitSeconds : 0f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        SetGameStateByScene(scene.name);
        UpdatePermanentUIForScene(scene.name);
        PlayBGMForScene(scene.name);
        _isGlobalTransitioning = false;

        // CurrentSceneNameはGameOver()メソッドで設定するので、このメソッドからは削除
        // ここでの設定は、特定のシーンへの遷移を妨げる可能性がありました。

        if (IsGameplayScene(scene.name) || scene.name == TitleSceneName || scene.name.Contains("StageSelect")) {
            if (_permanentUICanvas != null) {
                _permanentUICanvas.SetActive(true);
            }
        }

        SaveCurrentStageIndex();
    }

    private bool IsGameplayScene(string sceneName) {
        if (StageSceneNames == null) {
            return false;
        }

        foreach (string stageName in StageSceneNames) {
            if (stageName == sceneName) {
                return true;
            }
        }
        return false;
    }

    private void SetGameStateByScene(string sceneName) {
        if (sceneName == TitleSceneName || sceneName == "FirstScene") {
            SetState(GameState.Title);
            ResetGameData();
        }
        else if (sceneName.Contains("StageSelect")) {
            SetState(GameState.StageSelect);
        }
        else if (sceneName == _gameOverSceneName) {
            SetState(GameState.GameOver);
        }
        else if (sceneName == _clearSceneName || sceneName == _scoreSceneName) {
            SetState(GameState.StageClear);
        }
        else if (IsGameplayScene(sceneName)) {
            SetState(GameState.Gameplay);
            InitializeGameplayState();
        }
        else {
            Debug.LogWarning($"GameManager: 不明なシーン'{sceneName}'です。ゲーム状態をGameplayに設定します。", this);
            SetState(GameState.Gameplay);
        }
    }

    private void InitializeGameplayState() {
        _currentTime = _timeLimitSeconds;
        OnGemCountChanged?.Invoke(CurrentGemCount);
    }

    private void UpdateTimeDisplay() {
        if (_timeLimitText != null) {
            _timeLimitText.text = Mathf.CeilToInt(_currentTime).ToString();
            _timeLimitText.color = _currentTime <= 10f ? Color.red : Color.white;
        }
    }

    public void PlayBGM(AudioClip clip) {
        if (_audioSource == null) {
            Debug.LogWarning("GameManager: BGM用AudioSourceが割り当てられていません。", this);
            return;
        }
        if (_audioSource.clip == clip && _audioSource.isPlaying) {
            return;
        }

        _audioSource.Stop();
        if (clip != null) {
            _audioSource.clip = clip;
            _audioSource.volume = _initialBGMVolume;
            _audioSource.Play();
        }
    }

    private void PlayBGMForScene(string sceneName) {
        if (_sceneBGMMap.ContainsKey(sceneName)) {
            PlayBGM(_sceneBGMMap[sceneName]);
        }
        else {
            PlayBGM(null);
        }
    }

    private void UpdatePermanentUIForScene(string sceneName) {
        bool isGameplay = IsGameplayScene(sceneName);

        if (_permanentUICanvas != null) {
            _permanentUICanvas.SetActive(isGameplay || sceneName == TitleSceneName || sceneName.Contains("StageSelect"));
        }

        if (_scorePanel != null) {
            _scorePanel.SetActive(isGameplay);
        }
        if (_timeLimitText != null) {
            _timeLimitText.gameObject.SetActive(isGameplay && _isTimeLimited);
        }
        if (_mobileControlCanvas != null) {
            _mobileControlCanvas.SetActive(isGameplay && Application.isMobilePlatform);
        }
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName, float duration, System.Action onFadeOutComplete) {
        if (_globalFadeCanvasGroup == null) {
            yield break;
        }

        _globalFadeCanvasGroup.blocksRaycasts = true;
        _globalFadeCanvasGroup.interactable = true;
        _permanentUICanvas.SetActive(false);

        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, duration, onFadeOutComplete));
        yield return SceneManager.LoadSceneAsync(sceneName);

        _fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, 0f, duration, () => {
            _isGlobalTransitioning = false;
            if (_globalFadeCanvasGroup != null) {
                _globalFadeCanvasGroup.blocksRaycasts = false;
                _globalFadeCanvasGroup.interactable = false;
            }
        }));
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration, System.Action onComplete = null) {
        if (_globalFadeCanvasGroup == null) {
            onComplete?.Invoke();
            yield break;
        }

        float timer = 0f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            _globalFadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        _globalFadeCanvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }

    private void InitializeAudioSource() {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.loop = true;
        _audioSource.volume = _initialBGMVolume;
    }

    private void InitializeBGMMap() {
        foreach (SceneBGMData data in _sceneBGMList) {
            if (!_sceneBGMMap.ContainsKey(data.sceneName)) {
                _sceneBGMMap.Add(data.sceneName, data.bgmClip);
            }
        }
    }

    private void InitializeFadeCanvas() {
        if (_globalFadeCanvasGroup != null) {
            _globalFadeCanvasGroup.alpha = 0f;
            _globalFadeCanvasGroup.blocksRaycasts = false;
            _globalFadeCanvasGroup.interactable = false;
        }
    }

    private void SaveCurrentStageIndex() {
        string currentSceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < StageSceneNames.Length; i++) {
            if (StageSceneNames[i] == currentSceneName) {
                CurrentStageIndex = i;
                // CurrentSceneName = currentSceneName; // OnSceneLoaded()からは削除
                return;
            }
        }
        // ステージシーン以外の場合、インデックスをリセット
        CurrentStageIndex = -1;
        // CurrentSceneName = ""; // OnSceneLoaded()からは削除
    }
}
