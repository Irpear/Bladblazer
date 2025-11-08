using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private void Start()
    {
        Debug.Log("ScoreManager.Instance is: " + ScoreManager.Instance);

        // Huidige score tonen (alleen van huidige sessie)
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.GetCurrentScore());
        }
        else
        {
            UpdateScoreDisplay(0);
        }

        // Toon ALLE highscores (altijd, ook als er nog geen instance is)
        UpdateAllHighScores();
    }

    private void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.AddListener(UpdateScoreDisplay);
            ScoreManager.Instance.OnHighScoreChanged.AddListener(_ => UpdateAllHighScores());

        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.RemoveListener(UpdateScoreDisplay);
            ScoreManager.Instance.OnHighScoreChanged.RemoveListener(_ => UpdateAllHighScores());
        }
    }

    private void UpdateScoreDisplay(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = newScore.ToString();
            Debug.Log("Score updated to: " + newScore);
        }
    }

    private void UpdateAllHighScores()
    {
        if (highScoreText == null) return;

        int easyHigh = PlayerPrefs.GetInt("HighScore_0", 0);
        int normalHigh = PlayerPrefs.GetInt("HighScore_1", 0);
        int hardHigh = PlayerPrefs.GetInt("HighScore_2", 0);

        highScoreText.text =
            $"<b>Easy highscore</b>\n{easyHigh}\n\n" +
            $"<b>Normal highscore</b>\n{normalHigh}\n\n" +
            $"<b>Hard highscore</b>\n{hardHigh}";

        Debug.Log($"Highscores updated: Easy={easyHigh}, Normal={normalHigh}, Hard={hardHigh}");
        Debug.Log($"[ScoreManager] Difficulty = {GameSettings.Difficulty}");
        Debug.Log($"Saving highscore for key: HighScore_{GameSettings.Difficulty}");

    }
}
