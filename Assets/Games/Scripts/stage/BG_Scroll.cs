using UnityEngine;

/// <summary>
/// プレイヤーの横移動に応じて背景をスクロールさせるスクリプトです。
/// </summary>
public class BG_Scroll : MonoBehaviour {
    [Header("コンポーネント"), Tooltip("プレイヤーの移動速度を取得するためのPlayerMoveコンポーネント"), SerializeField]
    private PlayerMove m_playerMove;

    [Header("スクロール設定"), Tooltip("背景のスクロール速度を補正します（大きいほど遅くなる）。ゼロは不可。"), SerializeField] private float Division = 1.0f;

    private void Awake() {
        // PlayerMoveがInspectorで割り当てられていない場合、タグで検索して取得
        if(m_playerMove == null) {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if(playerObject != null) {
                m_playerMove = playerObject.GetComponent<PlayerMove>();
            }

            if(m_playerMove == null) {
                Debug.LogError("BG_Scroll: 'Player'タグを持つオブジェクトにPlayerMoveコンポーネントが見つかりません。Inspectorで設定するか、Playerオブジェクトにコンポーネントがアタッチされているか確認してください。",this);
            }
        }

        // Divisionが0の場合の警告と修正
        if(Division == 0.0f) {
            Debug.LogWarning("BG_Scroll: Divisionの値が0です。ゼロ除算を防ぐため、値を1.0fに設定します。",this);
            Division = 1.0f;
        }
    }

    private void Update() {
        // プレイヤーの参照がないか、Divisionが0の場合は処理をスキップ
        if(m_playerMove == null || Division == 0.0f) {
            return;
        }

        // プレイヤーの移動方向に応じて背景を動かす
        // PlayerMoveの速度をDivisionで割り、タイムデルタで滑らかに移動させる
        float move = (-m_playerMove.MoveSpeed / Division) * Time.deltaTime;

        // 背景を横方向に移動
        transform.Translate(new Vector3(move,0.0f,0.0f));
    }
}
