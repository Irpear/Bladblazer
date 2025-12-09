using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmationPopup : MonoBehaviour
{
    [SerializeField] private GameObject leavePanel;
    [SerializeField] private GameObject restartPanel;
    [SerializeField] private GameObject changeDifficultyPanel;


    public ScoreManager scoreManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideLeavePopup();
        HideRestartPopup();
        HideChangeDifficultyPopup();
    }

    // Update is called once per frame
    public void ShowLeavePopup()
    {
        leavePanel.SetActive(true);
    }

    private void HideLeavePopup()
    {
        leavePanel.SetActive(false);
    }

    public void ShowRestartPopup()
    {
        restartPanel.SetActive(true);
    }

    public void HideRestartPopup()
    {
        restartPanel.SetActive(false);
    }

    public void ShowChangeDifficultyPopup()
    {
        changeDifficultyPanel.SetActive(true);
    }

    public void HideChangeDifficultyPopup()
    {
        changeDifficultyPanel.SetActive(false);
    }

    public void LeaveGame()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneManager.LoadScene("StartScreen");
    }

    public void RestartGame()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ChangeDifficulty()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneManager.LoadScene("DiffScreen");

    }

    public void StayInGame()
    {
        HideLeavePopup();
        HideRestartPopup();
        HideChangeDifficultyPopup();
    }
}
