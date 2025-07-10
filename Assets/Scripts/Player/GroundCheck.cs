using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GroundCheck : MonoBehaviour
{
    // 接地しているかを格納する変数
    bool m_isGround;
    // 地面に触れているかを返す関数
    public bool GetIsGround()
    {
        return m_isGround;
    }


    // 毎フレーム最初に接地判定をリセットする
    private void FixedUpdate()
    {
        m_isGround = false;
    }


    // 2Dがつくので注意！
    private void OnTriggerStay2D(Collider2D collision)
    {
        // 地面のタグが付いたオブジェクトに衝突している
        if (collision.CompareTag("Ground"))
        {
            m_isGround = true;
        }
    }
}