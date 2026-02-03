using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class Warning : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI warningText;
    /// <summary>
    /// Time to move from startPos to endPos.
    /// </summary>
    [SerializeField] float travelTime = 0.5f;
    [SerializeField] float showPause = 0.25f;
    float timeRemaining = 0.0f;
    RectTransform rectTrans;
    float startPos;
    float endPos;

    void Awake()
    {
        rectTrans = GetComponent<RectTransform>();
    }

    void Start()
    {
        InitValues();
    }

    void InitValues()
    {
        startPos = rectTrans.anchoredPosition.y;
        endPos = startPos - rectTrans.rect.height;
    }

    IEnumerator ShowLerp()
    {
        float time = timeRemaining;
        float newPos;
        while (rectTrans.anchoredPosition.y > endPos)
        {
            time += Time.unscaledDeltaTime;
            timeRemaining = time;
            newPos = Mathf.Lerp(startPos, endPos, time / travelTime);
            rectTrans.anchoredPosition = new(rectTrans.anchoredPosition.x, newPos);
            yield return new WaitForEndOfFrame();
        }
        timeRemaining = 0.0f;
        StartCoroutine(HideLerp());
    }

    IEnumerator HideLerp()
    {
        float time = timeRemaining;
        float newPos;

        yield return new WaitForSecondsRealtime(showPause);

        while (rectTrans.anchoredPosition.y < startPos)
        {
            time += Time.unscaledDeltaTime;
            timeRemaining = travelTime - time;
            newPos = Mathf.Lerp(endPos, startPos, time / travelTime);
            rectTrans.anchoredPosition = new(rectTrans.anchoredPosition.x, newPos);
            yield return new WaitForEndOfFrame();
        }
        timeRemaining = 0.0f;
    }

    public void ShowWarning(string text)
    {
        warningText.text = text;
        StopAllCoroutines();
        StartCoroutine(ShowLerp());
    }
}
