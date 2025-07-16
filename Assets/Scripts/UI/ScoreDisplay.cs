using TMPro; // ★この行が正確に記述されていることを確認してください！★
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    // TextMeshProUGUI を使用する場合、Inspectorで割り当てる
    [SerializeField] private TextMeshUGUI gemCountText;

    void Awake()
    {
        // Inspectorで割り当てられていない場合、シーン内の "GemCountText" という名前のオブジェクトから取得を試みる
        if (gemCountText == null)
        {
            gemCountText = GameObject.Find("GemCountText")?.GetComponent<TextMeshUGUI>();

            if (gemCountText == null)
            {
                Debug.LogError("ScoreDisplay: GemCountText (TextMeshProUGUI) が見つかりません。Inspectorで設定するか、GameObjectの名前を確認してください。");
            }
        }
    }

    // ジェム数を更新するメソッド
    public void UpdateGemCount(int count)
    {
        if (gemCountText != null)
        {
            gemCountText.text = "Gems: " + count.ToString();
        }
        else
        {
            Debug.LogWarning("ScoreDisplay: gemCountText がnullのため、ジェム数を更新できません。");
        }
    }
}