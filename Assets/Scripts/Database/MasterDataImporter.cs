using Assets.Scripts.DTO;
using SQLite;
using System.Collections.Generic;
using System.IO;
using TheDugout.Data;
using TheDugout.Data.DTO;
using UnityEngine;

namespace TheDugout.Database
{
    public static class MasterDataImporter
    {
        public static void ImportInto(SQLiteConnection connection)
        {
            string basePath = Path.Combine(Application.streamingAssetsPath, "MasterData");

            var countries = LoadJsonArray<CountryDto>(Path.Combine(basePath, "countries.json"));
            var countryLookup = new Dictionary<string, int>();
            foreach (var c in countries)
            {
                var entity = new Country(c.name, c.code);
                connection.Insert(entity);
                countryLookup[c.code] = entity.Id;
            }

            var leagues = LoadJsonArray<LeagueDto>(Path.Combine(basePath, "leagues.json"));
            var leagueLookup = new Dictionary<string, int>();
            foreach (var l in leagues)
            {
                int countryId = countryLookup[l.countryCode];
                var entity = new League(l.name, countryId, l.tier);
                connection.Insert(entity);
                leagueLookup[l.name] = entity.Id;
            }

            var teams = LoadJsonArray<TeamDto>(Path.Combine(basePath, "teams.json"));
            foreach (var t in teams)
            {
                int leagueId = leagueLookup[t.leagueName];
                connection.Insert(new Team(t.name, leagueId, t.logoPath));
            }

            var cards = LoadJsonArray<CardDto>(Path.Combine(basePath, "cards.json"));
            foreach (var c in cards)
            {
                connection.Insert(new Card(c.code, c.category, c.effectType, c.displayName, c.description, c.dropWeight));
            }

            var deckSlots = LoadJsonArray<DeckSlotDto>(Path.Combine(basePath, "deckslots.json"));
            foreach (var d in deckSlots)
            {
                connection.Insert(new DeckSlotDefinition(d.cardCode, d.min, d.max));
            }

            Debug.Log($"Imported: {countries.Length} countries, {leagues.Length} leagues, {teams.Length} teams, {cards.Length} cards, {deckSlots.Length} deck slots.");
        }

        private static T[] LoadJsonArray<T>(string filePath)
        {
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