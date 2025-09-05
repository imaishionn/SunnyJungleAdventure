using UnityEngine;

/// <summary>
/// サウンド再生
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour {
    [Tooltip("空なら同じ GameObject の AudioSource を自動取得"), SerializeField]
    private AudioSource _audioSource;

    [Tooltip("スタートボタンを押したときに鳴る効果音"), SerializeField]
    private AudioClip _geMusutatoClip;

    private void Awake() {
        if(_audioSource == null) {
            _audioSource = GetComponent<AudioSource>();
        }

        _audioSource.playOnAwake = false;
    }

    /// <summary>
    /// UIButtonのOnClickから呼ぶメソッド
    /// </summary>
    public void PlayGeMusutato() {
        if(_geMusutatoClip != null) {
            _audioSource.PlayOneShot(_geMusutatoClip);
        }
        else {
            Debug.LogWarning("[SoundPlayer] geMusutatoClip が設定されていません");
        }
    }
}
