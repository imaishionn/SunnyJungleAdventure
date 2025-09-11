using UnityEngine;

/// <summary>
/// プレイヤーの横移動に応じて背景をスクロールさせるスクリプトです。
/// </summary>
public class BackGroundScroll : MonoBehaviour {
    [Header("コンポーネント"), SerializeField]
    private PlayerMove _playerMove; // プレイヤーの移動速度を取得するためのPlayerMoveコンポーネント

    [Header("スクロール設定"), SerializeField]
    private float _division = 1.0f; // 背景のスクロール速度を補正します（大きいほど遅くなる）。ゼロは不可。

    private void Awake() {
        if (_playerMove == null) {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) {
                _playerMove = playerObject.GetComponent<PlayerMove>();
            }

            if (_playerMove == null) {
                Debug.LogError("BackGroundScroll: 'Player'タグを持つオブジェクトにPlayerMoveコンポーネントが見つかりません。Inspectorで設定するか、Playerオブジェクトにコンポーネントがアタッチされているか確認してください。", this);
            }
        }

        if (_division == 0.0f) {
            Debug.LogWarning("BackGroundScroll: Divisionの値が0です。ゼロ除算を防ぐため、値を1.0fに設定します。", this);
            _division = 1.0f;
        }
    }

    private void Update() {
        if (_playerMove == null || _division == 0.0f) {
            return;
        }

        float move = (-_playerMove.MoveSpeed / _division) * Time.deltaTime;
        transform.Translate(new Vector3(move, 0.0f, 0.0f));
    }
}
