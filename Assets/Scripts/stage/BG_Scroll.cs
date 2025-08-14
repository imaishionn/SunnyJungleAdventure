using UnityEngine;
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

public class BG_Scroll : MonoBehaviour
{
    // プレイヤーへの参照
    // InspectorでPlayerオブジェクトを割り当てるか、Awakeで自動的に取得する
    [SerializeField] PlayerMove m_playerMove;

    // 移動速度補正 (大きいほど遅くなる)
    // ゼロ除算を防ぐため、デフォルト値を1.0fに設定
    [SerializeField, Header("移動速度補正 (大きいほど遅くなる)")]
    private float Division = 1.0f; // デフォルト値を0.0fから1.0fに変更

    void Start()
    {
        // PlayerMoveがInspectorで割り当てられていない場合、タグで検索して取得
        if (m_playerMove == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                m_playerMove = playerObject.GetComponent<PlayerMove>();
            }

            if (m_playerMove == null)
            {
                // エラーメッセージをより具体的に
                Debug.LogError("BG_Scroll: 'Player'タグを持つオブジェクトにPlayerMoveコンポーネントが見つかりません。Inspectorで設定するか、またはPlayerオブジェクトにアタッチされているか確認してください。", this);
            }
        }

        // Divisionが0の場合の警告と修正
        if (Division == 0.0f)
        {
            Debug.LogWarning("BG_Scroll: Divisionの値が0です。ゼロ除算を防ぐため、1.0fに設定します。", this);
            Division = 1.0f;
        }
    }

    void Update()
    {
        // 必要な参照がないか、Divisionが0の場合は処理をスキップ
        if (m_playerMove == null || Division == 0.0f)
        {
            return;
        }

        // プレイヤーの移動速度に応じて背景をスクロールさせる
        // PlayerMoveスクリプトの 'MoveSpeed' プロパティにアクセス
        // 背景はプレイヤーの移動方向と逆方向に動くため、負の値を適用
        float move = (-m_playerMove.MoveSpeed / Division) * Time.deltaTime;

        // 背景を移動する
        transform.Translate(new Vector3(move, 0.0f, 0.0f));
    }
}
