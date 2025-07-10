using UnityEngine;

public class BG_Scroll : MonoBehaviour
{
    // �v���C���[�ւ̎Q��
    // Inspector��Player�I�u�W�F�N�g�����蓖�Ă邩�AAwake�Ŏ����I�Ɏ擾����
    [SerializeField] PlayerMove m_playerMove;

    // �ړ����x�␳ (�傫���قǒx���Ȃ�)
    // �[�����Z��h�����߁A�f�t�H���g�l��1.0f�ɐݒ�
    [SerializeField, Header("�ړ����x�␳ (�傫���قǒx���Ȃ�)")]
    float Division = 1.0f; // ���C��: �f�t�H���g�l��0.0f����1.0f�ɕύX��

    void Start()
    {
        // PlayerMove��Inspector�Ŋ��蓖�Ă��Ă��Ȃ��ꍇ�A�^�O�Ō������Ď擾
        if (m_playerMove == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                m_playerMove = playerObject.GetComponent<PlayerMove>();
            }
            if (m_playerMove == null)
            {
                UnityEngine.Debug.LogError("BG_Scroll: PlayerMove�R���|�[�l���g��������܂���BPlayer�I�u�W�F�N�g�ɃA�^�b�`����Ă��邩�AInspector�Ŋ��蓖�Ă��Ă��邩�m�F���Ă��������B", this);
            }
        }

        // Division��0�̏ꍇ�̌x���ƏC��
        if (Division == 0.0f)
        {
            UnityEngine.Debug.LogWarning("BG_Scroll: Division�̒l��0�ł��B�[�����Z��h�����߁A1.0f�ɐݒ肵�܂��B");
            Division = 1.0f;
        }
    }

    void Update()
    {
        if (m_playerMove == null || Division == 0.0f)
        {
            // �K�v�ȎQ�Ƃ��Ȃ����ADivision��0�̏ꍇ�͏������X�L�b�v
            return;
        }

        // �v���C���[�̈ړ����x�ɉ����Ĕw�i���X�N���[��������
        // PlayerMove�X�N���v�g�� 'moveSpeed' �t�B�[���h�ɃA�N�Z�X
        // �w�i�̓v���C���[�̈ړ������Ƌt�����ɓ������߁A���̒l��K�p
        float move = (-m_playerMove.moveSpeed / Division) * Time.deltaTime; // ���C��: MoveSpeed -> moveSpeed, ����ѕ����𔽓]��

        // �w�i���ړ�����
        transform.Translate(new Vector3(move, 0.0f, 0.0f));
    }
}
