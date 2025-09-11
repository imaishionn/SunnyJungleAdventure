using UnityEngine;

/// <summary>
/// プレイヤーが触れるとスコアに加算される宝石の挙動を制御します。
/// </summary>
public class Gem : MonoBehaviour {
    [Header("スコア設定"), SerializeField]
    private int _scoreValue = 1; // 宝石を回収したときに加算されるスコア値

    private bool _isCollected = false; // 既に回収されたかどうかのフラグ
    private static ItemSoundPlayer _itemSoundPlayer; // シングルトン化されたItemSoundPlayerへの参照をキャッシュ

    private void Awake() {
        if (_itemSoundPlayer == null) {
            _itemSoundPlayer = FindObjectOfType<ItemSoundPlayer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (_isCollected) {
            return;
        }

        if (other.CompareTag("Player")) {
            _isCollected = true;

            if (GameManager.Instance != null) {
                GameManager.Instance.AddScore(_scoreValue);
            }

            if (_itemSoundPlayer != null) {
                _itemSoundPlayer.PlayGemSound();
            }

            if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) {
                sr.enabled = false;
            }

            if (TryGetComponent<Collider2D>(out Collider2D col)) {
                col.enabled = false;
            }

            Destroy(gameObject, 1.0f);
        }
    }
}
