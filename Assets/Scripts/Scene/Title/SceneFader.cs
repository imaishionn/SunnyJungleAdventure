using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // これが UnityEngine.UI.Image を使うために必要です

// ★★★ 重要: GameManagerがフェード処理を管理する場合、このスクリプトは不要です。★★★
// ★★★ その場合、このファイルはプロジェクトから削除してください。★★★

// ★★★ ここに 'using System.Net.Mime;' がないことを確認してください！ ★★★

public class SceneFader : MonoBehaviour
{
    // フェードにかかる時間 (秒)
    [SerializeField] private float fadeDuration = 1f;

    // シングルトンインスタンス
    public static SceneFader instance;

    // フェード用の黒いImage（コードで生成）
    private Image fadeImage; // <- この行 (19行目付近) でエラーが出ているはずです

    // フェードImageを保持するための専用Canvas（コードで生成）
    private Canvas fadeCanvas;

    // 次のシーンでフェードインが必要かどうかを示すフラグ
    private static bool shouldFadeInOnNextLoad = false;

    // GameManagerと同様に、シーン遷移後もこのオブジェクトが破棄されないようにする
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // このオブジェクトを永続化
            SetupFadeUIElements(); // フェード用のUI要素をセットアップ
        }
        else
        {
            // 既にインスタンスが存在する場合は、この重複を破棄
            Destroy(gameObject);
            return;
        }
    }

    // シーンロードイベントの登録と解除
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーンがロードされたときに呼び出されるメソッド
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (shouldFadeInOnNextLoad)
        {
            SetFadeAlpha(1f); // ロード直後は画面を黒くする
            StartCoroutine(FadeIn()); // フェードインを開始
            shouldFadeInOnNextLoad = false; // フラグをリセット
        }
        else
        {
            SetFadeAlpha(0f); // フェードインが必要ない場合は透明にする
        }
    }

    // --- 公開メソッド ---

    // 指定されたシーンにフェードアウトしながら遷移する
    public void FadeOutToScene(string sceneName)
    {
        if (instance == null || fadeImage == null)
        {
            // エラーが発生したら、フェードなしでシーン遷移を試みる
            SceneManager.LoadScene(sceneName);
            return;
        }

        shouldFadeInOnNextLoad = true; // 次のシーンでフェードインが必要なことを示す
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    // --- プライベートヘルパーメソッド ---

    // フェードImageと専用Canvasを動的に生成し、設定する
    private void SetupFadeUIElements()
    {
        // Canvasオブジェクトの生成と設定
        GameObject fadeCanvasObject = new GameObject("SceneFaderCanvas");
        // このスクリプトがアタッチされているオブジェクトの子にする
        fadeCanvasObject.transform.SetParent(this.transform);
        fadeCanvas = fadeCanvasObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // 画面全体にオーバーレイ表示
        fadeCanvas.sortingOrder = 999; // 他のUIよりも手前に表示

        // CanvasScalerで画面サイズ適応を設定
        CanvasScaler canvasScaler = fadeCanvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f; // 幅と高さの中間でスケール

        // UIイベントのRaycastを可能にする
        fadeCanvasObject.AddComponent<GraphicRaycaster>();

        // FadeImageオブジェクトの生成と設定
        GameObject fadeImageObject = new GameObject("FadeImage");
        fadeImageObject.transform.SetParent(fadeCanvas.transform); // Canvasの子にする
        fadeImageObject.transform.localScale = Vector3.one;
        fadeImage = fadeImageObject.AddComponent<Image>();

        fadeImage.color = Color.black; // 色を黒に設定

        // RectTransformで画面全体にストレッチさせる
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero; // 左下を基準
        rectTransform.anchorMax = Vector2.one;  // 右上を基準
        rectTransform.offsetMin = Vector2.zero; // オフセットなし
        rectTransform.offsetMax = Vector2.zero; // オフセットなし

        // Canvas内で一番手前（最前面）に描画されるようにする
        rectTransform.SetAsLastSibling();

        SetFadeAlpha(0f); // 初期状態は透明
    }

    // 画面が徐々に明るくなるフェードイン処理
    private IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // 時間スケールに影響されない時間を使う
            float alpha = 1f - (timer / fadeDuration); // 1から0へ変化
            SetFadeAlpha(alpha);
            yield return null; // 1フレーム待つ
        }
        SetFadeAlpha(0f); // 完全に透明にする
    }

    // 画面が完全に黒くなるフェードアウト処理とシーンロード
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // 時間スケールに影響されない時間を使う
            float alpha = timer / fadeDuration; // 0から1へ変化
            SetFadeAlpha(alpha);
            yield return null; // 1フレーム待つ
        }

        SetFadeAlpha(1f); // 完全に不透明にする

        // フェードアウト完了後、次のシーンをロード
        SceneManager.LoadScene(sceneName);
    }

    // フェードImageのアルファ値（透明度）を設定する
    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(alpha); // アルファ値を0～1の範囲に制限
            fadeImage.color = color;
        }
    }
}
