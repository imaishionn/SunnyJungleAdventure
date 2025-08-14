using UnityEngine;

public class StompCheck : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 敵のコライダーと接触したら、親オブジェクト（プレイヤー）に処理を委ねる
        if (other.gameObject.CompareTag("Enemy"))
        {
            PlayerMove playerMove = transform.parent.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                playerMove.StompEnemy(other.gameObject);
            }
        }
    }
}