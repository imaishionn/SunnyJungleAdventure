using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

/// <summary>
/// スイッチに連動して出現・消滅するギミック付きの足場を管理します。
/// タイムリミットやBGMの切り替え機能を含みます。
/// </summary>
public class TilemapSwitchPlatform : MonoBehaviour
{
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("プラットフォーム設定")]
    [Tooltip("このプラットフォームを制御するスイッチのID")]
    public int switchId;
    [Tooltip("プラットフォームが完全に現れる、または消えるまでの時間")]
    [SerializeField] private float fadeTime = 1.0f;
    [Tooltip("ゲーム開始時のプラットフォームの状態")]
    [SerializeField] private bool startsActive = false;

    [Header("タイムリミット設定")]
    [Tooltip("trueの場合、プラットフォームは一定時間経過後に自動的に消滅します")]
    [SerializeField] private bool hasTimeLimit = true;
    [Tooltip("プラットフォームが自動で消滅するまでの時間")]
    [SerializeField] private float displayDuration = 5.0f;

    [Header("オーディオ設定")]
    [Tooltip("プラットフォームがアクティブなときに再生するBGM")]
    [SerializeField] private AudioClip platformBGM;
    [Tooltip("シーンの通常BGM")]
    [SerializeField] private AudioClip normalBGM;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider;
    private Color initialColor;

    private bool isAnimating = false;
    private bool isActive;
    private Coroutine autoDisappearCoroutine;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start()
    {
        // コンポーネントの参照を取得
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();

        // 必須コンポーネントの存在チェック
        if (tilemap == null || tilemapRenderer == null || tilemapCollider == null)
        {
            Debug.LogError("このゲームオブジェクトには、必須コンポーネント(Tilemap, TilemapRenderer, またはTilemapCollider2D)がアタッチされていません。", this);
            return;
        }

        // 初期カラーと状態を設定
        initialColor = tilemap.color;
        isActive = startsActive;

        UpdatePlatformState(isActive);
    }

    // ----------------------------------------------------------------------------------------------------
    // パブリックメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// プラットフォームの状態を切り替え、それに伴うBGMの変更も行います。
    /// </summary>
    public void ToggleVisibilityWithBGM()
    {
        if (isAnimating) return;

        if (isActive)
        {
            DeactivatePlatformAndRestoreBGM();
        }
        else
        {
            ActivatePlatformAndPlayBGM();
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// プラットフォームの状態（アクティブ/非アクティブ）を即座に更新します。
    /// </summary>
    /// <param name="activeState">trueならアクティブ、falseなら非アクティブ</param>
    private void UpdatePlatformState(bool activeState)
    {
        if (activeState)
        {
            tilemap.color = initialColor;
            tilemapRenderer.enabled = true;
            tilemapCollider.enabled = true;
        }
        else
        {
            tilemap.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
            tilemapRenderer.enabled = false;
            tilemapCollider.enabled = false;
        }
    }

    /// <summary>
    /// プラットフォームをアクティブ化し、専用BGMを再生します。
    /// </summary>
    private void ActivatePlatformAndPlayBGM()
    {
        if (autoDisappearCoroutine != null)
        {
            StopCoroutine(autoDisappearCoroutine);
            autoDisappearCoroutine = null;
        }

        // 専用BGMを再生
        // GameManager.instance を GameManager.Instance に修正
        if (GameManager.Instance != null && platformBGM != null)
        {
            // GameManager.instance.PlayBGM(platformBGM) を GameManager.Instance.PlayBGM(platformBGM) に修正
            GameManager.Instance.PlayBGM(platformBGM);
        }

        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// プラットフォームを非アクティブ化し、通常のBGMに戻します。
    /// </summary>
    private void DeactivatePlatformAndRestoreBGM()
    {
        if (autoDisappearCoroutine != null)
        {
            StopCoroutine(autoDisappearCoroutine);
            autoDisappearCoroutine = null;
        }

        // 元のBGMに戻す
        // GameManager.instance を GameManager.Instance に修正
        if (GameManager.Instance != null && normalBGM != null)
        {
            // GameManager.instance.PlayBGM(normalBGM) を GameManager.Instance.PlayBGM(normalBGM) に修正
            GameManager.Instance.PlayBGM(normalBGM);
        }

        StartCoroutine(FadeOut());
    }

    /// <summary>
    /// 足場をフェードインさせるコルーチン
    /// </summary>
    private IEnumerator FadeIn()
    {
        isAnimating = true;
        isActive = true;
        float elapsedTime = 0f;

        tilemapRenderer.enabled = true;
        tilemapCollider.enabled = true;

        Color startColor = tilemap.color;
        Color targetColor = initialColor;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, targetColor.a, elapsedTime / fadeTime);
            tilemap.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }

        tilemap.color = initialColor;
        isAnimating = false;

        if (hasTimeLimit)
        {
            autoDisappearCoroutine = StartCoroutine(AutoDisappearCountdown());
        }
    }

    /// <summary>
    /// 足場をフェードアウトさせるコルーチン
    /// </summary>
    private IEnumerator FadeOut()
    {
        isAnimating = true;
        isActive = false;
        float elapsedTime = 0f;

        Color startColor = tilemap.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, targetColor.a, elapsedTime / fadeTime);
            tilemap.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }

        tilemap.color = targetColor;
        tilemapCollider.enabled = false;
        tilemapRenderer.enabled = false;

        isAnimating = false;
    }

    /// <summary>
    /// 自動で消えるためのカウントダウンコルーチン
    /// </summary>
    private IEnumerator AutoDisappearCountdown()
    {
        yield return new WaitForSeconds(displayDuration);

        // 時間切れ時にプラットフォームがアクティブな場合のみ処理を実行
        if (isActive)
        {
            DeactivatePlatformAndRestoreBGM();
        }
        autoDisappearCoroutine = null;
    }
}