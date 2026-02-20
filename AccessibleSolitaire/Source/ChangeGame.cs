using System.Diagnostics;

using Sa11ytaire4All.ViewModels;

namespace Sa11ytaire4All
{
    public partial class MainPage : ContentPage
    {
        static public SolitaireGameType currentGameType;

        public void LoadKlondikeGame()
        {
            var previousGame = currentGameType;
            if (previousGame == SolitaireGameType.Klondike)
            {
                return;
            }

            ChangeGameType(SolitaireGameType.Klondike);

            ComleteVisualsUpdateFollowingGameChange();
        }


        public void LoadBakersdozenGame()
        {
            var previousGame = currentGameType;
            if (previousGame == SolitaireGameType.Bakersdozen)
            {
                return;
            }

            ChangeGameType(SolitaireGameType.Bakersdozen);

            ComleteVisualsUpdateFollowingGameChange();
        }

        public void LoadPyramidGame()
        {
            var previousGame = currentGameType;
            if (previousGame == SolitaireGameType.Pyramid)
            {
                return;
            }

            ChangeGameType(SolitaireGameType.Pyramid);

            ArrangePyramidButtons();

            ComleteVisualsUpdateFollowingGameChange();
        }

        public void LoadTripeaksGame()
        {
            var previousGame = currentGameType;
            if (previousGame == SolitaireGameType.Tripeaks)
            {
                return;
            }

            ChangeGameType(SolitaireGameType.Tripeaks);

            ArrangePyramidButtons();

            ComleteVisualsUpdateFollowingGameChange();
        }

        public void LoadSpiderGame()
        {
            var previousGame = currentGameType;
            if (previousGame == SolitaireGameType.Spider)
            {
                return;
            }

            ChangeGameType(SolitaireGameType.Spider);

            ComleteVisualsUpdateFollowingGameChange();
        }

        public void LoadRoyalparadeGame()
        {
            var previousGame = currentGameType;
            if (previousGame == SolitaireGameType.Royalparade)
            {
                return;
            }

            ChangeGameType(SolitaireGameType.Royalparade);

            ComleteVisualsUpdateFollowingGameChange();
        }

        private void ComleteVisualsUpdateFollowingGameChange()
        {
            SetRemainingCardUIVisibility();

            // The number of rows in the main area may need to be updated based on the current game.
            SetOrientationLayout();

            ResizeDealtCard(false);

            RefreshAllCardVisuals();

            RefreshDealtCardPilesIsInAccessibleTree();
        }

        private void PrepareForGameTypeChange(SolitaireGameType currentGameType, SolitaireGameType targetGameType)
        {
            switch (targetGameType)
            {
                case SolitaireGameType.Klondike:
                case SolitaireGameType.Bakersdozen:
                case SolitaireGameType.Spider:
                    CardPileGrid.IsVisible = true;
                    CardPileGridPyramid.IsVisible = false;
                    break;

                case SolitaireGameType.Pyramid:
                case SolitaireGameType.Tripeaks:
                case SolitaireGameType.Royalparade:
                    CardPileGrid.IsVisible = false;
                    CardPileGridPyramid.IsVisible = true;
                    break;
            }

            var gameType = "";

            switch (targetGameType)
            {
                case SolitaireGameType.Klondike:
                    gameType = MyGetString("KlondikeSolitaire");
                    break;

                case SolitaireGameType.Pyramid:
                    gameType = MyGetString("PyramidSolitaire");
                    break;

                case SolitaireGameType.Tripeaks:
                    gameType = MyGetString("TripeaksSolitaire");
                    break;

                case SolitaireGameType.Bakersdozen:
                    gameType = MyGetString("BakersdozenSolitaire");
                    break;

                case SolitaireGameType.Spider:
                    gameType = MyGetString("SpiderSolitaire");
                    break;

                case SolitaireGameType.Royalparade:
                    gameType = MyGetString("RoyalparadeSolitaire");
                    break;

                default:
                    break;
            }

            string announcement = MyGetString("PreparingGameTypeLayout");
            announcement = string.Format(announcement, gameType);

            MakeDelayedScreenReaderAnnouncement(announcement, false);

            ClearPyramidCards();
        }

