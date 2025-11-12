using TMPro;
using UnityEngine;

public class GameHighscoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScoreText;

    private void Start()
    {
        if (highScoreText == null)
        {
            Debug.LogWarning("HighScoreText is not assigned in the Inspector.");
            return;
        }

        // Toon direct de highscore bij het starten van de scene
        UpdateCurrentHighScore();

        // Luister naar updates (bijv. bij Game Over)
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnHighScoreChanged.AddListener(_ => UpdateCurrentHighScore());
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnHighScoreChanged.RemoveListener(_ => UpdateCurrentHighScore());
        }
    }

    private void UpdateCurrentHighScore()
    {
        int diff = GameSettings.Difficulty;
        int currentHigh = PlayerPrefs.GetInt($"HighScore_{diff}", 0);

        highScoreText.text = $"Highscore: {currentHigh}";

        Debug.Log($"[GameHighscoreDisplay] Showing highscore for diff {diff}: {currentHigh}");
    }
}
