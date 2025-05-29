
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
                try
                {
                    var deckRemainingJson = JsonSerializer.Serialize(_deckRemaining);
                    Preferences.Set("DeckRemainingSession", deckRemainingJson);

                    var deckUpturnedJson = JsonSerializer.Serialize(_deckUpturned);
                    Preferences.Set("DeckUpturnedSession", deckUpturnedJson);

                    for (var i = 0; i < _targetPiles.Length; ++i)
                    {
                        var targetCardPileJson = JsonSerializer.Serialize(_targetPiles[i]);
                        Preferences.Set("TargetCardPileSession" + i.ToString(), targetCardPileJson);
                    }

                    for (var i = 0; i < vm.DealtCards.Length; ++i)
                    {
                        var dealtCardPileJson = JsonSerializer.Serialize(vm.DealtCards[i]);
                        Preferences.Set("DealtCardsSession" + i.ToString(), dealtCardPileJson);
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

            try
            {
                var deckRemainingJson = (string)Preferences.Get("DeckRemainingSession", "");
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

                NextCardDeck.IsEmpty = (_deckRemaining.Count == 0);

                var deckUpturnedJson = (string)Preferences.Get("DeckUpturnedSession", "");
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
                    var targetCardPileJson = (string)Preferences.Get("TargetCardPileSession" + i.ToString(), "");
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
                    var dealtCardPileJson = (string)Preferences.Get("DealtCardsSession" + i.ToString(), "");
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
                
                foreach(var targetPile in _targetPiles)
                {
                    cardCount += targetPile.Count;
                }

                foreach (var dealtCardPile in vm.DealtCards)
                {
                    // Don't include empty dealt card pile placeholders in the card count.
                    foreach (var dealtCard in dealtCardPile)
                    {
                        dealtCard.CardSelected = false;
                        dealtCard.InSelectedSet = false;

                        if (dealtCard.CardState != CardState.KingPlaceHolder)
                        {
                            ++cardCount;
                        }
                    }
                }

                if (cardCount == 52)
                {
                    loadedSession = true;

                    RefreshUpperCards();

                    ClearAllSelections(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadSession: " + ex);
            }

            if (!loadedSession)
            {
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
            }

            return loadedSession;
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