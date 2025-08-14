using UnityEngine;
public class DontDestroyThisObject : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}