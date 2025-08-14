using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class ItemSoundPlayer : MonoBehaviour
{
    // ★修正点: AudioSourceコンポーネントの参照を自動で取得するようにする
    private AudioSource audioSource;

    [Header("宝石獲得音")]
    public AudioClip gemClip;

    [Header("ジャンプ音")]
    public AudioClip jumpClip;

    [Header("ゲームオーバー音")]
    public AudioClip gameOverClip;

    [Header("敵撃破音")]
    public AudioClip enemyDefeatClip;

    // ★修正点: Awakeメソッドを追加してAudioSourceを取得する
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
            audioSource.PlayOneShot(gameOverClip);
        }
    }

    public void PlayEnemyDefeatSound()
    {
        if (audioSource != null && enemyDefeatClip != null)
        {
            audioSource.PlayOneShot(enemyDefeatClip);
        }
    }
}