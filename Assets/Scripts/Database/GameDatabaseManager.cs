using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using UnityEngine;
using TheDugout.Data;

namespace TheDugout.Database
{
    public class GameDatabaseManager : MonoBehaviour
    {
        public static GameDatabaseManager Instance { get; private set; }
        private SQLiteConnection _connection;
        public string CurrentSavePath { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadSave(string savePath)
        {
            _connection?.Close();
            CurrentSavePath = savePath;
            _connection = new SQLiteConnection(savePath);

            _connection.CreateTable<Season>();
            _connection.CreateTable<Competition>();
            _connection.CreateTable<Fixture>();

            Debug.Log("Save loaded: " + savePath);
        }

        // ------- Teams -------
        public List<Team> GetAllTeams() => _connection.Table<Team>().ToList();
        public Team GetTeamById(int id) => _connection.Table<Team>().FirstOrDefault(t => t.Id == id);
        public List<Team> GetTeamsByLeague(int leagueId) =>
            _connection.Table<Team>().Where(t => t.LeagueId == leagueId).ToList();
        public void UpdateTeam(Team team) => _connection.Update(team);

        // ------- Leagues / Countries -------
        public List<League> GetLeaguesByCountry(int countryId) =>
            _connection.Table<League>().Where(l => l.CountryId == countryId).ToList();
        public List<Country> GetAllCountries() => _connection.Table<Country>().ToList();

        // ------- Cards -------
        public List<Card> GetAllActiveCards() =>
            _connection.Table<Card>().Where(c => c.IsActive).ToList();

        // ------- Manager Profile -------
        public ManagerProfile CreateManagerProfile(string username, int teamId, int seasonId)
        {
            var profile = new ManagerProfile(username, teamId, seasonId);
            _connection.Insert(profile);
            return profile;
        }

        public ManagerProfile GetManagerProfile() =>
            _connection.Table<ManagerProfile>().FirstOrDefault();

        // ------- Season -------
        public Season CreateSeason(int number, bool isActive = true)
        {
            var season = new Season(number, isActive);
            _connection.Insert(season);
            return season;
        }
        public Season GetActiveSeason() =>
            _connection.Table<Season>().FirstOrDefault(s => s.IsActive);
        public Season GetSeasonById(int id) =>
            _connection.Table<Season>().FirstOrDefault(s => s.Id == id);

        // ------- Competition -------
        public Competition CreateCompetition(string name, string typeCode, int seasonId, int? leagueId = null)
        {
            var comp = new Competition(name, typeCode, seasonId, leagueId);
            _connection.Insert(comp);
            return comp;
        }
        public List<Competition> GetCompetitionsBySeason(int seasonId) =>
            _connection.Table<Competition>().Where(c => c.SeasonId == seasonId).ToList();
        public Competition GetLeagueCompetition(int seasonId, int leagueId) =>
            _connection.Table<Competition>()
                .FirstOrDefault(c => c.SeasonId == seasonId && c.LeagueId == leagueId && c.TypeCode == CompetitionTypes.League);

        // ------- Fixtures -------
        public void InsertFixtures(List<Fixture> fixtures) => _connection.InsertAll(fixtures);

        public List<Fixture> GetFixturesByCompetition(int competitionId) =>
            _connection.Table<Fixture>().Where(f => f.CompetitionId == competitionId).ToList();

        public List<Fixture> GetFixturesByTeam(int teamId, int competitionId) =>
            _connection.Table<Fixture>()
                .Where(f => f.CompetitionId == competitionId && (f.HomeTeamId == teamId || f.AwayTeamId == teamId))
                .ToList();

        public void UpdateFixture(Fixture fixture) => _connection.Update(fixture);

        public void RecordFixtureResult(int fixtureId, int homeGoals, int awayGoals)
        {
            var fixture = _connection.Table<Fixture>().First(f => f.Id == fixtureId);
            fixture.HomeGoals = homeGoals;
            fixture.AwayGoals = awayGoals;

            if (homeGoals > awayGoals) fixture.WinnerId = fixture.HomeTeamId;
            else if (awayGoals > homeGoals) fixture.WinnerId = fixture.AwayTeamId;
            else fixture.WinnerId = null;

            _connection.Update(fixture);
        }

        private void OnApplicationQuit()
        {
            _connection?.Close();
        }
    }
}