using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using TheDugout.Data;

namespace TheDugout.Logic
{
    public enum MatchPhase { InProgress, Finished }

    public class MatchResult
    {
        public int HomeGoals;
        public int AwayGoals;
    }

    public class MatchEngine
    {
        private readonly SQLiteConnection _connection;
        private readonly Random _rng = new Random();

        private List<Card> _homeDrawPile;
        private List<Card> _homeDiscardPile = new List<Card>();
        private List<Card> _awayDrawPile;
        private List<Card> _awayDiscardPile = new List<Card>();

        private List<Card> _cornerDeck;
        private List<Card> _penaltyDeck;

        private int _homeTeamId, _awayTeamId;
        private int _homeGoals, _awayGoals;
        private int _possessionTeamId; // кой отбор тегли в момента
        private int _pendingExtraDraws; // за Yellow/Red ефекти

        public MatchPhase Phase { get; private set; } = MatchPhase.InProgress;
        public List<string> Log { get; } = new List<string>();

        public MatchEngine(SQLiteConnection connection, int homeTeamId, int awayTeamId)
        {
            _connection = connection;
            _homeTeamId = homeTeamId;
            _awayTeamId = awayTeamId;

            _homeDrawPile = BuildShuffledDeck(homeTeamId);
            _awayDrawPile = BuildShuffledDeck(awayTeamId);

            _cornerDeck = LoadStaticDeck("CORNER"); // подготвяш отделни Card редове с Category = "CORNER_OUTCOME" ако решиш да ги добавиш
            _penaltyDeck = LoadStaticDeck("PENALTY_OUTCOME");

            _possessionTeamId = homeTeamId; // домакинът започва
        }

        private List<Card> BuildShuffledDeck(int teamId)
        {
            var deckCards = _connection.Table<TeamDeckCard>().Where(d => d.TeamId == teamId).ToList();
            var allCards = _connection.Table<Card>().ToList();

            var deck = new List<Card>();
            foreach (var entry in deckCards)
            {
                var card = allCards.First(c => c.Id == entry.CardId);
                for (int i = 0; i < entry.Quantity; i++)
                    deck.Add(card);
            }

            Shuffle(deck);
            return deck;
        }

        private List<Card> LoadStaticDeck(string category)
        {
            var cards = _connection.Table<Card>().Where(c => c.Category == category).ToList();
            Shuffle(cards);
            return cards;
        }

        private void Shuffle(List<Card> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Тегли следваща карта за текущия отбор на притежание и обработва ефекта ѝ.
        /// Извиква се на всеки "стъп" от мача (напр. при клик на бутон "Draw" в UI-a).
        /// </summary>
        public Card DrawNext()
        {
            if (Phase == MatchPhase.Finished) return null;

            // ако имаме "принудителни" тегления (Yellow/Red ефект), те се изчерпват първо
            bool isForcedDraw = _pendingExtraDraws > 0;
            if (isForcedDraw) _pendingExtraDraws--;

            Card card = DrawFromTeamPile(_possessionTeamId);
            ResolveCard(card, isForcedDraw);
            return card;
        }

        private Card DrawFromTeamPile(int teamId)
        {
            List<Card> drawPile = teamId == _homeTeamId ? _homeDrawPile : _awayDrawPile;
            List<Card> discardPile = teamId == _homeTeamId ? _homeDiscardPile : _awayDiscardPile;

            if (drawPile.Count == 0)
            {
                // reshuffle discard обратно в draw pile
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                Shuffle(drawPile);
                Log.Add($"Deck reshuffled for team {teamId}.");
            }

            Card card = drawPile[0];
            drawPile.RemoveAt(0);
            discardPile.Add(card);

            return card;
        }

        private void ResolveCard(Card card, bool isForcedDraw)
        {
            Log.Add($"Team {_possessionTeamId} drew: {card.DisplayName}");

            switch (card.EffectType)
            {
                case CardEffectTypes.KeepTurn:
                    // нищо не сменяме
                    break;

                case CardEffectTypes.PassTurn:
                    if (!isForcedDraw) SwitchPossession();
                    // ако е "принудително" теглене (в рамките на Yellow/Red поредица),
                    // не сменяме притежание междинно - продължаваме да броим forced draws
                    break;

                case CardEffectTypes.Goal:
                    RegisterGoal(_possessionTeamId);
                    if (!isForcedDraw) SwitchPossession();
                    break;

                case CardEffectTypes.OpponentDrawTwice:
                    SwitchPossession();
                    _pendingExtraDraws = 2;
                    break;

                case CardEffectTypes.OpponentDrawThrice:
                    SwitchPossession();
                    _pendingExtraDraws = 3;
                    break;

                case CardEffectTypes.OpenCornerDeck:
                    ResolveSubDeck(_cornerDeck);
                    // KeepTurn поведение - не сменяме притежание
                    break;

                case CardEffectTypes.OpenPenaltyDeck:
                    ResolveSubDeck(_penaltyDeck);
                    break;

                case CardEffectTypes.EndMatch:
                    Phase = MatchPhase.Finished;
                    Log.Add($"FULL TIME: {_homeGoals} - {_awayGoals}");
                    break;
            }
        }

        private void ResolveSubDeck(List<Card> subDeck)
        {
            if (subDeck.Count == 0) return;

            Card outcome = subDeck[0];
            subDeck.RemoveAt(0);
            subDeck.Add(outcome); // веднага се връща в края, статичните тестета не се "изчерпват"

            Log.Add($"Sub-deck result: {outcome.DisplayName}");

            if (outcome.EffectType == CardEffectTypes.Goal)
            {
                RegisterGoal(_possessionTeamId);
            }
        }

        private void RegisterGoal(int teamId)
        {
            if (teamId == _homeTeamId) _homeGoals++;
            else _awayGoals++;

            Log.Add($"GOAL! {_homeGoals} - {_awayGoals}");
        }

        private void SwitchPossession()
        {
            _possessionTeamId = _possessionTeamId == _homeTeamId ? _awayTeamId : _homeTeamId;
        }

        public MatchResult GetResult() => new MatchResult { HomeGoals = _homeGoals, AwayGoals = _awayGoals };
        public int CurrentPossessionTeamId => _possessionTeamId;
    }
}