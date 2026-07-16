using System.Collections.Generic;
using System.Linq;
using SQLite;
using TheDugout.Data;

namespace TheDugout.Logic
{
    public static class DeckBuilder
    {
        /// <summary>
        /// Създава базовото тесте (Min количество от всяка карта) за даден отбор.
        /// Извиква се веднъж при New Game, за всеки Team поотделно (играч + всички CPU отбори).
        /// </summary>
        public static List<TeamDeckCard> BuildBaseDeck(SQLiteConnection connection, int teamId)
        {
            var slotDefinitions = connection.Table<DeckSlotDefinition>().ToList();
            var cards = connection.Table<Card>().ToList();

            var result = new List<TeamDeckCard>();

            foreach (var slot in slotDefinitions)
            {
                var card = cards.FirstOrDefault(c => c.Code == slot.CardCode);
                if (card == null) continue;

                result.Add(new TeamDeckCard(teamId, card.Id, slot.MinCount));
            }

            return result;
        }

        /// <summary>
        /// Добавя количество карти от дадена категория към тестето на отбора,
        /// избирайки конкретния Code чрез DropWeight (претеглен избор) - това е "Store покупка" логиката.
        /// Респектира Max лимита от DeckSlotDefinition - ако вече е на таван, покупката се "прелива" в следваща по тегло опция.
        /// </summary>
        public static Card RollCardFromCategory(SQLiteConnection connection, string category, System.Random rng)
        {
            var candidates = connection.Table<Card>()
                .Where(c => c.Category == category && c.IsActive)
                .ToList();

            if (candidates.Count == 0) return null;

            int totalWeight = candidates.Sum(c => c.DropWeight);
            int roll = rng.Next(0, totalWeight);

            int cumulative = 0;
            foreach (var card in candidates)
            {
                cumulative += card.DropWeight;
                if (roll < cumulative)
                    return card;
            }

            return candidates.Last(); // fallback, не би трябвало да се стигне дотук
        }

        /// <summary>
        /// Добавя 1 брой от изтеглената (чрез RollCardFromCategory) карта към тестето на отбора,
        /// зачитайки Max лимита. Връща false ако вече е на таван (покупката не се "губи" - извикващият код решава какво да прави).
        /// </summary>
        public static bool AddCardToDeck(SQLiteConnection connection, int teamId, int cardId)
        {
            var card = connection.Table<Card>().FirstOrDefault(c => c.Id == cardId);
            if (card == null) return false;

            var slotDef = connection.Table<DeckSlotDefinition>().FirstOrDefault(s => s.CardCode == card.Code);
            int maxAllowed = slotDef != null ? slotDef.MaxCount : int.MaxValue;

            var existing = connection.Table<TeamDeckCard>()
                .FirstOrDefault(t => t.TeamId == teamId && t.CardId == cardId);

            if (existing != null)
            {
                if (existing.Quantity >= maxAllowed) return false;
                existing.Quantity++;
                connection.Update(existing);
            }
            else
            {
                if (maxAllowed < 1) return false;
                connection.Insert(new TeamDeckCard(teamId, cardId, 1));
            }

            return true;
        }
    }
}