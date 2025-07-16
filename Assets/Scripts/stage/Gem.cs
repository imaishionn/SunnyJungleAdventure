using System.Collections; // コルーチンを使うために必要
using UnityEngine;
// using System.Diagnostics; // この行がある場合は削除してください

public class Gem : MonoBehaviour
{
    [Header("スコア設定")]
    [SerializeField] private int scoreValue = 1; // このジェムが与えるスコア (GemManagerのAddGemに渡す値)

    [Header("効果音設定")]
    [SerializeField] private AudioClip collectSound; // 収集時の効果音
    [SerializeField, Range(0.01f, 1.0f), Tooltip("音が鳴ってからGemが消えるまでのディレイ秒数")]
    private float deactivateDelay = 0.2f; // 音が鳴ってからGemが消えるまでのディレイ秒数

    private AudioSource audioSource; // 効果音再生用

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // 自動再生しない
            audioSource.spatialBlend = 0; // 2Dサウンドとして再生（任意、必要に応じて）
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // GameManagerが存在すればスコアを加算し、ジェム収集を通知
            if (GameManager.instance != null)
            {
                // ★修正前: GameManager.instance.AddScore(scoreValue);
                // ★修正前: GameManager.instance.CollectGem();
                GameManager.instance.AddGem(scoreValue); // ★修正★
            }

            // 効果音を再生
            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
                // Debug.Log($"Gem: '{collectSound.name}' の音を再生しました。AudioSourceの音量: {audioSource.volume}");

                // コライダーを一時的に無効化して、複数回拾われるのを防ぐ
                if (GetComponent<Collider2D>() != null)
                {
                    GetComponent<Collider2D>().enabled = false;
                }

                // 効果音の再生が終了するまで待ってからオブジェクトを非アクティブにするコルーチンを開始
                StartCoroutine(DeactivateAfterSound(deactivateDelay));
            }
            else
            {
                // 音が設定されていないか、AudioSourceがない場合はすぐに非アクティブに
                if (collectSound == null) UnityEngine.Debug.LogWarning("Gem: collectSoundが設定されていません。");
                if (audioSource == null) UnityEngine.Debug.LogWarning("Gem: AudioSourceが見つかりません。");
                gameObject.SetActive(false); // ジェムを非アクティブにする
            }
        }
    }

    // 効果音の再生が終了するまで待ってからオブジェクトを非アクティブにするコルーチン
    private IEnumerator DeactivateAfterSound(float delay)
    {
        yield return new WaitForSeconds(delay); // 指定された時間待機
        gameObject.SetActive(false); // ジェムを非アクティブにする
        Destroy(gameObject); // GameObject自体を破棄する
    }
}