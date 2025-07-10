using UnityEngine;
using Debug = UnityEngine.Debug;                   // Debug は UnityEngine 側を使用

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    [Tooltip("空なら同じ GameObject の AudioSource を自動取得")]
    public AudioSource audioSource;

    [Tooltip("スタートボタンを押したときに鳴る効果音")]
    public AudioClip geMusutatoClip;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    /// <summary>UI Button の OnClick から呼ぶメソッド</summary>
    public void PlayGeMusutato()
    {
        if (geMusutatoClip != null)
            audioSource.PlayOneShot(geMusutatoClip);
        else
            Debug.LogWarning("[SoundPlayer] geMusutatoClip が設定されていません");
    }
}
