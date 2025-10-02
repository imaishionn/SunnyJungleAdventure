using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// タッチ操作でプレイヤーを動かすためのバーチャルジョイスティック。
/// </summary>
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler {
    /// <summary>
    /// インスタンス
    /// </summary>
    public static VirtualJoystick Instance { get; private set; }

    [Header("ジョイコンの動かない方"), SerializeField]
    private RectTransform _joystickBase;

    [Header("ジョイコンの動く方"), SerializeField]
    private RectTransform _joystickKnob;

    /// <summary>
    ///プライベートフィールドを使ってジョイスティックの入力方向を取得します。
    /// </summary>
    public Vector2 InputDirection { get; private set; }

    /// <summary>
    /// ジョイスティックの半径
    /// </summary>
    private float _joystickRadius;

    private void Awake() {
        // シングルトンインスタンスの割り当てと、シーンをまたぐ設定
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (_joystickBase == null || _joystickKnob == null) {
            Debug.LogError("VirtualJoystick: Required RectTransforms are not assigned!", this);
            return;
        }

        // Joystick baseの幅/高さの半分を半径とする
        _joystickRadius = _joystickBase.sizeDelta.x / 2;
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData) {
        // スクリーン座標をローカル座標に変換
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _joystickBase,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 position)
        ) {
            return;
        }

        // ローカル座標を正規化
        InputDirection = position / _joystickRadius;

        // 入力ベクトルの長さを1.0に制限
        InputDirection = Vector2.ClampMagnitude(InputDirection, 1.0f);

        // ツマミの位置を更新
        _joystickKnob.anchoredPosition = InputDirection * _joystickRadius;
    }

    public void OnPointerUp(PointerEventData eventData) {
        InputDirection = Vector2.zero;
        _joystickKnob.anchoredPosition = Vector2.zero;
    }
}
