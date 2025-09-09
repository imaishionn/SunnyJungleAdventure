using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UIボタンにクリック音とホバー音（選択音）を付けるためのスクリプトです。
/// ISelectHandlerインターフェースを実装し、UIのイベントを処理します。
/// </summary>
public class ButtonSoundEffect : MonoBehaviour, ISelectHandler, IDeselectHandler {
    [Header("ボタンがクリックされたときに再生する効果音"), SerializeField]
    private AudioClip _clickSound;

    [Header("ボタンにカーソルが乗ったとき、または選択されたときに再生する効果音"), SerializeField]
    private AudioClip _hoverSound;

    [Header("クリック音の音量 (0.0 から 1.0)"), SerializeField, Range(0f,1f)]
    private float _clickVolume = 1.0f;

    [Header("ホバー音の音量 (0.0 から 1.0)"), SerializeField, Range(0f,1f)]
    private float _hoverVolume = 1.0f;

    /// <summary>
    /// オーディオソース
    /// </summary>
    private AudioSource _audioSource;

    /// <summary>
    /// ボタン
    /// </summary>
    private Button _button;

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake() {
        // AudioSourceコンポーネントの参照を取得または追加
        _audioSource = GetComponent<AudioSource>();
        if(_audioSource == null) {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false; // シーン開始時に自動再生しない
            _audioSource.loop = false;        // ループ再生しない
        }

        // Buttonコンポーネントを取得し、onClickイベントにリスナーを登録
        _button = GetComponent<Button>();
        if(_button != null) {
            _button.onClick.AddListener(PlayClickSound);
        }
    }

    /// <summary>
    /// コンポーネントが破棄されるときに呼び出される
    /// </summary>
    private void OnDestroy() {
        // オブジェクトが破棄されるときにリスナーを解除
        if(!_button) {
            return;
        }
        _button.onClick.RemoveListener(PlayClickSound);
    }

    /// <summary>
    /// UIが選択されたときに呼び出されます。（キーボード、ゲームパッド、マウスホバー）
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnSelect(BaseEventData eventData) => PlayHoverSound();


    /// <summary>
    /// UIの選択が外れたときに呼び出されます。
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnDeselect(BaseEventData eventData) {
        // このイベントで何か処理が必要な場合はここに追加
    }

    /// <summary>
    /// ボタンのクリック音を再生します。
    /// </summary>
    public void PlayClickSound() {
        if(CanPlay(_clickSound) ) {
            _audioSource.PlayOneShot(_clickSound,_clickVolume);
        }
        else {
            string debugMsg = "ButtonSoundEffect: クリック音を再生できませんでした。";

            if(_audioSource == null) {
                debugMsg += "AudioSourceがnullです。";
            }
            else if(!_audioSource.isActiveAndEnabled) {
                debugMsg += "AudioSourceが無効または非アクティブです。";
            }
            else if(_clickSound == null) {
                debugMsg += "クリック音のAudioClipが割り当てられていません。";
            }

            Debug.LogWarning(debugMsg,this);
        }
    }

    /// <summary>
    /// ホバー音再生
    /// </summary>
    public void PlayHoverSound() {
        if(CanPlay(_hoverSound)) {
            _audioSource.PlayOneShot(_hoverSound,_hoverVolume);
        }
    }

    /// <summary>
    /// 指定されたAudioClipを再生できるかどうか
    /// </summary>
    public bool CanPlay(AudioClip audioClip) => _audioSource != null && _audioSource.isActiveAndEnabled && audioClip != null;
}
