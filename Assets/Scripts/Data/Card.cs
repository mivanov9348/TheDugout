using SQLite;

namespace TheDugout.Data
{
    [Table("Cards")]
    public class Card
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, Indexed]
        public string Code { get; set; }

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string FrontSpritePath { get; set; }
        public string BackSpritePath { get; set; }
        public bool IsActive { get; set; } = true;

        public Card() { }

        public Card(string code, string displayName, string description = "")
        {
            Code = code;
            DisplayName = displayName;
            Description = description;
        }
    }
}