using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の進行状況、状態、データ、UI、BGMなどを管理するシングルトンクラス。
/// シーンをまたいで存在し、ゲームの中心的な制御を担います。
/// </summary>
public class GameManager : MonoBehaviour {
    // ====================================================================================================
    // #region: シングルトン
    // ====================================================================================================
    public static GameManager Instance { get; private set; }

    // ====================================================================================================
    // #region: メンバ変数（インスペクターから設定）
    // ====================================================================================================
    [Header("UI要素")]
    [Tooltip("シーン遷移時に破壊されない永続的なUIキャンバス")]
    [SerializeField] private GameObject _permanentUICanvas;
    [Tooltip("シーンフェードに使用するキャンバスグループ")]
    [SerializeField] private CanvasGroup _globalFadeCanvasGroup;
    [Tooltip("シーンフェードに使用するパネルのImageコンポーネント")]
    [SerializeField] private UnityEngine.UI.Image _globalFadePanelImage;
    [Tooltip("永続的なEventSystem")]
    [SerializeField] private UnityEngine.EventSystems.EventSystem _permanentEventSystem;
    [Tooltip("スコア表示を管理するクラス (必要に応じて)")]
    [SerializeField] private ScoreDisplay _scoreDisplay;
    [Tooltip("スコアパネルのGameObject")]
    [SerializeField] private GameObject _scorePanel;
    [Tooltip("時間制限を表示するTextMeshProのテキスト")]
    [SerializeField] private TMPro.TextMeshProUGUI _timeLimitText;
    [Tooltip("モバイル操作用のUIキャンバス")]
    [SerializeField] private GameObject _mobileControlCanvas;
    [Tooltip("ゲームオーバーシーンの名前")]
    [SerializeField] private string _gameOverSceneName = "GameOverScene";
    [Tooltip("ゲームクリアシーンの名前")]
    [SerializeField] private string _clearSceneName = "ClearScene";
    [Tooltip("スコア表示シーンの名前")]
    [SerializeField] private string _scoreSceneName = "StageSelectScene";

    [Header("時間制限設定")]
    [Tooltip("ゲームプレイシーンで時間制限を有効にするか")]
    [SerializeField] private bool _isTimeLimited = false;
    [Tooltip("時間制限の合計時間 (秒)")]
    [SerializeField] private float _timeLimitSeconds = 60.0f;

    [Header("BGM管理")]
    [Tooltip("シーン名とBGMのAudioClipをマッピングします。")]
    [SerializeField] private List<SceneBGMData> _sceneBGMList;
    [Tooltip("BGMの初期音量 (0.0fから1.0f)")]
    [Range(0f,1f)]
    [SerializeField] private float _initialBGMVolume = 0.5f;

    // ====================================================================================================
    // #region: メンバ変数（スクリプト内部で管理）
    // ====================================================================================================
    private GameState _currentGameState = GameState.Gameplay;
    private float _currentTime;
    private Coroutine _fadeCoroutine;
    private AsyncOperation _asyncLoadOperation;
    private GameObject _globalFadeCanvasInstance;
    private bool _isGlobalTransitioning = false;
    private AudioSource _audioSource;
    private PlayerMove _player;
    private readonly Dictionary<string,AudioClip> _sceneBGMMap = new();

    private const string STAGE_CLEAR_KEY_PREFIX = "StageClear_";

    // ====================================================================================================
    // #region: 列挙型とクラス
    // ====================================================================================================
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

    // ====================================================================================================
    // #region: プロパティとパブリックイベント
    // ====================================================================================================
    public float BGMVolume {
        get => _audioSource != null ? _audioSource.volume : 0f;
        set => _audioSource.volume = Mathf.Clamp01(value);
    }
    // プロパティ名を大文字始まりに変更
    public int FinalScore { get; private set; }
    public float FinalTime { get; private set; }
    public int CurrentGemCount { get; private set; } = 0;
    [field: Tooltip("ステージシーンの配列")]
    [field: SerializeField]
    public string[] StageSceneNames { get; } = { "Stage1Scene","Stage2Scene","Stage3Scene","OpenCampus" };
    public int CurrentStageIndex { get; set; } = 0;
    [field: Header("シーン名 (Build Settingsに登録必須)")]
    [field: Tooltip("タイトルシーンの名前")]
    [field: SerializeField]
    public string TitleSceneName { get; } = "TitleScene";
    [field: Tooltip("ステージ選択シーンの名前")]
    [field: SerializeField]
    public string StageSelectSceneName { get; } = "StageSelectScene";
    [field: SerializeField]
    public float FadeDuration { get; } = 1.0f;

