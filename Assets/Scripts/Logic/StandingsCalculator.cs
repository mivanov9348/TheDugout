using System.Collections.Generic;
using System.Linq;
using TheDugout.Data;

namespace TheDugout.Logic
{
    public class StandingRow
    {
        public int TeamId;
        public string TeamName;
        public int Played, Won, Drawn, Lost, GoalsFor, GoalsAgainst;
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points => Won * 3 + Drawn;
    }

    public static class StandingsCalculator
    {
        public static List<StandingRow> Calculate(List<Fixture> fixtures, List<Team> teams)
        {
            var table = teams.ToDictionary(t => t.Id, t => new StandingRow { TeamId = t.Id, TeamName = t.Name });

            foreach (var f in fixtures.Where(f => f.IsPlayed))
            {
                var home = table[f.HomeTeamId];
                var away = table[f.AwayTeamId];

                home.Played++; away.Played++;
                home.GoalsFor += f.HomeGoals.Value; home.GoalsAgainst += f.AwayGoals.Value;
                away.GoalsFor += f.AwayGoals.Value; away.GoalsAgainst += f.HomeGoals.Value;

                if (f.HomeGoals > f.AwayGoals) { home.Won++; away.Lost++; }
                else if (f.HomeGoals < f.AwayGoals) { away.Won++; home.Lost++; }
                else { home.Drawn++; away.Drawn++; }
            }

            return table.Values
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalDifference)
                .ThenByDescending(r => r.GoalsFor)
                .ToList();
        }
    }
}