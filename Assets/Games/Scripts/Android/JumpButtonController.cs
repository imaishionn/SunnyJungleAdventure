using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ジャンプボタンUIのタッチイベントを処理し、プレイヤーのジャンプを呼び出すスクリプト。
/// </summary>
public class JumpButtonController : MonoBehaviour, IPointerDownHandler
{
    /// <summary>
    /// ボタンがタップされたときに呼び出されます。
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // GameManagerのインスタンスとプレイヤーの参照が有効か確認
        if (GameManager.Instance != null && GameManager.Instance.GetPlayerMove() != null)
        {
            // GameManager経由でプレイヤーのジャンプ関数を呼び出す
            GameManager.Instance.GetPlayerMove().Jump();
        }
    }
}
