using SQLite;

namespace TheDugout.Data
{
    [Table("Teams")]
    public class Team
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [Indexed]
        public int LeagueId { get; set; }

        public string LogoPath { get; set; }

        public Team() { }

        public Team(string name, int leagueId, string logoPath = "")
        {
            Name = name;
            LeagueId = leagueId;
            LogoPath = logoPath;
        }
    }
}