using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ゲームクリア後のシーンで、UIやシーン遷移を制御するスクリプトです。
/// </summary>
public class ClearSceneController : MonoBehaviour {
    [Header("ゲームパッド操作で最初に選択状態にしたいUI要素"), SerializeField]
    private Selectable _firstSelected;

    [Header("スコアシーンの名前"), SerializeField]
    private string _resultsceneName = "Resultscene"; 

    private bool _isTransitioning = false;

    private void Start() {
        if (_firstSelected != null) {
            EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
        }
        else {
            Debug.LogWarning("ClearSceneController: firstSelectedが割り当てられていません。", this);
        }

        _isTransitioning = false;
    }

    private void Update() {
        if (_isTransitioning) {
            return;
        }

        if (Input.GetButtonDown("Submit")) {
            OnClickGoToScoreScene();
        }
    }

    /// <summary>
    /// スコア画面へ進むボタンが押された際に呼び出されます。
    /// </summary>
    public void OnClickGoToScoreScene() {
        if (_isTransitioning) {
            return;
        }

        _isTransitioning = true;

        if (GameManager.Instance != null) {
            // GameManagerを使ってシーンをフェード付きでロード
            GameManager.Instance.LoadSceneWithFade(_resultsceneName);
        }
        else {
            Debug.LogError("ClearSceneController: GameManager インスタンスが見つかりません！直接シーンをロードします。", this);
            // 直接シーンをロード
            SceneManager.LoadScene(_resultsceneName);
        }
    }
}
