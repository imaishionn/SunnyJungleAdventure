using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // シーン管理のために必要
using Debug = UnityEngine.Debug;   // Debugの曖昧な参照を解消するため

public class BGMManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static BGMManager instance;

    // 各シーンのBGMクリップをInspectorから割り当てるためのフィールド
    [SerializeField, Header("タイトルBGM")]
    private AudioClip titleBGM;
    [SerializeField, Header("ゲームプレイBGM")]
    private AudioClip gameBGM;
    [SerializeField, Header("ゲームオーバーBGM")]
    private AudioClip gameOverBGM;
    [SerializeField, Header("ゲームクリアBGM")]
    private AudioClip clearBGM;

    // BGMのマスター音量 (0から1の範囲でInspectorから調整可能)
    [SerializeField, Range(0f, 1f), Header("BGMマスター音量")]
    private float bgmMasterVolume = 0.7f; // デフォルト値を0.7に設定

    // BGMを再生するためのAudioSourceコンポーネント
    private AudioSource audioSource;
    // 現在実行中のフェードコルーチンへの参照
    private Coroutine fadeCoroutine;

    // スクリプトがロードされた際に一度だけ呼び出される
    void Awake()
    {
        // シングルトンパターンの実装
        if (instance == null)
        {
            instance = this;
            // このGameObjectをシーン遷移しても破棄しないように設定
            DontDestroyOnLoad(gameObject);

            // AudioSourceコンポーネントを取得または追加
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false; // 自動再生しない
                audioSource.loop = true; // BGMはデフォルトでループするように設定
            }
            Debug.Log("BGMManager: インスタンスが設定され、DontDestroyOnLoadに設定されました。");

            // シーンがロードされたときに呼び出されるイベントを登録
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // 既にインスタンスがある場合は、この重複したオブジェクトを破棄
            Destroy(gameObject);
            Debug.Log("BGMManager: 重複したインスタンスを破棄しました。");
        }
    }

    // GameObjectが破棄されるときに呼び出される
    void OnDestroy()
    {
        // イベントの登録解除 (重複実行を防ぐため)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーンがロードされたときに呼び出されるコールバックメソッド
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"BGMManager: シーン '{scene.name}' がロードされました。適切なBGMを再生します。");
        // ロードされたシーンの名前によって再生するBGMを切り替える
        switch (scene.name)
        {
            case "TitleScene":
                PlayBGM(titleBGM);
                break;
            case "Demo_tileset": // ゲームプレイシーン
                PlayBGM(gameBGM);
                break;
            case "GameOver": // ゲームオーバーシーン
            case "GameOverScene": // 念のためGameOverSceneも対応
                PlayBGM(gameOverBGM);
                break;
            case "ClearScene": // ゲームクリアシーン
                PlayBGM(clearBGM);
                break;
            default:
                StopBGM(); // その他のシーンではBGMを停止する
                break;
        }
    }

    // 指定されたBGMクリップを再生する公共メソッド
    public void PlayBGM(AudioClip clip, bool loop = true, float fadeInDuration = 0.5f)
    {
        if (audioSource == null)
        {
            Debug.LogError("BGMManager: AudioSourceが見つかりません。BGMを再生できません。");
            return;
        }

        // 現在再生中のBGMが同じであれば、何もしない
        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            Debug.Log($"BGMManager: BGM '{clip?.name}' は既に再生中です。");
            return;
        }

        // 既存のフェードコルーチンがあれば停止する
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        audioSource.Stop(); // 現在のBGMを停止
        audioSource.clip = clip; // 新しいBGMクリップを設定
        audioSource.loop = loop; // ループ設定
        audioSource.volume = 0f; // フェードインのために初期ボリュームを0に設定
        audioSource.Play(); // 再生開始
        Debug.Log($"BGMManager: BGM '{clip?.name}' の再生を開始しました。");

        // フェードイン処理
        if (fadeInDuration > 0)
        {
            // フェードインの目標ボリュームをbgmMasterVolumeに設定
            fadeCoroutine = StartCoroutine(FadeAudio(audioSource, 0f, bgmMasterVolume, fadeInDuration));
        }
        else
        {
            audioSource.volume = bgmMasterVolume; // フェードインしない場合は即座にマスターボリューム
        }
    }

    // 現在のBGMを停止する公共メソッド
    public void StopBGM(float fadeOutDuration = 0.5f)
    {
        if (audioSource == null || !audioSource.isPlaying)
        {
            return; // AudioSourceがないか、再生中でなければ何もしない
        }

        // 既存のフェードコルーチンがあれば停止する
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // フェードアウト処理
        if (fadeOutDuration > 0)
        {
            // フェードアウトの開始ボリュームを現在のボリュームに設定
            fadeCoroutine = StartCoroutine(FadeAudio(audioSource, audioSource.volume, 0f, fadeOutDuration, true));
        }
        else
        {
            audioSource.Stop(); // フェードアウトしない場合は即座に停止
            audioSource.volume = bgmMasterVolume; // 次回再生のためにボリュームをリセット
        }
        Debug.Log("BGMManager: BGMを停止しました。");
    }

    // オーディオのフェードイン/アウトを行うコルーチン
    private IEnumerator FadeAudio(AudioSource source, float startVolume, float endVolume, float duration, bool stopOnEnd = false)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, endVolume, timer / duration);
            yield return null;
        }
        source.volume = endVolume; // 最終ボリュームを設定
        if (stopOnEnd && endVolume == 0)
        {
            source.Stop(); // フェードアウト完了後に停止
        }
        fadeCoroutine = null; // コルーチンが終了したので参照をクリア
    }
}
