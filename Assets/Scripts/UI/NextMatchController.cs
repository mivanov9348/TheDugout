using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TheDugout.Data;
using TheDugout.Database;
using TheDugout.Logic;

public class NextMatchController : MonoBehaviour
{
    [Header("Top Bar")]
    public TMP_Text scoreText;

    [Header("Decks")]
    public Image awayDeckImage;
    public TMP_Text awayDeckCountText;
    public Image homeDeckImage;
    public TMP_Text homeDeckCountText;

    [Header("Revealed Card")]
    public Image revealedCardImage;
    public TMP_Text revealedCardText;

    [Header("Controls")]
    public Button drawCardButton;
    public Button backButton;

    [Header("Card Back Sprites")]
    public Sprite cardBackSprite; // сложи тук гърба на картите (еднакъв за двете тестета)

    private MatchEngine matchEngine;
    private int homeTeamId, awayTeamId;
    private string homeTeamName, awayTeamName;
    private int currentFixtureId;

    void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("Hub"));
        drawCardButton.onClick.AddListener(OnDrawCardClicked);

        if (cardBackSprite != null)
        {
            awayDeckImage.sprite = cardBackSprite;
            homeDeckImage.sprite = cardBackSprite;
        }

        SetupMatch();
    }

    private void SetupMatch()
    {
        var profile = GameDatabaseManager.Instance.GetManagerProfile();
        var myTeam = GameDatabaseManager.Instance.GetTeamById(profile.ManagedTeamId);

        var activeSeason = GameDatabaseManager.Instance.GetActiveSeason();
        var competition = GameDatabaseManager.Instance.GetLeagueCompetition(activeSeason.Id, myTeam.LeagueId);
        var fixtures = GameDatabaseManager.Instance.GetFixturesByCompetition(competition.Id);

        var nextFixture = fixtures
            .Where(f => !f.IsPlayed && (f.HomeTeamId == myTeam.Id || f.AwayTeamId == myTeam.Id))
            .OrderBy(f => f.Round)
            .FirstOrDefault();

        if (nextFixture == null)
        {
            revealedCardText.text = "No upcoming fixtures found.";
            drawCardButton.interactable = false;
            return;
        }

        homeTeamId = nextFixture.HomeTeamId;
        awayTeamId = nextFixture.AwayTeamId;
        homeTeamName = GameDatabaseManager.Instance.GetTeamById(homeTeamId).Name;
        awayTeamName = GameDatabaseManager.Instance.GetTeamById(awayTeamId).Name;
        currentFixtureId = nextFixture.Id;

        matchEngine = new MatchEngine(GameDatabaseManager.Instance.Connection, homeTeamId, awayTeamId);

        UpdateScoreText();
        UpdateDeckCounts();
        revealedCardText.text = $"{homeTeamName} vs {awayTeamName} - Kick off!";
    }

    private void OnDrawCardClicked()
    {
        if (matchEngine == null || matchEngine.Phase == MatchPhase.Finished) return;

        Card drawn = matchEngine.DrawNext();
        if (drawn == null) return;

        // покажи разкритата карта визуално
        revealedCardText.text = drawn.DisplayName;

        if (!string.IsNullOrEmpty(drawn.FrontSpritePath))
        {
            // ако имаш система за зареждане на спрайтове по път, тук ще я викаш
            // засега placeholder - оставяме предишния sprite или default
        }

        UpdateScoreText();
        UpdateDeckCounts();

        if (matchEngine.Phase == MatchPhase.Finished)
        {
            var result = matchEngine.GetResult();
            GameDatabaseManager.Instance.RecordFixtureResult(currentFixtureId, result.HomeGoals, result.AwayGoals);
            drawCardButton.interactable = false;
            revealedCardText.text = $"FULL TIME: {homeTeamName} {result.HomeGoals} - {result.AwayGoals} {awayTeamName}";
        }
    }

    private void UpdateScoreText()
    {
        var result = matchEngine.GetResult();
        scoreText.text = $"{homeTeamName} {result.HomeGoals} - {result.AwayGoals} {awayTeamName}";
    }

    private void UpdateDeckCounts()
    {
        // по избор - ако искаш да показваш броя оставащи карти, MatchEngine трябва да expose-не тези числа
        // засега пропускаме, добавяме ако потрябва
    }
}