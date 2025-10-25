using TMPro;
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
        }

    }
}
