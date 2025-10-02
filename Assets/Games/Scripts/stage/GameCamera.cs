using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーを追跡し、設定された範囲内でカメラの移動を制限するスクリプトです。
/// </summary>
public class GameCamera : MonoBehaviour {

    [Header("追跡するプレイヤーオブジェクト"), SerializeField]
    private GameObject _player;

    [Header("プレイヤーからのカメラの相対位置。Zは通常負の値でカメラの奥行きを設定します。"), SerializeField]
    private Vector3 _cameraOffset = new(0f, 0f, -10f); 

    [Header("X座標の追跡を制限するかどうか"), SerializeField]
    private bool _useClampX = false; 

    [Header("Y座標の追跡を制限するかどうか"), SerializeField]
    private bool _useClampY = false; 

    [Header("カメラが移動できる最大位置 (X, Y)"), SerializeField]
    private Vector2 _cameraMaxPos = Vector2.zero; 

    [Header("カメラが移動できる最小位置 (X, Y)"), SerializeField]
    private Vector2 _cameraMinPos = Vector2.zero; 

    private void Start() {
        if (_player == null) {
            _player = GameObject.FindGameObjectWithTag("Player");
        }

        if (_player == null) {
            Debug.LogError("GameCamera: 'Player'タグのゲームオブジェクトが見つかりません！カメラ追跡ができません。", this);
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "ClearScene") {
            Debug.Log("GameCamera: ClearSceneのため、カメラの追跡を無効化します。");
            _player = null;
            return;
        }

        CameraUpdate();
    }

    private void LateUpdate() {
        if (_player == null) {
            return;
        }

        CameraUpdate();
    }

    /// <summary>
    /// カメラの位置を更新し、設定された範囲内でクランプします。
    /// </summary>
    private void CameraUpdate() {
        Vector3 targetPos = _player.transform.position + _cameraOffset;

        if (_useClampX) {
            targetPos.x = Mathf.Clamp(targetPos.x, _cameraMinPos.x, _cameraMaxPos.x);
        }
        if (_useClampY) {
            targetPos.y = Mathf.Clamp(targetPos.y, _cameraMinPos.y, _cameraMaxPos.y);
        }

        transform.position = targetPos;
    }
}
