using UnityEngine;
using UnityEngine.SceneManagement;
using TheDugout.Database;
using TMPro;

public class PlayerSetupController : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_Dropdown teamDropdown;

    // по-долу ще запълваме dropdown-а с реални отбори от MasterData

    public void OnStartCareerClicked()
    {
        string username = usernameInput.text;
        int selectedTeamId = /* вземи Id-то на избрания отбор от dropdown-а */ 1; // засега placeholder

        string newSavePath = SaveManager.CreateNewSave(username);
        GameDatabaseManager.Instance.LoadSave(newSavePath);
        GameDatabaseManager.Instance.CreateManagerProfile(username, selectedTeamId);

        SceneManager.LoadScene("Hub");
    }
}