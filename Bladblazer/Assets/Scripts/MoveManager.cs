using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Collections;


public class MoveManager : MonoBehaviour
{
    [SerializeField] private ConfirmationPopup leavePopup;
    [SerializeField] private ConfirmationPopup restartPopup;
    [SerializeField] private ConfirmationPopup changeDifficultyPopup;



    public int totalMoves;
    public int movesLeft;
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
            totalMoves = 4;
        }
        else if (GameSettings.Difficulty == 2)
        {
            totalMoves = 3;
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
        if (gameIsOver == false)
        {
            restartPopup.ShowRestartPopup();
        } else
        {
            restartPopup.RestartGame();
        }
        
    }

    public void GoToStart()
    {
        
        if (gameIsOver == false)
        {
            leavePopup.ShowLeavePopup();
        }
        else
        {
            leavePopup.LeaveGame();
        }
    }

    public void GoToDiff()
    {
        
        if (gameIsOver == false)
        {
            changeDifficultyPopup.ShowChangeDifficultyPopup();
        }
        else
        {
            changeDifficultyPopup.ChangeDifficulty();
        }
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
