using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSwitchPlatform : MonoBehaviour
{
    [Header("Switch Platform Settings")]
    [Tooltip("The ID of the switch that controls this platform.")]
    public int switchId;
    [Tooltip("The time it takes for the platform to fully appear or disappear.")]
    [SerializeField] private float fadeTime = 1.0f;
    [Tooltip("The initial state of the platform.")]
    [SerializeField] private bool startsActive = false;

    [Header("Time Limit Settings")]
    [Tooltip("If true, the platform will automatically disappear after a set duration.")]
    [SerializeField] private bool hasTimeLimit = true;
    [Tooltip("The duration in seconds the platform stays active before disappearing automatically.")]
    [SerializeField] private float displayDuration = 5.0f;

    [Header("Audio Settings")] // ★追加: オーディオ設定
    [Tooltip("BGM to play when this platform is active.")]
    [SerializeField] private AudioClip platformBGM;
    [Tooltip("The normal BGM for the scene.")]
    [SerializeField] private AudioClip normalBGM;

    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider;
    private Color initialColor;
    private bool isAnimating = false;
    private bool isActive;
    private Coroutine autoDisappearCoroutine;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();

        if (tilemap == null || tilemapRenderer == null || tilemapCollider == null)
        {
            UnityEngine.Debug.LogError("Missing required components (Tilemap, TilemapRenderer, or TilemapCollider2D) on this GameObject.", this);
            return;
        }

        initialColor = tilemap.color;
        initialColor.a = 1f;

        isActive = startsActive;

        if (isActive)
        {
            tilemap.color = initialColor;
            tilemapCollider.enabled = true;
            tilemapRenderer.enabled = true;
        }
        else
        {
            tilemap.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
            tilemapCollider.enabled = false;
            tilemapRenderer.enabled = false;
        }
    }

    // ★追加: 橋の状態を切り替え、BGMも制御するメソッド
    public void ToggleVisibilityWithBGM()
    {
        if (isAnimating) return;

        if (isActive)
        {
            DeactivatePlatformAndRestoreBGM(); // 橋を消し、元のBGMに戻す
        }
        else
        {
            ActivatePlatformAndPlayBGM(); // 橋を出し、専用BGMを流す
        }
    }

    private void ActivatePlatformAndPlayBGM()
    {
        if (autoDisappearCoroutine != null)
        {
            StopCoroutine(autoDisappearCoroutine);
            autoDisappearCoroutine = null;
        }

        // 専用BGMを再生
        if (GameManager.instance != null)
        {
            GameManager.instance.PlayBGM(platformBGM);
        }

        StartCoroutine(FadeIn());
    }

    private void DeactivatePlatformAndRestoreBGM()
    {
        if (autoDisappearCoroutine != null)
        {
            StopCoroutine(autoDisappearCoroutine);
            autoDisappearCoroutine = null;
        }

        // 元のBGMに戻す
        if (GameManager.instance != null)
        {
            GameManager.instance.PlayBGM(normalBGM);
        }

        StartCoroutine(FadeOut());
    }

    // 足場をフェードインさせるコルーチン
    private IEnumerator FadeIn()
    {
        isAnimating = true;
        isActive = true;
        float elapsedTime = 0f;

        tilemapRenderer.enabled = true;
        tilemapCollider.enabled = true;

        Color startColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
        Color targetColor = initialColor;
        tilemap.color = startColor;

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

    // 足場をフェードアウトさせるコルーチン
    private IEnumerator FadeOut()
    {
        isAnimating = true;
        isActive = false;
        float elapsedTime = 0f;

        Color startColor = initialColor;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
        tilemap.color = startColor;

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

    // 自動で消えるためのカウントダウンコルーチン
    private IEnumerator AutoDisappearCountdown()
    {
        yield return new WaitForSeconds(displayDuration);

        if (isActive)
        {
            DeactivatePlatformAndRestoreBGM(); // 時間切れ時に橋を消し、元のBGMに戻す
        }
        autoDisappearCoroutine = null;
    }
}
