using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleIntro : MonoBehaviour
{
    private const string INTRO_ANIM_KEY = "INTRO";
    private readonly int INTRO_ANIM_HASH = Animator.StringToHash(INTRO_ANIM_KEY);

    [SerializeField] private GameObject titleOverlap;
    [SerializeField] private Animator introAnimator;

    private bool _isInit = false;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        _isInit = true;
    }

    private void Update()
    {
        if (_isInit == false) return;

        var pointer = Pointer.current;
        if (pointer == null)
        {
            Log.WriteLog("[InGameIntro] Pointer.current가 null입니다.");
            return;
        }

        var pressed = pointer.press.isPressed;

        if (pressed)
        {
            introAnimator.SetTrigger(INTRO_ANIM_HASH);
            InterfaceManager.Instance.SetLoading(true);
            titleOverlap.SetActive(true);
            _isInit = false;

            StartCoroutine(nameof(GameDataLoadRoutine));
        }
    }

    private readonly WaitForSeconds _cached01Sec = new WaitForSeconds(1f);
    private IEnumerator GameDataLoadRoutine()
    {
        yield return _cached01Sec;
        yield return SceneManager.Instance.LoadSceneAsync(ESceneName.InGame);
    }
}
