using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームオーバー時にBGMを再生
/// </summary>
public class GameOverBGMPlayer : MonoBehaviour {
    [Header("ゲームオーバーBGM"), SerializeField]
    private AudioClip _gameOverBGM;

    [Header("デフォルト音量"), SerializeField, Range(0f,1f)]
    private float _defaultVolume = 0.5f;

    [Header("UIスライダー"), SerializeField]
    private Slider _volumeSlider;

    /// <summary>
    /// オーディオソース
    /// </summary>
    private AudioSource _audioSource;

    /// <summary>
    /// Start
    /// </summary>
    private void Start() {
        _audioSource = GetComponent<AudioSource>();

        if(_gameOverBGM != null) {
            _audioSource.clip = _gameOverBGM;
            _audioSource.loop = true;
            _audioSource.volume = _defaultVolume;
            _audioSource.Play();
        }
        else {
            Debug.LogWarning("ゲームオーバーBGMが設定されていません！");
        }

        // スライダーが設定されていれば初期化
        if(_volumeSlider != null) {
            _volumeSlider.value = _defaultVolume;
            _volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    /// <summary>
    /// ボリューム設定
    /// </summary>
    public void SetVolume(float volume) => _audioSource.volume = volume;
}
