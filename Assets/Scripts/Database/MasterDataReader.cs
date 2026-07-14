using System.IO;
using TheDugout.Data.DTO;
using UnityEngine;

namespace TheDugout.Database
{
    public static class MasterDataReader
    {
        public class TeamOption
        {
            public string TeamName;
            public string LeagueName;
            public string LogoPath;
        }

        public static System.Collections.Generic.List<string> GetAllLeagueNames()
        {
            var leagues = LoadJsonArray<LeagueDto>("leagues.json");
            var names = new System.Collections.Generic.List<string>();
            foreach (var l in leagues) names.Add(l.name);
            return names;
        }

        public static System.Collections.Generic.List<TeamOption> GetTeamsByLeague(string leagueName)
        {
            var teams = LoadJsonArray<TeamDto>("teams.json");
            var result = new System.Collections.Generic.List<TeamOption>();

            foreach (var t in teams)
            {
                if (t.leagueName == leagueName)
                {
                    result.Add(new TeamOption
                    {
                        TeamName = t.name,
                        LeagueName = t.leagueName,
                        LogoPath = t.logoPath
                    });
                }
            }
            return result;
        }

        private static T[] LoadJsonArray<T>(string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "MasterData", fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogError("Missing JSON file: " + filePath);
                return new T[0];
            }

            string rawJson = File.ReadAllText(filePath);
            string wrapped = "{\"items\":" + rawJson + "}";
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return wrapper.items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }
}