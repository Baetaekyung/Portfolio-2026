using UnityEngine;

public static class DeviceUtil
{
    private const long KB = 1024;
    private const long MB = KB * 1024;
    private const long GB = MB * 1024;

    // 안드로이드 플랫폼 여부 확인
    public static bool IsAndroid()
    {
#if UNITY_ANDROID
        return true;
#else
        return false;
#endif
    }

    // iOS 플랫폼 여부 확인
    public static bool IsIOS()
    {
#if UNITY_IOS
        return true;
#else
        return false;
#endif
    }

    public static bool IsRowMemeoryDevice()
    {
#if UNITY_ANDROID
        var GB_3 = 3*GB;

        if (SystemInfo.systemMemorySize < GB_3)
        {
            return true;
        }
        return false;
#elif UNITY_IOS
        var GB_4 = 4*GB;
        
        if (SystemInfo.systemMemorySize < GB_4)
        {
            return true;
        }
        return false;
#endif
    }
}
