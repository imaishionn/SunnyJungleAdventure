using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// タッチ操作でプレイヤーを動かすためのバーチャルジョイスティックを制御します。
/// </summary>
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    // ====================================================================================================
    // #region: インスペクターから設定する変数
    // ====================================================================================================
    [SerializeField] private RectTransform joystickBase; // ジョイスティックのベース部分（背景画像など）
    [SerializeField] private RectTransform joystickKnob; // ジョイスティックのツマミ部分（動く部分）
    [SerializeField] private float joystickRadius = 100f; // ジョイスティックの操作可能な半径

    // ====================================================================================================
    // #region: スクリプト内部で管理する変数
    // ====================================================================================================
    // 現在の入力方向を保持する変数。X軸とY軸のベクトルで表される。
    private Vector2 inputDirection = Vector2.zero;

    // 入力方向を外部（例: PlayerMove.cs）から取得するためのプロパティ
    public Vector2 InputDirection => inputDirection;

    // ====================================================================================================
    // #region: MonoBehaviour ライフサイクル
    // ====================================================================================================

    /// <summary>
    /// オブジェクトがアクティブになった時に一度だけ実行される初期化処理
    /// </summary>
    private void Awake()
    {
        // 必須コンポーネントが正しく割り当てられているか確認し、エラーをログに出力
        if (joystickBase == null)
        {
            UnityEngine.Debug.LogError("VirtualJoystick: ジョイスティックのベースが割り当てられていません！");
        }
        if (joystickKnob == null)
        {
            UnityEngine.Debug.LogError("VirtualJoystick: ジョイスティックのツマミが割り当てられていません！");
        }
    }

    // ====================================================================================================
    // #region: ポインターイベント
    // ====================================================================================================

    /// <summary>
    /// 指が画面に触れたときに呼び出されます。
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // 指が触れた時点でドラッグ処理を開始
        OnDrag(eventData);
    }

    /// <summary>
    /// 指を動かしている間、毎フレーム呼び出されます。
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // 1. タッチした画面座標を、ジョイスティックのベース（RectTransform）のローカル座標に変換
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBase,
            eventData.position,
            eventData.pressEventCamera,
            out position
        );

        // 2. ローカル座標を正規化（-1.0から1.0の範囲に変換）
        // 中心を原点とし、半径で割ることで、座標を単位ベクトルに近づける
        position = position / joystickRadius;

        // 3. 入力方向を計算
        inputDirection = position;
        // 入力ベクトルの長さが1.0を超えた場合、正規化して円内に収める
        if (inputDirection.magnitude > 1.0f)
        {
            inputDirection.Normalize();
        }

        // 4. ツマミの位置を更新
        // 計算した入力方向と半径を掛け合わせることで、ツマミを正しい位置に配置
        joystickKnob.anchoredPosition = inputDirection * joystickRadius;
    }

    /// <summary>
    /// 指を画面から離したときに呼び出されます。
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // 1. 入力方向をリセット
        inputDirection = Vector2.zero;
        // 2. ツマミを元の（中心の）位置に戻す
        joystickKnob.anchoredPosition = Vector2.zero;
    }
}
