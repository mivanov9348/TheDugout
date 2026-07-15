using SQLite;

namespace TheDugout.Data
{
    [Table("Competitions")]
    public class Competition
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }          

        [NotNull, Indexed]
        public string TypeCode { get; set; }        

        [Indexed]
        public int SeasonId { get; set; }

        public int? LeagueId { get; set; }           

        public Competition() { }

        public Competition(string name, string typeCode, int seasonId, int? leagueId = null)
        {
            Name = name;
            TypeCode = typeCode;
            SeasonId = seasonId;
            LeagueId = leagueId;
        }
    }
}