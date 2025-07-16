using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
// using System.Net.Mime; // ★この行があれば削除する！★
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ★ここがUnityEngine.UIであることを確認★
using static System.Net.Mime.MediaTypeNames;

public class GameManager : MonoBehaviour
{
    // シーン名の定数
    public const string TitleSceneName = "TitleScene";
    public const string StageSelectSceneName = "StageSelect";
    public const string UISceneName = "UI";
    public const string MainGameSceneName = "Demo_tileset2"; // あなたのゲームプレイシーン名に合わせてください
    public const string GameOverSceneName = "GameOverScene";
    public const string GameClearSceneName = "GameClearScene"; // ゲームクリアシーン名を追加（もしあれば）

    // シングルトンインスタンス
    public static GameManager instance { get; private set; }

    // ゲームの状態
    public enum GameState
    {
        enGameState_Init,
        enGameState_Title,
        enGameState_StageSelect,
        enGameState_Play,
        enGameState_Pause,
        enGameState_GameOver,
        enGameState_GameClear, // ゲームクリア状態
        enGameState_Clear // 互換性のため（enGameState_GameClearと同じ意味合いで使われている可能性）
    }

    private GameState m_currentGameState = GameState.enGameState_Init;

    // --- フェード関連 ---
    [Header("フェード設定")]
    [SerializeField] private GameObject permanentCanvasPrefab;
    [SerializeField] private GameObject globalFadeCanvasPrefab;
    [SerializeField] private GameObject globalFadePanelPrefab;
    [SerializeField] private GameObject permanentEventSystemPrefab;
    [SerializeField] private Sprite fadePanelSprite;

    private GameObject m_permanentCanvasInstance;
    private GameObject m_globalFadeCanvasInstance;
    private Image m_globalFadePanelImage; // ★これがUnityEngine.UI.Imageを参照していることを確認★
    private GameObject m_permanentEventSystemInstance;

    private Coroutine m_fadeCoroutine;
    private bool m_isTransitioning = false;

    // --- UI関連 ---
    [Header("UI要素 (ランタイムで設定)")]
    public ScoreDisplay scoreDisplay; // Inspectorで設定する、またはFindObjectOfTypeで探す
    public GameObject scorePanel; // ScoreDisplayを親にするパネル

    [Header("ゲームプレイ設定")]
    [SerializeField] private int initialGemCount = 0;
    public int currentGemCount { get; private set; }

    // --- ステージ選択関連 ---
    [Header("ステージ選択設定")]
    // StageSelectManagerから参照される配列とインデックス
    public string[] stageSceneNames = { "Stage1", "Stage2", "Stage3" }; // あなたの実際のステージ名に合わせる
    public int currentStageIndex = 0;


    // --- Awake ---
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: Awake - DontDestroyOnLoadを設定しました。");

            SetupPermanentUIElements();
            InitializeGame();

