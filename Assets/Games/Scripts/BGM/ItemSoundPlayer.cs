using UnityEngine;

/// <summary>
/// アイテム取得やアクションに応じた効果音を再生
/// </summary>
public class ItemSoundPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("宝石獲得音")]
    public AudioClip gemClip;

    [Header("ジャンプ音")]
    public AudioClip jumpClip;

    [Header("ゲームオーバー音")]
    public AudioClip gameOverClip;

    [Header("敵撃破音")]
    public AudioClip enemyDefeatClip;

    // ★追加: 敵撃破音とゲームオーバー音の音量設定
    [Header("音量設定")]
    [Tooltip("敵撃破音の再生音量")]
    [Range(0f, 1f)]
    [SerializeField] private float enemyDefeatVolume = 1.0f;
    [Tooltip("ゲームオーバー音の再生音量")]
    [Range(0f, 1f)]
    [SerializeField] private float gameOverVolume = 1.0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("ItemSoundPlayer: AudioSourceコンポーネントがアタッチされていません。");
        }
    }

    public void PlayGemSound()
    {
        if (audioSource != null && gemClip != null)
        {
            audioSource.PlayOneShot(gemClip);
        }
    }

    public void PlayJumpSound()
    {
        if (audioSource != null && jumpClip != null)
        {
            audioSource.PlayOneShot(jumpClip);
        }
    }

    public void PlayGameOverSound()
    {
        if (audioSource != null && gameOverClip != null)
        {
            // ★修正: gameOverVolumeを引数として渡す
            audioSource.PlayOneShot(gameOverClip, gameOverVolume);
        }
    }

    public void PlayEnemyDefeatSound()
    {
        if (audioSource != null && enemyDefeatClip != null)
        {
            // ★修正: enemyDefeatVolumeを引数として渡す
            audioSource.PlayOneShot(enemyDefeatClip, enemyDefeatVolume);
        }
    }
}