using UnityEngine;
using TMPro;

public class PlayerValueController : MonoBehaviour
{
    [Header("Value Settings")]
    public int currentValue = 1;

    [Header("UI")]
    public TMP_Text valueText;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        UpdateText();
    }

    public void AddValue(int amount)
    {
        currentValue += amount;
        currentValue = Mathf.Max(1, currentValue);

        UpdateText();
        PunchScale();
    }

    void UpdateText()
    {
        if (valueText != null)
            valueText.text = currentValue.ToString();
    }

    void PunchScale()
    {
        StopAllCoroutines();
        StartCoroutine(ScalePunch());
    }

    System.Collections.IEnumerator ScalePunch()
    {
        float t = 0f;
        float duration = 0.15f;
        Vector3 targetScale = originalScale * 1.2f;

        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
