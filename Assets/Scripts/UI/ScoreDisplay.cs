using TMPro; // TextMeshProUGUIを使用するために追加
using UnityEngine;

/// <summary>
/// ゲーム内のジェムカウントをUIに表示する役割を担うスクリプトです。
/// GameManagerから通知を受け取り、表示を更新します。
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するUI要素
    // ----------------------------------------------------------------------------------------------------
    [Header("UI要素")]
    [Tooltip("ジェムのカウントを表示するTextMeshProUGUIコンポーネント")]
    [SerializeField] private TextMeshProUGUI gemCountText;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
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
        if (GameManager.instance != null)
        {
            // GameManagerのOnGemCountChangedイベントに、スコア更新メソッドを購読
            GameManager.instance.OnGemCountChanged += UpdateGemCount;

            // スクリプトが有効になった時点で、現在のジェム数を表示に反映
            UpdateGemCount(GameManager.instance.currentGemCount);
        }
    }

    /// <summary>
    /// このコンポーネントが無効になったときに呼び出されます。
    /// </summary>
    private void OnDisable()
    {
        if (GameManager.instance != null)
        {
            // イベントの購読を解除。メモリリークを防ぐための重要な処理です。
            GameManager.instance.OnGemCountChanged -= UpdateGemCount;
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // パブリックメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// ジェムのカウントをUIに表示するテキストを更新します。
    /// このメソッドはGameManagerのイベントから呼び出されます。
    /// </summary>
    /// <param name="newCount">新しいジェムの数</param>
    public void UpdateGemCount(int newCount)
    {
        if (gemCountText != null)
        {
            gemCountText.text = "Gems: " + newCount;
        }
    }
}