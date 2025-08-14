using System.Collections;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("スコア設定")]
    [SerializeField] private int scoreValue = 1;

    // Gem.csからはAudioSourceとAudioClipの参照を削除
    private bool m_isCollected = false;

    // Awake()からAudioSource関連のコードを削除
    void Awake()
    {
        // AudioSource関連のコードは不要になる
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_isCollected) return;

        if (other.CompareTag("Player"))
        {
            m_isCollected = true;

            if (GameManager.instance != null)
            {
                GameManager.instance.AddGem(scoreValue);
            }

            // ★修正点1: ItemSoundPlayerをシーンから見つける
            ItemSoundPlayer itemSoundPlayer = FindObjectOfType<ItemSoundPlayer>();
            if (itemSoundPlayer != null)
            {
                // ★修正点2: ItemSoundPlayerに音の再生を依頼
                itemSoundPlayer.PlayGemSound();
            }

            // コライダーを無効化
            if (TryGetComponent<Collider2D>(out Collider2D col))
            {
                col.enabled = false;
            }

            // スプライトを非表示にする
            if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
            {
                sr.enabled = false;
            }

            // オブジェクトをすぐに破壊する
            Destroy(gameObject);
        }
    }
}