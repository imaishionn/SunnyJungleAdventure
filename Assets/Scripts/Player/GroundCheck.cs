using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GroundCheck : MonoBehaviour
{
    // �ڒn���Ă��邩���i�[����ϐ�
    bool m_isGround;
    // �n�ʂɐG��Ă��邩��Ԃ��֐�
    public bool GetIsGround()
    {
        return m_isGround;
    }


    // ���t���[���ŏ��ɐڒn��������Z�b�g����
    private void FixedUpdate()
    {
        m_isGround = false;
    }


    // 2D�����̂Œ��ӁI
    private void OnTriggerStay2D(Collider2D collision)
    {
        // �n�ʂ̃^�O���t�����I�u�W�F�N�g�ɏՓ˂��Ă���
        if (collision.CompareTag("Ground"))
        {
            m_isGround = true;
        }
    }
}