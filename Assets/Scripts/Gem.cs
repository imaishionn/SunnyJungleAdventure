using UnityEngine;
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

/// <summary>
/// 宝石（Gem）の動作を制御するスクリプト。
/// プレイヤーに触れるとスコアを加算し、自身を破棄します。
/// </summary>
public class Gem : MonoBehaviour
{
    [SerializeField, Tooltip("獲得できるポイント")]
    int Point = 10; // ★デフォルト値を0以外に設定 (例: 10) - Inspectorで変更可能★

    private ItemSoundPlayer soundPlayer;

    void Awake()
    {
        // シーン内にある ItemSoundPlayer を自動で探して取得
        // FindObjectOfTypeはAwakeやStartで一度だけ呼び出すのが効率的
        soundPlayer = FindObjectOfType<ItemSoundPlayer>();
        if (soundPlayer == null)
        {
            Debug.LogWarning("Gem: ItemSoundPlayer がシーンに見つかりません。");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"Gem: プレイヤー ({collision.gameObject.name}) が宝石 ({gameObject.name}) を獲得しました！");

            // スコア加算
            // GameManagerがシングルトンインスタンスとして設定されていることを前提とします。
            if (GameManager.instance != null)
            {
                GameManager.instance.AddScore(Point);
                Debug.Log($"Gem: スコア {Point} を加算しました。現在の合計スコア: {GameManager.instance.GetCurrentPlayScore()}");
            }
            else
            {
                Debug.LogError("Gem: GameManager.instance が見つかりません！スコアを加算できませんでした。");
            }

            // 音を鳴らす
            if (soundPlayer != null)
            {
                soundPlayer.PlayGemSound();
                Debug.Log("Gem: 獲得音を再生しました。");
            }
            else
            {
                Debug.LogWarning("Gem: ItemSoundPlayer が見つからないため、獲得音を再生できません。");
            }

            // アイテム削除
            Destroy(gameObject);
            Debug.Log($"Gem: 宝石オブジェクト ({gameObject.name}) を破棄しました。");
        }
    }
}
