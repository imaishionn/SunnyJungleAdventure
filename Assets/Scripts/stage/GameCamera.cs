using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーを追従するゲームカメラのスクリプト。
/// X軸とY軸の追従範囲をInspectorから設定できるようにします。
/// </summary>
public class GameCamera : MonoBehaviour
{
    GameObject m_player; // 追従するプレイヤーオブジェクト

    [SerializeField]
    [Tooltip("カメラのオフセット（プレイヤーからの相対位置）。Zは通常負の値でカメラの奥行きを設定します。")]
    Vector3 CameraAddPos = new Vector3(0f, 0f, -10f); // プレイヤーからのカメラの相対位置

    [Header("カメラ追従範囲の制限")]
    [SerializeField]
    [Tooltip("カメラのX座標の追従を制限するかどうか")]
    bool UseClampX = false; // X座標の制限を使用するか
    [SerializeField]
    [Tooltip("カメラのY座標の追従を制限するかどうか")]
    bool UseClampY = false; // Y座標の制限を使用するか

    [SerializeField]
    [Tooltip("カメラが移動できる最大位置 (X, Y)")]
    Vector2 CameraMaxPos = Vector2.zero; // カメラの最大位置 (X, Y)
    [SerializeField]
    [Tooltip("カメラが移動できる最小位置 (X, Y)")]
    Vector2 CameraMinPos = Vector2.zero; // カメラの最小位置 (X, Y)

    // ★以前の StopFollowY と m_isCameraStopped は削除しました。
    // ★これにより、カメラが完全に停止するのではなく、指定範囲内で追従するようになります。

    // シングルトンインスタンス (PlayerMoveなどから簡単に参照できるようにするため)
    public static GameCamera Instance { get; private set; }

    void Awake()
    {
        // シングルトンインスタンスの設定
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // カメラが常に存在する必要がなければ不要。シーンごとに配置する場合はコメントアウトのまま。
        }
        else
        {
            Destroy(gameObject); // 既にインスタンスが存在する場合は、新しい方を破棄
        }
    }

    void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // ClearSceneではプレイヤー追尾しない
        if (currentSceneName != "ClearScene")
        {
            m_player = GameObject.FindGameObjectWithTag("Player"); // "Player"タグのオブジェクトを検索

            if (m_player != null)
            {
                // ゲーム開始時にカメラをプレイヤーの初期位置に合わせる
                CameraUpdate();
            }
            else
            {
                UnityEngine.Debug.LogError("Playerタグのゲームオブジェクトが見つかりません！カメラ追従ができません。");
            }
        }
    }

    void LateUpdate()
    {
        // LateUpdateでカメラを更新することで、プレイヤーの移動後にカメラが追従し、滑らかな動きになります。
        if (m_player == null) return;

        // カメラの追従ロジックを実行
        CameraUpdate();
    }

    /// <summary>
    /// カメラの位置を更新し、設定された範囲内でクランプします。
    /// </summary>
    void CameraUpdate()
    {
        if (m_player == null) return;

        // プレイヤーの位置にオフセットを加えた目標位置を計算
        Vector3 targetPos = m_player.transform.position + CameraAddPos;

        // X座標の制限
        if (UseClampX)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, CameraMinPos.x, CameraMaxPos.x);
        }

        // Y座標の制限
        if (UseClampY)
        {
            targetPos.y = Mathf.Clamp(targetPos.y, CameraMinPos.y, CameraMaxPos.y);
        }

        // カメラの位置を目標位置に設定
        transform.position = targetPos;
    }

    // ★以前の StopCameraFollow() と StartCameraFollow() は、このクランプ方式では不要なため削除しました。
}
