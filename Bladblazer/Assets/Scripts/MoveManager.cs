using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MoveManager : MonoBehaviour
{

    public int totalMoves = 10;
    private int movesLeft;
    public Text movesText;
    public bool gameIsOver = false;

    public Board board;

    public GameObject gameOverScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board = FindFirstObjectByType<Board>();
        movesLeft = totalMoves;
    }

    public void UseMove()
    {
        if (gameIsOver) return;
            movesLeft--;
            UpdateMovesUI();
            Debug.Log("Moves left: " + movesLeft);

        if (movesLeft <= 0)
        {
            GameOver();
        }
    }

    public void AddExtraMove()
    {
        movesLeft++;
        UpdateMovesUI();
    }

    void UpdateMovesUI()
    {
        if (movesText != null)
        {
            movesText.text = "Moves: " + movesLeft;
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over! No moves left.");
        gameOverScreen.SetActive(true);
        gameIsOver = true;
        ScoreManager.Instance.CheckAndUpdateHighScore();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToStart()
    {
        SceneManager.LoadScene("StartScreen");
    }
}
