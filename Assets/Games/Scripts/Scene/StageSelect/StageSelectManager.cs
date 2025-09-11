using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ステージ選択シーンのUIとイベントを管理するスクリプトです。
/// </summary>
public class StageSelectManager : MonoBehaviour {
    [Header("ページ設定"), SerializeField]
    private int _pageIndex = 0;

    [Header("UIコンポーネント"), SerializeField]
    private Transform _stageButtonsParent;

    [Header("クリア時に出る文字"), SerializeField]
    private GameObject[] _clearIndicators;

    [Header("次のページへ行くボタン"), SerializeField]
    private Button _pageButton;

    [Header("ステージ選択画面全体のページオブジェクト"), SerializeField]
    private GameObject _stageSelectPage;

    private Button[] _stageButtons;
    private const int STAGES_PER_PAGE = 3;

    private void Awake() {
        if (_stageButtonsParent != null) {
            // "StageButton"タグを持つボタンだけを正確に取得する
            _stageButtons = _stageButtonsParent.GetComponentsInChildren<Button>();
        }
        else {
            Debug.LogError("StageSelectManager: ステージボタン親が設定されていません。インスペクターを確認してください。", this);
            return;
        }

        if (_clearIndicators.Length != _stageButtons.Length) {
            Debug.LogWarning($"StageSelectManager: クリア表示とボタンの数が一致しません。 ボタン数:{_stageButtons.Length}, クリア表示数:{_clearIndicators.Length}", this);
        }

        SetupStageButtons();

        if (_pageButton != null) {
            _pageButton.onClick.RemoveAllListeners();
            _pageButton.onClick.AddListener(OnPageButtonClicked);
        }
    }

    private void Start() {
        if (EventSystem.current != null && _stageButtons.Length > 0 && _stageButtons[0] != null) {
            EventSystem.current.SetSelectedGameObject(_stageButtons[0].gameObject);
        }
    }

    private void SetupStageButtons() {
        if (GameManager.Instance == null) {
            Debug.LogError("StageSelectManager: GameManagerインスタンスが見つかりません。ゲームの開始シーンを確認してください。", this);
            return;
        }

        for (int i = 0; i < _stageButtons.Length; i++) {
            if (_stageButtons[i] == null) {
                continue;
            }

            int globalStageIndex = (_pageIndex * STAGES_PER_PAGE) + i;

            // ボタンがクリックされた時のイベントリスナーを設定
            _stageButtons[i].onClick.RemoveAllListeners();
            _stageButtons[i].onClick.AddListener(() => OnStageButtonClicked(globalStageIndex));

            // ステージが存在するかどうかに基づいてボタンを有効化/無効化
            if (globalStageIndex < GameManager.Instance.StageSceneNames.Length) {
                _stageButtons[i].interactable = true;

                // クリア表示のオブジェクトを正しく設定
                if (i < _clearIndicators.Length && _clearIndicators[i] != null) {
                    _clearIndicators[i].SetActive(GameManager.Instance.IsStageClear(globalStageIndex));
                }
            }
            else {
                _stageButtons[i].interactable = false;

                if (i < _clearIndicators.Length && _clearIndicators[i] != null) {
                    _clearIndicators[i].SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// ステージボタンがクリックされたときに呼び出されます。
    /// 該当ステージをロードし、シーン遷移を開始します。
    /// </summary>
    /// <param name="globalStageIndex">クリックされたボタンに対応するステージのインデックス。</param>
    private void OnStageButtonClicked(int globalStageIndex) {
        if (GameManager.Instance != null && globalStageIndex < GameManager.Instance.StageSceneNames.Length) {
            GameManager.Instance.CurrentStageIndex = globalStageIndex;
            GameManager.Instance.LoadSceneWithFade(GameManager.Instance.StageSceneNames[globalStageIndex]);
        }
        else {
            Debug.LogError($"StageSelectManager: 無効なステージインデックス、またはGameManagerが見つかりません。インデックス: {globalStageIndex}", this);
        }
    }

    /// <summary>
    /// ページ切り替えボタンがクリックされたときに呼び出されます。
    /// 次のページまたは前のページへ遷移します。
    /// </summary>
    private void OnPageButtonClicked() {
        if (GameManager.Instance == null) {
            Debug.LogError("GameManagerが見つかりません。");
            return;
        }

        if (_pageIndex == 0) {
            GameManager.Instance.LoadSceneWithFade(GameManager.Instance.StageSelect2SceneName);
        }
        else if (_pageIndex == 1) {
            GameManager.Instance.LoadSceneWithFade(GameManager.Instance.StageSelectSceneName);
        }
    }
}
