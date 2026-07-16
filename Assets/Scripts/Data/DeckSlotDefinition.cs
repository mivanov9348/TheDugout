using SQLite;

namespace TheDugout.Data
{
    [Table("DeckSlotDefinitions")]
    public class DeckSlotDefinition
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, Indexed]
        public string CardCode { get; set; }

        public int MinCount { get; set; }
        public int MaxCount { get; set; }

        public DeckSlotDefinition() { }

        public DeckSlotDefinition(string cardCode, int min, int max)
        {
            CardCode = cardCode;
            MinCount = min;
            MaxCount = max;
        }
    }
}