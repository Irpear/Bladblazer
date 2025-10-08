using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{

    //public Text scoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    private void Start()
    {
        UpdateScoreDisplay(0);
    }

    private void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.AddListener(UpdateScoreDisplay);
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.RemoveListener(UpdateScoreDisplay);
        }
    }

    private void UpdateScoreDisplay(int newScore)
    {
        // newScore is niks
        scoreText.text = "" + newScore;
        Debug.Log("Score updated to: " + newScore);
    }
}
