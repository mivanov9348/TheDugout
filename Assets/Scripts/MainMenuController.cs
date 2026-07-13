using UnityEngine;
using UnityEngine.SceneManagement;
using TheDugout.Database;

public class MainMenuController : MonoBehaviour
{
    public LoadGamePopupController loadGamePopup;

    public void OnNewGameClicked()
    {
        SceneManager.LoadScene("PlayerSetup");
    }

    public void OnLoadGameClicked()
    {
        loadGamePopup.OpenPopup();
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }
}