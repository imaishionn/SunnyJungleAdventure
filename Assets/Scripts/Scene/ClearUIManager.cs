using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearUIManager : MonoBehaviour
{
    public void OnTitleButton()
    {
        Time.timeScale = 1f; // ���Ԃ�߂�
        SceneFader.instance.FadeOutToScene("TitleScene");
    }
}
