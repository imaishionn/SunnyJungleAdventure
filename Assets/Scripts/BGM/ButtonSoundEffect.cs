using UnityEngine;
using UnityEngine.EventSystems; // EventTrigger を使用するために必要
using UnityEngine.UI; // Button クラスを使用するために必要

public class ButtonSoundEffect : MonoBehaviour
{
    [SerializeField]
    private AudioClip clickSound; // クリック音
    [SerializeField]
    private AudioClip hoverSound; // ホバー音 (カーソルが当たった時や選択時)

    private AudioSource audioSource;
    private bool isHovered = false; // ホバー状態を追跡するフラグ

    void Awake()
    {
        // このGameObjectにAudioSourceコンポーネントがアタッチされているか確認
        // なければ新しく追加する
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Awake時に自動再生しない
            audioSource.spatialBlend = 0; // 2Dサウンドとして再生 (UIサウンドに適している)
        }

        // このGameObject、またはその子オブジェクトに含まれる全てのButtonコンポーネントを取得
        // (true を指定することで、非アクティブな子オブジェクト内のボタンも取得対象にする)
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            // --- 1. クリック音の設定 ---
            // ButtonのonClickイベントにリスナーを追加
            // ボタンがクリックされたときにPlayClickSound()が呼ばれるようになる
            button.onClick.AddListener(() => PlayClickSound());

            // --- 2. ホバー音 (PointerEnter, Select) の設定 ---
            // EventTriggerコンポーネントを取得または追加する
            // EventTriggerは、Buttonコンポーネントだけでは扱えない様々なUIイベントを捕捉するために使用
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                // Debug.LogWarning($"ButtonSoundEffect: Adding EventTrigger to {button.gameObject.name}");
                trigger = button.gameObject.AddComponent<EventTrigger>();
            }

            // a. PointerEnter (マウスカーソルがボタンに乗り上げた時)
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter; // イベントタイプをPointerEnterに設定
            entryEnter.callback.AddListener((data) => {
                // ホバー中ではない場合のみホバー音を再生し、isHoveredフラグをtrueにする
                // これにより、マウスがボタン上を動き回るたびに音が鳴るのを防ぐ
                if (!isHovered)
                {
                    PlayHoverSound();
                    isHovered = true;
                }
            });
            trigger.triggers.Add(entryEnter); // EventTriggerにイベントエントリーを追加

            // b. PointerExit (マウスカーソルがボタンから離れた時)
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit; // イベントタイプをPointerExitに設定
            entryExit.callback.AddListener((data) => {
                isHovered = false; // ホバー状態をリセット
            });
            trigger.triggers.Add(entryExit); // EventTriggerにイベントエントリーを追加

            // c. Select (キーボードやゲームパッドでボタンが選択された時)
            EventTrigger.Entry entrySelect = new EventTrigger.Entry();
            entrySelect.eventID = EventTriggerType.Select; // イベントタイプをSelectに設定
            entrySelect.callback.AddListener((data) => {
                // Select時もホバー音を再生する
                // isHoveredフラグのチェックは不要、選択されたら常に鳴らす
                PlayHoverSound();
            });
            trigger.triggers.Add(entrySelect); // EventTriggerにイベントエントリーを追加

            // d. Deselect (キーボードやゲームパッドでボタンの選択が外れた時)
            EventTrigger.Entry entryDeselect = new EventTrigger.Entry();
            entryDeselect.eventID = EventTriggerType.Deselect; // イベントタイプをDeselectに設定
            entryDeselect.callback.AddListener((data) => {
                // Deselect時は特に音を鳴らす必要がなければ、ここでは何もしない
            });
            trigger.triggers.Add(entryDeselect); // EventTriggerにイベントエントリーを追加
        }
        Debug.Log("ButtonSoundEffect: ボタンにSEリスナーを設定しました。");
    }

    // クリック音を再生するメソッド
    private void PlayClickSound()
    {
        // clickSoundとaudioSourceがnullでないことを確認してから再生
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound); // OneShotで重ねて再生可能
            Debug.Log("ButtonSoundEffect: クリック音を再生しました。");
        }
        else if (clickSound == null)
        {
            Debug.LogWarning("ButtonSoundEffect: クリック音のAudioClipが設定されていません。");
        }
    }

    // ホバー音を再生するメソッド
    private void PlayHoverSound()
    {
        // hoverSoundがnullでなければ再生。
        // ここから 'else if (hoverSound == null)' のDebug.LogWarningを削除
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound); // OneShotで重ねて再生可能
            Debug.Log("ButtonSoundEffect: ホバー音を再生しました。");
        }
        // ホバー音のAudioClipが設定されていなくても、警告は表示しない
    }
}