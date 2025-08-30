
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform joystickBase; // ジョイスティックのベース部分
    [SerializeField] private RectTransform joystickKnob; // ジョイスティックのツマミ部分
    [SerializeField] private float joystickRadius = 100f; // ジョイスティックの半径

    // 現在の入力方向を保持する変数
    private Vector2 inputDirection = Vector2.zero;

    // 入力方向を外部から取得するためのプロパティ
    public Vector2 InputDirection => inputDirection;

    private void Awake()
    {
        if (joystickBase == null)
        {
            Debug.LogError("ジョイスティックのベースが割り当てられていません！");
        }
        if (joystickKnob == null)
        {
            Debug.LogError("ジョイスティックのツマミが割り当てられていません！");
        }
    }

    // 指が画面に触れたとき
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    // 指を動かしたとき
    public void OnDrag(PointerEventData eventData)
    {
        // ベースのローカル座標に変換
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBase,
            eventData.position,
            eventData.pressEventCamera,
            out position
        );

        // 中心を原点に
        position = position / joystickRadius;

        // 入力方向を計算
        inputDirection = position;
        if (inputDirection.magnitude > 1.0f)
        {
            inputDirection.Normalize();
        }

        // ツマミの位置を更新
        joystickKnob.anchoredPosition = inputDirection * joystickRadius;
    }

    // 指を画面から離したとき
    public void OnPointerUp(PointerEventData eventData)
    {
        // 入力をリセット
        inputDirection = Vector2.zero;
        // ツマミを元の位置に戻す
        joystickKnob.anchoredPosition = Vector2.zero;
    }
}