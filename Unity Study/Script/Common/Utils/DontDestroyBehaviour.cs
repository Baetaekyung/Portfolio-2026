using UnityEngine;

public sealed class DontDestroyBehaviour : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Log.WriteLog($"[{gameObject.name}]이 DontDestory 설정이 되었습니다.");
    }
}
