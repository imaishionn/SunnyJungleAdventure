using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
// using System.Net.Mime; // �����̍s������΍폜����I��
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ��������UnityEngine.UI�ł��邱�Ƃ��m�F��
using static System.Net.Mime.MediaTypeNames;

public class GameManager : MonoBehaviour
{
    // �V�[�����̒萔
    public const string TitleSceneName = "TitleScene";
    public const string StageSelectSceneName = "StageSelect";
    public const string UISceneName = "UI";
    public const string MainGameSceneName = "Demo_tileset2"; // ���Ȃ��̃Q�[���v���C�V�[�����ɍ��킹�Ă�������
    public const string GameOverSceneName = "GameOverScene";
    public const string GameClearSceneName = "GameClearScene"; // �Q�[���N���A�V�[������ǉ��i��������΁j

    // �V���O���g���C���X�^���X
    public static GameManager instance { get; private set; }

    // �Q�[���̏��
    public enum GameState
    {
        enGameState_Init,
        enGameState_Title,
        enGameState_StageSelect,
        enGameState_Play,
        enGameState_Pause,
        enGameState_GameOver,
        enGameState_GameClear, // �Q�[���N���A���
        enGameState_Clear // �݊����̂��߁ienGameState_GameClear�Ɠ����Ӗ������Ŏg���Ă���\���j
    }

    private GameState m_currentGameState = GameState.enGameState_Init;

    // --- �t�F�[�h�֘A ---
    [Header("�t�F�[�h�ݒ�")]
    [SerializeField] private GameObject permanentCanvasPrefab;
    [SerializeField] private GameObject globalFadeCanvasPrefab;
    [SerializeField] private GameObject globalFadePanelPrefab;
    [SerializeField] private GameObject permanentEventSystemPrefab;
    [SerializeField] private Sprite fadePanelSprite;

    private GameObject m_permanentCanvasInstance;
    private GameObject m_globalFadeCanvasInstance;
    private Image m_globalFadePanelImage; // �����ꂪUnityEngine.UI.Image���Q�Ƃ��Ă��邱�Ƃ��m�F��
    private GameObject m_permanentEventSystemInstance;

    private Coroutine m_fadeCoroutine;
    private bool m_isTransitioning = false;

    // --- UI�֘A ---
    [Header("UI�v�f (�����^�C���Őݒ�)")]
    public ScoreDisplay scoreDisplay; // Inspector�Őݒ肷��A�܂���FindObjectOfType�ŒT��
    public GameObject scorePanel; // ScoreDisplay��e�ɂ���p�l��

    [Header("�Q�[���v���C�ݒ�")]
    [SerializeField] private int initialGemCount = 0;
    public int currentGemCount { get; private set; }

    // --- �X�e�[�W�I���֘A ---
    [Header("�X�e�[�W�I��ݒ�")]
    // StageSelectManager����Q�Ƃ����z��ƃC���f�b�N�X
    public string[] stageSceneNames = { "Stage1", "Stage2", "Stage3" }; // ���Ȃ��̎��ۂ̃X�e�[�W���ɍ��킹��
    public int currentStageIndex = 0;


    // --- Awake ---
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: Awake - DontDestroyOnLoad��ݒ肵�܂����B");

            SetupPermanentUIElements();
            InitializeGame();

