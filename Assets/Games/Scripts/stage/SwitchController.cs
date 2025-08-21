using UnityEngine;

// This script controls a toggle switch's functionality and visual state.
// It detects player interaction and triggers a method on the target platform.
public class SwitchController : MonoBehaviour
{
    [Header("Switch Settings")]
    [Tooltip("The ID of this switch. It should match the ID on the target platform.")]
    public int switchId = 1;
    [Tooltip("The platform GameObject that this switch will control.")]
    public TilemapSwitchPlatform targetPlatform;

    [Header("Visuals")]
    [Tooltip("The SpriteRenderer component of this switch.")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("The sprite for the 'up' state of the crank.")]
    [SerializeField] private Sprite crankUpSprite;
    [Tooltip("The sprite for the 'down' state of the crank.")]
    [SerializeField] private Sprite crankDownSprite;

    [Header("Audio")] // ★追加: オーディオ設定
    [Tooltip("The sound effect to play when the switch is toggled.")]
    [SerializeField] private AudioClip switchSoundEffect;
    private AudioSource audioSource; // AudioSourceコンポーネントへの参照

    // Current state of the switch (true if 'up', false if 'down')
    private bool isUpState;

    void Start()
    {
        // Get SpriteRenderer if not assigned in Inspector
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initialize the switch state based on its initial sprite
        if (spriteRenderer != null)
        {
            if (spriteRenderer.sprite == crankUpSprite)
            {
                isUpState = true;
            }
            else if (spriteRenderer.sprite == crankDownSprite)
            {
                isUpState = false;
            }
            else
            {
                isUpState = false;
                spriteRenderer.sprite = crankDownSprite;
                UnityEngine.Debug.LogWarning("SwitchController: Initial sprite is not recognized as crankUpSprite or crankDownSprite. Defaulting to crankDownSprite.", this);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("SwitchController: SpriteRenderer is not assigned or found.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ToggleSwitchState();
        }
    }

    private void ToggleSwitchState()
    {
        isUpState = !isUpState;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isUpState ? crankUpSprite : crankDownSprite;
        }

        // Play the sound effect
        if (audioSource != null && switchSoundEffect != null)
        {
            audioSource.PlayOneShot(switchSoundEffect);
        }

        // Trigger the corresponding action on the target platform
        if (targetPlatform != null)
        {
            // ★変更: BGMを切り替えるメソッドを呼び出す
            targetPlatform.ToggleVisibilityWithBGM();
        }
        else
        {
            UnityEngine.Debug.LogWarning("SwitchController: Target Platform not assigned for switch with ID: " + switchId, this);
        }
    }
}
