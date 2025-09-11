using UnityEngine;

/// <summary>
/// このゲームオブジェクトを、シーンが切り替わっても破壊されないようにするスクリプト。
/// 主にゲーム全体を通して存在し続けるManager系のオブジェクトに使用されます。
/// </summary>
public class DontDestroyThisObject : MonoBehaviour {
    private void Awake() =>
        // DontDestroyOnLoadを呼び出し、このスクリプトがアタッチされた
        // GameObjectがシーン遷移後も破棄されないようにします。
        DontDestroyOnLoad(gameObject);
}
