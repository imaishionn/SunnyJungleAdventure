using UnityEngine;

/// <summary>
/// サウンド再生
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour {
    [Tooltip("空なら同じ GameObject の AudioSource を自動取得"), SerializeField]
    private AudioSource audioSource;

    [Tooltip("スタートボタンを押したときに鳴る効果音"), SerializeField]
    private AudioClip geMusutatoClip;

    void Awake() {
        if(audioSource == null) {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// UIButtonのOnClickから呼ぶメソッド
    /// </summary>
    public void PlayGeMusutato() {
        if(geMusutatoClip != null) {
            audioSource.PlayOneShot(geMusutatoClip);
        }
        else {
            Debug.LogWarning("[SoundPlayer] geMusutatoClip が設定されていません");
        }
    }
}
