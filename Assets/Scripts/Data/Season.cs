using SQLite;

namespace TheDugout.Data
{
    [Table("Seasons")]
    public class Season
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int Number { get; set; }      
        public bool IsActive { get; set; }    

        public Season() { }

        public Season(int number, bool isActive = true)
        {
            Number = number;
            IsActive = isActive;
        }
    }
}