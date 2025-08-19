using UnityEngine;

/// <summary>
/// このゲームオブジェクトを、シーンが切り替わっても破壊されないようにするスクリプト。
/// 主にゲーム全体を通して存在し続けるManager系のオブジェクトに使用されます。
/// </summary>
public class DontDestroyThisObject : MonoBehaviour
{
    void Awake()
    {
        // DontDestroyOnLoadを呼び出す
        // これにより、このスクリプトがアタッチされたGameObjectは
        // 新しいシーンがロードされても破棄されずに残り続けます。
        DontDestroyOnLoad(this.gameObject);
    }
}