using System.Collections;
using UnityEngine;

public class OpeningLogo : MonoBehaviour
{
    private const string ANIM_OPENING_KEY = "OPENING";
    private readonly int ANIM_OPENING_HASH = Animator.StringToHash(ANIM_OPENING_KEY);

    [SerializeField] private Animator openingAnim;
    [SerializeField] private CanvasGroup cg;

    private void Awake()
    {
        if (Validator.IsValidReferences(out var err, openingAnim) == false)
        {
            Log.WriteError(err);
            return;
        }
    }

    public IEnumerator StartOpening()
    {
        cg.alpha = .0f;
        gameObject.SetActive(true);
        openingAnim.SetTrigger(ANIM_OPENING_HASH);

        yield return new WaitForSeconds(3f);
    }

    public void EndOpening()
    {
        cg.alpha = 0f;
        gameObject.SetActive(false);
    }
}
