using System.Collections;
using UnityEngine;

public class Patch : MonoBehaviour
{
    [SerializeField] private OpeningLogo openingLogo;

    public void Awake()
    {
        // 참조 유효성 검사 (유효하지 않으면 에러 출력)
        if (!Validator.IsValidReferences(out var err, openingLogo))
        {
            Log.WriteError(err);
            return;
        }

        openingLogo.gameObject.SetActive(true);
    }

    private void Start()
    {
        StartCoroutine(nameof(PatchSequenceRoutine));
    }

    private IEnumerator PatchSequenceRoutine()
    {
        yield return openingLogo.StartOpening();
        openingLogo.EndOpening();

        SceneManager.Instance.LoadSceneSync(ESceneName.Title);
    }
}
