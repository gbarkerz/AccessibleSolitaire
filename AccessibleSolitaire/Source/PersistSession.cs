
using Microsoft.Maui.Controls;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
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

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return false;
            }

            var preferenceSuffix = "";

            if (currentGameType == SolitaireGameType.Pyramid)
            {
                preferenceSuffix = "Pyramid";
            }
            else if (currentGameType == SolitaireGameType.Tripeaks)
            {
                preferenceSuffix = "Tripeaks";
            }
            else if (currentGameType == SolitaireGameType.Bakersdozen)
            {
                preferenceSuffix = "Bakersdozen";
            }
            else if (currentGameType == SolitaireGameType.Spider)
            {
                preferenceSuffix = "Spider";
            }

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

                if (currentGameType == SolitaireGameType.Spider)
                {
                    Preferences.Set("SpiderDiscardedSequenceCount", vm.SpiderDiscardedSequenceCount);
                }

                savedSession = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveSession: " + ex);
            }

            if (!savedSession)
            {
                Preferences.Set("DeckRemainingSession", "");
                Preferences.Set("DeckUpturnedSession", "");
                Preferences.Set("TargetPilesSession", "");
                Preferences.Set("DealtCardsSession", "");
            }

            // Persist the current time spent playing this game.
            SaveCurrentTimeSpentPlayingStateIfAppropriate();

            return savedSession;
        }

        // If the current game is not paused, persist the total time spent playing the current session.
        // We do this when saving a game when the app is closed, or when switching away from an in-progess
        // game to another type of solitaire game, or when pausing the current game.
        private void SaveCurrentTimeSpentPlayingStateIfAppropriate()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            Debug.WriteLine("SaveCurrentTimeSpentPlayingStateIfAppropriate: " +
                "Check whether to persist the time spent playing this game's session.");

            int timeSpentPlayingPrevious = 0;
            int timeSession = 0;

            if ((currentGameType == SolitaireGameType.Klondike) && !vm.GamePausedKlondike)
            {
                Debug.WriteLine("SaveCurrentTimeSpentPlayingStateIfAppropriate: Persisting Klondike session time.");

                timeSpentPlayingPrevious = (int)Preferences.Get("KlondikeSessionDuration", 0);

                timeSession = (int)(DateTime.Now - timeStartOfThisKlondikeSession).TotalSeconds;

                Preferences.Set("KlondikeSessionDuration", timeSpentPlayingPrevious + timeSession);
            }
            else if ((currentGameType == SolitaireGameType.Pyramid) && !vm.GamePausedPyramid)
            {
                Debug.WriteLine("SaveCurrentTimeSpentPlayingStateIfAppropriate: Persisting Pyramid session time.");

                timeSpentPlayingPrevious = (int)Preferences.Get("PyramidSessionDuration", 0);

                timeSession = (int)(DateTime.Now - timeStartOfThisPyramidSession).TotalSeconds;

                Preferences.Set("PyramidSessionDuration", timeSpentPlayingPrevious + timeSession);
            }
            else if ((currentGameType == SolitaireGameType.Tripeaks) && !vm.GamePausedTripeaks)
            {
                Debug.WriteLine("SaveCurrentTimeSpentPlayingStateIfAppropriate: Persisting Tripeaks session time.");

                timeSpentPlayingPrevious = (int)Preferences.Get("TripeaksSessionDuration", 0);

                timeSession = (int)(DateTime.Now - timeStartOfThisTripeaksSession).TotalSeconds;

                Preferences.Set("TripeaksSessionDuration", timeSpentPlayingPrevious + timeSession);
            }
            else if ((currentGameType == SolitaireGameType.Bakersdozen) && !vm.GamePausedBakersdozen)
            {
                Debug.WriteLine("SaveCurrentTimeSpentPlayingStateIfAppropriate: Persisting Bakersdozen session time.");

                timeSpentPlayingPrevious = (int)Preferences.Get("BakersdozenSessionDuration", 0);

                timeSession = (int)(DateTime.Now - timeStartOfThisBakersdozenSession).TotalSeconds;

                Preferences.Set("BakersdozenSessionDuration", timeSpentPlayingPrevious + timeSession);
            }
            else if ((currentGameType == SolitaireGameType.Spider) && !vm.GamePausedSpider)
            {
                Debug.WriteLine("SaveCurrentTimeSpentPlayingStateIfAppropriate: Persisting Spider session time.");

                timeSpentPlayingPrevious = (int)Preferences.Get("SpiderSessionDuration", 0);

                timeSession = (int)(DateTime.Now - timeStartOfThisSpiderSession).TotalSeconds;

                Preferences.Set("SpiderSessionDuration", timeSpentPlayingPrevious + timeSession);
            }

            Debug.WriteLine("SaveCurrentTimeSpentPlayingStateIfAppropriate: Time persisted spent playing this game. Previous " +
                timeSpentPlayingPrevious + ", Current " + timeSession);
        }

        // Load up the current paused state of the Klondike, Pyramid, Tri Peaks, and Baker's Dozen games.
        private void LoadAllGamesPausedState()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            vm.GamePausedKlondike = (bool)Preferences.Get("GamePausedKlondike", false);
            vm.GamePausedPyramid = (bool)Preferences.Get("GamePausedPyramid", false);
            vm.GamePausedTripeaks = (bool)Preferences.Get("GamePausedTripeaks", false);
            vm.GamePausedBakersdozen = (bool)Preferences.Get("GamePausedBakersdozen", false);
            vm.GamePausedSpider = (bool)Preferences.Get("GamePausedSpider", false);

            Debug.WriteLine("LoadAllGamesPausedState: Loaded current games' paused state. " + 
                "Klondike " + vm.GamePausedKlondike + ", Pyramid " + vm.GamePausedPyramid +
                ", Tripeaks " + vm.GamePausedTripeaks + ", Bakersdozen " + vm.GamePausedBakersdozen +
                ", Spider " + vm.GamePausedSpider);

            SetPauseResumeButtonState();
        }

        // If the current game is not paused, set the time of the current session for the game to be now.
        // We do this when loading up a game when the app is started, or when switching back to an 
        // in-progess game from another type of solitaire game, or when resuming the current game.
        private void SetNowAsStartOfCurrentGameSessionIfAppropriate()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            Debug.WriteLine("SetNowAsStartOfCurrentGameSessionIfAppropriate: Check current paused states.");

            if ((currentGameType == SolitaireGameType.Klondike) && !vm.GamePausedKlondike)
            {
                Debug.WriteLine("SetNowAsStartOfCurrentGameSessionIfAppropriate: Klondike set start of this session to now.");

                timeStartOfThisKlondikeSession = DateTime.Now;
            }
            else if ((currentGameType == SolitaireGameType.Pyramid) && !vm.GamePausedPyramid)
            {
                Debug.WriteLine("SetNowAsStartOfCurrentGameSessionIfAppropriate: Pyramid set start of this session to now.");

                timeStartOfThisPyramidSession = DateTime.Now;
            }
            else if ((currentGameType == SolitaireGameType.Tripeaks) && !vm.GamePausedTripeaks)
            {
                Debug.WriteLine("SetNowAsStartOfCurrentGameSessionIfAppropriate: Tripeaks set start of this session to now.");

                timeStartOfThisTripeaksSession = DateTime.Now;
            }
            else if ((currentGameType == SolitaireGameType.Bakersdozen) && !vm.GamePausedBakersdozen)
            {
                Debug.WriteLine("SetNowAsStartOfCurrentGameSessionIfAppropriate: Barkersdozen set start of this session to now.");

                timeStartOfThisBakersdozenSession = DateTime.Now;
            }
            else if ((currentGameType == SolitaireGameType.Spider) && !vm.GamePausedSpider)
            {
                Debug.WriteLine("SetNowAsStartOfCurrentGameSessionIfAppropriate: Spider set start of this session to now.");

                timeStartOfThisSpiderSession = DateTime.Now;
            }
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

            TargetPileC.Card = null;
            TargetPileD.Card = null;
            TargetPileH.Card = null;
            TargetPileS.Card = null;

            foreach (var dealtCardPile in vm.DealtCards)
            {
                dealtCardPile.Clear();
            }

            if (!IsGameCollectionViewBased())
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