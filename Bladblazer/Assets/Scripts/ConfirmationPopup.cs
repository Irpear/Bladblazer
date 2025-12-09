using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmationPopup : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    public ScoreManager scoreManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HidePopup();
    }

    // Update is called once per frame
    public void ShowPopup()
    {
        popupPanel.SetActive(true);
    }

    private void HidePopup()
    {
        popupPanel.SetActive(false);
    }

    public void LeaveGame()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneManager.LoadScene("StartScreen");
    }

    public void StayInGame()
    {
        HidePopup();
    }
}
