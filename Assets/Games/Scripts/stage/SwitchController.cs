using UnityEngine;

/// <summary>
/// スイッチの機能と見た目を制御するスクリプトです。
/// プレイヤーとの接触を検知し、対応するプラットフォームのメソッドをトリガーします。
/// </summary>
public class SwitchController : MonoBehaviour {
    [Header("このスイッチのID"), SerializeField]
    private int _switchId = 1;
    [Header("このスイッチが制御するターゲットのプラットフォーム"), SerializeField]
    private TilemapSwitchPlatform _targetPlatform;

    [Header("このスイッチのSpriteRendererコンポーネント"), SerializeField]
    private SpriteRenderer _spriteRenderer; 

    [Header("クランクの「上」状態のスプライト"), SerializeField]
    private Sprite _crankUpSprite; 

    [Header("クランクの「下」状態のスプライト"), SerializeField]
    private Sprite _crankDownSprite; 

    [Header("スイッチが切り替わったときに再生する効果音"), SerializeField]
    private AudioClip _switchSoundEffect;


    /// <summary>
    /// AudioSource
    /// </summary>
    private AudioSource _audioSource; 

    /// <summary>
    /// スイッチの現在の状態(true = 上, false = 下）
    /// </summary>
    private bool _isUpState; 

    private void Start() {
        // SpriteRendererが未割り当ての場合、自動取得
        if (_spriteRenderer == null) {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // AudioSourceコンポーネントを取得または追加
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 初期スプライトに基づいてスイッチの状態を初期化
        if (_spriteRenderer != null) {
            if (_spriteRenderer.sprite == _crankUpSprite) {
                _isUpState = true;
            }
            else if (_spriteRenderer.sprite == _crankDownSprite) {
                _isUpState = false;
            }
            else {
                _isUpState = false;
                _spriteRenderer.sprite = _crankDownSprite;
                Debug.LogWarning("SwitchController: 初期スプライトが認識されません。crankDownSpriteに設定します。", this);
            }
        }
        else {
            Debug.LogError("SwitchController: SpriteRendererが割り当てられていません。", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            ToggleSwitchState();
        }
    }

    /// <summary>
    /// スイッチの状態を切り替え、見た目とサウンド、プラットフォームの動作を更新します。
    /// </summary>
    private void ToggleSwitchState() {
        _isUpState = !_isUpState;

        if (_spriteRenderer != null) {
            _spriteRenderer.sprite = _isUpState ? _crankUpSprite : _crankDownSprite;
        }

        if (_audioSource != null && _switchSoundEffect != null) {
            _audioSource.PlayOneShot(_switchSoundEffect);
        }

        if (_targetPlatform != null) {
            _targetPlatform.ToggleVisibilityWithBGM();
        }
        else {
            Debug.LogWarning("SwitchController: ID " + _switchId + " に対応するターゲットプラットフォームが割り当てられていません。", this);
        }
    }
}
