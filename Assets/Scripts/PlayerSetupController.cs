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

        string username = usernameInput.text.Trim();

        string newSavePath = SaveManager.CreateNewSave(selectedTeamName);
        GameDatabaseManager.Instance.LoadSave(newSavePath);

        var allTeamsInSave = GameDatabaseManager.Instance.GetAllTeams();
        var matchedTeam = allTeamsInSave.FirstOrDefault(t => t.Name == selectedTeamName);
        int teamId = matchedTeam != null ? matchedTeam.Id : allTeamsInSave.First().Id;

        var season = GameDatabaseManager.Instance.CreateSeason(1, isActive: true);
        GameDatabaseManager.Instance.CreateManagerProfile(username, teamId, season.Id);

        var leagueTeams = GameDatabaseManager.Instance.GetTeamsByLeague(matchedTeam.LeagueId);
        var leagueCompetition = GameDatabaseManager.Instance.CreateCompetition(
            $"League Season {season.Number}",
            CompetitionTypes.League,
            season.Id,
            matchedTeam.LeagueId
        );

        var fixtures = TheDugout.Logic.FixtureGenerator.GenerateRoundRobin(
            leagueCompetition.Id, leagueTeams, DateTime.Now
        );
        GameDatabaseManager.Instance.InsertFixtures(fixtures);

        // ---- Генериране на базово тесте за ВСЕКИ отбор в лигата (играч + CPU) ----
        GameDatabaseManager.Instance.GenerateBaseDecksForTeams(leagueTeams.Select(t => t.Id).ToList());

        Debug.Log($"Generated {fixtures.Count} fixtures and decks for {leagueTeams.Count} teams, season {season.Number}.");

        SceneManager.LoadScene("Hub");
    }
}   