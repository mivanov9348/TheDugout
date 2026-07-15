using System;
using System.Collections.Generic;
using System.Linq;
using TheDugout.Data;

namespace TheDugout.Logic
{
    public static class FixtureGenerator
    {
        public static List<Fixture> GenerateRoundRobin(int competitionId, List<Team> teams, DateTime startDate)
        {
            var fixtures = new List<Fixture>();
            var teamIds = teams.Select(t => t.Id).ToList();

            if (teamIds.Count % 2 != 0)
                teamIds.Add(-1); // "bye" отбор при нечетен брой

            int numRounds = teamIds.Count - 1;
            int half = teamIds.Count / 2;
            var rotating = new List<int>(teamIds);

            for (int round = 0; round < numRounds; round++)
            {
                for (int i = 0; i < half; i++)
                {
                    int home = rotating[i];
                    int away = rotating[rotating.Count - 1 - i];

                    if (home != -1 && away != -1)
                    {
                        fixtures.Add(new Fixture(competitionId, round + 1,
                            startDate.AddDays(round * 7), home, away));
                    }
                }

                // ротация - първият остава фиксиран, останалите се въртят
                var last = rotating[rotating.Count - 1];
                rotating.RemoveAt(rotating.Count - 1);
                rotating.Insert(1, last);
            }

            // втори полусезон - реванши с обърнати домакин/гост
            int firstHalfCount = fixtures.Count;
            for (int i = 0; i < firstHalfCount; i++)
            {
                var f = fixtures[i];
                fixtures.Add(new Fixture(competitionId, f.Round + numRounds,
                    f.MatchDate.AddDays(numRounds * 7), f.AwayTeamId, f.HomeTeamId));
            }

            return fixtures;
        }
    }
}