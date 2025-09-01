using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ジャンプボタンUIのタッチイベントを処理し、プレイヤーのジャンプを呼び出すスクリプト。
/// </summary>
public class JumpButtonController : MonoBehaviour, IPointerDownHandler
{
    private PlayerMove m_playerMove;

    /// <summary>
    /// GameManagerからPlayerMoveの参照を受け取るためのメソッド
    /// </summary>
    public void SetPlayerMove(PlayerMove player)
    {
        m_playerMove = player;
    }

    /// <summary>
    /// ボタンがタップされたときに呼び出されます。
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // プレイヤーオブジェクトが存在し、アクティブな場合のみジャンプを呼び出す
        if (m_playerMove != null && m_playerMove.gameObject.activeInHierarchy)
        {
            m_playerMove.Jump();
        }
    }
}