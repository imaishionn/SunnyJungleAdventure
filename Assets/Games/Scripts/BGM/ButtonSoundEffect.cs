using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UIボタンにクリック音とホバー音（選択音）を付けるためのスクリプトです。
/// ISelectHandlerインターフェースを実装し、UIのイベントを処理します。
/// </summary>
public class ButtonSoundEffect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("サウンド設定")]
    [Tooltip("ボタンがクリックされたときに再生する効果音")]
    [SerializeField] private AudioClip clickSound;
    [Tooltip("ボタンにカーソルが乗ったとき、または選択されたときに再生する効果音")]
    [SerializeField] private AudioClip hoverSound;
    [Tooltip("クリック音の音量 (0.0 から 1.0)")]
    [SerializeField, Range(0f, 1f)] private float clickVolume = 1.0f;
    [Tooltip("ホバー音の音量 (0.0 から 1.0)")]
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 1.0f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private AudioSource m_audioSource;
    private Button m_button;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Awake()
    {
        // AudioSourceコンポーネントの参照を取得または追加
        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource == null)
        {
            m_audioSource = gameObject.AddComponent<AudioSource>();
            m_audioSource.playOnAwake = false; // シーン開始時に自動再生しない
            m_audioSource.loop = false;        // ループ再生しない
        }

        // Buttonコンポーネントを取得し、onClickイベントにリスナーを登録
        m_button = GetComponent<Button>();
        if (m_button != null)
        {
            m_button.onClick.AddListener(PlayClickSound);
        }
    }

    private void OnDestroy()
    {
        // オブジェクトが破棄されるときにリスナーを解除
        if (m_button != null)
        {
            m_button.onClick.RemoveListener(PlayClickSound);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // UIイベントハンドラー (ISelectHandler, IDeselectHandler)
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// UIが選択されたときに呼び出されます。（キーボード、ゲームパッド、マウスホバー）
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnSelect(BaseEventData eventData)
    {
        PlayHoverSound();
    }

    /// <summary>
    /// UIの選択が外れたときに呼び出されます。
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnDeselect(BaseEventData eventData)
    {
        // このイベントで何か処理が必要な場合はここに追加
    }

    // OnPointerClickメソッドは不要になったため削除

    // ----------------------------------------------------------------------------------------------------
    // サウンド再生メソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// ボタンのクリック音を再生します。
    /// </summary>
    public void PlayClickSound()
    {
        if (m_audioSource != null && m_audioSource.isActiveAndEnabled && clickSound != null)
        {
            m_audioSource.PlayOneShot(clickSound, clickVolume);
        }
        else
        {
            string debugMsg = "ButtonSoundEffect: クリック音を再生できませんでした。";
            if (m_audioSource == null) debugMsg += "AudioSourceがnullです。";
            else if (!m_audioSource.isActiveAndEnabled) debugMsg += "AudioSourceが無効または非アクティブです。";
            else if (clickSound == null) debugMsg += "クリック音のAudioClipが割り当てられていません。";
            Debug.LogWarning(debugMsg, this);
        }
    }

    /// <summary>
    /// ホバー音を再生します。
    /// </summary>
    public void PlayHoverSound()
    {
        if (m_audioSource != null && m_audioSource.isActiveAndEnabled && hoverSound != null)
        {
            m_audioSource.PlayOneShot(hoverSound, hoverVolume);
        }
    }
}