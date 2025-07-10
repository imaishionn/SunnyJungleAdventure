using UnityEngine;
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

/// <summary>
/// アイテム獲得時やジャンプ時などの効果音を再生するためのスクリプト。
/// シーンに一つだけ存在し、各オブジェクトから呼び出されます。
/// </summary>
public class ItemSoundPlayer : MonoBehaviour
{
    // AudioSourceコンポーネントはInspectorで割り当てる必要があります
    public AudioSource audioSource;

    [Header("宝石獲得音")]
    public AudioClip gemClip;

    [Header("ジャンプ音")]
    public AudioClip jumpClip;  // ジャンプ音用クリップ

    /// <summary>
    /// 宝石獲得音を再生します。
    /// 同じクリップを2回再生していますが、意図的でなければ1回に減らすことを検討してください。
    /// </summary>
    public void PlayGemSound()
    {
        if (audioSource != null && gemClip != null)
        {
            audioSource.PlayOneShot(gemClip);
            audioSource.PlayOneShot(gemClip, 0.4f); // 2回目の再生 (音量0.4f)
            Debug.Log("ItemSoundPlayer: 宝石獲得音を再生しました。");
        }
        else
        {
            if (audioSource == null) Debug.LogWarning("ItemSoundPlayer: AudioSourceが割り当てられていません。");
            if (gemClip == null) Debug.LogWarning("ItemSoundPlayer: 宝石獲得音クリップが割り当てられていません。");
        }
    }

    /// <summary>
    /// ジャンプ音を再生します。
    /// </summary>
    public void PlayJumpSound()
    {
        if (audioSource != null && jumpClip != null)
        {
            audioSource.PlayOneShot(jumpClip);
            Debug.Log("ItemSoundPlayer: ジャンプ音を再生しました。");
        }
        else
        {
            if (audioSource == null) Debug.LogWarning("ItemSoundPlayer: AudioSourceが割り当てられていません。");
            if (jumpClip == null) Debug.LogWarning("ItemSoundPlayer: ジャンプ音クリップが割り当てられていません。");
        }
    }
}
