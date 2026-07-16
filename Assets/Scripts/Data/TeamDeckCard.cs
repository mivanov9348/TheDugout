using SQLite;

namespace TheDugout.Data
{
    [Table("TeamDeckCards")]
    public class TeamDeckCard
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int TeamId { get; set; }

        [Indexed]
        public int CardId { get; set; }

        public int Quantity { get; set; }

        public TeamDeckCard() { }

        public TeamDeckCard(int teamId, int cardId, int quantity)
        {
            TeamId = teamId;
            CardId = cardId;
            Quantity = quantity;
        }
    }
}