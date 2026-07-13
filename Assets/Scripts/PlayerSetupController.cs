using UnityEngine;
using UnityEngine.SceneManagement;
using TheDugout.Database;
using TMPro;

public class PlayerSetupController : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_Dropdown teamDropdown;

    public void OnStartCareerClicked()
    {
        if (usernameInput == null)
        {
            Debug.LogError("usernameInput is NULL - not assigned in Inspector!");
            return;
        }

        if (GameDatabaseManager.Instance == null)
        {
            Debug.LogError("GameDatabaseManager.Instance is NULL!");
            return;
        }

        string username = string.IsNullOrEmpty(usernameInput.text) ? "Manager" : usernameInput.text;
        string newSavePath = SaveManager.CreateNewSave(username);
        GameDatabaseManager.Instance.LoadSave(newSavePath);
        GameDatabaseManager.Instance.CreateManagerProfile(username, 1);

        SceneManager.LoadScene("Hub");
    }
}