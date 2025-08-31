using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// ゲームがモバイルプラットフォームで実行されている場合に、UIを有効/無効にするスクリプト。
/// </summary>
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
            // それ以外（PCなど）の場合はUIを無効にする
            gameObject.SetActive(false);
        }
    }
}