            if (SceneManager.GetActiveScene().name == "Bootstrap")
            {
                Debug.Log("GameManager: Bootstrap����N�����܂����BTitleScene�ւ̑J�ڂ��J�n���܂��B");
                LoadSceneWithFade(TitleSceneName);
            }
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("GameManager: �����̃C���X�^���X�����邽�߁A����GameManager��j�����܂����B");
        }
    }

    // --- OnEnable / OnDisable ---
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("GameManager: SceneManager.sceneLoaded �C�x���g��o�^���܂����B");
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("GameManager: SceneManager.sceneLoaded �C�x���g���������܂����B");
    }

    // --- �Q�[�������� ---
    private void InitializeGame()
    {
        SetState(GameState.enGameState_Init);
        currentGemCount = initialGemCount;
    }

    // --- �V�[�����[�h���̃R�[���o�b�N ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager: �V�[��'{scene.name}'�����[�h����܂����B���[�h: {mode}");

        if (scene.name == MainGameSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log($"GameManager: �Q�[���v���C�V�[��'{scene.name}'�����[�h����܂����B");
            if (!SceneManager.GetSceneByName(UISceneName).isLoaded)
            {
                Debug.Log("GameManager: UI�V�[�����܂����[�h����Ă��Ȃ����߁AAdditive�Ń��[�h���܂��B");
                SceneManager.LoadScene(UISceneName, LoadSceneMode.Additive);
            }
            SetState(GameState.enGameState_Play);
            InitializeGemCount();

            // ���C���Q�[���V�[�������[�h���ꂽ��UI��\��
            SetScoreUIActive(true);
        }
        else if (scene.name == UISceneName && mode == LoadSceneMode.Additive)
        {
            Debug.Log("GameManager: UI�V�[�������[�h����܂����BPermanentCanvas���A�N�e�B�u�ɂ��܂��B");
            if (m_permanentCanvasInstance != null)
            {
                m_permanentCanvasInstance.SetActive(true);
            }

            // ScoreDisplay�̃C���X�^���X���擾���Đݒ�
            scoreDisplay = FindObjectOfType<ScoreDisplay>();
            if (scoreDisplay != null)
            {
                Debug.Log("GameManager: ScoreDisplay��������܂����B");
                scoreDisplay.transform.SetParent(m_permanentCanvasInstance.transform, false);
                Debug.Log("GameManager: ScoreDisplay�̐e�q�֌W��ݒ肵�܂��B");
            }
            else
            {
                Debug.LogWarning("GameManager: ScoreDisplay��������܂���BUI�\���ɖ�肪����\��������܂��B");
            }

            // ScorePanel�̃C���X�^���X���擾���Đݒ�
            scorePanel = GameObject.Find("ScorePanel");
            if (scorePanel != null)
            {
                scorePanel.transform.SetParent(m_permanentCanvasInstance.transform, false);
                Debug.Log("GameManager: ScorePanel�̐e�q�֌W��ݒ肵�܂��B");
            }
            else
            {
                Debug.LogWarning("GameManager: ScorePanel��������܂���BUI�\���ɖ�肪����\��������܂��B");
            }

            // UI�V�[�������[�h���ꂽ���_�ł́A�X�R�AUI�͔�\���ɂ��Ă���
            SetScoreUIActive(false);
        }
        else if (scene.name == TitleSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log("GameManager: TitleScene�����[�h����܂����B");
            SetState(GameState.enGameState_Title);
            StartFadeIn(true);
            // �^�C�g���V�[���ł�UI���\��
            SetScoreUIActive(false);
        }
        else if (scene.name == StageSelectSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log("GameManager: StageSelect�����[�h����܂����B");
            SetState(GameState.enGameState_StageSelect);
            StartFadeIn(false);
            // �X�e�[�W�I���V�[���ł�UI���\��
            SetScoreUIActive(false);
        }
        else if (scene.name == GameOverSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log("GameManager: GameOverScene �����[�h����܂����B");
            SetState(GameState.enGameState_GameOver);
            StartFadeIn(false);
            // �Q�[���I�[�o�[�V�[���ł�UI���\��
            SetScoreUIActive(false);
        }
        else if (scene.name == GameClearSceneName && mode == LoadSceneMode.Single)
        {
            Debug.Log("GameManager: GameClearScene �����[�h����܂����B");
            SetState(GameState.enGameState_GameClear);
            StartFadeIn(false);
            // �Q�[���N���A�V�[���ł�UI���\��
            SetScoreUIActive(false);
        }
        else
        {
            if (scene.name != "Bootstrap")
            {
                StartFadeIn(false);
            }
            else
            {
                Debug.Log($"GameManager: �V�[��'{scene.name}'�ł̓t�F�[�h�C���͊J�n����܂���B");
            }
            // ���̑��̃V�[���ł�UI���\�� (�K�v�ɉ����Ē���)
            SetScoreUIActive(false);
        }
    }

    // �Q�[����Ԃ̕ύX
    public void SetState(GameState newState)
    {
        if (m_currentGameState == newState) return;

        Debug.Log($"GameManager: �Q�[���̏�Ԃ� {newState} �ɕύX���܂����B");
        m_currentGameState = newState;

        // �Q�[���̏�Ԃɉ������^�C���X�P�[���̐ݒ�
        switch (m_currentGameState)
        {
            case GameState.enGameState_Play:
                Time.timeScale = 1f;
                break;
            case GameState.enGameState_Pause:
                Time.timeScale = 0f;
                break;
            case GameState.enGameState_GameOver:
                Time.timeScale = 0f;
                break;
            case GameState.enGameState_GameClear:
            case GameState.enGameState_Clear: // �����̃R�[�h�݊����̂���
                Time.timeScale = 0f;
                break;
            default:
                Time.timeScale = 1f;
                break;
        }
    }

    // ���݂̃Q�[����Ԃ��擾���邽�߂� public ���\�b�h
    public GameState GetCurrentGameState()
    {
        return m_currentGameState;
    }

    // �Q�[���I�[�o�[��Ԃւ̑J�ڂ𑦍��ɊJ�n
    public void SetGameOverStateImmediately()
    {
        if (m_isTransitioning)
        {
            Debug.Log("GameManager: ���ɃV�[���J�ڒ��̂��߁ASetGameOverStateImmediately���X�L�b�v���܂��B");
            return;
        }

        Debug.Log("GameManager: �Q�[���I�[�o�[��Ԃɐݒ肵�AGameOverScene�ւ̑J�ڂ��J�n���܂����B");
        SetState(GameState.enGameState_GameOver);
        LoadSceneWithFade(GameOverSceneName);
    }

    // �V�[���J��
    public void LoadSceneWithFade(string sceneName)
    {
        if (m_isTransitioning)
        {
            Debug.Log($"GameManager: ���ɃV�[���J�ڒ��̂��߁A'{sceneName}'�ւ̃��[�h���X�L�b�v���܂��B");
            return;
        }

        m_isTransitioning = true;

        Debug.Log($"GameManager: �V�[��'{sceneName}'�ւ̃t�F�[�h�A�E�g�ƃ��[�h���J�n���܂��B");

        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
            Debug.Log("GameManager: �����̃t�F�[�h�R���[�`�����~���܂��� (LoadSceneWithFade)�B");
        }
        m_fadeCoroutine = StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    // �W�F���֘A
    public void InitializeGemCount()
    {
        currentGemCount = 0;
        Debug.Log($"GameManager: �W�F���������������܂����B���W�F����: {currentGemCount}");
        if (scoreDisplay != null)
        {
            scoreDisplay.UpdateGemCount(currentGemCount);
        }
    }

    // �W�F���̎��W�ƃX�R�A���Z�𓝍��������\�b�h
    public void AddGem(int amount)
    {
        currentGemCount += amount;
        Debug.Log($"GameManager: �W�F�����E���܂����B����: {currentGemCount}");
        if (scoreDisplay != null)
        {
            scoreDisplay.UpdateGemCount(currentGemCount);
        }
    }

    // Permanent��UI�v�f�̃Z�b�g�A�b�v
    private void SetupPermanentUIElements()
    {
        Debug.Log("GameManager: SetupPermanentUIElements���J�n���܂��B");

        // PermanentCanvas�̐����Ɛݒ�
        if (permanentCanvasPrefab != null && m_permanentCanvasInstance == null)
        {
            m_permanentCanvasInstance = Instantiate(permanentCanvasPrefab);
            m_permanentCanvasInstance.name = "PermanentCanvas_DontDestroy";
            DontDestroyOnLoad(m_permanentCanvasInstance);
            m_permanentCanvasInstance.SetActive(false); // �����͔�A�N�e�B�u
            Debug.Log("GameManager: PermanentCanvas_DontDestroy�𐶐����܂����B");
        }
        else if (permanentCanvasPrefab == null)
        {
            Debug.LogWarning("GameManager: permanentCanvasPrefab�����蓖�Ă��Ă��܂���B");
        }

        // GlobalFadeCanvas�̐����Ɛݒ�
        if (globalFadeCanvasPrefab != null && m_globalFadeCanvasInstance == null)
        {
            m_globalFadeCanvasInstance = Instantiate(globalFadeCanvasPrefab);
            m_globalFadeCanvasInstance.name = "GlobalFadeCanvas_DontDestroy";
            DontDestroyOnLoad(m_globalFadeCanvasInstance);
            m_globalFadeCanvasInstance.SetActive(false); // �����͔�A�N�e�B�u
            Debug.Log("GameManager: GlobalFadeCanvas_DontDestroy�𐶐����܂����B");
        }
        else if (globalFadeCanvasPrefab == null)
        {
            Debug.LogWarning("GameManager: globalFadeCanvasPrefab�����蓖�Ă��Ă��܂���B");
        }

        // PermanentEventSystem�̐����Ɛݒ�
        if (permanentEventSystemPrefab != null && m_permanentEventSystemInstance == null)
        {
            m_permanentEventSystemInstance = Instantiate(permanentEventSystemPrefab);
            m_permanentEventSystemInstance.name = "PermanentEventSystem_DontDestroy";
            DontDestroyOnLoad(m_permanentEventSystemInstance);
            Debug.Log("GameManager: PermanentEventSystem_DontDestroy�𐶐����܂����B");
        }
        else if (permanentEventSystemPrefab == null)
        {
            Debug.LogWarning("GameManager: permanentEventSystemPrefab�����蓖�Ă��Ă��܂���B");
        }

        // GlobalFadePanel�̐����Ɛݒ�
        if (globalFadePanelPrefab != null && m_globalFadeCanvasInstance != null)
        {
            GameObject fadePanelObject = Instantiate(globalFadePanelPrefab, m_globalFadeCanvasInstance.transform);
            fadePanelObject.name = "GlobalFadePanel";
            m_globalFadePanelImage = fadePanelObject.GetComponent<Image>();
            if (m_globalFadePanelImage == null)
            {
                Debug.LogError("GameManager: GlobalFadePanelPrefab��Image�R���|�[�l���g������܂���B");
            }

            if (fadePanelSprite != null)
            {
                m_globalFadePanelImage.sprite = fadePanelSprite;
                Debug.Log("GameManager: FadePanelSprite�����蓖�Ă܂����B");
            }
            else
            {
                Debug.LogWarning("GameManager: FadePanelSprite�����蓖�Ă��Ă��܂���B");
            }

            m_globalFadePanelImage.color = new Color(0, 0, 0, 0); // �����͊��S�ɓ���
            m_globalFadePanelImage.gameObject.SetActive(false); // �����͔�A�N�e�B�u
            Debug.Log("GameManager: GlobalFadePanel�𐶐����܂����B������Ԃ͓����Ŕ�A�N�e�B�u�ł��B");
        }
        else if (globalFadePanelPrefab == null)
        {
            Debug.LogWarning("GameManager: globalFadePanelPrefab�����蓖�Ă��Ă��܂���B");
        }

        Debug.Log("GameManager: SetupPermanentUIElements���������܂����B");
    }

    // �t�F�[�h�C������
    private void StartFadeIn(bool isInitialLoad)
    {
        if (m_globalFadePanelImage == null)
        {
            Debug.LogError("GameManager: �t�F�[�h�p�l����Image��null�ł��B�t�F�[�h�C���ł��܂���B");
            m_isTransitioning = false;
            return;
        }

        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
            Debug.Log("GameManager: �����̃t�F�[�h�R���[�`�����~���܂��� (OnSceneLoaded / �ʏ�)�B");
        }

        Debug.Log($"GameManager: {(isInitialLoad ? "����" : "�ʏ�")}�t�F�[�h�C�����J�n���܂� (�V�[��: {SceneManager.GetActiveScene().name})�B");
        m_fadeCoroutine = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn(float duration = 2.0f)
    {
        Debug.Log($"GameManager: FadeIn���J�n���܂��B����: {duration}�b");
        m_globalFadeCanvasInstance.SetActive(true);
        m_globalFadePanelImage.gameObject.SetActive(true);

        m_globalFadePanelImage.color = new Color(0, 0, 0, 1); // �ŏ��͕s����
        Debug.Log("GameManager: �t�F�[�h�C���J�n (�s���� -> ����)");

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / duration); // 1����0�֕��
            m_globalFadePanelImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        m_globalFadePanelImage.color = new Color(0, 0, 0, 0); // ���S�ɓ�����
        m_globalFadePanelImage.gameObject.SetActive(false);
        m_globalFadeCanvasInstance.SetActive(false);
        m_isTransitioning = false;
        Debug.Log("GameManager: FadeIn: �������܂����B�p�l���Ɛ�pCanvas���A�N�e�B�u�ɂ��܂����B");
    }


    // �t�F�[�h�A�E�g�ƃV�[�����[�h����
    private IEnumerator FadeOutAndLoadScene(string sceneName, float duration = 1.0f)
    {
        Debug.Log($"GameManager: FadeOutAndLoadScene���J�n���܂��B�ΏۃV�[��: {sceneName}");
        m_globalFadeCanvasInstance.SetActive(true);
        m_globalFadePanelImage.gameObject.SetActive(true);

        m_globalFadePanelImage.color = new Color(0, 0, 0, 0); // �ŏ��͓���
        Debug.Log("GameManager: �t�F�[�h�A�E�g�J�n (���� -> �s����)");

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / duration); // 0����1�֕��
            m_globalFadePanelImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        m_globalFadePanelImage.color = new Color(0, 0, 0, 1); // ���S�ɕs������
        Debug.Log($"GameManager: �V�[�����[�h�J�n: {sceneName}");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // �V�[���̃A�N�e�B�u����҂�

        while (!asyncLoad.isDone)
        {
            // 0.9f�ɒB������A�V�[���̃A�N�e�B�u��������
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
                Debug.Log($"GameManager: �V�[�� '{sceneName}' �̃A�N�e�B�u���������܂����B");
            }
            yield return null;
        }
    }

    // �X�R�AUI�̕\��/��\���𐧌䂷��V�������\�b�h
    private void SetScoreUIActive(bool isActive)
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(isActive);
            Debug.Log($"GameManager: ScorePanel�̕\���� {(isActive ? "�L��" : "����")} �ɂ��܂����B");
        }
        else
        {
            Debug.LogWarning("GameManager: SetScoreUIActive - scorePanel��null�ł��B");
        }
    }

    // �Q�[�����I������
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("GameManager: �Q�[�����I�����܂��B");
    }
}