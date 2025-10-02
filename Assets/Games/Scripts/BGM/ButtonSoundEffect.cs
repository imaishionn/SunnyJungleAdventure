using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UIボタンにクリック音とホバー音を付けるためのスクリプトです。
/// </summary>
public class ButtonSoundEffect : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, ISelectHandler, ISubmitHandler {
    
    [Header("クリック音の設定"),SerializeField]
    private AudioClip _クリック音;

    [Header("選択中の音設定"), SerializeField]
    private AudioClip _ホバー音;

    [Header("クリックの音量の設定"), SerializeField, Range(0f, 1f)]
    private float _クリック音量 = 1.0f;

    [Header("選択中の音の設定"), SerializeField, Range(0f, 1f)]
    private float _ホバー音量 = 1.0f;

    /// <summary>
    /// オーディオソース
    /// </summary>
    private AudioSource _audioSource;

    /// <summary>
    /// ボタンコンポーネント
    /// </summary>
    private Button _button;

    private void Awake() {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
        }

        _button = GetComponent<Button>();
        if (_button != null) {
            _button.onClick.AddListener(PlayClickSound);
        }
    }

    private void OnDestroy() {
        if (_button != null) {
            _button.onClick.RemoveListener(PlayClickSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => PlayHoverSound();

    public void OnSelect(BaseEventData eventData) => PlayHoverSound();

    public void OnPointerDown(PointerEventData eventData) {
    }

    public void OnSubmit(BaseEventData eventData) => PlayClickSound();

    private void PlayClickSound() {
        if (_クリック音 != null && _audioSource != null) {
            _audioSource.PlayOneShot(_クリック音, _クリック音量);
        }
    }

    private void PlayHoverSound() {
        if (_ホバー音 != null && _audioSource != null) {
            _audioSource.PlayOneShot(_ホバー音, _ホバー音量);
        }
    }
}
