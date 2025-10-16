using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{

    //public Text scoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    private void Start()
    {
        Debug.Log("ScoreManager.Instance is: " + ScoreManager.Instance);

        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.GetCurrentScore());
            UpdateHighScoreDisplay(ScoreManager.Instance.GetHighScore());
        }
        else
        {
            UpdateScoreDisplay(0);
            UpdateHighScoreDisplay(0);
        }

    }

    private void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.AddListener(UpdateScoreDisplay);
            ScoreManager.Instance.OnHighScoreChanged.AddListener(UpdateHighScoreDisplay);
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.RemoveListener(UpdateScoreDisplay);
            ScoreManager.Instance.OnHighScoreChanged.RemoveListener(UpdateHighScoreDisplay);
        }
    }

    private void UpdateScoreDisplay(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "" + newScore;
            Debug.Log("Score updated to: " + newScore);
        }

    }

    private void UpdateHighScoreDisplay(int newHighscore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = "Highscore: " + newHighscore;
            Debug.Log("Highscore updated to: " + newHighscore);
        }

    }
}
