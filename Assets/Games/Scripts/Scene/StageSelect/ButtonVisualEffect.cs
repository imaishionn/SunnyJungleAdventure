using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UIボタンに視覚的なフィードバック効果を追加するスクリプトです。
/// 選択時の拡大/縮小、クリック時の点滅効果を管理します。
/// </summary>
public class ButtonVisualEffect : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler {
    private Vector3 _originalScale; // 元の大きさを保存
    private Image _buttonImage;
    private Color _originalColor;
    private Coroutine _blinkCoroutine;
    private Coroutine _scaleCoroutine;

    [Header("拡大率"), SerializeField]
    private float _scaleFactor = 1.1f;

    [Header("拡大/縮小にかかる時間"), SerializeField]
    private float _transitionDuration = 0.1f; 

    [Header("点滅回数"), SerializeField]
    private int _blinkCount = 3;

    [Header("点滅速度（短いほど速い"), SerializeField]
    private float _blinkSpeed = 0.1f;

    [Header("点滅時の色"), SerializeField]
    private Color _blinkColor = Color.white;

    // UnityのMonoBehaviourはコンストラクタを使用しないため削除

    private void Awake() {
        _originalScale = transform.localScale;
        _buttonImage = GetComponent<Image>();
        if (_buttonImage != null) {
            _originalColor = _buttonImage.color;
        }
    }

    /// <summary>
    /// UIが選択されたときに呼び出されます。（キーボード、ゲームパッド、マウスホバー）
    /// </summary>
    public void OnSelect(BaseEventData eventData) {
        // 拡大を開始。既存の拡大コルーチンを停止
        if (_scaleCoroutine != null) {
            StopCoroutine(_scaleCoroutine);
        }
        _scaleCoroutine = StartCoroutine(ScaleButton(transform.localScale, _originalScale * _scaleFactor));
    }

    /// <summary>
    /// UIの選択が外れたときに呼び出されます。
    /// </summary>
    public void OnDeselect(BaseEventData eventData) {
        // 元の大きさに戻す。既存の拡大コルーチンを停止
        if (_scaleCoroutine != null) {
            StopCoroutine(_scaleCoroutine);
        }
        _scaleCoroutine = StartCoroutine(ScaleButton(transform.localScale, _originalScale));
    }

    /// <summary>
    /// ボタンがクリックされたときに呼び出されます。
    /// </summary>
    public void OnPointerClick(PointerEventData eventData) {
        if (gameObject.activeInHierarchy) {
            // 既存の点滅コルーチンを停止
            if (_blinkCoroutine != null) {
                StopCoroutine(_blinkCoroutine);
            }
            _blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    /// <summary>
    /// ボタンの拡大/縮小を滑らかに行うコルーチン
    /// </summary>
    private IEnumerator ScaleButton(Vector3 startScale, Vector3 endScale) {
        float timer = 0f;
        while (timer < _transitionDuration) {
            timer += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, timer / _transitionDuration);
            yield return null;
        }
        transform.localScale = endScale;
        _scaleCoroutine = null;
    }

    /// <summary>
    /// ボタンを点滅させるコルーチン
    /// </summary>
    private IEnumerator BlinkEffect() {
        // 拡大効果と点滅効果を分離
        if (_scaleCoroutine != null) {
            StopCoroutine(_scaleCoroutine);
            transform.localScale = _originalScale;
        }

        // 点滅効果
        for (int i = 0; i < _blinkCount; i++) {
            if (_buttonImage != null) {
                _buttonImage.color = _blinkColor;
            }
            yield return new WaitForSecondsRealtime(_blinkSpeed);

            if (_buttonImage != null) {
                _buttonImage.color = _originalColor;
            }
            yield return new WaitForSecondsRealtime(_blinkSpeed);
        }

        _blinkCoroutine = null;
    }
}
