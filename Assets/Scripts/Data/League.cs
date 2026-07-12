using SQLite;

namespace TheDugout.Data
{
    [Table("Leagues")]
    public class League
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [Indexed]
        public int CountryId { get; set; }

        public int Tier { get; set; }

        public League() { }

        public League(string name, int countryId, int tier)
        {
            Name = name;
            CountryId = countryId;
            Tier = tier;
        }
    }
}