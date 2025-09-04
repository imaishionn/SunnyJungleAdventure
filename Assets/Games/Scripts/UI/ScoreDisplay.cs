using TMPro;
using UnityEngine;

/// <summary>
/// ゲーム内のジェムカウントをUIに表示する役割を担うスクリプトです。
/// GameManagerから通知を受け取り、表示を更新します。
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    [Header("UI要素"), Tooltip("ジェムのカウントを表示するTextMeshProUGUIコンポーネント"), SerializeField]
    private TextMeshProUGUI gemCountText;

    private void Awake()
    {
        // 必要なUI要素が割り当てられているか確認
        if (gemCountText == null)
        {
            Debug.LogError("ScoreDisplay: gemCountText がインスペクターで割り当てられていません！", this);
        }
    }

    /// <summary>
    /// このコンポーネントが有効になったときに呼び出されます。
    /// </summary>
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGemCountChanged += UpdateGemCount;
            UpdateGemCount(GameManager.Instance.currentGemCount);
        }
    }

    /// <summary>
    /// このコンポーネントが無効になったときに呼び出されます。
    /// </summary>
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            // イベントの購読を解除。メモリリークを防ぐための重要な処理です。
            GameManager.Instance.OnGemCountChanged -= UpdateGemCount;
        }
    }

    /// <summary>
    /// ジェムのカウントをUIに表示するテキストを更新します。
    /// このメソッドはGameManagerのイベントから呼び出されます。
    /// </summary>
    /// <param name="newCount">新しいジェムの数</param>
    public void UpdateGemCount(int newCount)
    {
        if (gemCountText != null)
        {
            // テキストを短くして1行に収まるように変更
            gemCountText.text = ":" + newCount;
        }
    }
}