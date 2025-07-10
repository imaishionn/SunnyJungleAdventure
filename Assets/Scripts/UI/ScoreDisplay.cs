using TMPro; // TextMeshProを使用するためにこの行が必要です
using UnityEngine;
using UnityEngine.UI; // これが UnityEngine.UI.Image を提供します

// ここに 'using System.Net.Mime;' があれば削除してください。

// このスクリプトがアタッチされるGameObjectには、必ずTMP_Textコンポーネントが必要であることをUnityに伝えます。
[RequireComponent(typeof(TMP_Text))]
public class ScoreDisplay : MonoBehaviour
{
    // スコア表示用のTextコンポーネント (TextMeshProUGUIを使用)
    // Inspectorで割り当てられていなくても、Awakeで自動的に取得を試みます。
    [SerializeField] private TMP_Text scoreText;

    // スコアの背景Imageへの参照をUnityEngine.UI.Imageと明示的に指定
    // Inspectorで割り当てるか、AwakeでFind("Image")などで取得することを想定
    [SerializeField] private UnityEngine.UI.Image scoreBackgroundImage;

    void Awake()
    {
        // InspectorでscoreTextが割り当てられていない場合、
        // または何らかの理由で参照が失われた場合に、GetComponentで取得を試みます。
        if (scoreText == null)
        {
            scoreText = GetComponent<TMP_Text>();
        }

        // それでもscoreTextがnullの場合（例：TMP_Textコンポーネントが本当にない場合）、エラーをログに出します。
        if (scoreText == null)
        {
            UnityEngine.Debug.LogError("ScoreDisplay: TextMeshProUGUI コンポーネントが見つかりません。ScoreTextオブジェクトにアタッチされているか確認してください。", this);
        }

        // scoreBackgroundImageが割り当てられていない場合、親のImageコンポーネントを探す
        // ScoreTextの親がImageであるという構造を前提としています。
        if (scoreBackgroundImage == null && transform.parent != null)
        {
            scoreBackgroundImage = transform.parent.GetComponent<UnityEngine.UI.Image>(); // 明示的に指定
            if (scoreBackgroundImage == null)
            {
                // 親にImageがない場合、同じCanvas内の"Image"という名前のオブジェクトを探す
                GameObject imgGO = GameObject.Find("Image");
                if (imgGO != null)
                {
                    scoreBackgroundImage = imgGO.GetComponent<UnityEngine.UI.Image>(); // 明示的に指定
                }
            }
        }
        if (scoreBackgroundImage == null)
        {
            UnityEngine.Debug.LogWarning("ScoreDisplay: スコアの背景Imageコンポーネントが見つかりません。");
        }
    }

    // 現在のスコアを更新するPublicメソッド。GameManagerから呼ばれる
    public void ScoreUpdate(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString(); // スコアを文字列に変換して表示
            UnityEngine.Debug.Log($"ScoreDisplay ScoreUpdate: UIを {score} に更新。");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Score Text UI is not assigned or found on ScoreDisplay!");
        }
    }

    // スコア表示（テキストと背景）を非表示にする
    public void HideScore()
    {
        if (scoreText != null && scoreText.gameObject.activeSelf)
        {
            scoreText.gameObject.SetActive(false);
            UnityEngine.Debug.Log("ScoreDisplay: スコアテキストを非表示にしました。");
        }
        // 背景画像も非表示にする
        if (scoreBackgroundImage != null && scoreBackgroundImage.gameObject.activeSelf)
        {
            scoreBackgroundImage.gameObject.SetActive(false);
            UnityEngine.Debug.Log("ScoreDisplay: スコア背景画像を非表示にしました。");
        }
    }

    // スコア表示（テキストと背景）を表示する
    public void ShowScore()
    {
        if (scoreText != null && !scoreText.gameObject.activeSelf)
        {
            scoreText.gameObject.SetActive(true);
            UnityEngine.Debug.Log("ScoreDisplay: スコアテキストを表示しました。");
        }
        // 背景画像も表示する
        if (scoreBackgroundImage != null && !scoreBackgroundImage.gameObject.activeSelf)
        {
            scoreBackgroundImage.gameObject.SetActive(true);
            UnityEngine.Debug.Log("ScoreDisplay: スコア背景画像を表示しました。");
        }
    }
}
