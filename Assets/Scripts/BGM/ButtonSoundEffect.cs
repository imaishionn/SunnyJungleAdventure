using UnityEngine;
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

[RequireComponent(typeof(AudioSource))] // AudioSourceコンポーネントが必須であることを示す
public class ButtonSoundEffect : MonoBehaviour
{
    [Tooltip("ボタンクリック時に再生する効果音")]
    public AudioClip clickSound;

    // 効果音の音量 (0から1の範囲でInspectorから調整可能)
    [SerializeField, Range(0f, 1f), Header("効果音音量")]
    private float sfxVolume = 0.7f; // デフォルト値を0.7に設定

    private AudioSource audioSource;

    void Awake()
    {
        // 同じGameObjectにアタッチされているAudioSourceを取得
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // 自動再生しない
        audioSource.loop = false; // ループしない（効果音のため）
    }

    /// <summary>
    /// UI Button の OnClick イベントから呼び出すメソッド。
    /// 設定されたクリック音を一度だけ再生します。
    /// </summary>
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            // PlayOneShotの第2引数で音量を指定
            audioSource.PlayOneShot(clickSound, sfxVolume);
            Debug.Log($"ButtonSoundEffect: '{clickSound.name}' を音量 {sfxVolume} で再生しました。");
        }
        else
        {
            Debug.LogWarning($"ButtonSoundEffect: '{gameObject.name}' の clickSound が設定されていません。");
        }
    }
}