        // Resize the CardWidth and CardHeight (to which so much UI is bound) based on the
        // current dimensions of the app main grid, and the current solitaire game.
        public void ResizeDealtCard(bool forceResize)
        {
            Debug.WriteLine("ResizeDealtCard: Begin resizing.");

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            if ((InnerMainGrid.Width <= 0) || (InnerMainGrid.Height <= 0))
            {
                return;
            }

            if (MainPage.IsPortrait())
            {
                vm.CardWidth = InnerMainGrid.Width / 5;

                var currentCardHeight = 0.0;

                // The card height is based on the current game and the current number of rows.

                switch (currentGameType)
                {
                    // For Tri Peaks, match the height of the cards in the Pyramid game,
                    // leaving some space in the main area.

                    case SolitaireGameType.Pyramid:
                    case SolitaireGameType.Tripeaks:
                    case SolitaireGameType.Royalparade:

                        currentCardHeight = (3 * InnerMainGrid.Height / 13);

                        break;

                    case SolitaireGameType.Bakersdozen:

                        currentCardHeight = (InnerMainGrid.Height / 15) - 1;

                        break;

                    case SolitaireGameType.Spider:

                        currentCardHeight = (InnerMainGrid.Height / 12) - 1;

                        break;

                    default: // Klondike:

                        currentCardHeight = (InnerMainGrid.Height / 10) - 1;

                        break;
                }

                if ((currentCardHeight > 0) && (currentCardHeight != vm.CardHeight))
                {
                    Debug.WriteLine("ResizeDealtCard: Set vm.CardHeight to: " + currentCardHeight);

                    vm.CardHeight = currentCardHeight;
                }
            }
            else // Landscape.
            {
                if (!IsGameCollectionViewBased())
                {
                    vm.CardHeight = (3 * MainPageGrid.Height) / 13;

                    var currentCardWidth = MainPageGrid.Width /
                                            (currentGameType == SolitaireGameType.Pyramid ? 7 : 10);

                    if (currentCardWidth != vm.CardWidth)
                    {
                        vm.CardWidth = currentCardWidth;

                        Debug.WriteLine("ResizeDealtCard: Set vm.CardWidth to " + vm.CardWidth);
                    }
                }
                else
                {
                    vm.CardHeight = (3 * MainPageGrid.Height) / 13;

                    var currentCardWidth = (MainPageGrid.Width - 1) / GetGameCardPileCount();

                    if (currentCardWidth != vm.CardWidth)
                    {
                        vm.CardWidth = currentCardWidth;

                        Debug.WriteLine("ResizeDealtCard: Set vm.CardWidth to " + vm.CardWidth);
                    }
                }
            }
        }

        private void RefreshDealtCardPilesIsInAccessibleTree()
        {
            if (!IsGameCollectionViewBased())
            {
                return;
            }

            // The first seven dealt card piles are always of interest.

            for (int i = 8; i < 13; i++)
            {
                var collectionView = (CollectionView)CardPileGrid.FindByName("CardPile" + i);

                // Piles 8, 9, and 10 are of interest to both Spider and Baker's Dozen.
                // Piles 11, 12, and 13 are only of interest to Baker's Dozen.

                var hideDealtCardPile = (currentGameType == SolitaireGameType.Klondike) ||
                                         ((currentGameType == SolitaireGameType.Spider) && (i > 10));

                // Important: Do not use SetIsInAccessibleTree here, as that won't impact the
                // items contained within the CollectionViews.

                AutomationProperties.SetExcludedWithChildren(collectionView, hideDealtCardPile);
            }
        }

        private void SetRemainingCardUIVisibility()
        {
            if (currentGameType == SolitaireGameType.Spider)
            {
                NextCardDeck.IsVisible = true;

                CardDeckUpturnedObscuredLower.IsVisible = false;
                CardDeckUpturnedObscuredHigher.IsVisible = false;

                CardDeckUpturned.IsVisible = false;

                TargetPileC.IsVisible = false;
                TargetPileD.IsVisible = false;
                TargetPileH.IsVisible = false;
                TargetPileS.IsVisible = false;

                var cardCount = _deckRemaining.Count;

                var vm = this.BindingContext as DealtCardViewModel;
                if ((vm != null) && (vm.DealtCards != null))
                {
                    for (int i = 0; i < 10; ++i)
                    {
                        if ((vm.DealtCards[i] != null) && (vm.DealtCards[i].Count > 0))
                        {
                            var card = vm.DealtCards[i][0];
                            if (card.CardState != CardState.KingPlaceHolder)
                            {
                                cardCount += vm.DealtCards[i].Count;
                            }
                        }
                    }
                }

                SetSpiderDiscardedSequenceDetails();

                SpiderDiscardedSequenceCountLabelContainer.IsVisible = true;

#if (ANDROID || WINDOWS)
                // Spider solitaire does not show a group of upturned card or target card elements.
                SemanticProperties.SetDescription(UpturnedCardsGrid, "");
                SemanticProperties.SetDescription(TargetPiles, "");
#endif

                CardDeckUpturned.IsVisible = false;
            }
            else
            {
#if (ANDROID || WINDOWS)
                // Spider solitaire does not show a group of upturned card elements.
                SemanticProperties.SetDescription(UpturnedCardsGrid, MyGetString("UpturnedCards"));
                SemanticProperties.SetDescription(TargetPiles, MyGetString("TargetCardPiles"));
#endif

                SpiderDiscardedSequenceCountLabelContainer.IsVisible = false;

                TargetPileC.IsVisible = true;
                TargetPileD.IsVisible = true;
                TargetPileH.IsVisible = true;
                TargetPileS.IsVisible = true;

                if (currentGameType == SolitaireGameType.Bakersdozen)
                {
                    NextCardDeck.IsVisible = false;

                    CardDeckUpturnedObscuredLower.IsVisible = false;
                    CardDeckUpturnedObscuredHigher.IsVisible = false;
                    CardDeckUpturned.IsVisible = false;
                }
                else
                {
                    NextCardDeck.IsVisible = true;

                    CardDeckUpturned.IsVisible = true;

                    CardDeckUpturnedObscuredHigher.IsVisible = (currentGameType != SolitaireGameType.Tripeaks);
                    CardDeckUpturnedObscuredLower.IsVisible = (currentGameType == SolitaireGameType.Klondike);
                }
            }
        }

