using UnityEngine;

public abstract class Popup : MonoBehaviour
{
    private const string POPUP_ANIM_KEY = "POPUP";
    private const string POPDOWN_ANIM_KEY = "POPDOWN";

    private readonly int POPUP_ANIM_HASH = Animator.StringToHash(POPUP_ANIM_KEY);
    private readonly int POPDOWN_ANIM_HASH = Animator.StringToHash(POPDOWN_ANIM_KEY);

    [SerializeField] private Animator popupAnimator;

    private PopupData _data;
    private PopupContext _context;

    public PopupData Data => _data;
    public PopupContext Context => _context;

    public virtual void InitPopup(PopupData data)
    {
        _data = data;
        _context = data.context;
    }

    public virtual void OnPopup(PopupContext context)
    {
        popupAnimator.ResetTrigger(POPDOWN_ANIM_HASH);
        popupAnimator.SetTrigger(POPUP_ANIM_HASH);
    }

    public virtual void OnPopdown()
    {
        popupAnimator.ResetTrigger(POPUP_ANIM_HASH);
        popupAnimator.SetTrigger(POPDOWN_ANIM_HASH);
    }
}
