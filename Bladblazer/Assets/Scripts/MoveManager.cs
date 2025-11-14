using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    public GameObject pauseScreen;
    public TextMeshProUGUI pauseButtonText;

    public Button pauseButton;

    private Coroutine gameOverCheckRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board = FindFirstObjectByType<Board>();

        if (GameSettings.Difficulty == 0)
        {
            totalMoves = 5;
        }
        else if (GameSettings.Difficulty == 1)
        {
            totalMoves = 5;
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

        if (gameOverCheckRoutine != null)
        {
            StopCoroutine(gameOverCheckRoutine);
        }

        // Start nieuwe check
        gameOverCheckRoutine = StartCoroutine(CheckGameOverWithDelay());
    }

    public void AddExtraMove()
    {
        if (movesLeft < 10)
        {
            movesLeft++;
            UpdateMovesUI();
        }
    }

    void UpdateMovesUI()
    {
        if (movesText != null)
        {
            movesText.text = "" + movesLeft;
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over! No moves left.");
        gameOverScreen.SetActive(true);
        pauseButton.gameObject.SetActive(false);
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

    public void OpenPause()
    {
        if (pauseScreen.activeSelf == false)
        {
            pauseScreen.SetActive(true);
            pauseButtonText.text = "X";
        } else
        {
            ClosePause();
        }
        
    }
    public void ClosePause()
    {
        pauseScreen.SetActive(false);
        pauseButtonText.text = "ll";
    }

    private IEnumerator CheckGameOverWithDelay()
    {
        // Wacht een halve seconde, zodat matches en extra moves verwerkt zijn
        yield return new WaitForSeconds(1.3f);

        if (!gameIsOver && movesLeft <= 0)
        {
            GameOver();
        }

        gameOverCheckRoutine = null;
    }

}
