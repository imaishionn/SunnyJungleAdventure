using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// スイッチに連動して出現・消滅するギミック付きの足場を管理します。
/// タイムリミットやBGMの切り替え機能を含みます。
/// </summary>
public class TilemapSwitchPlatform : MonoBehaviour {
    [Header("このプラットフォームを制御するスイッチのID"), SerializeField]
    private int _switchId; 

    [Header("プラットフォームが完全に現れる、または消えるまでの時間"), SerializeField]
    private float _fadeTime = 1.0f; 

    [Header("ゲーム開始時のプラットフォームの状態"), SerializeField]
    private bool _startsActive = false; 

    [Header("trueの場合、プラットフォームは一定時間経過後に自動的に消滅します"), SerializeField]
    private bool _hasTimeLimit = true;

    [Header("プラットフォームが自動で消滅するまでの時間"), SerializeField]
    private float _displayDuration = 5.0f; 

    [Header("プラットフォームがアクティブなときに再生するBGM"), SerializeField]
    private AudioClip _platformBGM; 

    [Header("シーンの通常BGM"),SerializeField]
    private AudioClip _normalBGM;


    /// <summary>
    /// Tilemapコンポーネントへの参照
    /// </summary>
    private Tilemap _tilemap;

    /// <summary>
    /// TilemapRendererコンポーネントへの参照
    /// </summary>
    private TilemapRenderer _tilemapRenderer;

    /// <summary>
    /// TilemapCollider2Dコンポーネントへの参照
    /// </summary>
    private TilemapCollider2D _tilemapCollider;

    /// <summary>
    /// プラットフォームの初期カラー（透明度を含む）
    /// </summary>
    private Color _initialColor;

    /// <summary>
    /// アニメーション中かどうかのフラグ
    /// </summary>
    private bool _isAnimating = false;

    /// <summary>
    /// プラットフォームが現在アクティブかどうかのフラグ
    /// </summary>
    private bool _isActive;

    /// <summary>
    /// 自動消滅のコルーチン参照
    /// </summary>
    private Coroutine _autoDisappearCoroutine;

    private void Start() {
        _tilemap = GetComponent<Tilemap>();
        _tilemapRenderer = GetComponent<TilemapRenderer>();
        _tilemapCollider = GetComponent<TilemapCollider2D>();

        if (_tilemap == null || _tilemapRenderer == null || _tilemapCollider == null) {
            Debug.LogError("このゲームオブジェクトには、必須コンポーネント(Tilemap, TilemapRenderer, またはTilemapCollider2D)がアタッチされていません。", this);
            return;
        }

        _initialColor = _tilemap.color;
        _isActive = _startsActive;

        UpdatePlatformState(_isActive);
    }

    /// <summary>
    /// プラットフォームの状態を切り替え、それに伴うBGMの変更も行います。
    /// </summary>
    public void ToggleVisibilityWithBGM() {
        if (_isAnimating) {
            return;
        }

        if (_isActive) {
            DeactivatePlatformAndRestoreBGM();
        }
        else {
            ActivatePlatformAndPlayBGM();
        }
    }

    /// <summary>
    /// プラットフォームの状態（アクティブ/非アクティブ）を即座に更新します。
    /// </summary>
    /// <param name="activeState">trueならアクティブ、falseなら非アクティブ</param>
    private void UpdatePlatformState(bool activeState) {
        if (activeState) {
            _tilemap.color = _initialColor;
            _tilemapRenderer.enabled = true;
            _tilemapCollider.enabled = true;
        }
        else {
            _tilemap.color = new Color(_initialColor.r, _initialColor.g, _initialColor.b, 0);
            _tilemapRenderer.enabled = false;
            _tilemapCollider.enabled = false;
        }
    }

    /// <summary>
    /// プラットフォームをアクティブ化し、専用BGMを再生します。
    /// </summary>
    private void ActivatePlatformAndPlayBGM() {
        if (_autoDisappearCoroutine != null) {
            StopCoroutine(_autoDisappearCoroutine);
            _autoDisappearCoroutine = null;
        }

        if (GameManager.Instance != null && _platformBGM != null) {
            GameManager.Instance.PlayBGM(_platformBGM);
        }

        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// プラットフォームを非アクティブ化し、通常のBGMに戻します。
    /// </summary>
    private void DeactivatePlatformAndRestoreBGM() {
        if (_autoDisappearCoroutine != null) {
            StopCoroutine(_autoDisappearCoroutine);
            _autoDisappearCoroutine = null;
        }

        if (GameManager.Instance != null && _normalBGM != null) {
            GameManager.Instance.PlayBGM(_normalBGM);
        }

        StartCoroutine(FadeOut());
    }

    /// <summary>
    /// 足場をフェードインさせるコルーチン
    /// </summary>
    private IEnumerator FadeIn() {
        _isAnimating = true;
        _isActive = true;
        float elapsedTime = 0f;

        _tilemapRenderer.enabled = true;
        _tilemapCollider.enabled = true;

        Color startColor = _tilemap.color;
        Color targetColor = _initialColor;

        while (elapsedTime < _fadeTime) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, targetColor.a, elapsedTime / _fadeTime);
            _tilemap.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }

        _tilemap.color = _initialColor;
        _isAnimating = false;

        if (_hasTimeLimit) {
            _autoDisappearCoroutine = StartCoroutine(AutoDisappearCountdown());
        }
    }

    /// <summary>
    /// 足場をフェードアウトさせるコルーチン
    /// </summary>
    private IEnumerator FadeOut() {
        _isAnimating = true;
        _isActive = false;
        float elapsedTime = 0f;

        Color startColor = _tilemap.color;
        var targetColor = new Color(_initialColor.r, _initialColor.g, _initialColor.b, 0);

        while (elapsedTime < _fadeTime) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, targetColor.a, elapsedTime / _fadeTime);
            _tilemap.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }

        _tilemap.color = targetColor;
        _tilemapCollider.enabled = false;
        _tilemapRenderer.enabled = false;

        _isAnimating = false;
    }

    /// <summary>
    /// 自動で消えるためのカウントダウンコルーチン
    /// </summary>
    private IEnumerator AutoDisappearCountdown() {
        yield return new WaitForSeconds(_displayDuration);

        if (_isActive) {
            DeactivatePlatformAndRestoreBGM();
        }
        _autoDisappearCoroutine = null;
    }
}
