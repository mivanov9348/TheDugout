using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnNewGameClicked()
    {
        SceneManager.LoadScene("Dashboard");
    }

    public void OnExitClicked()
    {
        Application.Quit();
        Debug.Log("Quit pressed (не работи в Editor, само в билднатата игра)");
    }
}