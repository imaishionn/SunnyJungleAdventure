using UnityEngine;
using Debug = UnityEngine.Debug;

public class MobileUIController : MonoBehaviour
{
    private void Awake()
    {
        // ゲームがモバイルプラットフォームで実行されているか確認する
        if (UnityEngine.Application.isMobilePlatform)
        {
            // モバイルの場合はUIを有効にする
            gameObject.SetActive(true);
        }
        else
        {
            // それ以外の場合はUIを無効にする
            gameObject.SetActive(false);
        }
    }
}