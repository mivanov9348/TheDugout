using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnNewGameClicked()
    {
        SceneManager.LoadScene("PlayerSetup");
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }
}