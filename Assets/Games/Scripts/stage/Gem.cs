using UnityEngine;

/// <summary>
/// プレイヤーが触れるとスコアに加算される宝石の挙動を制御します。
/// </summary>
public class Gem : MonoBehaviour {
    [Header("スコア設定")]
    [SerializeField] private int scoreValue = 1;

    // 既に回収されたかどうかのフラグ
    private bool m_isCollected = false;

    // シングルトン化されたItemSoundPlayerへの参照をキャッシュ
    private static ItemSoundPlayer s_itemSoundPlayer;

    private void Awake() {
        // GameManagerと同様に、ItemSoundPlayerがシングルトンであることを前提とする
        if(s_itemSoundPlayer == null) {
            s_itemSoundPlayer = FindObjectOfType<ItemSoundPlayer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // 既に回収済みの場合は何もしない
        if(m_isCollected) return;

        // プレイヤーに触れたかチェック
        if(other.CompareTag("Player")) {
            m_isCollected = true;

            // GameManagerにスコアを加算
            // GameManager.instance を GameManager.Instance に修正
            if(GameManager.Instance != null) {
                // GameManager.instance.AddGem(scoreValue) を GameManager.Instance.AddGem(scoreValue) に修正
                GameManager.Instance.AddGem(scoreValue);
            }

            // 効果音を再生
            if(s_itemSoundPlayer != null) {
                s_itemSoundPlayer.PlayGemSound();
            }

            // オブジェクトの見た目を即座に非表示にする
            if(TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) {
                sr.enabled = false;
            }

            // コライダーを無効化
            if(TryGetComponent<Collider2D>(out Collider2D col)) {
                col.enabled = false;
            }

            // 効果音の再生が完了した後にオブジェクトを破棄
            // ここではオーディオクリップの長さを取得して待機するのが理想的
            // 例として1秒後に破棄する
            Destroy(gameObject,1.0f);
        }
    }
}