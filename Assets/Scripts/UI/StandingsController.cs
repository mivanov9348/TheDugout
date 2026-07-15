using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TheDugout.Data;
using TheDugout.Database;
using TheDugout.Logic;

public class StandingsController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown competitionDropdown;
    public Transform rowsContent;
    public GameObject rowPrefab;
    public Button backButton;

    private List<Competition> competitions;
    private int myTeamId;

    void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("Hub"));

        var profile = GameDatabaseManager.Instance.GetManagerProfile();
        myTeamId = profile.ManagedTeamId;

        var activeSeason = GameDatabaseManager.Instance.GetActiveSeason();
        competitions = GameDatabaseManager.Instance.GetCompetitionsBySeason(activeSeason.Id);

        PopulateCompetitionDropdown();

        competitionDropdown.onValueChanged.AddListener(OnCompetitionChanged);

        // избери по подразбиране първенството, в което е твоя отбор
        int defaultIndex = FindMyCompetitionIndex();
        competitionDropdown.value = defaultIndex;
        OnCompetitionChanged(defaultIndex);
    }

    private void PopulateCompetitionDropdown()
    {
        competitionDropdown.ClearOptions();
        List<string> names = competitions.Select(c => c.Name).ToList();
        competitionDropdown.AddOptions(names);
    }

    private int FindMyCompetitionIndex()
    {
        var myTeam = GameDatabaseManager.Instance.GetTeamById(myTeamId);

        for (int i = 0; i < competitions.Count; i++)
        {
            if (competitions[i].TypeCode == CompetitionTypes.League &&
                competitions[i].LeagueId == myTeam.LeagueId)
            {
                return i;
            }
        }
        return 0;
    }

    private void OnCompetitionChanged(int index)
    {
        Competition selected = competitions[index];

        var fixtures = GameDatabaseManager.Instance.GetFixturesByCompetition(selected.Id);

        List<Team> teams;
        if (selected.LeagueId.HasValue)
            teams = GameDatabaseManager.Instance.GetTeamsByLeague(selected.LeagueId.Value);
        else
            teams = new List<Team>(); // за Cup/EuroCup по-нататък ще имаме отделна логика за участници

        var standings = StandingsCalculator.Calculate(fixtures, teams);

        PopulateRows(standings);
    }

    private void PopulateRows(List<StandingRow> standings)
    {
        foreach (Transform child in rowsContent)
        {
            Destroy(child.gameObject);
        }

        int position = 1;
        foreach (var row in standings)
        {
            GameObject rowObj = Instantiate(rowPrefab, rowsContent);
            rowObj.SetActive(true);

            SetText(rowObj, "PosText", position.ToString());
            SetText(rowObj, "TeamNameText", row.TeamName);  
            SetText(rowObj, "PlayedText", row.Played.ToString());
            SetText(rowObj, "WonText", row.Won.ToString());
            SetText(rowObj, "DrawnText", row.Drawn.ToString());
            SetText(rowObj, "LostText", row.Lost.ToString());
            SetText(rowObj, "GFText", row.GoalsFor.ToString());
            SetText(rowObj, "GAText", row.GoalsAgainst.ToString());
            SetText(rowObj, "GDText", row.GoalDifference.ToString());
            SetText(rowObj, "PointsText", row.Points.ToString());

            if (row.TeamId == myTeamId)
            {
                Image bg = rowObj.GetComponent<Image>();
                if (bg != null) bg.color = new Color(0.3f, 0.8f, 0.4f, 0.5f); // зелен полу-прозрачен highlight
            }

            position++;
        }
    }

    private void SetText(GameObject row, string childName, string value)
    {
        Transform child = row.transform.Find(childName);
        if (child != null)
        {
            TMP_Text text = child.GetComponent<TMP_Text>();
            if (text != null) text.text = value;
        }
        else
        {
            // Това ще ти покаже точно кое име не съвпада!
            Debug.LogError($"[ГРЕШКА] Не мога да намеря обект с име '{childName}' в Prefab-а!");
        }
    }
}