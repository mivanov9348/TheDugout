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

        [NotNull, Indexed]
        public string Category { get; set; }            

        [NotNull]
        public string EffectType { get; set; }          

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string FrontSpritePath { get; set; }
        public string BackSpritePath { get; set; }

        public int DropWeight { get; set; } = 1;          
        public bool IsActive { get; set; } = true;

        public Card() { }

        public Card(string code, string category, string effectType, string displayName,
            string description = "", int dropWeight = 1)
        {
            Code = code;
            Category = category;
            EffectType = effectType;
            DisplayName = displayName;
            Description = description;
            DropWeight = dropWeight;
        }
    }
}