    public System.Action<int> OnGemCountChanged;

    // ====================================================================================================
    // #region: MonoBehaviour ライフサイクル
    // ====================================================================================================
    private void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManagerインスタンスが作成され、DontDestroyOnLoadに設定されました。",this);

            // UIやオーディオの初期化はAwake()で行う
            CheckUIReferences();
            InitializeAudioSource();
            InitializeBGMMap();
            InitializeFadeCanvas();
        }
        else {
            Debug.LogWarning("GameManagerのインスタンスが既に存在するため、このオブジェクトは破棄されました。",this);
            Destroy(gameObject);
        }
    }

    private void Start() {
        // 初回起動時（FirstScene）のみ、タイトルへ遷移する
        string currentSceneName = SceneManager.GetActiveScene().name;
        if(currentSceneName == "FirstScene") {
            // ここを修正：フェードなしのシーンロードに変更
            SceneManager.LoadScene(TitleSceneName);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnDestroy() {
        if(Instance == this) {
            Instance = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update() {
        if(_isTimeLimited && _currentGameState == GameState.Gameplay) {
            _currentTime -= Time.deltaTime;
            UpdateTimeDisplay();

            if(_currentTime <= 0) {
                _currentTime = 0;
                GameOver();
            }
        }
    }

    // ====================================================================================================
    // #region: パブリックメソッド
    // ====================================================================================================
    public GameState GetCurrentGameState() => _currentGameState;
    public void SetState(GameState newState) {
        _currentGameState = newState;
        switch(newState) {
            case GameState.Gameplay:
            case GameState.Cutscene:
                Time.timeScale = 1;
                break;
            case GameState.Pause:
            case GameState.GameOver:
            case GameState.StageClear:
                Time.timeScale = 0;
                break;
            case GameState.Title:
                break;
            case GameState.StageSelect:
                break;
            default:
                Time.timeScale = 1;
                break;
        }
    }
    public void AddGem(int amount) {
        CurrentGemCount += amount;
        // 修正: ヌル合意演算子から明示的なヌルチェックに変更
        OnGemCountChanged?.Invoke(CurrentGemCount);
    }
    public void SetGameOverStateImmediately() {
        if(_currentGameState != GameState.GameOver) {
            SetState(GameState.GameOver);
            if(_isGlobalTransitioning) {
                return;
            }

            LoadSceneWithFade(_gameOverSceneName);
        }
    }
    public void RestartGame() {
        SetState(GameState.Gameplay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void RetryLastStage() {
        if(_isGlobalTransitioning) {
            return;
        }

        SetState(GameState.Gameplay);
        if(CurrentStageIndex >= 0 && CurrentStageIndex < StageSceneNames.Length) {
            string sceneName = StageSceneNames[CurrentStageIndex];
            LoadSceneWithFade(sceneName);
        }
        else {
            Debug.LogError("GameManager: 再プレイするステージが見つかりません。ステージ選択へ戻ります。",this);
            LoadSceneWithFade(StageSelectSceneName);
        }
    }
    public void SetBGMVolume(float volume) => BGMVolume = volume;
    public PlayerMove GetPlayerMove() => _player;
    public void LoadSceneWithFade(string sceneName,float duration = 1.0f,System.Action onFadeOutComplete = null) {
        if(_isGlobalTransitioning) {
            return;
        }

        if(_globalFadePanelImage == null || _globalFadeCanvasGroup == null) {
            Debug.LogError("GameManager: フェードに必要なUI要素が割り当てられていません！フェードなしでロードします。",this);
            SceneManager.LoadScene(sceneName);
            return;
        }
        _isGlobalTransitioning = true;
        _globalFadeCanvasInstance.SetActive(true);
        _globalFadePanelImage.gameObject.SetActive(true);

        _fadeCoroutine = StartCoroutine(FadeOutAndLoadScene(sceneName,duration,onFadeOutComplete));
    }
    public void GameOver() {
        if(_currentGameState == GameState.GameOver || _isGlobalTransitioning) {
            UnityEngine.Debug.LogWarning("GameManager: 既にゲームオーバー状態か、シーン遷移中のため、GameOver処理をスキップしました。",this);
            return;
        }
        SetState(GameState.GameOver);
        PlayBGM(null);
        SaveCurrentStageIndex();
        ResetGameData();
        LoadSceneWithFade(_gameOverSceneName);
    }
    public void GameClear() {
        if(_currentGameState == GameState.StageClear || _isGlobalTransitioning) {
            UnityEngine.Debug.LogWarning("GameManager: 既にゲームクリア状態か、シーン遷移中のため、GameClear処理をスキップしました。",this);
            return;
        }
        SetState(GameState.StageClear);
        PlayBGM(null);
        // プロパティ名を大文字始まりに変更
        FinalScore = CurrentGemCount;
        FinalTime = _currentTime;
        LoadSceneWithFade(_clearSceneName);
    }
    public void GoToNextStage() {
        if(_isGlobalTransitioning) {
            return;
        }

        CurrentStageIndex++;
        if(CurrentStageIndex < StageSceneNames.Length) {
            LoadSceneWithFade(StageSceneNames[CurrentStageIndex]);
        }
        else {
            UnityEngine.Debug.Log("すべてのステージをクリアしました！タイトルシーンに戻ります。");
            LoadSceneWithFade(TitleSceneName);
        }
    }
    public void SetStageClear(int stageIndex) {
        PlayerPrefs.SetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex,1);
        PlayerPrefs.Save();
        UnityEngine.Debug.Log($"ステージ{stageIndex + 1}をクリアしました。");
    }
    public bool IsStageClear(int stageIndex) => PlayerPrefs.GetInt(STAGE_CLEAR_KEY_PREFIX + stageIndex,0) == 1;
    public void ClearAllStageData() {
        for(int i = 0;i < StageSceneNames.Length;i++) {
            PlayerPrefs.DeleteKey(STAGE_CLEAR_KEY_PREFIX + i);
        }
        PlayerPrefs.Save();
        UnityEngine.Debug.Log("すべてのステージクリアデータを削除しました。");
    }
    public void ResetGameData() {
        CurrentGemCount = 0;
        // 修正: ヌル合意演算子から明示的なヌルチェックに変更
        OnGemCountChanged?.Invoke(CurrentGemCount);
        _currentTime = _timeLimitSeconds;
    }

    // ====================================================================================================
    // #region: プライベートメソッド（ヘルパー）
    // ====================================================================================================
    private void OnSceneLoaded(Scene scene,LoadSceneMode mode) {
        SetGameStateByScene(scene.name);
        UpdateSceneReferences(scene.name);
        PlayBGMForScene(scene.name);

        // ここを修正：_isGlobalTransitioningがtrueの場合のみフェードインを開始
        if(_isGlobalTransitioning) {
            StartFadeIn();
        }
        else {
            // 通常のシーンロード（フェードなし）の場合、フェードUIを非表示にする
            if(_globalFadeCanvasGroup != null) {
                _globalFadeCanvasGroup.alpha = 0f;
                _globalFadeCanvasGroup.blocksRaycasts = false;
                _globalFadeCanvasGroup.interactable = false;
            }
        }
        _isGlobalTransitioning = false;
    }

    private void UpdateSceneReferences(string sceneName) => UpdateSceneReferences(sceneName,_mobileControlCanvas);

    private void UpdateSceneReferences(string sceneName,GameObject mobileControlCanvas) {
        bool isGameplay = IsGameplayScene(sceneName);
        _player = isGameplay ? FindObjectOfType<PlayerMove>() : null;
        if(_player != null && mobileControlCanvas != null) {
            // 修正: ヌル合意演算子を明示的なヌルチェックに変更
            Transform joystickComponent = mobileControlCanvas.transform.Find("JoystickBase");
            VirtualJoystick joystick = null;
            if(joystickComponent != null) {
                joystick = joystickComponent.GetComponent<VirtualJoystick>();
            }
            Transform jumpButtonComponent = mobileControlCanvas.transform.Find("JumpButton");
            JumpButtonController jumpButton = null;
            if(jumpButtonComponent != null) {
                jumpButton = jumpButtonComponent.GetComponent<JumpButtonController>();
            }

            _player.SetMobileControls(joystick,jumpButton);

            // 追加: ジャンプボタンにプレイヤーの参照を渡す
            if(jumpButton != null) {
                jumpButton.SetPlayerMove(_player);
            }
        }
        UpdatePermanentUIForScene(sceneName);
    }
    private bool IsGameplayScene(string sceneName) {
        return (sceneName == "Stage1Scene" ||
            sceneName == "Stage2Scene" ||
            sceneName == "Stage3Scene" ||
            sceneName == "OpenCampus");
    }
    private void SetGameStateByScene(string sceneName) {
        if(sceneName == TitleSceneName || sceneName == "FirstScene") {
            SetState(GameState.Title);
            ResetGameData();
        }
        else if(sceneName == StageSelectSceneName || sceneName.Contains("StageSelect")) {
            SetState(GameState.StageSelect);
        }
        else if(sceneName == _gameOverSceneName) {
            SetState(GameState.GameOver);
        }
        else if(sceneName == _clearSceneName || sceneName == _scoreSceneName) {
            SetState(GameState.StageClear);
        }
        else if(IsGameplayScene(sceneName)) {
            SetState(GameState.Gameplay);
            InitializeGameplayState();
        }
        else {
            UnityEngine.Debug.LogWarning($"GameManager: 未知のシーン'{sceneName}'から起動しました。デフォルトのゲーム状態をGameplayに設定。",this);
            SetState(GameState.Gameplay);
        }
    }
    private void InitializeGameplayState() => _currentTime = _timeLimitSeconds;
    private void UpdateTimeDisplay() {
        if(_timeLimitText != null) {
            _timeLimitText.text = Mathf.CeilToInt(_currentTime).ToString();
            _timeLimitText.color = _currentTime <= 10f ? Color.red : Color.white;
        }
    }

    /// <summary>
    /// BGMを再生します。clipがnullの場合は停止します。
    /// </summary>
    public void PlayBGM(AudioClip clip) {
        if(_audioSource == null) {
            UnityEngine.Debug.LogWarning("GameManager: BGM再生用のAudioSourceが割り当てられていません。",this);
            return;
        }
        if(_audioSource.clip == clip && _audioSource.isPlaying) {
            return;
        }

        _audioSource.Stop();
        if(clip != null) {
            _audioSource.clip = clip;
            _audioSource.volume = _initialBGMVolume;
            _audioSource.Play();
        }
    }
    private void PlayBGMForScene(string sceneName) {
        if(_sceneBGMMap.ContainsKey(sceneName)) {
            PlayBGM(_sceneBGMMap[sceneName]);
        }
        else {
            PlayBGM(null);
        }
    }
    private void UpdatePermanentUIForScene(string sceneName) {
        bool isGameplay = IsGameplayScene(sceneName);
        if(_permanentUICanvas != null) {
            _permanentUICanvas.SetActive(isGameplay);
        }

        if(_scorePanel != null) {
            _scorePanel.SetActive(isGameplay);
        }

        if(_timeLimitText != null) {
            _timeLimitText.gameObject.SetActive(isGameplay && _isTimeLimited);
        }

        if(_mobileControlCanvas != null) {
            _mobileControlCanvas.SetActive(isGameplay && UnityEngine.Application.isMobilePlatform);
        }
    }
    private IEnumerator FadeOutAndLoadScene(string sceneName,float duration,System.Action onFadeOutComplete) {
        if(_globalFadeCanvasGroup != null) {
            _globalFadeCanvasGroup.blocksRaycasts = true;
            _globalFadeCanvasGroup.interactable = true;
        }
        // 修正: ヌル合意演算子から明示的なヌルチェックに変更
        yield return StartCoroutine(FadeCanvasGroup(0f,1f,duration,() => {
            onFadeOutComplete?.Invoke();
        }));

        _asyncLoadOperation = SceneManager.LoadSceneAsync(sceneName);
        _asyncLoadOperation.allowSceneActivation = false;
        while(_asyncLoadOperation.progress < 0.9f) {
            yield return null;
        }

        while(_globalFadeCanvasGroup.alpha < 0.99f) {
            yield return null;
        }

        _asyncLoadOperation.allowSceneActivation = true;
    }
    private IEnumerator FadeCanvasGroup(float startAlpha,float endAlpha,float duration,System.Action onComplete = null) {
        if(_globalFadeCanvasGroup == null || _globalFadePanelImage == null) {
            // 修正: ヌル合意演算子から明示的なヌルチェックに変更
            onComplete?.Invoke();
            yield break;
        }
        _globalFadeCanvasGroup.alpha = startAlpha;
        _globalFadeCanvasGroup.blocksRaycasts = startAlpha > 0;
        _globalFadeCanvasGroup.interactable = startAlpha > 0;
        _globalFadePanelImage.color = new Color(0f,0f,0f,_globalFadePanelImage.color.a);
        float timer = 0f;
        while(timer < duration) {
            timer += Time.unscaledDeltaTime;
            _globalFadeCanvasGroup.alpha = Mathf.Lerp(startAlpha,endAlpha,timer / duration);
            yield return null;
        }
        _globalFadeCanvasGroup.alpha = endAlpha;
        // 修正: ヌル合意演算子から明示的なヌルチェックに変更
        onComplete?.Invoke();
    }
    private void StartFadeIn() {
        if(_fadeCoroutine != null) {
            StopCoroutine(_fadeCoroutine);
        }

        _globalFadeCanvasInstance.SetActive(true);
        _globalFadePanelImage.gameObject.SetActive(true);
        if(_globalFadeCanvasGroup != null) {
            _globalFadeCanvasGroup.blocksRaycasts = true;
            _globalFadeCanvasGroup.interactable = true;
        }
        _fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f,0f,FadeDuration,() => {
            if(_globalFadeCanvasGroup != null) {
                _globalFadeCanvasGroup.alpha = 0f;
                _globalFadeCanvasGroup.blocksRaycasts = false;
                _globalFadeCanvasGroup.interactable = false;
            }
        }));
    }
    private void CheckUIReferences() {
        if(_permanentUICanvas == null) {
            Debug.LogError("GameManager: Permanent UICanvasが割り当てられていません。",this);
        }

        if(_globalFadePanelImage == null) {
            Debug.LogError("GameManager: Global Fade Panel Imageが割り当てられていません。",this);
        }

        if(_globalFadeCanvasGroup == null) {
            Debug.LogError("GameManager: Global Fade Canvas Groupが割り当てられていません。",this);
        }

        if(_permanentEventSystem == null) {
            Debug.LogError("GameManager: Permanent Event Systemが割り当てられていません。",this);
        }

        if(_scoreDisplay == null) {
            Debug.LogError("GameManager: Score Displayが割り当てられていません。",this);
        }

        if(_scorePanel == null) {
            Debug.LogError("GameManager: Score Panelが割り当てられていません。",this);
        }

        if(_timeLimitText == null) {
            Debug.LogWarning("GameManager: Time Limit Textが割り当てられていません。時間制限UIは表示されません。",this);
        }

        if(_mobileControlCanvas == null) {
            Debug.LogError("GameManager: Mobile Control Canvasが割り当てられていません。",this);
        }
    }
    private void InitializeAudioSource() {
        _audioSource = GetComponent<AudioSource>();
        if(_audioSource == null) {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _audioSource.loop = true;
        _audioSource.volume = _initialBGMVolume;
    }
    private void InitializeBGMMap() {
        foreach(SceneBGMData data in _sceneBGMList) {
            if(!_sceneBGMMap.ContainsKey(data.sceneName)) {
                _sceneBGMMap.Add(data.sceneName,data.bgmClip);
            }
        }
    }
    private void InitializeFadeCanvas() {
        if(_globalFadeCanvasGroup != null) {
            _globalFadeCanvasInstance = _globalFadeCanvasGroup.gameObject;
            _globalFadeCanvasGroup.alpha = 0f;
            _globalFadeCanvasGroup.blocksRaycasts = false;
            _globalFadeCanvasGroup.interactable = false;
        }
    }
    private void SaveCurrentStageIndex() {
        string currentSceneName = SceneManager.GetActiveScene().name;
        for(int i = 0;i < StageSceneNames.Length;i++) {
            if(StageSceneNames[i] == currentSceneName) {
                CurrentStageIndex = i;
                break;
            }
        }
    }
}
