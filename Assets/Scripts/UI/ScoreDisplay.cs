using TMPro; // �����̍s�����m�ɋL�q����Ă��邱�Ƃ��m�F���Ă��������I��
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    // TextMeshProUGUI ���g�p����ꍇ�AInspector�Ŋ��蓖�Ă�
    [SerializeField] private TextMeshUGUI gemCountText;

    void Awake()
    {
        // Inspector�Ŋ��蓖�Ă��Ă��Ȃ��ꍇ�A�V�[������ "GemCountText" �Ƃ������O�̃I�u�W�F�N�g����擾�����݂�
        if (gemCountText == null)
        {
            gemCountText = GameObject.Find("GemCountText")?.GetComponent<TextMeshUGUI>();

            if (gemCountText == null)
            {
                Debug.LogError("ScoreDisplay: GemCountText (TextMeshProUGUI) ��������܂���BInspector�Őݒ肷�邩�AGameObject�̖��O���m�F���Ă��������B");
            }
        }
    }

    // �W�F�������X�V���郁�\�b�h
    public void UpdateGemCount(int count)
    {
        if (gemCountText != null)
        {
            gemCountText.text = "Gems: " + count.ToString();
        }
        else
        {
            Debug.LogWarning("ScoreDisplay: gemCountText ��null�̂��߁A�W�F�������X�V�ł��܂���B");
        }
    }
}