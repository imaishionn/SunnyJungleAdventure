using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// スコアシーンでスコア表示、ランク付け、リーダーボード管理を行うスクリプト。
/// </summary>
public class ScoreManager : MonoBehaviour {
   
    [Header("今回のスコア"), SerializeField]
    private TextMeshProUGUI _scoreText;

    [Header("残り時間"), SerializeField]
    private TextMeshProUGUI _timeText;

    [Header("今回のランク"), SerializeField]
    private TextMeshProUGUI _rankText;

    [Header("タイトル画面に戻るボタン"), SerializeField]
    private Button _titleButton;

    [Header("ランキング表示用のText UI要素のリスト"), SerializeField]
    private List<TextMeshProUGUI> _leaderboardEntries = new();

    [Header("ランキング二保存する最大件数"), SerializeField]
    private int _maxLeaderboardCount = 3; 

    
    [Header("ゲームパッド操作で最初に選択状態にしたいUI要素"), SerializeField]
    private Selectable _firstSelected;

   
    [Header("タイトルシーンの名前"), SerializeField]
    private string _titleSceneName;

    // PlayerPrefsのキー
    private const string LEADERBOARD_SCORE_KEY_PREFIX = "LeaderboardScore_";
    private const string LEADERBOARD_TIME_KEY_PREFIX = "LeaderboardTime_";

    private void OnEnable() {
        // GameManagerのインスタンスが存在するか確認
        if (GameManager.Instance == null) {
            UnityEngine.Debug.LogError("GameManagerのインスタンスが見つかりません。スコア表示はできません。");
            // GameManagerがない場合は、ダミーデータで表示
            DisplayResults(0, 0);
        }
        else {
            // GameManagerから最終スコアと時間を取得
            int finalScore = GameManager.Instance.FinalScore;
            float finalTime = GameManager.Instance.FinalTime;

            // スコアと時間をUIに表示
            DisplayResults(finalScore, finalTime);

            // スコアをリーダーボードに保存
            SaveToLeaderboard(finalScore, finalTime);
        }

        // リーダーボードを更新
        UpdateLeaderboardDisplay();

        // ボタンのクリックイベントにメソッドを登録
        if (_titleButton != null) {
            _titleButton.onClick.RemoveAllListeners(); // 重複登録防止
            _titleButton.onClick.AddListener(OnTitleButtonClicked);
        }

        // 最初のUI要素を選択
        if (_firstSelected != null) {
            EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
        }
    }

    /// <summary>
    /// スコアと時間をUIに表示します。
    /// </summary>
    /// <param name="score">表示するスコア</param>
    /// <param name="time">表示する時間</param>
    private void DisplayResults(int score, float time) {
        if (_scoreText != null) {
            _scoreText.text = "スコア" + score.ToString();
        }

        if (_timeText != null) {
            _timeText.text = "残り時間" + Mathf.RoundToInt(time).ToString() + "秒";
        }

        // 評価を計算して表示
        string rank = CalculateRank(score);
        if (_rankText != null) {
            _rankText.text = "ランク" + rank;
        }
    }

    /// <summary>
    /// 新しいスコアと時間をリーダーボードに保存します。
    /// </summary>
    /// <param name="newScore">保存するスコア</param>
    /// <param name="newTime">保存する時間</param>
    private void SaveToLeaderboard(int newScore, float newTime) {
        List<KeyValuePair<int, float>> scoreEntries = LoadLeaderboardEntries();
        scoreEntries.Add(new KeyValuePair<int, float>(newScore, newTime));

        // スコアを降順にソートし、スコアが同じ場合は時間を昇順にソート
        scoreEntries.Sort((a, b) => {
            int scoreComparison = b.Key.CompareTo(a.Key);
            return scoreComparison != 0 ? scoreComparison : a.Value.CompareTo(b.Value);
        });

        // 最大件数を超えたら削除
        if (scoreEntries.Count > _maxLeaderboardCount) {
            scoreEntries.RemoveRange(_maxLeaderboardCount, scoreEntries.Count - _maxLeaderboardCount);
        }

        // PlayerPrefsに保存
        for (int i = 0; i < scoreEntries.Count; i++) {
            PlayerPrefs.SetInt(LEADERBOARD_SCORE_KEY_PREFIX + i, scoreEntries[i].Key);
            PlayerPrefs.SetFloat(LEADERBOARD_TIME_KEY_PREFIX + i, scoreEntries[i].Value);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// リーダーボードのエントリをPlayerPrefsから読み込みます。
    /// </summary>
    /// <returns>読み込んだスコアと時間のペアのリスト</returns>
    private List<KeyValuePair<int, float>> LoadLeaderboardEntries() {
        var entries = new List<KeyValuePair<int, float>>();
        for (int i = 0; i < _maxLeaderboardCount; i++) {
            if (PlayerPrefs.HasKey(LEADERBOARD_SCORE_KEY_PREFIX + i)) {
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
    private void UpdateLeaderboardDisplay() {
        List<KeyValuePair<int, float>> entries = LoadLeaderboardEntries();

        // リーダーボードのUI要素を更新
        for (int i = 0; i < _leaderboardEntries.Count; i++) {
            if (i < entries.Count) {
                int score = entries[i].Key;
                float time = entries[i].Value;

                _leaderboardEntries[i].text = (i + 1).ToString() + ".スコア:" + score.ToString() + "(" + Mathf.RoundToInt(time).ToString() + "秒)";

                _leaderboardEntries[i].gameObject.SetActive(true);
            }
            else {
                // エントリがない場合は非表示にするか、デフォルトテキストを設定
                _leaderboardEntries[i].text = (i + 1).ToString() + ". ---";
                _leaderboardEntries[i].gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// スコアに基づいて評価ランクを計算します。
    /// </summary>
    /// <param name="score">計算するスコア</param>
    /// <returns>評価ランク（例："S"）</returns>
    private string CalculateRank(int score) => score >= 5000 ? "S" : score >= 4000 ? "A" : score >= 2000 ? "B" : "C";

    /// <summary>
    /// タイトルボタンがクリックされた時の処理。
    /// </summary>
    private void OnTitleButtonClicked() {
        // GameManagerのインスタンスを使用してタイトルシーンに遷移
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.TitleSceneName)) {
            GameManager.Instance.LoadSceneWithFade(GameManager.Instance.TitleSceneName);
        }
        else if (!string.IsNullOrEmpty(_titleSceneName)) {
            // GameManagerがない場合は直接シーンをロード
            SceneManager.LoadScene(_titleSceneName);
        }
        else {
            UnityEngine.Debug.LogError("ScoreManager: タイトルシーン名が設定されていません。");
        }
    }
}
