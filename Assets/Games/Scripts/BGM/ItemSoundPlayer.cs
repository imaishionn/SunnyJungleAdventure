using UnityEngine;

/// <summary>
/// アイテム取得やアクションに応じた効果音を再生
/// </summary>
public class ItemSoundPlayer : MonoBehaviour {
    [Header("宝石獲得音"), SerializeField]
    private AudioClip _gemClip;

    [Header("ジャンプ音"), SerializeField]
    private AudioClip _jumpClip;

    [Header("ゲームオーバー音"), SerializeField]
    private AudioClip _gameOverClip;

    [Header("敵撃破音"), SerializeField]
    private AudioClip _enemyDefeatClip;

    [Header("敵撃破音の再生音量"), SerializeField, Range(0f, 1f)]
    private float _enemyDefeatVolume = 1.0f;

    [Header("ゲームオーバー音の再生音量"), SerializeField, Range(0f, 1f)]
    private float _gameOverVolume = 1.0f;

    /// <summary>
    /// オーディオソース
    /// </summary>
    private AudioSource _audioSource;

    /// <summary>
    /// インスタンス
    /// </summary>
    public static ItemSoundPlayer Instance { get; private set; }

    /// <summary>
    /// Awake
    /// </summary>
    public void Awake() {
        // シングルトンインスタンスの割り当てと、シーンをまたぐ設定
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) {
            Debug.LogError("ItemSoundPlayer: AudioSourceコンポーネントがアタッチされていません。");
        }
    }

    /// <summary>
    /// 効果音を再生できるか
    /// </summary>
    private bool CanPlaySound(AudioClip audioClip) => _audioSource != null && audioClip != null;

    /// <summary>
    /// 再生
    /// </summary>
    private void Play(AudioClip audioClip, float volume = 1f) {
        if (CanPlaySound(audioClip)) {
            _audioSource.PlayOneShot(audioClip, volume);
        }
    }

    /// <summary>
    /// 宝石獲得音を再生
    /// </summary>
    public void PlayGemSound() => Play(_gemClip);

    /// <summary>
    /// ジャンプ音を再生
    /// </summary>
    public void PlayJumpSound() => Play(_jumpClip);

    /// <summary>
    /// ゲームオーバー音を再生
    /// </summary>
    public void PlayGameOverSound() => Play(_gameOverClip, _gameOverVolume);

    /// <summary>
    /// 敵撃破音を再生
    /// </summary>
    public void PlayEnemyDefeatSound() => Play(_enemyDefeatClip, _enemyDefeatVolume);
}
