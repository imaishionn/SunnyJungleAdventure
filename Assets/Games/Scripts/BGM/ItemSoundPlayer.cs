using UnityEngine;

/// <summary>
/// アイテム取得やアクションに応じた効果音を再生
/// </summary>
public class ItemSoundPlayer : MonoBehaviour {
    private AudioSource _audioSource;

    [Header("宝石獲得音")]
    public AudioClip gemClip;

    [Header("ジャンプ音")]
    public AudioClip jumpClip;

    [Header("ゲームオーバー音")]
    public AudioClip gameOverClip;

    [Header("敵撃破音")]
    public AudioClip enemyDefeatClip;

    // ★追加: 敵撃破音とゲームオーバー音の音量設定
    [Header("音量設定")]
    [Tooltip("敵撃破音の再生音量")]
    [Range(0f,1f)]
    [SerializeField] private float _enemyDefeatVolume = 1.0f;
    [Tooltip("ゲームオーバー音の再生音量")]
    [Range(0f,1f)]
    [SerializeField] private float _gameOverVolume = 1.0f;

    public void Awake() {
        _audioSource = GetComponent<AudioSource>();
        if(_audioSource == null) {
            Debug.LogError("ItemSoundPlayer: AudioSourceコンポーネントがアタッチされていません。");
        }
    }

    public void PlayGemSound() {
        if(_audioSource != null && gemClip != null) {
            _audioSource.PlayOneShot(gemClip);
        }
    }

    public void PlayJumpSound() {
        if(_audioSource != null && jumpClip != null) {
            _audioSource.PlayOneShot(jumpClip);
        }
    }

    public void PlayGameOverSound() {
        if(_audioSource != null && gameOverClip != null) {
            // ★修正: gameOverVolumeを引数として渡す
            _audioSource.PlayOneShot(gameOverClip,_gameOverVolume);
        }
    }

    public void PlayEnemyDefeatSound() {
        if(_audioSource != null && enemyDefeatClip != null) {
            // ★修正: enemyDefeatVolumeを引数として渡す
            _audioSource.PlayOneShot(enemyDefeatClip,_enemyDefeatVolume);
        }
    }
}
