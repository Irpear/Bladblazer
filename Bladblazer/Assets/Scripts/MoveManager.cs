using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Collections;


public class MoveManager : MonoBehaviour
{

    public int totalMoves;
    private int movesLeft;
    public Text movesText;
    public bool gameIsOver = false;

    public Board board;

    public GameObject gameOverScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board = FindFirstObjectByType<Board>();

        if (GameSettings.Difficulty == 0)
        {
            totalMoves = 10;
        }
        else if (GameSettings.Difficulty == 1)
        {
            totalMoves = 8;
        }
        else if (GameSettings.Difficulty == 2)
        {
            totalMoves = 5;
        }

        
        movesLeft = totalMoves;
        UpdateMovesUI();
    }

    public void UseMove()
    {
        if (gameIsOver) return;
            movesLeft--;
            UpdateMovesUI();
            Debug.Log("Moves left: " + movesLeft);

        StartCoroutine(CheckGameOverWithDelay());
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
        ScoreManager.Instance.ResetScore();
    }

    public void GoToStart()
    {
        SceneManager.LoadScene("StartScreen");
        ScoreManager.Instance.ResetScore();
    }

    public void GoToDiff()
    {
        SceneManager.LoadScene("DiffScreen");
    }

    private IEnumerator CheckGameOverWithDelay()
    {
        // Wacht een halve seconde, zodat matches en extra moves verwerkt zijn
        yield return new WaitForSeconds(1.6f);

        if (movesLeft <= 0)
        {
            GameOver();
        }
    }

}
