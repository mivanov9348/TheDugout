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
    public Image homeDeckImage;

    [Header("Slots")]
    public Image myCardImage;
    public TMP_Text myCardText;
    public Image cpuCardImage;
    public TMP_Text cpuCardText;

    [Header("Controls")]
    public Button drawCardButton;
    public Button backButton;

    [Header("Sprites")]
    public Sprite cardBackSprite;

    [Header("Stats")]
    public TMP_Text cardsDrawnText;
    public TMP_Text possessionText;

    private MatchEngine matchEngine;
    private int homeTeamId, awayTeamId;
    private string homeTeamName, awayTeamName;
    private int currentFixtureId;
    private bool isMyTeamHome;

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
            myCardText.text = "No fixtures.";
            drawCardButton.interactable = false;
            return;
        }

        homeTeamId = nextFixture.HomeTeamId;
        awayTeamId = nextFixture.AwayTeamId;
        homeTeamName = GameDatabaseManager.Instance.GetTeamById(homeTeamId).Name;
        awayTeamName = GameDatabaseManager.Instance.GetTeamById(awayTeamId).Name;
        currentFixtureId = nextFixture.Id;
        isMyTeamHome = (homeTeamId == myTeam.Id);

        matchEngine = new MatchEngine(GameDatabaseManager.Instance.Connection, homeTeamId, awayTeamId);

        UpdateScoreText();
        myCardText.text = "—";
        cpuCardText.text = "—";

        UpdateStatsText();
    }

    private void OnDrawCardClicked()
    {
        if (matchEngine == null || matchEngine.Phase == MatchPhase.Finished) return;

        bool possessionIsHome = matchEngine.CurrentPossessionTeamId == homeTeamId;
        bool possessionIsMine = possessionIsHome == isMyTeamHome;

        Card drawn = matchEngine.DrawNext();
        if (drawn == null) return;

        Sprite cardSprite = LoadCardSprite(drawn.Code);

        if (possessionIsMine)
        {
            myCardText.text = drawn.DisplayName;
            if (cardSprite != null) myCardImage.sprite = cardSprite;
        }
        else
        {
            cpuCardText.text = drawn.DisplayName;
            if (cardSprite != null) cpuCardImage.sprite = cardSprite;
        }

        UpdateScoreText();
        UpdateStatsText();

        if (matchEngine.Phase == MatchPhase.Finished)
        {
            var result = matchEngine.GetResult();
            GameDatabaseManager.Instance.RecordFixtureResult(currentFixtureId, result.HomeGoals, result.AwayGoals);
            drawCardButton.interactable = false;
        }
    }

    private Sprite LoadCardSprite(string cardCode)
    {
        Sprite sprite = Resources.Load<Sprite>("Cards/" + cardCode);
        if (sprite == null)
        {
            Debug.LogWarning($"No sprite found for card code: {cardCode}");
        }
        return sprite;
    }

    private void UpdateScoreText()
    {
        var result = matchEngine.GetResult();
        scoreText.text = $"{homeTeamName} {result.HomeGoals} - {result.AwayGoals} {awayTeamName}";
    }

    private void UpdateStatsText()
    {
        int totalCards = matchEngine.HomeCardsDrawn + matchEngine.AwayCardsDrawn;
        cardsDrawnText.text = $"Cards drawn: {totalCards}";

        int myCards = isMyTeamHome ? matchEngine.HomeCardsDrawn : matchEngine.AwayCardsDrawn;
        int cpuCards = isMyTeamHome ? matchEngine.AwayCardsDrawn : matchEngine.HomeCardsDrawn;

        if (totalCards > 0)
        {
            int myPercent = Mathf.RoundToInt((float)myCards / totalCards * 100);
            possessionText.text = $"Possession: {myPercent}% - {100 - myPercent}%";
        }
        else
        {
            possessionText.text = "Possession: —";
        }
    }
}