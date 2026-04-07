using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static AudioManager Audio => AudioManager.Instance;
    private static LogManager Logger => LogManager.Instance;
    private static GameDataLoader GameData => GameDataLoader.Instance;
    private static LocalDataManager LocalData => LocalDataManager.Instance;
    private static PoolManager Pool => PoolManager.Instance;

    // 화면 전환
    public event Action<bool> OnApplicationFocusChanged;

    private void OnApplicationFocus(bool focus)
    {
        if (focus == false)
        {
            Log.WriteLog("화면이 백그라운드 상태로 변경되었습니다.");
        }
        else
        {
            Log.WriteLog("화면이 인게임 상태로 변경되었습니다.");
        }

        OnApplicationFocusChanged?.Invoke(focus);
    }
}
