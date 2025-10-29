using System.Collections;
using TMPro;
using Unity.Jobs;
using UnityEngine;

public class MultiplierDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI multiplierText;

    private void Start()
    {
        if (multiplierText != null)
        {
            multiplierText.enabled = false;
        }

        if (ScoreManager.Instance != null)
        {
            Debug.Log("Subscribing in start - Instance exists");
            ScoreManager.Instance.OnMultiplierChanged.AddListener(UpdateMultiplierDisplay);
        }
        else
        {
             Debug.Log("ScoreManager.Instance is still NULL");
        }
    }

    private void OnEnable()
    {
        Debug.Log("MultiplierDisplay OnEnable called");
        if (ScoreManager.Instance != null)
        {
            Debug.Log("ScoreManager.Instance exists, subscribing to event");
        }
        else
        {
            Debug.Log("ScoreManager.Instance is NULL!");
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnMultiplierChanged.RemoveListener(UpdateMultiplierDisplay);
        }
    }

    private void UpdateMultiplierDisplay(float newMultiplier)
    {
        Debug.Log("UpdateMultiplierDisplay called with" + newMultiplier);
        if (multiplierText != null)
        {
            multiplierText.text = "x" + newMultiplier.ToString("F1");
            multiplierText.enabled = (newMultiplier > 1.0f);

            if (newMultiplier > 1.0f)
            {
                StartCoroutine(PulseAnimation());
            }
        }

    }

    private IEnumerator PulseAnimation()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * 1.2f;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

}
