using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MoveManager : MonoBehaviour
{

    public int totalMoves = 10;
    private int movesLeft;
    public Text movesText;
    public bool gameIsOver = false;

    public GameObject gameOverScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
