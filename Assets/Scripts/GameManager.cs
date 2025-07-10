using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshProを使うため追加
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

public class GameManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static GameManager instance;

    // ゲームの状態を定義
    public enum GameState
    {
        enGameState_Play,       // プレイ中
        enGameState_GameOver,   // ゲームオーバー
        enGameState_Clear,      // ゲームクリア
    }

    [SerializeField, Header("ゲームの状態")]
    private GameState m_gameState = GameState.enGameState_Play;

    // 現在のゲームプレイのスコア
    [SerializeField, Header("現在のスコア")]
    private int currentPlayScore = 0;

    // スコア表示用UIへの参照 (ScoreDisplayはUIシーンに存在し、シーンロード時に再取得する)
    private ScoreDisplay m_uiScore;

    // シーン遷移時のフェードパネルとフェード時間
    [SerializeField, Header("フェードパネル")]
    private UnityEngine.UI.Image m_fadePanel; // UnityEngine.UI.Image を明示的に指定
    [SerializeField, Header("フェード時間")]
    private float m_fadeDuration = 1.0f; // ★デフォルト値が適切か確認 (1.0fが妥当)★

    // フェードパネルに割り当てるスプライトをInspectorから設定できるようにする
    [SerializeField, Header("フェードパネル用スプライト (必須)")]
    private Sprite m_fadePanelSprite;

    // ゲームオーバー音 (BGMではなく効果音として扱う)
    // [SerializeField, Header("ゲームオーバー音")] // このフィールドは削除済み
    // private AudioClip gameOverSoundClip;
    private AudioSource gameManagerAudioSource; // 効果音再生用

    // 現在実行中のフェードコルーチンへの参照
    private Coroutine currentFadeCoroutine = null;

    // GameManagerが生成される際に一度だけ呼び出される
    void Awake()
    {
        Debug.Log("GameManager Awakeが呼び出されました！");

        if (instance == null)
        {
            instance = this;
            // このGameManagerオブジェクトをシーン遷移しても破棄しない
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManagerインスタンスが設定され、DontDestroyOnLoadに設定されました。");

            // フェードパネルとEventSystemを自動的にセットアップし、永続化する
            SetupPermanentUIElements();

            // AudioSourceコンポーネントを取得または追加 (他の効果音用として残す)
            gameManagerAudioSource = GetComponent<AudioSource>();
            if (gameManagerAudioSource == null)
            {
                gameManagerAudioSource = gameObject.AddComponent<AudioSource>();
                gameManagerAudioSource.playOnAwake = false; // 自動再生しない
                gameManagerAudioSource.loop = false; // 効果音なのでループしない
                Debug.Log("GameManagerにAudioSourceを追加しました。(効果音用)");
            }

            // AudioListenerコンポーネントを取得または追加
            // シーン内にAudioListenerが一つだけ存在するようにGameManagerが管理する
            AudioListener existingListener = FindObjectOfType<AudioListener>();
            if (existingListener == null)
            {
                // シーンにAudioListenerが一つもなければ、GameManagerにアタッチ
                gameObject.AddComponent<AudioListener>();
                Debug.Log("GameManagerにAudioListenerを追加しました。");
            }
            else if (existingListener.gameObject != gameObject)
            {
                // 既に別のオブジェクトにAudioListenerがある場合、それを無効化または破棄
                Debug.LogWarning($"既存のAudioListenerが '{existingListener.gameObject.name}' に見つかりました。GameManagerがAudioListenerを持つようにします。");
                // existingListener.enabled = false; // 無効化する
                // Destroy(existingListener); // あるいは破棄する（DontDestroyOnLoadオブジェクトの場合は注意）
                gameObject.AddComponent<AudioListener>(); // 重複するが、他のものを無効化/破棄する前提
            }

            // UIシーンを現在のシーンに追加ロードする (Additive)
            if (!SceneManager.GetSceneByName("UI").isLoaded)
            {
                SceneManager.LoadScene("UI", LoadSceneMode.Additive);
                Debug.Log("GameManager Awake: UIシーンを合成ロードしました。");
            }
            else
            {
                Debug.Log("GameManager Awake: UIシーンは既にロードされています。");
            }

            // TitleSceneをSingleモードでロードし、既存のシーンをアンロードする
            if (!SceneManager.GetSceneByName("TitleScene").isLoaded)
            {
                SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
                Debug.Log("GameManager Awake: TitleSceneを単独ロードしました。(Single Mode)");
            }
            else
            {
                Debug.Log("GameManager Awake: TitleSceneは既にロードされています。");
            }

        }
        else
        {
            // 既にインスタンスがある場合は、この重複したオブジェクトを破棄
            Debug.Log("重複したGameManagerインスタンスを破棄します。");
            Destroy(gameObject);
            return; // これ以上処理しない
        }
    }

    // シーンがロードされた際に呼び出されるイベントを登録/解除
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // UnityのStartメソッド。スクリプトが有効になった最初のフレームで呼び出される
    void Start()
    {
        // StartCoroutine(FadeIn(m_fadeDuration)); // フェードインはOnSceneLoadedで制御
    }

    // シーンがロードされるたびに呼び出されるコールバックメソッド
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager OnSceneLoaded: シーン '{scene.name}' がロードされました。モード: {mode}");

        // UIシーンがロードされた場合の初期化 (ScoreDisplayの参照取得と親子関係の設定)
        if (scene.name == "UI")
        {
            m_uiScore = GameObject.FindGameObjectWithTag("ScoreText")?.GetComponent<ScoreDisplay>();

            if (m_uiScore != null)
            {
                m_uiScore.ScoreUpdate(currentPlayScore);
                Debug.Log($"GameManager OnSceneLoaded: UIシーンロード時、UIスコアを {currentPlayScore} に初期更新しました。");

                Canvas permanentCanvas = GameObject.Find("PermanentCanvas_DontDestroy")?.GetComponent<Canvas>();

                if (permanentCanvas != null)
                {
                    if (m_uiScore.transform.parent != permanentCanvas.transform)
                    {
                        m_uiScore.transform.SetParent(permanentCanvas.transform, false);
                        m_uiScore.transform.SetAsLastSibling();

                        RectTransform rectT = m_uiScore.GetComponent<RectTransform>();
                        TMP_Text tmpText = m_uiScore.GetComponent<TMP_Text>();

                        rectT.anchorMin = new Vector2(0f, 1f);
                        rectT.anchorMax = new Vector2(0f, 1f);
                        rectT.pivot = new Vector2(0f, 1f);
                        rectT.anchoredPosition = new Vector2(20f, -20f);
                        rectT.sizeDelta = new Vector2(250f, 60f);
                        rectT.localScale = Vector3.one;

                        if (tmpText != null)
                        {
                            tmpText.fontSize = 60;
                            tmpText.alignment = TextAlignmentOptions.Left;
                        }
                        Debug.Log("ScoreTextを永続Canvasの子に設定し、RectTransformと文字サイズを調整しました。");
                    }

                    GameObject scorePanel = GameObject.Find("Image")?.gameObject;
                    if (scorePanel != null && scorePanel.transform.parent != permanentCanvas.transform)
                    {
                        scorePanel.transform.SetParent(permanentCanvas.transform, false);
                        scorePanel.transform.SetAsFirstSibling();

                        RectTransform panelRectT = scorePanel.GetComponent<RectTransform>();
                        panelRectT.anchorMin = new Vector2(0f, 1f);
                        panelRectT.anchorMax = new Vector2(0f, 1f);
                        panelRectT.pivot = new Vector2(0f, 1f);
                        panelRectT.anchoredPosition = new Vector2(15f, -15f);
                        panelRectT.sizeDelta = new Vector2(270f, 70f);
                        panelRectT.localScale = Vector3.one;

                        Debug.Log("Score Panelを永続Canvasの子に設定し、RectTransformを調整しました。");
                    }
                }
                else
                {
                    Debug.LogError("PermanentCanvas_DontDestroyが見つかりません！UIが正しく表示されない可能性があります。GameManagerのSetupPermanentUIElements()を確認してください。");
                }
            }
            else
            {
                Debug.LogWarning("ScoreDisplay (with tag ScoreText) not found in UI scene! UI表示ができません。");
            }
        }
        // ゲームプレイシーン（"Demo_tileset"）がロードされた場合の初期化
        else if (scene.name == "Demo_tileset")
        {
            SetState(GameState.enGameState_Play); // ゲームの状態をプレイ中に設定
            currentPlayScore = 0; // 新しいゲーム開始なのでスコアをリセット
            Debug.Log($"GameManager OnSceneLoaded: {scene.name} の現在のスコアを {currentPlayScore} にリセットしました。");
            currentFadeCoroutine = StartCoroutine(FadeIn(m_fadeDuration));

            // タイトルシーンのUI要素を非表示にする（元のシーンにあるもの）
            GameObject titleLogo = GameObject.Find("Image"); // タイトルロゴのGameObject名
            if (titleLogo != null)
            {
                titleLogo.SetActive(false);
                Debug.Log("GameManager OnSceneLoaded: ゲームシーンロード時にタイトルロゴを非表示にしました。");
            }
            GameObject startButton = GameObject.Find("Button"); // スタートボタンのGameObject名
            if (startButton != null)
            {
                startButton.SetActive(false);
                Debug.Log("GameManager OnSceneLoaded: ゲームシーンロード時にスタートボタンを非表示にしました。");
            }

            // スコアUIをリセットされたスコアで更新し、表示する
            if (m_uiScore != null)
            {
                m_uiScore.ScoreUpdate(currentPlayScore);
                m_uiScore.ShowScore(); // スコアテキストと背景を表示
                Debug.Log($"GameManager OnSceneLoaded: {scene.name} ロード後、UIスコアを {currentPlayScore} に更新し、表示しました。");
            }
            else
            {
                Debug.LogWarning("GameManager OnSceneLoaded: m_uiScoreがnullのため、スコアUIを更新・表示できませんでした。");
            }
        }
        // タイトルシーンがロードされた場合の初期化
        else if (scene.name == "TitleScene")
        {
            currentPlayScore = 0;
            SetState(GameState.enGameState_Play);
            Debug.Log($"GameManager OnSceneLoaded: {scene.name} の現在のスコアを {currentPlayScore} にリセットしました。");
            currentFadeCoroutine = StartCoroutine(FadeIn(m_fadeDuration));

            if (m_uiScore != null)
            {
                m_uiScore.ScoreUpdate(currentPlayScore);
                m_uiScore.HideScore();
                Debug.Log($"GameManager OnSceneLoaded: {scene.name} ロード後、UIスコアを {currentPlayScore} に更新し、非表示にしました。");
            }
            else
            {
                Debug.LogWarning("GameManager OnSceneLoaded: m_uiScoreがnullのため、スコアUIを更新・非表示できませんでした。");
            }

            GameObject titleLogo = GameObject.Find("Image");
            if (titleLogo != null)
            {
                titleLogo.SetActive(true);
                Debug.Log("GameManager OnSceneLoaded: タイトルロゴをアクティブにしました。");
            }
            else
            {
                Debug.LogWarning("タイトルロゴ (Image) がTitleSceneに見つかりません。名前を確認してください。");
            }

            GameObject startButton = GameObject.Find("Button");
            if (startButton != null)
            {
                startButton.SetActive(true);
                Debug.Log("GameManager OnSceneLoaded: スタートボタンをアクティブにしました。");
            }
            else
            {
                Debug.LogWarning("スタートボタン (Button) がTitleSceneに見つかりません。名前を確認してください。");
            }
        }
        // ゲームオーバーシーンまたはゲームクリアシーンがロードされた場合
        else if (scene.name == "GameOver" || scene.name == "GameOverScene" || scene.name == "GameClear" || scene.name == "ClearScene")
        {
            SetState(GameState.enGameState_GameOver); // ゲームオーバー/クリア状態に設定
            Debug.Log($"GameManager OnSceneLoaded: {scene.name} ロード完了。フェードイン開始。");
            currentFadeCoroutine = StartCoroutine(FadeIn(m_fadeDuration)); // フェードインを開始

            // スコアUIは非表示のまま維持するか、必要に応じて表示制御
            if (m_uiScore != null)
            {
                m_uiScore.HideScore(); // ゲームオーバー/クリア画面ではスコアを非表示
                Debug.Log($"GameManager OnSceneLoaded: {scene.name} ロード後、UIスコアを非表示にしました。");
            }
            else
            {
                Debug.LogWarning("GameManager OnSceneLoaded: m_uiScoreがnullのため、スコアUIを非表示できませんでした。");
            }

            // タイトルシーンのUI要素を非表示にする（元のシーンにあるもの）
            GameObject titleLogo = GameObject.Find("Image");
            if (titleLogo != null)
            {
                titleLogo.SetActive(false);
                Debug.Log("GameManager OnSceneLoaded: ゲームオーバー/クリアシーンロード時にタイトルロゴを非表示にしました。");
            }
            GameObject startButton = GameObject.Find("Button");
            if (startButton != null)
            {
                startButton.SetActive(false);
                Debug.Log("GameManager OnSceneLoaded: ゲームオーバー/クリアシーンロード時にスタートボタンを非表示にしました。");
            }
        }
    }

    // 毎フレーム呼び出される (必要に応じて処理を追加)
    void Update()
    {
        // ...
    }

    // ゲームの状態を設定する
    public void SetState(GameState state)
    {
        m_gameState = state;
        // ゲームクリア時にも現在のスコアをハイスコアと比較して保存
        if (m_gameState == GameState.enGameState_Clear) // else if から if に変更 (ゲームオーバー音削除のため)
        {
            Debug.Log($"GameManager SetState: ゲームクリア。");
        }
    }

    // ゲームの状態を取得する
    public GameState GetState()
    {
        return m_gameState;
    }

    // スコアを加算し、UIを更新する
    public void AddScore(int score)
    {
        currentPlayScore += score;
        if (m_uiScore != null)
        {
            m_uiScore.ScoreUpdate(currentPlayScore);
            Debug.Log($"GameManager AddScore: 現在のスコアを {currentPlayScore} に更新しました。");
        }
        else
        {
            Debug.LogWarning("ScoreDisplay (m_uiScore) is null when trying to update score.");
        }
    }

    // 現在のゲームプレイのスコアを取得する
    public int GetCurrentPlayScore()
    {
        return currentPlayScore;
    }

    // 指定したシーンにフェードアウトしながら遷移する公共メソッド
    public void LoadSceneWithFade(string sceneName)
    {
        Debug.Log($"GameManager LoadSceneWithFade: シーン '{sceneName}' への遷移を開始します。");
        // 既存のフェードコルーチンがあれば停止する
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            Debug.Log("LoadSceneWithFade: 既存のフェードコルーチンを停止しました。");
        }
        currentFadeCoroutine = StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    // --- プライベートヘルパーメソッド（UI初期化とフェード処理） ---

    // フェードパネルとEventSystemを生成・設定し、DontDestroyOnLoadに設定する
    private void SetupPermanentUIElements()
    {
        // 永続的なCanvasを見つけるか、新しく作成する
        Canvas permanentCanvas = GameObject.Find("PermanentCanvas_DontDestroy")?.GetComponent<Canvas>();

        if (permanentCanvas == null)
        {
            GameObject canvasGO = new GameObject("PermanentCanvas_DontDestroy");
            permanentCanvas = canvasGO.AddComponent<Canvas>();
            permanentCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // 画面全体にオーバーレイ表示

            // CanvasScalerの設定を強化し、画面サイズに合わせてスケーリングする
            CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080); // 基準となる解像度 (例: フルHD)
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // 幅と高さの中間でスケーリング

            canvasGO.AddComponent<GraphicRaycaster>();
            permanentCanvas.sortingOrder = 100; // 最前面に表示されるように高いソート順を設定
            DontDestroyOnLoad(canvasGO); // Canvas自体を永続化
            Debug.Log("PermanentCanvas_DontDestroyを作成しました。親: " + canvasGO.transform.parent + ", DDOL設定済み.");
        }
        else
        {
            Debug.Log("PermanentCanvas_DontDestroyは既に存在します。");
            // 既存のCanvasScalerの設定を再確認または適用
            CanvasScaler canvasScaler = permanentCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = permanentCanvas.gameObject.AddComponent<CanvasScaler>();
            }
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            Debug.Log("PermanentCanvas_DontDestroyのCanvasScaler設定を調整しました。");
        }

        // 永続的なEventSystemを見つけるか、新しく作成する
        EventSystem permanentEventSystem = GameObject.Find("PermanentEventSystem_DontDestroy")?.GetComponent<EventSystem>();
        if (permanentEventSystem == null)
        {
            // シーン内に存在する他のEventSystemを全て削除してから、新しく作成する
            EventSystem[] existingEventSystems = FindObjectsOfType<EventSystem>();
            foreach (EventSystem es in existingEventSystems)
            {
                if (es.gameObject.name != "PermanentEventSystem_DontDestroy")
                {
                    Debug.Log($"既存のEventSystem '{es.gameObject.name}' を破棄します。");
                    Destroy(es.gameObject);
                }
            }

            GameObject eventSystemGO = new GameObject("PermanentEventSystem_DontDestroy");
            permanentEventSystem = eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(eventSystemGO);
            Debug.Log("PermanentEventSystem_DontDestroyを作成しました。DDOL設定済み.");
        }
        else
        {
            Debug.Log("PermanentEventSystem_DontDestroyは既に存在します。");
            EventSystem[] existingEventSystems = FindObjectsOfType<EventSystem>();
            if (existingEventSystems.Length > 1)
            {
                foreach (EventSystem es in existingEventSystems)
                {
                    if (es.gameObject.name != "PermanentEventSystem_DontDestroy" && es != permanentEventSystem)
                    {
                        Debug.LogWarning($"重複するEventSystem '{es.gameObject.name}' が検出されました。破棄します。");
                        Destroy(es.gameObject);
                    }
                }
            }
        }

        // フェードパネル（Image）を作成または既存のものを利用する
        if (m_fadePanel == null)
        {
            if (permanentCanvas == null)
            {
                Debug.LogError("フェードパネル作成中にPermanentCanvas_DontDestroyが見つかりません。これは致命的なエラーです。");
                return;
            }

            GameObject fadePanelGO = new GameObject("GlobalFadePanel");
            fadePanelGO.transform.SetParent(permanentCanvas.transform); // PermanentCanvasの子にする
            m_fadePanel = fadePanelGO.AddComponent<UnityEngine.UI.Image>(); // UnityEngine.UI.Image を明示的に指定

            RectTransform rectT = m_fadePanel.GetComponent<RectTransform>();
            rectT.anchorMin = Vector2.zero;
            rectT.anchorMax = Vector2.one;
            rectT.sizeDelta = Vector2.zero;
            rectT.anchoredPosition = Vector2.zero;

            // Inspectorで割り当てられたスプライトを使用する
            if (m_fadePanelSprite != null)
            {
                m_fadePanel.sprite = m_fadePanelSprite;
                Debug.Log("GameManager: フェードパネルにInspectorから割り当てられたスプライトを設定しました。");
            }
            else
            {
                // ここは警告ログに留める。エラーではない
                Debug.LogWarning("GameManager: フェードパネル用スプライト (m_fadePanelSprite) がInspectorで割り当てられていません。フェードパネルが正しく表示されない可能性があります。");
            }

            m_fadePanel.color = new Color(0, 0, 0, 0); // 初期は透明
            m_fadePanel.transform.SetAsLastSibling(); // 最も手前（最上層）に描画されるようにする
            m_fadePanel.raycastTarget = false; // クリックを透過させる
            Debug.Log("GlobalFadePanelを作成しました。親: " + fadePanelGO.transform.parent.name + ", アクティブ状態: " + fadePanelGO.activeSelf);
        }
        else
        {
            Debug.Log("GlobalFadePanelは既に存在します。");
            // 既存のフェードパネルのRectTransformが画面全体を覆うように再設定
            RectTransform rectT = m_fadePanel.GetComponent<RectTransform>();
            if (rectT != null)
            {
                // AnchorMinとAnchorMaxをVector2.zeroとVector2.oneに設定し、画面全体を覆うようにする
                rectT.anchorMin = Vector2.zero;
                rectT.anchorMax = Vector2.one;
                rectT.sizeDelta = Vector2.zero; // sizeDeltaをゼロに設定
                rectT.anchoredPosition = Vector2.zero; // anchoredPositionをゼロに設定
                m_fadePanel.transform.SetAsLastSibling(); // 最も手前（最上層）に描画されるようにする
                Debug.Log("GlobalFadePanelのRectTransformを画面全体に再設定しました。");
            }
            // 既存のフェードパネルにスプライトが割り当てられていない場合、Inspectorから割り当てられたスプライトを設定
            if (m_fadePanel.sprite == null && m_fadePanelSprite != null)
            {
                m_fadePanel.sprite = m_fadePanelSprite;
                Debug.Log("GameManager: 既存のフェードパネルにInspectorから割り当てられたスプライトを設定しました。");
            }
        }
        // フェードパネルの初期状態をログ出力
        if (m_fadePanel != null)
        {
            Debug.Log($"SetupPermanentUIElements: m_fadePanelの初期色: {m_fadePanel.color}, アクティブ状態: {m_fadePanel.gameObject.activeSelf}, ソート順: {m_fadePanel.canvas.sortingOrder}");
        }
    }

    // 画面が完全に黒くなるフェードアウト処理
    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            Debug.Log("FadeOutAndLoadScene: 既存のフェードコルーチンを停止しました。");
        }

        if (m_fadePanel == null)
        {
            Debug.LogWarning("Fade Panel is null, directly loading scene.");
            SceneManager.LoadScene(sceneName); // Singleモードでロード
            yield break;
        }

        Debug.Log("FadeOutAndLoadScene: フェードアウト開始。");
        m_fadePanel.gameObject.SetActive(true);
        float timer = 0f;
        Color panelColor = m_fadePanel.color;
        panelColor.a = 0f; // フェードアウト開始時は透明
        m_fadePanel.color = panelColor; // 色を透明に設定し直す
        Debug.Log($"FadeOutAndLoadScene: 開始時のパネル色: {m_fadePanel.color}, アクティブ状態: {m_fadePanel.gameObject.activeSelf}");

        while (timer < m_fadeDuration)
        {
            timer += Time.deltaTime;
            panelColor.a = Mathf.Lerp(0f, 1f, timer / m_fadeDuration);
            m_fadePanel.color = panelColor;
            Debug.Log($"FadeOut: Alpha={panelColor.a:F2}"); // フェードアウト中のアルファ値ログ
            yield return null;
        }
        panelColor.a = 1f; // 完全に黒
        m_fadePanel.color = panelColor;
        Debug.Log($"FadeOutAndLoadScene: フェードアウト完了時のパネル色: {m_fadePanel.color}, アクティブ状態: {m_fadePanel.gameObject.activeSelf}");

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    // 画面が徐々に明るくなるフェードイン処理
    private IEnumerator FadeIn(float duration)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            Debug.Log("FadeIn: 既存のフェードコルーチンを停止しました。");
        }

        if (m_fadePanel == null)
        {
            Debug.LogWarning("Fade Panel is null, cannot perform FadeIn.");
            yield break;
        }

        Debug.Log("FadeIn: フェードイン開始。");
        m_fadePanel.gameObject.SetActive(true); // フェードパネルをアクティブにする

        float timer = 0f;
        Color panelColor = m_fadePanel.color;
        panelColor.a = 1f; // 最初は完全に黒
        m_fadePanel.color = panelColor;
        Debug.Log($"FadeIn: フェードイン開始時のパネル色: {m_fadePanel.color}, アクティブ状態: {m_fadePanel.gameObject.activeSelf}");

        while (timer < duration)
        {
            timer += Time.deltaTime;
            panelColor.a = Mathf.Lerp(1f, 0f, timer / duration);
            m_fadePanel.color = panelColor;
            Debug.Log($"FadeIn: Alpha={panelColor.a:F2}"); // フェードイン中のアルファ値ログ
            yield return null;
        }
        panelColor.a = 0f; // 完全に透明
        m_fadePanel.color = panelColor;
        m_fadePanel.gameObject.SetActive(false); // フェードイン完了後、パネルを非アクティブにする
        Debug.Log($"FadeIn: フェードイン完了時のパネル色: {m_fadePanel.color}, アクティブ状態: {m_fadePanel.gameObject.activeSelf}");
        currentFadeCoroutine = null; // コルーチンが終了したので参照をクリア
    }
}
