using UnityEngine;
using UnityEngine.SceneManagement;

public class HubController : MonoBehaviour
{
    public void GoToNextMatch()
    {
        SceneManager.LoadScene("NextMatch");
    } 

    public void GoToFixtures()
    {
        SceneManager.LoadScene("Fixtures");
    }

    public void GoToStandings()
    {
        SceneManager.LoadScene("Standings");
    }

    public void ExitHub()
    {
        SceneManager.LoadScene("MainMenu");
    } 
}