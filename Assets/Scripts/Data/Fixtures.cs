using System;
using SQLite;

namespace TheDugout.Data
{
    [Table("Fixtures")]
    public class Fixture
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int CompetitionId { get; set; }

        public int Round { get; set; }
        public DateTime MatchDate { get; set; }

        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }

        public int? HomeGoals { get; set; }
        public int? AwayGoals { get; set; }

        public int? WinnerId { get; set; }   

        public bool IsPlayed => HomeGoals.HasValue && AwayGoals.HasValue;

        public Fixture() { }

        public Fixture(int competitionId, int round, DateTime matchDate, int homeTeamId, int awayTeamId)
        {
            CompetitionId = competitionId;
            Round = round;
            MatchDate = matchDate;
            HomeTeamId = homeTeamId;
            AwayTeamId = awayTeamId;
        }
    }
}