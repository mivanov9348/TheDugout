using System;
using System.Collections.Generic;
using System.Linq;
using TheDugout.Data;
using TheDugout.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSetupController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_Dropdown leagueDropdown;
    public Transform teamGridContent;
    public GameObject teamCardPrefab;
    public Button startCareerButton;
    public GameObject errorText;
    public TMP_Text errorTextLabel;

    [Header("Confirm Popup")]
    public GameObject confirmPopup;
    public TMP_Text confirmMessageText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    private List<string> leagueNames;
    private List<MasterDataReader.TeamOption> currentTeams;
    private string selectedTeamName;
    private GameObject selectedCardInstance;

    void Start()
    {
        PopulateLeagueDropdown();
        leagueDropdown.onValueChanged.AddListener(OnLeagueChanged);
        OnLeagueChanged(0);

        errorText.SetActive(false);
        confirmPopup.SetActive(false);

        // ТУК БЕШЕ ПРОБЛЕМЪТ: Свързваме бутона с кода!
        startCareerButton.onClick.AddListener(OnStartCareerClicked);

        confirmYesButton.onClick.AddListener(ConfirmAndStart);
        confirmNoButton.onClick.AddListener(() => confirmPopup.SetActive(false));
    }

    private void PopulateLeagueDropdown()
    {
        leagueNames = MasterDataReader.GetAllLeagueNames();
        leagueDropdown.ClearOptions();
        leagueDropdown.AddOptions(leagueNames);
    }

    private void OnLeagueChanged(int index)
    {
        string leagueName = leagueNames[index];
        currentTeams = MasterDataReader.GetTeamsByLeague(leagueName);
        PopulateTeamGrid();
    }

    private void PopulateTeamGrid()
    {
        foreach (Transform child in teamGridContent)
        {
            Destroy(child.gameObject);
        }

        selectedTeamName = null;
        selectedCardInstance = null;

        foreach (var team in currentTeams)
        {
            GameObject card = Instantiate(teamCardPrefab, teamGridContent);

            TMP_Text nameText = card.transform.Find("TeamNameText")?.GetComponent<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = team.TeamName;
                nameText.color = Color.white; // По подразбиране текстът е бял
            }

            Image img = card.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.1f, 0.1f, 0.1f); // По подразбиране бутонът е почти черен
            }

            Button cardButton = card.GetComponent<Button>();
            string capturedName = team.TeamName;
            GameObject capturedCard = card;

            cardButton.onClick.AddListener(() => SelectTeam(capturedName, capturedCard));
        }
    }

    private void SelectTeam(string teamName, GameObject cardInstance)
    {
        // Връщаме стария бутон към нормален стил (тъмен фон, бял текст)
        if (selectedCardInstance != null)
        {
            Image prevImg = selectedCardInstance.GetComponent<Image>();
            if (prevImg != null) prevImg.color = new Color(0.1f, 0.1f, 0.1f);

            TMP_Text prevText = selectedCardInstance.transform.Find("TeamNameText")?.GetComponent<TMP_Text>();
            if (prevText != null) prevText.color = Color.white;
        }

        // Оцветяваме новия избран бутон (бял фон, черен текст за контраст)
        Image img = cardInstance.GetComponent<Image>();
        if (img != null) img.color = Color.white;

        TMP_Text currentText = cardInstance.transform.Find("TeamNameText")?.GetComponent<TMP_Text>();
        if (currentText != null) currentText.color = Color.black;

        selectedTeamName = teamName;
        selectedCardInstance = cardInstance;

        HideError();
    }

    // ------- ВАЛИДАЦИЯ -------

    public void OnStartCareerClicked()
    {
        string username = usernameInput.text.Trim();

        // Вече проверяваме и дали случайно не пише текста по подразбиране
        if (string.IsNullOrEmpty(username) || username == "Enter your name...")
        {
            ShowError("Please enter a manager name.");
            return;
        }

        if (string.IsNullOrEmpty(selectedTeamName))
        {
            ShowError("Please select a team.");
            return;
        }

        ShowConfirmPopup(username, selectedTeamName);
    }

    private void ShowError(string message)
    {
        errorTextLabel.text = message;
        errorText.SetActive(true);
    }

    private void HideError()
    {
        errorText.SetActive(false);
    }

    // ------- ПОТВЪРЖДЕНИЕ -------

    private void ShowConfirmPopup(string username, string teamName)
    {
        confirmMessageText.text = $"Start career as <b>{username}</b>\nmanaging <b>{teamName}</b>?";
        confirmPopup.SetActive(true);
    }

    private void ConfirmAndStart()
    {
        confirmPopup.SetActive(false);
        Debug.Log("Step 1: Popup closed");

        string username = usernameInput.text.Trim();

        string newSavePath = SaveManager.CreateNewSave(selectedTeamName);
        Debug.Log("Step 2: Save created at " + newSavePath);

        GameDatabaseManager.Instance.LoadSave(newSavePath);
        Debug.Log("Step 3: Save loaded");

        var allTeamsInSave = GameDatabaseManager.Instance.GetAllTeams();
        Debug.Log("Step 4: Teams count = " + allTeamsInSave.Count);

        var matchedTeam = allTeamsInSave.FirstOrDefault(t => t.Name == selectedTeamName);
        Debug.Log("Step 5: Matched team = " + (matchedTeam != null ? matchedTeam.Name : "NULL"));

        int teamId = matchedTeam != null ? matchedTeam.Id : allTeamsInSave.First().Id;

        var season = GameDatabaseManager.Instance.CreateSeason(1, isActive: true);
        Debug.Log("Step 6: Season created, Id = " + season.Id);

        GameDatabaseManager.Instance.CreateManagerProfile(username, teamId, season.Id);
        Debug.Log("Step 7: Manager profile created");

        var leagueTeams = GameDatabaseManager.Instance.GetTeamsByLeague(matchedTeam.LeagueId);
        Debug.Log("Step 8: League teams count = " + leagueTeams.Count);

        var leagueCompetition = GameDatabaseManager.Instance.CreateCompetition(
            $"League Season {season.Number}",
            CompetitionTypes.League,
            season.Id,
            matchedTeam.LeagueId
        );
        Debug.Log("Step 9: Competition created, Id = " + leagueCompetition.Id);

        var fixtures = TheDugout.Logic.FixtureGenerator.GenerateRoundRobin(
            leagueCompetition.Id, leagueTeams, DateTime.Now
        );
        Debug.Log("Step 10: Fixtures generated, count = " + fixtures.Count);

        GameDatabaseManager.Instance.InsertFixtures(fixtures);
        Debug.Log("Step 11: Fixtures inserted");

        GameDatabaseManager.Instance.GenerateBaseDecksForTeams(leagueTeams.Select(t => t.Id).ToList());
        Debug.Log("Step 12: Decks generated");

        Debug.Log($"Generated {fixtures.Count} fixtures and decks for {leagueTeams.Count} teams, season {season.Number}.");

        SceneManager.LoadScene("Hub");
        Debug.Log("Step 13: Scene loading Hub");
    }
}   