using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{

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
    }

    public void OnClickPlayNormal()
    {
        GameSettings.Difficulty = 1;
        SceneManager.LoadScene("blokjestest");
    }

    public void OnClickPlayHard()
    {
        GameSettings.Difficulty = 2;
        SceneManager.LoadScene("blokjestest");
    }

}
