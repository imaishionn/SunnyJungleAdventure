using System.Collections; // Coroutine�̂��߂ɕK�v
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("�ړ��ݒ�")]
    [Tooltip("�ړ����x")]
    [SerializeField] private float moveSpeed = 2.0f;

    [Tooltip("�ړI�n�ɓ��B���Ă��玟�̈ړ����J�n����܂ł̑ҋ@����")]
    [SerializeField] private float waitTimeAtPoint = 1.0f;

    [Tooltip("X�������ւ̈ړ����� (���̒l�ŉE�A���̒l�ō�)")]
    [SerializeField] private float moveDistanceX = 0f; // ��: 5.0f

    [Tooltip("Y�������ւ̈ړ����� (���̒l�ŏ�A���̒l�ŉ�)")]
    [SerializeField] private float moveDistanceY = 0f; // ��: 3.0f

    private Vector3 initialPosition; // ����̏����ʒu
    private Vector3 targetPosition;  // ���݂̖ڕW�n�_ (�����ʒu�܂��͏I�_)
    private Vector3 endOffsetPosition; // �����ʒu����̃I�t�Z�b�g�����Z�����I�_

    private bool movingToEnd = true; // true�Ȃ�I�t�Z�b�g�n�_�ցAfalse�Ȃ珉���ʒu�ֈړ���

    void Start()
    {
        initialPosition = transform.position; // �X�N���v�g�J�n���̈ʒu�������ʒu�Ƃ���
        endOffsetPosition = initialPosition + new Vector3(moveDistanceX, moveDistanceY, 0f); // �I�_���v�Z

        targetPosition = endOffsetPosition; // �ŏ��͏I�_�֌�����
        StartCoroutine(MovePlatform()); // �R���[�`���ňړ����J�n
    }

    // ����̈ړ��𐧌䂷��R���[�`��
    private IEnumerator MovePlatform()
    {
        while (true) // �������[�v�ŉ����ړ��𑱂���
        {
            // �ڕW�n�_�Ɍ������Ĉړ�
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null; // 1�t���[���ҋ@
            }

            // �ڕW�n�_�ɓ��B������ҋ@
            yield return new WaitForSeconds(waitTimeAtPoint);

            // ���̖ڕW�n�_��ݒ�i�ړ������𔽓]�j
            movingToEnd = !movingToEnd;
            if (movingToEnd)
            {
                targetPosition = endOffsetPosition;
            }
            else
            {
                targetPosition = initialPosition;
            }
        }
    }

    // �v���C���[������ɏ�������̏���
    private void OnTriggerEnter2D(Collider2D other)
    {
        // �v���C���[�̃^�O���`�F�b�N (��: "Player")
        if (other.CompareTag("Player"))
        {
            // �v���C���[�𑫏�̎q�I�u�W�F�N�g�ɂ���
            // ����ɂ��A���ꂪ�ړ�����ƃv���C���[���ꏏ�Ɉړ�����
            other.transform.SetParent(transform);
        }
    }

    // �v���C���[�����ꂩ�痣�ꂽ���̏���
    private void OnTriggerExit2D(Collider2D other)
    {
        // �v���C���[�̃^�O���`�F�b�N (��: "Player")
        if (other.CompareTag("Player"))
        {
            // �v���C���[�̐e����������
            // null��ݒ肷��ƁA�V�[���̃��[�g�ɖ߂�
            other.transform.SetParent(null);
        }
    }

    // Scene�r���[�ňړ��o�H����������
    void OnDrawGizmos()
    {
        // �G�f�B�^���s���łȂ��ꍇ�A�܂��͏����ʒu���܂��ݒ肳��Ă��Ȃ��ꍇ�͕`�悵�Ȃ�
        if (!Application.isPlaying)
        {
            initialPosition = transform.position;
            endOffsetPosition = initialPosition + new Vector3(moveDistanceX, moveDistanceY, 0f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(initialPosition, endOffsetPosition);
        Gizmos.DrawWireSphere(initialPosition, 0.2f); // �J�n�n�_
        Gizmos.DrawWireSphere(endOffsetPosition, 0.2f); // �I���n�_
    }
}
