using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonController : MonoBehaviour
{
    public void GoBackToHub()
    {
        SceneManager.LoadScene("Hub");
    }
}