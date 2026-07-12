using System.IO;
using System.Linq;
using SQLite;
using UnityEngine;
using TheDugout.Data;

namespace TheDugout.Database
{
    public class MasterDatabaseManager : MonoBehaviour
    {
        public static MasterDatabaseManager Instance { get; private set; }
        private SQLiteConnection _connection;

        public static string MasterDbPath =>
            Path.Combine(Application.persistentDataPath, "MasterData.db");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitMasterDatabase();
        }

        private void InitMasterDatabase()
        {
            _connection = new SQLiteConnection(MasterDbPath);

            _connection.CreateTable<Country>();
            _connection.CreateTable<League>();
            _connection.CreateTable<Team>();
            _connection.CreateTable<Card>();

            SeedIfEmpty();

            Debug.Log("MasterData ready at: " + MasterDbPath);
        }

        private void SeedIfEmpty()
        {
            if (_connection.Table<Country>().Count() > 0) return; // вече е seed-нато, не повтаряй

            var bg = new Country("Bulgaria", "BG");
            var en = new Country("England", "EN");
            _connection.Insert(bg);
            _connection.Insert(en);

            var efbetLiga = new League("Efbet Liga", bg.Id, 1);
            var premierLeague = new League("Premier League", en.Id, 1);
            _connection.Insert(efbetLiga);
            _connection.Insert(premierLeague);

            _connection.Insert(new Team("Levski Sofia", efbetLiga.Id, 75, 2000000));
            _connection.Insert(new Team("CSKA Sofia", efbetLiga.Id, 72, 1800000));
            _connection.Insert(new Team("Arsenal", premierLeague.Id, 90, 150000000));

            _connection.Insert(new Card("PASS", "Pass", "Retains possession"));
            _connection.Insert(new Card("GOAL", "GOAL!", "Automatic goal"));
            _connection.Insert(new Card("CHANCE", "Chance", "Chance to score based on team strength"));
            _connection.Insert(new Card("OFFSIDE", "Offside", "Loses possession immediately"));
            _connection.Insert(new Card("TACKLE", "Tackle", "Usually loses possession"));
            _connection.Insert(new Card("PENALTY", "Penalty", "Penalty mini-game"));
            _connection.Insert(new Card("INJURY", "Injury", "Weakens team"));
            _connection.Insert(new Card("RED_CARD", "Red Card", "Player sent off"));
            _connection.Insert(new Card("END_OF_HALF", "End of Half", "Ends current half"));

            Debug.Log("MasterData seeded.");
        }

        // Четенето от MasterData ще ни трябва само за: показване на списък отбори при PlayerSetup

        public System.Collections.Generic.List<Team> GetAllTeams() =>
            _connection.Table<Team>().ToList();

        public System.Collections.Generic.List<Country> GetAllCountries() =>
            _connection.Table<Country>().ToList();

        private void OnApplicationQuit()
        {
            _connection?.Close();
        }
    }
}