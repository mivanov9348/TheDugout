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

        /// <summary>
        /// Извиква се веднага след SaveManager.CreateNewSave(), за да "отвориш" тази игра.
        /// </summary>
        public void LoadSave(string savePath)
        {
            _connection?.Close();

            CurrentSavePath = savePath;
            _connection = new SQLiteConnection(savePath);

            // тази таблица не съществува в MasterData, само в Save файловете - създаваме я тук
            _connection.CreateTable<ManagerProfile>();

            Debug.Log("Save loaded: " + savePath);
        }

        // ------- Teams -------
        public List<Team> GetAllTeams() => _connection.Table<Team>().ToList();
        public Team GetTeamById(int id) => _connection.Table<Team>().FirstOrDefault(t => t.Id == id);
        public void UpdateTeam(Team team) => _connection.Update(team);

        // ------- Leagues / Countries -------
        public List<League> GetLeaguesByCountry(int countryId) =>
            _connection.Table<League>().Where(l => l.CountryId == countryId).ToList();

        public List<Country> GetAllCountries() => _connection.Table<Country>().ToList();

        // ------- Cards -------
        public List<Card> GetAllActiveCards() =>
            _connection.Table<Card>().Where(c => c.IsActive).ToList();

        // ------- Manager Profile -------
        public void CreateManagerProfile(string username, int teamId)
        {
            var profile = new ManagerProfile(username, teamId);
            _connection.Insert(profile);
        }

        public ManagerProfile GetManagerProfile() =>
            _connection.Table<ManagerProfile>().FirstOrDefault();

        private void OnApplicationQuit()
        {
            _connection?.Close();
        }
    }
}