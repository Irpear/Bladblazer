using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    
    public void OnClickPlay()
    {
        SceneManager.LoadScene("blokjestest");
    }


}
