using TMPro;
using UnityEngine;

/// <summary>
/// ゲーム内のジェムカウントをUIに表示する役割を担うスクリプトです。
/// GameManagerから通知を受け取り、表示を更新します。
/// </summary>
public class ScoreDisplay : MonoBehaviour {


    [Header("スコアのUI要素"), SerializeField]
    private TextMeshProUGUI _gemCountText;

    private void Awake() {
        if (_gemCountText == null) {
            Debug.LogError("ScoreDisplay: gemCountText がインスペクターで割り当てられていません！", this);
        }
    }

    private void OnEnable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGemCountChanged += UpdateGemCount;
            UpdateGemCount(GameManager.Instance.CurrentGemCount);
        }
    }

    private void OnDisable() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGemCountChanged -= UpdateGemCount;
        }
    }

    /// <summary>
    /// ジェムのカウントをUIに表示するテキストを更新します。
    /// このメソッドはGameManagerのイベントから呼び出されます。
    /// </summary>
    /// <param name="newCount">新しいジェムの数</param>
    public void UpdateGemCount(int newCount) {
        if (_gemCountText != null) {
            _gemCountText.text = $"{newCount}";
        }
    }
}
