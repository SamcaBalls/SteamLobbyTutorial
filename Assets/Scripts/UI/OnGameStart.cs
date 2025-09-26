using System.Collections;
using UnityEngine;

public class OnGameStart : MonoBehaviour
{
    [SerializeField] SteamLobby steamLobby;
    [SerializeField] CanvasGroup fadeGroup; // pøetáhni sem v Inspectoru panel s CanvasGroup
    [SerializeField] float fadeDuration = 1f;
    [SerializeField] GameObject image;

    private void Start()
    {
        steamLobby.HostLobby();
        StartCoroutine(WaitForBack());
    }

    private IEnumerator WaitForBack()
    {
        yield return new WaitForSeconds(0.1f);
        if (steamLobby != null)
        {
            steamLobby.LeaveLobby();
            FadeImage(false);
        }
        else
        {
            WaitForBack();
        }

    }

    public void FadeImage(bool fadeIn)
    {
        Debug.Log("Fade");
        StartCoroutine(FadeRoutine(fadeIn));
    }

    private IEnumerator FadeRoutine(bool fadeIn)
    {
        image.SetActive(true);

        float start = fadeGroup.alpha;
        float end = fadeIn ? 1f : 0f;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, end, time / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = end;

        if (!fadeIn)
        {
            image.SetActive(false);
        }
    }
}

