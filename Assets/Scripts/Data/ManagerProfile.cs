using System;
using SQLite;

namespace TheDugout.Data
{
    [Table("ManagerProfile")]
    public class ManagerProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Username { get; set; }

        [Indexed]
        public int ManagedTeamId { get; set; }    // сочи към Team.Id В СЪЩИЯ Save файл

        public DateTime CreatedAt { get; set; }
        public int CurrentSeason { get; set; } = 1;

        public ManagerProfile() { }

        public ManagerProfile(string username, int managedTeamId)
        {
            Username = username;
            ManagedTeamId = managedTeamId;
            CreatedAt = DateTime.Now;
        }
    }
}