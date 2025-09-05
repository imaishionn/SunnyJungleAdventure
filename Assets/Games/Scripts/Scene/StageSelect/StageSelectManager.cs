using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// ステージ選択画面のUIとロジックを管理するクラス。
/// </summary>
public class StageSelectManager : MonoBehaviour {
    // === インスペクターから設定するステージボタン ===
    [SerializeField] private Button[] _stageButtons;
    [SerializeField] private Button _nextButton;
    [SerializeField] private GameObject _stageSelectPage;

    // === 新しく追加されたクリア表示UI ===
    [Header("クリア表示UI")]
    [Tooltip("各ステージに対応する「クリア」表示のゲームオブジェクト")]
    [SerializeField] private GameObject[] _clearIndicators;

    private void Start() =>
        // シーンロード時に、すべてのボタンをアクティブ（表示）にし、クリア表示を更新する
        UpdateButtonsAndClearIndicators();

    private void UpdateButtonsAndClearIndicators() {
        // GameManagerが存在するか確認
        if (GameManager.Instance == null) {
            Debug.LogError("UpdateButtonsAndClearIndicators: GameManagerが見つかりません。クリア表示を更新できません。", this);
            return;
        }

        // 意図的にすべてのボタンを常に表示状態にする
        foreach (Button button in _stageButtons) {
            if (button != null) {
                button.gameObject.SetActive(true);
            }
        }

        if (_nextButton != null) {
            _nextButton.gameObject.SetActive(true);
        }

        // クリア表示の配列がステージボタンの配列と同じサイズか確認
        if (_clearIndicators.Length != _stageButtons.Length) {
            Debug.LogWarning("UpdateButtonsAndClearIndicators: クリア表示とステージボタンの配列の数が一致しません。クリア表示が正しく動作しない可能性があります。", this);
        }

        // ステージクリア情報を確認し、クリアしたステージのクリア表示を有効にする
        for (int i = 0; i < _stageButtons.Length; i++) {
            // ステージインデックスがGameManagerのステージ配列内に存在するか確認
            if (i < GameManager.Instance.StageSceneNames.Length) {
                bool isClear = GameManager.Instance.IsStageClear(i);
                if (_clearIndicators.Length > i && _clearIndicators[i] != null) {
                    _clearIndicators[i].SetActive(isClear);
                }
            }
            else {
                // インデックス範囲外の場合、クリア表示を非アクティブにする
                if (_clearIndicators.Length > i && _clearIndicators[i] != null) {
                    _clearIndicators[i].SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// ステージボタンがクリックされたときに呼び出されるメソッド
    /// </summary>
    /// <param name="stageIndex">クリックされたステージのインデックス</param>
    public void OnStageButtonClicked(int stageIndex) {
        if (GameManager.Instance != null && stageIndex < GameManager.Instance.StageSceneNames.Length) {
            string sceneName = GameManager.Instance.StageSceneNames[stageIndex];
            GameManager.Instance.LoadSceneWithFade(sceneName);
        }
        else if (GameManager.Instance == null) {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！シーン遷移できません。", this);
        }
    }

    /// <summary>
    /// NEXTボタンがクリックされたときに呼び出されるメソッド
    /// </summary>
    public void OnNextButtonClicked() {
        if (GameManager.Instance != null) {
            // NEXTボタンはStageSelect2Sceneへ遷移
            GameManager.Instance.LoadSceneWithFade("StageSelect2Scene");
        }
        else {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！フェードなしで次のステージ選択画面に遷移します。", this);
            SceneManager.LoadScene("StageSelect2Scene");
        }
    }
}
