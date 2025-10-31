using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float duration = 1f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Initialize(int score, Vector3 worldPosition)
    {
        scoreText.text = "+" + score;

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        rectTransform.position = screenPosition;

        Debug.Log("Initializing ScorePopup at position: " + screenPosition + " with score: " + score);

        StartCoroutine(AnimatePopup());
    }

    private System.Collections.IEnumerator AnimatePopup()
    {
        float elapsed = 0f;
        Vector3 startPosition = rectTransform.position;
        Debug.Log("Starting popup animation at position: " + startPosition);

        while (elapsed < duration)
        {
            Debug.Log("Animating popup: elapsed=" + elapsed);
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rectTransform.position = startPosition + Vector3.up * (moveSpeed * elapsed);

            canvasGroup.alpha = 1f - t;

            yield return null;
        }

        Destroy(gameObject);
    }
}