        private bool LoadedCardCountUnexpected()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return false;
            }

            var countUnexpected = false;

            var countCardsLoaded = CountCards();

            if ((countCardsLoaded == 0) && (currentGameType != SolitaireGameType.Spider))
            {
                countUnexpected = true;
            }
            else if (((currentGameType == SolitaireGameType.Klondike) ||
                      (currentGameType == SolitaireGameType.Bakersdozen)) &&
                     (countCardsLoaded != 52))
            {
                countUnexpected = true;
            }
            else if (currentGameType == SolitaireGameType.Spider)
            {
                if (countCardsLoaded % 13 != 0)
                {
                    countUnexpected = true;
                }
                else if ((countCardsLoaded == 0) && (vm.SpiderDiscardedSequenceCount != 8))
                {
                    // The count is only empty when we've discarded 8 sequences.
                    countUnexpected = true;
                }
            }

            return countUnexpected;
        }

        private void ChangeGameLoadSessionPostprocess(bool loadSucceeded)
        {
            Debug.WriteLine("ChangeGameLoadSessionPostprocess: START");

            // Barker: For Klondike games, we can no longer only check for a target pile being complete
            // simply by the count of cards in the pile, as auto-complete games simply puts a King as
            // the displayed card on top of whatever else is there in the pile. So check for an
            // auto-completed game here. The check for the current game being a Klondike game is
            // made beneath CheckForAutoComplete().

            var setAutoCompleteVisuals = false;

            if (loadSucceeded && LoadedCardCountUnexpected())
            {
                setAutoCompleteVisuals = CheckForAutoComplete();
                if (!setAutoCompleteVisuals)
                {
                    loadSucceeded = false;
                }
            }

            if (loadSucceeded)
            {
                if (!IsGameCollectionViewBased())
                {
                    var pyramidPostprocessSucceeded = DealPyramidCardsPostprocess(false);

                    Debug.WriteLine("ChangeGameLoadSessionPostprocess: pyramidPostprocessSucceeded " +
                                        pyramidPostprocessSucceeded);
                }
                else
                {
                    // Without this refreshing of the cards' accessible name, the N in MofN is stuck
                    // as it was when the card was added to the pile, and doesn't account for the 
                    // total number of cards added. 
                    for (int i = 0; i < GetGameCardPileCount(); i++)
                    {
                        var dealtCardPile = (CollectionView)CardPileGrid.FindByName("CardPile" + (i + 1));
                        if (dealtCardPile != null)
                        {
                            RefreshDealtCardPileAccessibleNames(dealtCardPile);
                        }
                    }
                }

                if (!setAutoCompleteVisuals)
                {
                    RefreshUpperCards();
                }
                else
                {
                    // Set up the layout for an auto-completed game, but don't show any message.
                    AutoCompleteGameNow(false);
                }

                ClearAllSelections(true);

                LoadAllGamesPausedState();

                SetNowAsStartOfCurrentGameSessionIfAppropriate();

                Debug.WriteLine("ChangeGameLoadSessionPostprocess: Note time of start of this game session.");
            }
            else
            {
                ClearAllPiles();

                Debug.WriteLine("ChangeGameLoadSessionPostprocess: Failed to LoadSession, so restart game.");

                RestartGame(true /* screenReaderAnnouncement. */);
            }
        }
    }
}
