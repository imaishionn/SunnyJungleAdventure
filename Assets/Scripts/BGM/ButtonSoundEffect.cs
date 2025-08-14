using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Debugの曖昧な参照を解消するため、UnityEngine.Debugを明示的に指定
using Debug = UnityEngine.Debug;

// もしスクリプトのどこかに以下の行がある場合、コメントアウトまたは削除してください
// using System.Diagnostics;

public class ButtonSoundEffect : MonoBehaviour, ISelectHandler // IPointerEnterHandler, IPointerExitHandler を削除
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    private AudioSource m_audioSource;

    void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource == null)
        {
            m_audioSource = gameObject.AddComponent<AudioSource>();
            m_audioSource.playOnAwake = false;
            m_audioSource.loop = false;
        }

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(PlayClickSound);
        }
        else
        {
            Debug.LogWarning("ButtonSoundEffect: Buttonコンポーネントが見つかりません。", this);
        }
    }

    public void PlayClickSound()
    {
        if (m_audioSource != null && m_audioSource.isActiveAndEnabled && clickSound != null)
        {
            m_audioSource.PlayOneShot(clickSound);
        }
        else
        {
            string debugMsg = "ButtonSoundEffect: クリック音を再生できませんでした。";
            if (m_audioSource == null) debugMsg += " AudioSourceがnullです。";
            else if (!m_audioSource.isActiveAndEnabled) debugMsg += " AudioSourceが無効または非アクティブです。";
            else if (clickSound == null) debugMsg += " クリック音のAudioClipが割り当てられていません。";
            Debug.LogWarning(debugMsg, this);
        }
    }

    public void PlayHoverSound()
    {
        if (m_audioSource != null && m_audioSource.isActiveAndEnabled && hoverSound != null)
        {
            m_audioSource.PlayOneShot(hoverSound);
        }

    }

    public void OnSelect(BaseEventData eventData)
    {
        PlayHoverSound();

    }
}