            if (SceneManager.GetActiveScene().name == "Bootstrap")
            {
                Debug.Log("GameManager: Bootstrapから起動しました。TitleSceneへの遷移を開始します。");
                LoadSceneWithFade(TitleSceneName);
            }
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("GameManager: 既存のインスタンスがあるため、このGameManagerを破棄しました。");
        }
    }

    // --- OnEnable / OnDisable ---
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("GameManager: SceneManager.sceneLoaded イベントを登録しました。");
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("GameManager: SceneManager.sceneLoaded イベントを解除しました。");
    }

    // --- ゲーム初期化 ---
    private void InitializeGame()
    {
        SetState(GameState.enGameState_Init);
        currentGemCount = initialGemCount;
    }

    // --- シーンロード時のコールバック ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager: シーン'{scene.name}'がロードされました。モード: {mode}");

        if (scene.name == MainGameSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log($"GameManager: ゲームプレイシーン'{scene.name}'がロードされました。");
            if (!SceneManager.GetSceneByName(UISceneName).isLoaded)
            {
                Debug.Log("GameManager: UIシーンがまだロードされていないため、Additiveでロードします。");
                SceneManager.LoadScene(UISceneName, LoadSceneMode.Additive);
            }
            SetState(GameState.enGameState_Play);
            InitializeGemCount();

            // メインゲームシーンがロードされたらUIを表示
            SetScoreUIActive(true);
        }
        else if (scene.name == UISceneName && mode == LoadSceneMode.Additive)
        {
            Debug.Log("GameManager: UIシーンがロードされました。PermanentCanvasをアクティブにします。");
            if (m_permanentCanvasInstance != null)
            {
                m_permanentCanvasInstance.SetActive(true);
            }

            // ScoreDisplayのインスタンスを取得して設定
            scoreDisplay = FindObjectOfType<ScoreDisplay>();
            if (scoreDisplay != null)
            {
                Debug.Log("GameManager: ScoreDisplayが見つかりました。");
                scoreDisplay.transform.SetParent(m_permanentCanvasInstance.transform, false);
                Debug.Log("GameManager: ScoreDisplayの親子関係を設定します。");
            }
            else
            {
                Debug.LogWarning("GameManager: ScoreDisplayが見つかりません。UI表示に問題がある可能性があります。");
            }

            // ScorePanelのインスタンスを取得して設定
            scorePanel = GameObject.Find("ScorePanel");
            if (scorePanel != null)
            {
                scorePanel.transform.SetParent(m_permanentCanvasInstance.transform, false);
                Debug.Log("GameManager: ScorePanelの親子関係を設定します。");
            }
            else
            {
                Debug.LogWarning("GameManager: ScorePanelが見つかりません。UI表示に問題がある可能性があります。");
            }

            // UIシーンがロードされた時点では、スコアUIは非表示にしておく
            SetScoreUIActive(false);
        }
        else if (scene.name == TitleSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log("GameManager: TitleSceneがロードされました。");
            SetState(GameState.enGameState_Title);
            StartFadeIn(true);
            // タイトルシーンではUIを非表示
            SetScoreUIActive(false);
        }
        else if (scene.name == StageSelectSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log("GameManager: StageSelectがロードされました。");
            SetState(GameState.enGameState_StageSelect);
            StartFadeIn(false);
            // ステージ選択シーンではUIを非表示
            SetScoreUIActive(false);
        }
        else if (scene.name == GameOverSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log("GameManager: GameOverScene がロードされました。");
            SetState(GameState.enGameState_GameOver);
            StartFadeIn(false);
            // ゲームオーバーシーンではUIを非表示
            SetScoreUIActive(false);
        }
        else if (scene.name == GameClearSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log("GameManager: GameClearScene がロードされました。");
            SetState(GameState.enGameState_GameClear);
            StartFadeIn(false);
            // ゲームクリアシーンではUIを非表示
            SetScoreUIActive(false);
        }
        else
        {
            if (scene.name != "Bootstrap")
            {
                StartFadeIn(false);
            }
            else
            {
                Debug.Log($"GameManager: シーン'{scene.name}'ではフェードインは開始されません。");
            }
            // その他のシーンではUIを非表示 (必要に応じて調整)
            SetScoreUIActive(false);
        }
    }

    // ゲーム状態の変更
    public void SetState(GameState newState)
    {
        if (m_currentGameState == newState) return;

        Debug.Log($"GameManager: ゲームの状態を {newState} に変更しました。");
        m_currentGameState = newState;

        // ゲームの状態に応じたタイムスケールの設定
        switch (m_currentGameState)
        {
            case GameState.enGameState_Play:
                Time.timeScale = 1f;
                break;
            case GameState.enGameState_Pause:
                Time.timeScale = 0f;
                break;
            case GameState.enGameState_GameOver:
                Time.timeScale = 0f;
                break;
            case GameState.enGameState_GameClear:
            case GameState.enGameState_Clear: // 既存のコード互換性のため
                Time.timeScale = 0f;
                break;
            default:
                Time.timeScale = 1f;
                break;
        }
    }

    // 現在のゲーム状態を取得するための public メソッド
    public GameState GetCurrentGameState()
    {
        return m_currentGameState;
    }

    // ゲームオーバー状態への遷移を即座に開始
    public void SetGameOverStateImmediately()
    {
        if (m_isTransitioning)
        {
            Debug.Log("GameManager: 既にシーン遷移中のため、SetGameOverStateImmediatelyをスキップします。");
            return;
        }

        Debug.Log("GameManager: ゲームオーバー状態に設定し、GameOverSceneへの遷移を開始しました。");
        SetState(GameState.enGameState_GameOver);
        LoadSceneWithFade(GameOverSceneName);
    }

    // シーン遷移
    public void LoadSceneWithFade(string sceneName)
    {
        if (m_isTransitioning)
        {
            Debug.Log($"GameManager: 既にシーン遷移中のため、'{sceneName}'へのロードをスキップします。");
            return;
        }

        m_isTransitioning = true;

        Debug.Log($"GameManager: シーン'{sceneName}'へのフェードアウトとロードを開始します。");

        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
            Debug.Log("GameManager: 既存のフェードコルーチンを停止しました (LoadSceneWithFade)。");
        }
        m_fadeCoroutine = StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    // ジェム関連
    public void InitializeGemCount()
    {
        currentGemCount = 0;
        Debug.Log($"GameManager: ジェム数を初期化しました。総ジェム数: {currentGemCount}");
        if (scoreDisplay != null)
        {
            scoreDisplay.UpdateGemCount(currentGemCount);
        }
    }

    // ジェムの収集とスコア加算を統合したメソッド
    public void AddGem(int amount)
    {
        currentGemCount += amount;
        Debug.Log($"GameManager: ジェムを拾いました。現在: {currentGemCount}");
        if (scoreDisplay != null)
        {
            scoreDisplay.UpdateGemCount(currentGemCount);
        }
    }

    // PermanentなUI要素のセットアップ
    private void SetupPermanentUIElements()
    {
        Debug.Log("GameManager: SetupPermanentUIElementsを開始します。");

        // PermanentCanvasの生成と設定
        if (permanentCanvasPrefab != null && m_permanentCanvasInstance == null)
        {
            m_permanentCanvasInstance = Instantiate(permanentCanvasPrefab);
            m_permanentCanvasInstance.name = "PermanentCanvas_DontDestroy";
            DontDestroyOnLoad(m_permanentCanvasInstance);
            m_permanentCanvasInstance.SetActive(false); // 初期は非アクティブ
            Debug.Log("GameManager: PermanentCanvas_DontDestroyを生成しました。");
        }
        else if (permanentCanvasPrefab == null)
        {
            Debug.LogWarning("GameManager: permanentCanvasPrefabが割り当てられていません。");
        }

        // GlobalFadeCanvasの生成と設定
        if (globalFadeCanvasPrefab != null && m_globalFadeCanvasInstance == null)
        {
            m_globalFadeCanvasInstance = Instantiate(globalFadeCanvasPrefab);
            m_globalFadeCanvasInstance.name = "GlobalFadeCanvas_DontDestroy";
            DontDestroyOnLoad(m_globalFadeCanvasInstance);
            m_globalFadeCanvasInstance.SetActive(false); // 初期は非アクティブ
            Debug.Log("GameManager: GlobalFadeCanvas_DontDestroyを生成しました。");
        }
        else if (globalFadeCanvasPrefab == null)
        {
            Debug.LogWarning("GameManager: globalFadeCanvasPrefabが割り当てられていません。");
        }

        // PermanentEventSystemの生成と設定
        if (permanentEventSystemPrefab != null && m_permanentEventSystemInstance == null)
        {
            m_permanentEventSystemInstance = Instantiate(permanentEventSystemPrefab);
            m_permanentEventSystemInstance.name = "PermanentEventSystem_DontDestroy";
            DontDestroyOnLoad(m_permanentEventSystemInstance);
            Debug.Log("GameManager: PermanentEventSystem_DontDestroyを生成しました。");
        }
        else if (permanentEventSystemPrefab == null)
        {
            Debug.LogWarning("GameManager: permanentEventSystemPrefabが割り当てられていません。");
        }

        // GlobalFadePanelの生成と設定
        if (globalFadePanelPrefab != null && m_globalFadeCanvasInstance != null)
        {
            GameObject fadePanelObject = Instantiate(globalFadePanelPrefab, m_globalFadeCanvasInstance.transform);
            fadePanelObject.name = "GlobalFadePanel";
            m_globalFadePanelImage = fadePanelObject.GetComponent<Image>();
            if (m_globalFadePanelImage == null)
            {
                Debug.LogError("GameManager: GlobalFadePanelPrefabにImageコンポーネントがありません。");
            }

            if (fadePanelSprite != null)
            {
                m_globalFadePanelImage.sprite = fadePanelSprite;
                Debug.Log("GameManager: FadePanelSpriteを割り当てました。");
            }
            else
            {
                Debug.LogWarning("GameManager: FadePanelSpriteが割り当てられていません。");
            }

            m_globalFadePanelImage.color = new Color(0, 0, 0, 0); // 初期は完全に透明
            m_globalFadePanelImage.gameObject.SetActive(false); // 初期は非アクティブ
            Debug.Log("GameManager: GlobalFadePanelを生成しました。初期状態は透明で非アクティブです。");
        }
        else if (globalFadePanelPrefab == null)
        {
            Debug.LogWarning("GameManager: globalFadePanelPrefabが割り当てられていません。");
        }

        Debug.Log("GameManager: SetupPermanentUIElementsを完了しました。");
    }

    // フェードイン処理
    private void StartFadeIn(bool isInitialLoad)
    {
        if (m_globalFadePanelImage == null)
        {
            Debug.LogError("GameManager: フェードパネルのImageがnullです。フェードインできません。");
            m_isTransitioning = false;
            return;
        }

        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
            Debug.Log("GameManager: 既存のフェードコルーチンを停止しました (OnSceneLoaded / 通常)。");
        }

        Debug.Log($"GameManager: {(isInitialLoad ? "初回" : "通常")}フェードインを開始します (シーン: {SceneManager.GetActiveScene().name})。");
        m_fadeCoroutine = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn(float duration = 2.0f)
    {
        Debug.Log($"GameManager: FadeInを開始します。時間: {duration}秒");
        m_globalFadeCanvasInstance.SetActive(true);
        m_globalFadePanelImage.gameObject.SetActive(true);

        m_globalFadePanelImage.color = new Color(0, 0, 0, 1); // 最初は不透明
        Debug.Log("GameManager: フェードイン開始 (不透明 -> 透明)");

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / duration); // 1から0へ補間
            m_globalFadePanelImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        m_globalFadePanelImage.color = new Color(0, 0, 0, 0); // 完全に透明に
        m_globalFadePanelImage.gameObject.SetActive(false);
        m_globalFadeCanvasInstance.SetActive(false);
        m_isTransitioning = false;
        Debug.Log("GameManager: FadeIn: 完了しました。パネルと専用Canvasを非アクティブにしました。");
    }


    // フェードアウトとシーンロード処理
    private IEnumerator FadeOutAndLoadScene(string sceneName, float duration = 1.0f)
    {
        Debug.Log($"GameManager: FadeOutAndLoadSceneを開始します。対象シーン: {sceneName}");
        m_globalFadeCanvasInstance.SetActive(true);
        m_globalFadePanelImage.gameObject.SetActive(true);

        m_globalFadePanelImage.color = new Color(0, 0, 0, 0); // 最初は透明
        Debug.Log("GameManager: フェードアウト開始 (透明 -> 不透明)");

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / duration); // 0から1へ補間
            m_globalFadePanelImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        m_globalFadePanelImage.color = new Color(0, 0, 0, 1); // 完全に不透明に
        Debug.Log($"GameManager: シーンロード開始: {sceneName}");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // シーンのアクティブ化を待つ

        while (!asyncLoad.isDone)
        {
            // 0.9fに達したら、シーンのアクティブ化を許可
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
                Debug.Log($"GameManager: シーン '{sceneName}' のアクティブ化を許可しました。");
            }
            yield return null;
        }
    }

    // スコアUIの表示/非表示を制御する新しいメソッド
    private void SetScoreUIActive(bool isActive)
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(isActive);
            Debug.Log($"GameManager: ScorePanelの表示を {(isActive ? "有効" : "無効")} にしました。");
        }
        else
        {
            Debug.LogWarning("GameManager: SetScoreUIActive - scorePanelがnullです。");
        }
    }

    // ゲームを終了する
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("GameManager: ゲームを終了します。");
    }
}