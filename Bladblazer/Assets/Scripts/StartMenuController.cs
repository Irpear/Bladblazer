using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public GameObject tutorialScreen;

    public void OnClickHome()
    {
        SceneManager.LoadScene("StartScreen");
    }
    public void OnClickHighscores()
    {
        SceneManager.LoadScene("HighscoreScreen");
    }
    public void OnClickPlay()
    {
        SceneManager.LoadScene("DiffScreen");
    }
    public void OnClickPlayEasy()
    {
        GameSettings.Difficulty = 0;
        SceneManager.LoadScene("blokjestest");
        ScoreManager.Instance.ResetScore();
    }

    public void OnClickPlayNormal()
    {
        GameSettings.Difficulty = 1;
        SceneManager.LoadScene("blokjestest");
        ScoreManager.Instance.ResetScore();
    }

    public void OnClickPlayHard()
    {
        GameSettings.Difficulty = 2;
        SceneManager.LoadScene("blokjestest");
        ScoreManager.Instance.ResetScore();
    }

    public void OnClickOpenTutorial()
    {
        tutorialScreen.SetActive(true);
    }
    public void OnClickCloseTutorial()
    {
        tutorialScreen.SetActive(false);
    }

}
