using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームオーバー時にBGMを再生
/// </summary>
public class GameOverBGMPlayer : MonoBehaviour {
    [SerializeField]
    private AudioClip _gameOverBGM;
    [SerializeField, Range(0f, 1f)]
    private float _defaultVolume = 0.5f;
    [SerializeField]
    private Slider _volumeSlider; // UIのスライダー（任意）

    private AudioSource _audioSource;

    private void Start() {
        _audioSource = GetComponent<AudioSource>();

        if (_gameOverBGM != null) {
            _audioSource.clip = _gameOverBGM;
            _audioSource.loop = true;
            _audioSource.volume = _defaultVolume;
            _audioSource.Play();
        }
        else {
            Debug.LogWarning("ゲームオーバーBGMが設定されていません！");
        }

        // スライダーが設定されていれば初期化
        if (_volumeSlider != null) {
            _volumeSlider.value = _defaultVolume;
            _volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float volume) {
        if (_audioSource != null) {
            _audioSource.volume = volume;
        }
    }
}
