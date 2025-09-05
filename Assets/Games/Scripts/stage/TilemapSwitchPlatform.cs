using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// スイッチに連動して出現・消滅するギミック付きの足場を管理します。
/// タイムリミットやBGMの切り替え機能を含みます。
/// </summary>
public class TilemapSwitchPlatform : MonoBehaviour {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("プラットフォーム設定")]
    [Tooltip("このプラットフォームを制御するスイッチのID")]
    public int switchId;
    [Tooltip("プラットフォームが完全に現れる、または消えるまでの時間")]
    [SerializeField] private float _fadeTime = 1.0f;
    [Tooltip("ゲーム開始時のプラットフォームの状態")]
    [SerializeField] private bool _startsActive = false;

    [Header("タイムリミット設定")]
    [Tooltip("trueの場合、プラットフォームは一定時間経過後に自動的に消滅します")]
    [SerializeField] private bool _hasTimeLimit = true;
    [Tooltip("プラットフォームが自動で消滅するまでの時間")]
    [SerializeField] private float _displayDuration = 5.0f;

    [Header("オーディオ設定")]
    [Tooltip("プラットフォームがアクティブなときに再生するBGM")]
    [SerializeField] private AudioClip _platformBGM;
    [Tooltip("シーンの通常BGM")]
    [SerializeField] private AudioClip _normalBGM;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Tilemap _tilemap;
    private TilemapRenderer _tilemapRenderer;
    private TilemapCollider2D _tilemapCollider;
    private Color _initialColor;

    private bool _isAnimating = false;
    private bool _isActive;
    private Coroutine _autoDisappearCoroutine;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start() {
        // コンポーネントの参照を取得
        _tilemap = GetComponent<Tilemap>();
        _tilemapRenderer = GetComponent<TilemapRenderer>();
        _tilemapCollider = GetComponent<TilemapCollider2D>();

        // 必須コンポーネントの存在チェック
        if(_tilemap == null || _tilemapRenderer == null || _tilemapCollider == null) {
            Debug.LogError("このゲームオブジェクトには、必須コンポーネント(Tilemap, TilemapRenderer, またはTilemapCollider2D)がアタッチされていません。",this);
            return;
        }

        // 初期カラーと状態を設定
        _initialColor = _tilemap.color;
        _isActive = _startsActive;

        UpdatePlatformState(_isActive);
    }

    // ----------------------------------------------------------------------------------------------------
    // パブリックメソッド
    // ----------------------------------------------------------------------------------------------------
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

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// プラットフォームの状態（アクティブ/非アクティブ）を即座に更新します。
    /// </summary>
    /// <param name="activeState">trueならアクティブ、falseなら非アクティブ</param>
    private void UpdatePlatformState(bool activeState) {
        if(activeState) {
            _tilemap.color = _initialColor;
            _tilemapRenderer.enabled = true;
            _tilemapCollider.enabled = true;
        }
        else {
            _tilemap.color = new Color(_initialColor.r,_initialColor.g,_initialColor.b,0);
            _tilemapRenderer.enabled = false;
            _tilemapCollider.enabled = false;
        }
    }

    /// <summary>
    /// プラットフォームをアクティブ化し、専用BGMを再生します。
    /// </summary>
    private void ActivatePlatformAndPlayBGM() {
        if(_autoDisappearCoroutine != null) {
            StopCoroutine(_autoDisappearCoroutine);
            _autoDisappearCoroutine = null;
        }

        // 専用BGMを再生
        // GameManager.instance を GameManager.Instance に修正
        if(GameManager.Instance != null && _platformBGM != null) {
            // GameManager.instance.PlayBGM(platformBGM) を GameManager.Instance.PlayBGM(platformBGM) に修正
            GameManager.Instance.PlayBGM(_platformBGM);
        }

        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// プラットフォームを非アクティブ化し、通常のBGMに戻します。
    /// </summary>
    private void DeactivatePlatformAndRestoreBGM() {
        if(_autoDisappearCoroutine != null) {
            StopCoroutine(_autoDisappearCoroutine);
            _autoDisappearCoroutine = null;
        }

        // 元のBGMに戻す
        // GameManager.instance を GameManager.Instance に修正
        if(GameManager.Instance != null && _normalBGM != null) {
            // GameManager.instance.PlayBGM(normalBGM) を GameManager.Instance.PlayBGM(normalBGM) に修正
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

        while(elapsedTime < _fadeTime) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a,targetColor.a,elapsedTime / _fadeTime);
            _tilemap.color = new Color(targetColor.r,targetColor.g,targetColor.b,alpha);
            yield return null;
        }

        _tilemap.color = _initialColor;
        _isAnimating = false;

        if(_hasTimeLimit) {
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

        while(elapsedTime < _fadeTime) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a,targetColor.a,elapsedTime / _fadeTime);
            _tilemap.color = new Color(targetColor.r,targetColor.g,targetColor.b,alpha);
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

        // 時間切れ時にプラットフォームがアクティブな場合のみ処理を実行
        if(_isActive) {
            DeactivatePlatformAndRestoreBGM();
        }
        _autoDisappearCoroutine = null;
    }
}
