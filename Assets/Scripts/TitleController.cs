using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FadeOut : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        yield return new WaitForSeconds(7f); // Espera 3 segundos antes de começar o fade

        float timeElapsed = 0f;
        float duration = 1f;

        while (timeElapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
