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

        public int Strength { get; set; }
        public int Budget { get; set; }   

        public Team() { }

        public Team(string name, int leagueId, int strength, int budget)
        {
            Name = name;
            LeagueId = leagueId;
            Strength = strength;
            Budget = budget;
        }
    }
}