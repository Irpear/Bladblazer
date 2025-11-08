using System;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] public int pointsPerBlock = 100;
    private int currentScore = 0;
    private int[] highScores = new int[3]; // 0=Easy,1=Normal,2=Hard
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

        LoadHighScores();
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

    // Laad alle highscores tegelijk
    private void LoadHighScores()
    {
        for (int i = 0; i < highScores.Length; i++)
        {
            highScores[i] = PlayerPrefs.GetInt($"{HIGH_SCORE_KEY}_{i}", 0);
        }
        // Trigger event voor huidige difficulty
        OnHighScoreChanged?.Invoke(highScores[GameSettings.Difficulty]);
    }

    public void CheckAndUpdateHighScore()
    {
        int diff = GameSettings.Difficulty;
        if (currentScore > highScores[diff])
        {
            highScores[diff] = currentScore;
            PlayerPrefs.SetInt($"{HIGH_SCORE_KEY}_{diff}", currentScore);
            PlayerPrefs.Save();
            OnHighScoreChanged?.Invoke(highScores[diff]);
        }
    }

    public int GetCurrentScore()
    {
        Debug.Log("Current Score: " + currentScore);
        return currentScore;
    }

    // Geeft highscore van de huidige difficulty
    public int GetHighScore()
    {
        int diff = GameSettings.Difficulty;
        Debug.Log("Highscore: " + highScores[diff]);
        return highScores[diff];
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    public int GetHighScoreForCurrentDifficulty()
    {
        return highScores[GameSettings.Difficulty];
    }
}


public static class GameSettings
{
    public static int Difficulty = 0;
}
