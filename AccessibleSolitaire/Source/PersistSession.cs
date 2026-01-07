
using Microsoft.Maui.Controls;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Sa11ytaire4All
{
    public partial class MainPage : ContentPage
    {
        public bool SaveSession()
        {
            var savedSession = false;

            if (!OptionKeepGameAcrossSessions)
            {
                return savedSession;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                var preferenceSuffix = (currentGameType == SolitaireGameType.Pyramid ? "Pyramid" : "");

                try
                {
                    var deckRemainingJson = JsonSerializer.Serialize(_deckRemaining);
                    Preferences.Set("DeckRemainingSession" + preferenceSuffix, deckRemainingJson);

                    var deckUpturnedJson = JsonSerializer.Serialize(_deckUpturned);
                    Preferences.Set("DeckUpturnedSession" + preferenceSuffix, deckUpturnedJson);

                    for (var i = 0; i < _targetPiles.Length; ++i)
                    {
                        var targetCardPileJson = JsonSerializer.Serialize(_targetPiles[i]);
                        Preferences.Set("TargetCardPileSession" + i.ToString() + preferenceSuffix, targetCardPileJson);
                    }

                    for (var i = 0; i < vm.DealtCards.Length; ++i)
                    {
                        var dealtCardPileJson = JsonSerializer.Serialize(vm.DealtCards[i]);
                        Preferences.Set("DealtCardsSession" + i.ToString() + preferenceSuffix, dealtCardPileJson);
                    }

                    savedSession = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SaveSession: " + ex);
                }
            }

            if (!savedSession)
            {
                Preferences.Set("DeckRemainingSession", "");
                Preferences.Set("DeckUpturnedSession", "");
                Preferences.Set("TargetPilesSession", "");
                Preferences.Set("DealtCardsSession", "");
            }

            // Persist the current time spent playing this game.
            int timeSpentPlayingPrevious;
            int timeSession;

            if (currentGameType == SolitaireGameType.Klondike)
            {
                timeSpentPlayingPrevious = (int)Preferences.Get("KlondikeSessionDuration", 0);

                timeSession = (int)(DateTime.Now - timeStartOfThisKlondikeSession).TotalSeconds;

                Preferences.Set("KlondikeSessionDuration", timeSpentPlayingPrevious + timeSession);
            }
            else
            {
                timeSpentPlayingPrevious = (int)Preferences.Get("PyramidSessionDuration", 0);

                timeSession = (int)(DateTime.Now - timeStartOfThisPyramidSession).TotalSeconds;

                Preferences.Set("PyramidSessionDuration", timeSpentPlayingPrevious + timeSession);
            }

            Debug.WriteLine("Accessible Solitaire: Persist time spent playing this game. Previous " +
                timeSpentPlayingPrevious + ", Current " + timeSession);

            return savedSession;
        }

        public bool LoadSession()
        {
            bool loadedSession = false;

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return loadedSession;
            }

            var preferenceSuffix = (currentGameType == SolitaireGameType.Pyramid ? "Pyramid" : "");

            Debug.WriteLine("LoadSession: currentGameType " + currentGameType);

            try
            {
                var deckRemainingJson = (string)Preferences.Get(
                                            "DeckRemainingSession" + preferenceSuffix, "");
                if (!string.IsNullOrEmpty(deckRemainingJson))
                {
                    var deckRemaining = JsonSerializer.Deserialize<List<Card>>(deckRemainingJson);
                    if (deckRemaining != null)
                    {
                        foreach (var card in deckRemaining)
                        {
                            _deckRemaining.Add(card);
                        }
                    }
                }

                NextCardDeck.State = GetNextCardPileState();

                var deckUpturnedJson = (string)Preferences.Get(
                                            "DeckUpturnedSession" + preferenceSuffix, "");
                if (!string.IsNullOrEmpty(deckUpturnedJson))
                {
                    var deckUpturned = JsonSerializer.Deserialize<List<Card>>(deckUpturnedJson);
                    if (deckUpturned != null)
                    {
                        foreach (var card in deckUpturned)
                        {
                            _deckUpturned.Add(card);
                        }
                    }
                }

                for (var i = 0; i < _targetPiles.Length; ++i)
                {
                    var targetCardPileJson = (string)Preferences.Get(
                                                "TargetCardPileSession" + i.ToString() + preferenceSuffix, "");
                    if (!string.IsNullOrEmpty(targetCardPileJson))
                    {
                        var targetCardPile = JsonSerializer.Deserialize<ObservableCollection<Card>>(targetCardPileJson);
                        if (targetCardPile != null)
                        {
                            foreach (var card in targetCardPile)
                            {
                                _targetPiles[i].Add(card);
                            }
                        }
                    }
                }

                for (var i = 0; i < cCardPiles; ++i)
                {
                    var dealtCardPileJson = (string)Preferences.Get(
                                                "DealtCardsSession" + i.ToString() + preferenceSuffix, "");
                    if (!string.IsNullOrEmpty(dealtCardPileJson))
                    {
                        var dealtCardPile = JsonSerializer.Deserialize<ObservableCollection<DealtCard>>(dealtCardPileJson);
                        if (dealtCardPile != null)
                        {
                            foreach (var dealtCard in dealtCardPile)
                            {
                                vm.DealtCards[i].Add(dealtCard);
                            }
                        }
                    }
                }

                // Verify the sum of all the piles is a full pack.
                var cardCount = _deckRemaining.Count + _deckUpturned.Count;

                Debug.WriteLine("LoadSession: cardCount " + cardCount);

                foreach(var targetPile in _targetPiles)
                {
                    cardCount += targetPile.Count;
                }

                foreach (var dealtCardPile in vm.DealtCards)
                {
                    // Don't include empty dealt card pile placeholders in the card count.
                    for (int cardIndex = 0; cardIndex < dealtCardPile.Count; ++cardIndex)
                    {
                        var dealtCard = dealtCardPile[cardIndex];

                        // Don't include empty card piles in the card count.
                        if ((dealtCard != null) && (dealtCard.Card != null) && 
                            (dealtCard.CardState != CardState.KingPlaceHolder))
                        {
                            if (cardIndex == dealtCardPile.Count - 1)
                            {
                                dealtCard.IsLastCardInPile = true;
                            }

                            if (dealtCard.CardState == CardState.FaceDown)
                            {
                                dealtCard.FaceDown = true;
                            }

                            ++cardCount;
                        }
                    }
                }

                Debug.WriteLine("LoadSession: FULL cardCount " + cardCount);

                // Barker Todo: Don't assume all cards in the Pyramid game are present and correct.
                if ((cardCount == 52) || (currentGameType == SolitaireGameType.Pyramid))
                {
                    if (currentGameType == SolitaireGameType.Klondike)
                    {
                        // Without this refreshing of the cards' accessible name, the N in MofN is stuck
                        // as it was when the card was added to the pile, and doesn't account for the 
                        // total number of cards added. 
                        for (int i = 0; i < cCardPiles; i++)
                        {
                            var dealtCardPile = (CollectionView)CardPileGrid.FindByName("CardPile" + (i + 1));
                            if (dealtCardPile != null)
                            {
                                RefreshDealtCardPileAccessibleNames(dealtCardPile);
                            }
                        }

                        loadedSession = true;
                    }
                    else if (currentGameType == SolitaireGameType.Pyramid)
                    {
                        loadedSession = DealPyramidCardsPostprocess(false);
                    }

                    if (loadedSession)
                    {
                        RefreshUpperCards();

                        ClearAllSelections(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadSession: " + ex);
            }

            if (!loadedSession)
            {
                ClearAllPiles();
            }

            if (currentGameType == SolitaireGameType.Klondike)
            {
                timeStartOfThisKlondikeSession = DateTime.Now;
            }
            else
            {
                timeStartOfThisPyramidSession = DateTime.Now;
            }

            Debug.WriteLine("Accessible Solitaire: Note time of start of this game session.");

            return loadedSession;
        }

        private void ClearAllPiles()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            _deckRemaining.Clear();
            _deckUpturned.Clear();

            foreach (var targetPile in _targetPiles)
            {
                targetPile.Clear();
            }

            foreach (var dealtCardPile in vm.DealtCards)
            {
                dealtCardPile.Clear();
            }

            if (currentGameType == SolitaireGameType.Pyramid)
            {
                ClearPyramidCardsSelection();
            }
        }

        private void ClearPyramidCardsSelection()
        {
            var pyramidCards = CardPileGridPyramid.Children;

            foreach (var pyramidCard in pyramidCards)
            {
                var card = pyramidCard as CardButton;
                if (card != null)
                {
                    card.IsToggled = false;
                }
            }
        }

        private void RefreshUpperCards()
        {
            SetUpturnedCardsVisuals();

            var targetCardPileCount = _targetPiles[0].Count;
            TargetPileC.Card = (targetCardPileCount > 0 ? _targetPiles[0][targetCardPileCount - 1] : null);

            targetCardPileCount = _targetPiles[1].Count;
            TargetPileD.Card = (targetCardPileCount > 0 ? _targetPiles[1][targetCardPileCount - 1] : null);

            targetCardPileCount = _targetPiles[2].Count;
            TargetPileH.Card = (targetCardPileCount > 0 ? _targetPiles[2][targetCardPileCount - 1] : null);

            targetCardPileCount = _targetPiles[3].Count;
            TargetPileS.Card = (targetCardPileCount > 0 ? _targetPiles[3][targetCardPileCount - 1] : null);
        }
    }
}