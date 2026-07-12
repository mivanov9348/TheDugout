using SQLite;

namespace TheDugout.Data
{
    [Table("Countries")]
    public class Country
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        public string Code { get; set; }

        public Country() { }

        public Country(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }
}