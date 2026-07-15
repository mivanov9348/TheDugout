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
        public int ManagedTeamId { get; set; }

        [Indexed]
        public int CurrentSeasonId { get; set; }   

        public DateTime CreatedAt { get; set; }

        public ManagerProfile() { }

        public ManagerProfile(string username, int managedTeamId, int currentSeasonId)
        {
            Username = username;
            ManagedTeamId = managedTeamId;
            CurrentSeasonId = currentSeasonId;
            CreatedAt = DateTime.Now;
        }
    }
}