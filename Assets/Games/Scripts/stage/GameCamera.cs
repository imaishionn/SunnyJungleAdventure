using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーを追跡し、設定された範囲内でカメラの移動を制限するスクリプトです。
/// </summary>
public class GameCamera : MonoBehaviour {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("追跡対象")]
    [Tooltip("追跡するプレイヤーオブジェクト")]
    [SerializeField] private GameObject _player;

    [Header("カメラ設定")]
    [Tooltip("プレイヤーからのカメラの相対位置。Zは通常負の値でカメラの奥行きを設定します。")]
    [SerializeField] private Vector3 _cameraOffset = new(0f, 0f, -10f);

    [Header("カメラ追跡範囲の制限")]
    [Tooltip("X座標の追跡を制限するかどうか")]
    [SerializeField] private bool _useClampX = false;
    [Tooltip("Y座標の追跡を制限するかどうか")]
    [SerializeField] private bool _useClampY = false;
    [Tooltip("カメラが移動できる最大位置 (X, Y)")]
    [SerializeField] private Vector2 _cameraMaxPos = Vector2.zero;
    [Tooltip("カメラが移動できる最小位置 (X, Y)")]
    [SerializeField] private Vector2 _cameraMinPos = Vector2.zero;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    // カメラをシングルトンとして管理する場合
    // public static GameCamera Instance { get; private set; }

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start() {
        // "Player"タグを持つオブジェクトを検索して設定
        if(_player == null) {
            _player = GameObject.FindGameObjectWithTag("Player");
        }

        // カメラ追跡の有効性をチェック
        if(_player == null) {
            Debug.LogError("GameCamera: 'Player'タグのゲームオブジェクトが見つかりません！カメラ追跡ができません。",this);
            return;
        }

        // シーン名に基づいて追跡を無効化
        string currentSceneName = SceneManager.GetActiveScene().name;
        if(currentSceneName == "ClearScene") {
            Debug.Log("GameCamera: ClearSceneのため、カメラの追跡を無効化します。");
            _player = null; // 追跡を停止するため、参照をクリア
            return;
        }

        // ゲーム開始時にカメラをプレイヤーの初期位置に合わせる
        CameraUpdate();
    }

    private void LateUpdate() {
        // LateUpdateでカメラを更新することで、プレイヤーの移動後に追跡し、より滑らかな動きになります。
        if (_player == null) {
            return;
        }

        // カメラの追跡ロジックを実行
        CameraUpdate();
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// カメラの位置を更新し、設定された範囲内でクランプします。
    /// </summary>
    private void CameraUpdate() {
        // プレイヤーの位置にオフセットを加えた目標位置を計算
        Vector3 targetPos = _player.transform.position + _cameraOffset;

        // 追跡範囲の制限を適用
        if(_useClampX) {
            targetPos.x = Mathf.Clamp(targetPos.x,_cameraMinPos.x,_cameraMaxPos.x);
        }
        if(_useClampY) {
            targetPos.y = Mathf.Clamp(targetPos.y,_cameraMinPos.y,_cameraMaxPos.y);
        }

        // カメラの位置を目標位置に設定
        transform.position = targetPos;
    }
}
