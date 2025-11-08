using System;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager: MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] public int pointsPerBlock = 100;
    private int currentScore = 0;
    private int highScore = 0;
    public float currentMultiplier = 1.0f;

    private const string HIGH_SCORE_KEY = "HighScore";

    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int> OnHighScoreChanged;
    public UnityEvent<float> OnMultiplierChanged;

    private void Awake()
    {
        Debug.Log("ScoreManager Awake called");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("ScoreManager Instance set");
        DontDestroyOnLoad(gameObject);

        LoadHighScore();
    }

    private void OnEnable()
    {
        GameEvents.OnBlocksRemoved.AddListener(HandleBlocksRemoved);
    }

    private void OnDisable()
    {
        GameEvents.OnBlocksRemoved.RemoveListener(HandleBlocksRemoved);
    }

    private void HandleBlocksRemoved(int runLength)
    {
        Debug.Log($"HandleBlockRemoved: BlockCOunt={runLength}, currentMultiplier={currentMultiplier}");
        int scoreEarned = Mathf.RoundToInt(runLength * pointsPerBlock * currentMultiplier);
        AddScore(scoreEarned);
    }

    public void SetMultiplier(float multiplier)
    {
        currentMultiplier = multiplier;
        Debug.Log("SetMultiplier called with" + multiplier);
        OnMultiplierChanged?.Invoke(currentMultiplier);
    }

    public float GetMultiplier()
    {
        return currentMultiplier;
    }

    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        OnHighScoreChanged?.Invoke(highScore);
    }

    public void CheckAndUpdateHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    public int GetCurrentScore()
    {
        Debug.Log("Current Score: " + currentScore);
        return currentScore;
    }

    public int GetHighScore()
    {
        Debug.Log("Highscore: " + highScore);
        return highScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }


}


public static class GameSettings
{
    public static int Difficulty = 0;
}
