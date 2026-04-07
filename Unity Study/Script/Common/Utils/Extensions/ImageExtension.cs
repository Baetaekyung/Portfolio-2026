using UnityEngine;
using UnityEngine.UI;

public static class ImageExtension
{
    public static void SetSprite(this Image img, Sprite sprite, bool ignoreWarning = false)
    {
        if (sprite == null)
        {
            if (ignoreWarning == false)
            {
                Log.WriteWarning("Sprite is null");
            }
        }

        img.sprite = sprite;
    }
}
