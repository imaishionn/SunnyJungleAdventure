using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// スコアシーンでスコア表示、ランク付け、リーダーボード管理を行うスクリプト。
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // UI設定
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private Button titleButton;

    [Header("Leaderboard Settings")]
    [Tooltip("ランキング表示用のText UI要素のリスト。インスペクターで設定してください。")]
    [SerializeField] private List<TextMeshProUGUI> leaderboardEntries = new List<TextMeshProUGUI>();
    [Tooltip("ランキングに保存する最大件数")]
    [SerializeField] private int maxLeaderboardCount = 5;

    // ゲームパッド設定
    [Header("GamePad Setting")]
    [Tooltip("ゲームパッド操作で最初に選択状態にしたいUI要素")]
    [SerializeField] private Selectable firstSelected;

    // シーン名
    [Header("Scene Names")]
    [Tooltip("タイトルシーンの名前。GameManagerから取得する方が望ましい")]
    [SerializeField] private string titleSceneName;

    // PlayerPrefsのキー
    private const string LEADERBOARD_SCORE_KEY_PREFIX = "LeaderboardScore_";
    private const string LEADERBOARD_TIME_KEY_PREFIX = "LeaderboardTime_";

    void OnEnable()
    {
        // GameManagerのインスタンスが存在するか確認
        if (GameManager.instance == null)
        {
            UnityEngine.Debug.LogError("GameManagerのインスタンスが見つかりません。スコア表示はできません。");
            // GameManagerがない場合は、ダミーデータで表示
            DisplayResults(0, 0);
        }
        else
        {
            // GameManagerから最終スコアと時間を取得
            int finalScore = GameManager.instance.finalScore;
            float finalTime = GameManager.instance.finalTime;

            // スコアと時間をUIに表示
            DisplayResults(finalScore, finalTime);

            // スコアをリーダーボードに保存
            SaveToLeaderboard(finalScore, finalTime);
        }

        // リーダーボードを更新
        UpdateLeaderboardDisplay();

        // ボタンのクリックイベントにメソッドを登録
        if (titleButton != null)
        {
            titleButton.onClick.RemoveAllListeners(); // 重複登録防止
            titleButton.onClick.AddListener(OnTitleButtonClicked);
        }

        // 最初のUI要素を選択
        if (firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }
    }

    /// <summary>
    /// スコアと時間をUIに表示します。
    /// </summary>
    /// <param name="score">表示するスコア</param>
    /// <param name="time">表示する時間</param>
    private void DisplayResults(int score, float time)
    {
        if (scoreText != null)
        {
            scoreText.text = "スコア: " + score.ToString();
        }

        if (timeText != null)
        {
            timeText.text = "残り時間: " + Mathf.RoundToInt(time).ToString() + "秒";
        }

        // 評価を計算して表示
        string rank = CalculateRank(score);
        if (rankText != null)
        {
            rankText.text = "評価: " + rank;
        }
    }

    /// <summary>
    /// 新しいスコアと時間をリーダーボードに保存します。
    /// </summary>
    /// <param name="newScore">保存するスコア</param>
    /// <param name="newTime">保存する時間</param>
    private void SaveToLeaderboard(int newScore, float newTime)
    {
        List<KeyValuePair<int, float>> scoreEntries = LoadLeaderboardEntries();
        scoreEntries.Add(new KeyValuePair<int, float>(newScore, newTime));

        // スコアを降順にソートし、スコアが同じ場合は時間を昇順にソート
        scoreEntries.Sort((a, b) =>
        {
            int scoreComparison = b.Key.CompareTo(a.Key);
            if (scoreComparison != 0)
            {
                return scoreComparison;
            }
            return a.Value.CompareTo(b.Value);
        });

        // 最大件数を超えたら削除
        if (scoreEntries.Count > maxLeaderboardCount)
        {
            scoreEntries.RemoveRange(maxLeaderboardCount, scoreEntries.Count - maxLeaderboardCount);
        }

        // PlayerPrefsに保存
        for (int i = 0; i < scoreEntries.Count; i++)
        {
            PlayerPrefs.SetInt(LEADERBOARD_SCORE_KEY_PREFIX + i, scoreEntries[i].Key);
            PlayerPrefs.SetFloat(LEADERBOARD_TIME_KEY_PREFIX + i, scoreEntries[i].Value);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// リーダーボードのエントリをPlayerPrefsから読み込みます。
    /// </summary>
    /// <returns>読み込んだスコアと時間のペアのリスト</returns>
    private List<KeyValuePair<int, float>> LoadLeaderboardEntries()
    {
        List<KeyValuePair<int, float>> entries = new List<KeyValuePair<int, float>>();
        for (int i = 0; i < maxLeaderboardCount; i++)
        {
            if (PlayerPrefs.HasKey(LEADERBOARD_SCORE_KEY_PREFIX + i))
            {
                int score = PlayerPrefs.GetInt(LEADERBOARD_SCORE_KEY_PREFIX + i);
                float time = PlayerPrefs.GetFloat(LEADERBOARD_TIME_KEY_PREFIX + i);
                entries.Add(new KeyValuePair<int, float>(score, time));
            }
        }
        return entries;
    }

    /// <summary>
    /// UIにリーダーボードのランキングを表示します。
    /// </summary>
    void UpdateLeaderboardDisplay()
    {
        List<KeyValuePair<int, float>> entries = LoadLeaderboardEntries();

        // リーダーボードのUI要素を更新
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            if (i < entries.Count)
            {
                int score = entries[i].Key;
                float time = entries[i].Value;

                leaderboardEntries[i].text = (i + 1).ToString() + ". スコア: " + score.ToString() + " (" + Mathf.RoundToInt(time).ToString() + "秒)";

                leaderboardEntries[i].gameObject.SetActive(true);
            }
            else
            {
                // エントリがない場合は非表示にするか、デフォルトテキストを設定
                leaderboardEntries[i].text = (i + 1).ToString() + ". ---";
                leaderboardEntries[i].gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// スコアに基づいて評価ランクを計算します。
    /// </summary>
    /// <param name="score">計算するスコア</param>
    /// <returns>評価ランク（例："S"）</returns>
    string CalculateRank(int score)
    {
        if (score >= 5000)
        {
            return "S";
        }
        else if (score >= 4000)
        {
            return "A";
        }
        else if (score >= 2000)
        {
            return "B";
        }
        else
        {
            return "C";
        }
    }

    /// <summary>
    /// タイトルボタンがクリックされた時の処理。
    /// </summary>
    void OnTitleButtonClicked()
    {
        // GameManagerのインスタンスを使用してタイトルシーンに遷移
        if (GameManager.instance != null && !string.IsNullOrEmpty(GameManager.instance.TitleSceneName))
        {
            GameManager.instance.LoadSceneWithFade(GameManager.instance.TitleSceneName);
        }
        else if (!string.IsNullOrEmpty(titleSceneName))
        {
            // GameManagerがない場合は直接シーンをロード
            SceneManager.LoadScene(titleSceneName);
        }
        else
        {
            UnityEngine.Debug.LogError("ScoreManager: タイトルシーン名が設定されていません。");
        }
    }
}