using TMPro; // ★この行を追加★
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gemCountText; // TextMeshUGUI を TextMeshProUGUI に変更

    private void Awake()
    {
        if (gemCountText == null)
        {
            Debug.LogError("ScoreDisplay: gemCountText が割り当てられていません！", this);
        }
    }

    private void OnEnable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGemCountChanged += UpdateGemCount;
            UpdateGemCount(GameManager.instance.currentGemCount); // 初期値を設定
        }
    }

    private void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGemCountChanged -= UpdateGemCount;
        }
    }

    public void UpdateGemCount(int newCount)
    {
        if (gemCountText != null)
        {
            gemCountText.text = "Gems: " + newCount;
        }
    }
}