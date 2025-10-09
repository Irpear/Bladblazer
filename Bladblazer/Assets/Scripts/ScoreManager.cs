using UnityEngine;
using UnityEngine.Events;

public class ScoreManager: MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] public int pointsPerBlock = 100;
    private int currentScore = 0;

    public UnityEvent<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        int scoreEarned = runLength * pointsPerBlock;
        AddScore(scoreEarned);
    }

    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }

    public int GetCurrentScore()
    {
        Debug.Log("Current Score: " + currentScore);
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }


}
