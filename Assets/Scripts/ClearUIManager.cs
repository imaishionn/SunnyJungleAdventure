using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearUIManager : MonoBehaviour
{
    public void OnTitleButton()
    {
        Time.timeScale = 1f; // ŽžŠÔ‚ð–ß‚·
        SceneFader.instance.FadeOutToScene("TitleScene");
    }
}
