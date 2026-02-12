using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    public enum SolitaireGameType
    {
        NoGame = 0,
        Klondike = 1,
        Pyramid = 2,
        Tripeaks = 3,
        Bakersdozen = 4,
        Spider = 5,
    }

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

        private void ComleteVisualsUpdateFollowingGameChange()
        {
            SetRemainingCardUIVisibility();

            // The number of rows in the main area may need to be updated based on the current game.
            SetOrientationLayout();

            ResizeDealtCard(false);

            RefreshAllCardVisuals();

            RefreshDealtCardPilesIsInAccessibleTree();
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
                                         ((currentGameType == SolitaireGameType.Spider) && (i >10));

                // Important: Do not use SetIsInAccessibleTree here, as that won't impact the
                // items contained within the CollectionViews.

                AutomationProperties.SetExcludedWithChildren(collectionView, hideDealtCardPile);
            }
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

        private void SetRemainingCardUIVisibility()
        {
            if (currentGameType == SolitaireGameType.Spider)
            {
                NextCardDeck.IsVisible = true;

                CardDeckUpturnedObscuredLower.IsVisible = false;
                CardDeckUpturnedObscuredHigher.IsVisible = false;

                CardDeckUpturned.IsVisible = true;

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

                var sequencesComplete = (104 - cardCount) / 13;

                SetSpiderDiscardedSequenceDetails(sequencesComplete.ToString());

                SpiderDiscardedSequenceCountLabelContainer.IsVisible = true;

#if (ANDROID || WINDOWS)
                // Spider solitaire does not show a group of upturned card elements.
                SemanticProperties.SetDescription(UpturnedCardsGrid, "");
#endif

                CardDeckUpturned.IsVisible = false;
            }
            else
            {
#if (ANDROID || WINDOWS)
                // Spider solitaire does not show a group of upturned card elements.
                SemanticProperties.SetDescription(UpturnedCardsGrid, MyGetString("UpturnedCards"));
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

                default:
                    break;
            }

            string announcement = MyGetString("PreparingGameTypeLayout");
            announcement = string.Format(announcement, gameType);

            MakeDelayedScreenReaderAnnouncement(announcement, false);

            ClearPyramidCards();
        }

        private void ClearPyramidCards()
        {
            if ((currentGameType == SolitaireGameType.Pyramid) ||
                (currentGameType == SolitaireGameType.Tripeaks))
            {
                var pyramidCards = CardPileGridPyramid.Children;

                foreach (var pyramidCard in pyramidCards)
                {
                    var card = pyramidCard as CardButton;
                    if ((card != null) && (card.Card != null))
                    {
                        card.Card = null;

                        card.RefreshVisuals();
                    }
                }
            }
        }

        private bool LoadedCardCountUnexpected()
        {
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

        private void AddPyramidButtons()
        {
            CardButton button;

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            // First add all the CardButtons for the pyramid.
            for (int i = 0; i < 28; i++)
            {
                button = new CardButton();

                button.Margin = new Thickness(2, 0, 2, 0);

                button.IsToggled = false;

                CardPileGridPyramid.Children.Add(button);
            }

            // Now arrange the buttons for the pyramid.
            ArrangePyramidButtons();
        }

        // Barker Todo: No action required here if the cards are alread arranged as required.
        private void ArrangePyramidButtons()
        {
            if (Application.Current == null)
            {
                return;
            }

            if (IsGameCollectionViewBased())
            {
                return;
            }

            var countOfCardsPerRow = 1;
            var countOfCardsOnCurrentRow = 0;

            var isPyramid = (currentGameType == SolitaireGameType.Pyramid);

            var startColumn = isPyramid ? 6 : 3;
            
            var currentColumn = startColumn;

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            var setSemanticHeading = true;

            if (CardPileGridPyramid.Children.Count != 28)
            {
                Debug.WriteLine("ArrangePyramidButtons: Unexpected CardButton pyramid count " + 
                    CardPileGridPyramid.Children.Count);

                return;
            }

            CardButton button;

            var columnDefinitions = CardPileGridPyramid.ColumnDefinitions;
            if (columnDefinitions.Count != 20)
            {
                Debug.WriteLine("ArrangePyramidButtons: Unexpected CardButton pyramid column count " +
                    columnDefinitions.Count);

                return;
            }

            var columnCount = columnDefinitions.Count;

            for (var i = 0; i < columnCount; i++)
            {
                if (isPyramid && (i > 13))
                {
                    columnDefinitions[i].Width = GridLength.Auto;
                }
                else
                {
                    columnDefinitions[i].Width = GridLength.Star;
                }
            }

            for (int i = 0; i < 28; i++)
            {
                button = (CardButton)CardPileGridPyramid.Children[i];

                if (!isPyramid) // Tripeaks.
                {
                    if (i < 3)
                    {
                        countOfCardsPerRow = 3;
                    }
                    else if (i < 9)
                    {
                        countOfCardsPerRow = 6;
                    }
                    else if (i < 18)
                    {
                        countOfCardsPerRow = 9;
                    }
                    else
                    {
                        countOfCardsPerRow = 10;

                        button.IsFaceUp = true;
                    }
                }

                if (!isPyramid && !button.IsFaceUp)
                {
                    button.BackgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                    Color.FromRgb(0x0E, 0xD1, 0x45) : Color.FromRgb(0x0B, 0xA9, 0x38));
                }
                else
                { 
                    button.BackgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                                Colors.White : Colors.Black);
                }

                var tripeaksRowIndex = 3;

                if (isPyramid)
                {
                    CardPileGridPyramid.SetRow(button, countOfCardsPerRow - 1);
                }
                else // Tripeaks.
                {
                    if (i < 18)
                    {
                        tripeaksRowIndex = (countOfCardsPerRow / 3) - 1;
                    }

                    CardPileGridPyramid.SetRow(button, tripeaksRowIndex);
                }

                CardPileGridPyramid.SetRowSpan(button, 3);

                if (!isPyramid) // Tripeaks.
                {
                    if (i == 1)
                    {
                        currentColumn = 9;
                    }
                    else if (i == 2)
                    {
                        currentColumn = 15;
                    }
                    else if (i == 5)
                    {
                        currentColumn = 8;
                    }
                    else if (i == 7)
                    {
                        currentColumn = 14;
                    }
                }

                CardPileGridPyramid.SetColumn(button, currentColumn);
                CardPileGridPyramid.SetColumnSpan(button, 2);

                if (setSemanticHeading)
                {
                    var firstCardInRowIsHeading = vm.CardButtonsHeadingState;

                    button.SetHeadingState(firstCardInRowIsHeading);

                    setSemanticHeading = false;
                }

                ++countOfCardsOnCurrentRow;

                currentColumn += 2;

                if (countOfCardsOnCurrentRow == countOfCardsPerRow)
                {
                    ++countOfCardsPerRow;

                    countOfCardsOnCurrentRow = 0;

                    if (isPyramid)
                    {
                        --startColumn;
                    }
                    else // Tripeaks.
                    {
                        startColumn = 2 - tripeaksRowIndex;
                    }

                    currentColumn = startColumn;

                    setSemanticHeading = true;
                }
            }
        }

        private void SetPyramidCardButtonBindingProperties(CardButton cardButton)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Barker Todo: I've yet to get the binding to work on all the dynamically created cards.
            // So until I figure that out, set the values explicitly here, and require the game to 
            // be restarted if the related settings are changed.

            // BARKER IMPORTANT: On Android Release and Debug builds, the app would periodically crash (with no details)
            // after restarting the game. The crashes weren't repro if I took out the suit color work done here. So remove
            // this until I figure out what's going on.

            cardButton.LongPressZoomDuration = vm.LongPressZoomDuration;

            //var image = (Image)cardButton.FindByName("TintedCardImage");
            //if (image != null)
            //{
            //    //button.BindingContext = vm;

            //    //button.SetBinding(CardButton.LongPressZoomDurationProperty, 
            //    //                    static (DealtCardViewModel vm) => vm.LongPressZoomDuration);

            //    //button.SetBinding(CardButton.SuitColoursClubsSwitchProperty,
            //    //                    static (DealtCardViewModel vm) => vm.SuitColoursClubs);

            //    cardButton.LongPressZoomDuration = vm.LongPressZoomDuration;

            //    var iconTintColorBehavior = new IconTintColorBehavior();
            //    iconTintColorBehavior.TintColor = Colors.Transparent;

            //    if (cardButton.Card != null)
            //    {
            //        switch (cardButton.Card.Suit)
            //        {
            //            case Suit.Clubs:
            //                iconTintColorBehavior.TintColor = vm.SuitColoursClubs;
            //                break;
            //            case Suit.Diamonds:
            //                iconTintColorBehavior.TintColor = vm.SuitColoursDiamonds;
            //                break;
            //            case Suit.Hearts:
            //                iconTintColorBehavior.TintColor = vm.SuitColoursHearts;
            //                break;
            //            case Suit.Spades:
            //                iconTintColorBehavior.TintColor = vm.SuitColoursSpades;
            //                break;
            //            default:
            //                iconTintColorBehavior.TintColor = vm.SuitColoursSpades;
            //                break;
            //        }
            //    }

            //    if (iconTintColorBehavior.TintColor != Colors.Transparent)
            //    {
            //        image.Behaviors.Add(iconTintColorBehavior);
            //    }
            //}
        }

        private bool DealPyramidCardsPostprocess(bool setDealtCardProperties)
        {
            if (Application.Current == null)
            {
                return false;
            }

            if (IsGameCollectionViewBased())
            {
                return false;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return false;
            }

            if (vm.DealtCards[0].Count == 0)
            {
                return false;
            }

            var cardButtonsUI = CardPileGridPyramid.Children;

            var countCardsPerRow = 1;

            var cardUIIndex = 0;

            var totalCountOfVisibleCardsInPyramid = 0;

            // Pyramid has 7 rows, TriPeaks has 4 rows.
            var countOfRows = (currentGameType == SolitaireGameType.Pyramid ? 7 : 4);

            for (int i = 0; i < countOfRows; ++i)
            {
                var currentVisibleIndexOfCardOnRow = 0;

                // How many cards are currently shows on this row?
                var countOfVisibleCardsOnRow = 0;

                // For TriPeaks, the count of card on each row is not simply the same as the row index.
                if (currentGameType == SolitaireGameType.Tripeaks)
                {
                    if (i == countOfRows - 1)
                    {
                        countCardsPerRow = 10;
                    }
                    else
                    {
                        countCardsPerRow = 3 * (i + 1);
                    }
                }

                try
                {
                    for (int j = 0; j < countCardsPerRow; ++j)
                    {
                        var dealtCard = vm.DealtCards[i][j];
                        if (dealtCard.Card != null)
                        {
                            ++countOfVisibleCardsOnRow;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DealPyramidCardsPostprocess: ex " + ex.Message);

                    return false;
                }

                totalCountOfVisibleCardsInPyramid += countOfVisibleCardsOnRow;

                var setSemanticHeading = true;

                for (int j = 0; j < countCardsPerRow; ++j)
                {
                    var dealtCard = vm.DealtCards[i][j];

                    // All of these are zero-based.
                    dealtCard.PyramidRow = i;
                    dealtCard.PyramidCardOriginalIndexInRow = j;
                    dealtCard.PyramidCardCurrentCountOfCardsOnRow = countOfVisibleCardsOnRow;

                    dealtCard.PyramidCardCurrentIndexInRow = currentVisibleIndexOfCardOnRow;

                    if (dealtCard.Card != null)
                    {
                        ++currentVisibleIndexOfCardOnRow;
                    }

                    var isBottomRow = false;

                    // The index of the bottom row depends on the heights of the pyramids.

                    // Barker Todo: Given the TriPeaks game only has 4 rows, consider whether
                    // the cards could be taller in that game.

                    if (currentGameType == SolitaireGameType.Pyramid)
                    {
                        isBottomRow = (dealtCard.PyramidRow == 6);

                        dealtCard.FaceDown = false;
                    }
                    else if (currentGameType == SolitaireGameType.Tripeaks)
                    {
                        isBottomRow = (dealtCard.PyramidRow == 3);

                        if (setDealtCardProperties)
                        {
                            dealtCard.FaceDown = !isBottomRow;
                        }
                    }

                    if (setDealtCardProperties)
                    {
                        dealtCard.Open = isBottomRow;
                    }

                    var cardUI = cardButtonsUI[cardUIIndex] as CardButton;
                    if (cardUI != null)
                    {
                        var cardButtonIsVisible = (dealtCard.Card != null);

                        cardUI.IsVisible = cardButtonIsVisible;

                        cardUI.Card = dealtCard.Card;

                        if (currentGameType == SolitaireGameType.Tripeaks)
                        {
                            cardUI.IsFaceUp = dealtCard.Open;

                            // Barker Todo: Replace everywhere there's explicitly setting of the background colour
                            // with appropriate binding.
                            if (!cardUI.IsFaceUp)
                            {
                                cardUI.BackgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                                Color.FromRgb(0x0E, 0xD1, 0x45) : Color.FromRgb(0x0B, 0xA9, 0x38));
                            }
                            else
                            {
                                cardUI.BackgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                                            Colors.White : Colors.Black);
                            }

                            if (setSemanticHeading)
                            {
                                var firstCardInRowIsHeading = vm.CardButtonsHeadingState;

                                cardUI.SetHeadingState(firstCardInRowIsHeading);

                                setSemanticHeading = false;
                            }
                        }
                        else
                        {
                            cardUI.IsFaceUp = true;
                        }

                        cardUI.RefreshAccessibleName();

                        SetPyramidCardButtonBindingProperties(cardUI);
                    }

                    ++cardUIIndex;
                }

                ++countCardsPerRow;
            }

            // For Tripeaks, automatically turn over the top remaining card.
            if (setDealtCardProperties && (currentGameType == SolitaireGameType.Tripeaks))
            {
                PerformNextCardAction();
            }

            return true;
        }

        // Handle a click on one of the CardButtons in the pyramd.
        private void HandlePyramidCardClick(CardButton cardButtonClicked)
        {
            if (cardButtonClicked.Card == null)
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            if (currentGameType == SolitaireGameType.Tripeaks)
            {
                HandleTripeaksPyramidCardClick(cardButtonClicked);

                return;
            }

            var discardMessage = "";

            // Track whether we'll be removing the card from the pyramid.
            var removeCard = false;

            // First check whether the main upturned card is selected and totals 13.
            if (CardDeckUpturned.IsToggled && (CardDeckUpturned.Card != null))
            {
                // Remove the selected Upturned card if appropriate.
                string upturnedCardAccessibleName;
                removeCard = CanRemoveUpturnedCardAndPyramidCard(CardDeckUpturned, cardButtonClicked, out upturnedCardAccessibleName);
                if (removeCard)
                {
                    cardButtonClicked.IsToggled = false;

                    discardMessage = MainPage.MyGetString("Discarded");
                    discardMessage += " " + upturnedCardAccessibleName;

                    // There is now currently no Upturned card. Leave the state of the waste pile unchanged.
                    CardDeckUpturned.Card = null;
                }
                else
                {
                    // The clicked pryamid card does not total 13 with the already selected Upturned card.
                    PlaySound(false);
                }
            }

            // If we didn't remove the Upturned card, check the top of the Waste pile (ie the higher obscured card).
            if (!removeCard)
            {
                if (CardDeckUpturnedObscuredHigher.IsToggled && (CardDeckUpturnedObscuredHigher.Card != null))
                {
                    string obscuredHigherCardAccessibleName;
                    removeCard = CanRemoveUpturnedCardAndPyramidCard(CardDeckUpturnedObscuredHigher, cardButtonClicked,
                                                                        out obscuredHigherCardAccessibleName);
                    if (removeCard)
                    {
                        CardDeckUpturnedObscuredHigher.IsToggled = false;

                        discardMessage = MainPage.MyGetString("Discarded");
                        discardMessage += " " + obscuredHigherCardAccessibleName;

                        CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 1 ?
                                            _deckUpturned[_deckUpturned.Count - 2] : null);
                    }
                    else
                    {
                        // The clicked pryamid card does not total 13 with the already selected card on the Waste pile.
                        PlaySound(false);
                    }
                }
            }

            CardButton? cardAlreadySelected = null;

            // If we've not removed the Upturned card or higher Obscured card, check the rest of the pyramid.
            if (!removeCard)
            {
                // Is any card in the pyramid already selected?
                var gridCards = CardPileGridPyramid.Children;

                foreach (var gridCard in gridCards)
                {
                    var card = gridCard as CardButton;
                    if ((card != null) && (card != cardButtonClicked) && card.IsToggled)
                    {
                        cardAlreadySelected = card;

                        break;
                    }
                }

                // Was another pyramid card already selected?
                if ((cardAlreadySelected != null) && (cardAlreadySelected.Card != null))
                {
                    // Do the selected pyramid cards add up to 13?
                    var total = cardButtonClicked.Card.Rank + cardAlreadySelected.Card.Rank;
                    if (total == 13)
                    {
                        // Remove the already selected card.
                        CollectionView? list;
                        var dealtCardAlreadySelected = FindDealtCardFromCard(cardAlreadySelected.Card, false, out list);
                        if (dealtCardAlreadySelected != null)
                        {
                            discardMessage = MainPage.MyGetString("Discarded");
                            discardMessage += " " + dealtCardAlreadySelected.AccessibleNameWithoutSelectionAndMofN;

                            // Has a card now been revealed? 
                            SetOnTopStateFollowingMove(dealtCardAlreadySelected, false);

                            RefreshCardButtonMofNInRow(cardAlreadySelected);

                            // Always null out this card after the call to refresh the accessible name.
                            vm.DealtCards[dealtCardAlreadySelected.PyramidRow]
                                [dealtCardAlreadySelected.PyramidCardOriginalIndexInRow].Card = null;
                        }

                        cardAlreadySelected.IsVisible = false;
                        cardAlreadySelected.IsToggled = false;

                        removeCard = true;
                    }
                }
                else if (cardButtonClicked.Card.Rank == 13)
                {
                    // If the clicked card is simply a King, remove it.
                    removeCard = true;
                }
                else
                {
                    // Change the selection state of the target pile CardButton.
                    //SetCardButtonToggledSelectionState(cardButtonClicked, false);

                    cardButtonClicked.RefreshCardButtonMofN();

                    // No other card was selected, so we'll simply select the pyramid card.
                    string? announcement = cardButtonClicked.CardPileAccessibleNameWithoutMofN;
                    MakeDelayedScreenReaderAnnouncement(announcement, false);
                }
            }

            // Should we remove the clicked pyramid card?
            if (removeCard)
            {
                CollectionView? list;
                var dealtCard = FindDealtCardFromCard(cardButtonClicked.Card, false, out list);
                if (dealtCard != null)
                {
                    if (string.IsNullOrEmpty(discardMessage))
                    {
                        discardMessage = MainPage.MyGetString("Discarded");
                    }
                    else
                    {
                        discardMessage += " " + MainPage.MyGetString("And") + " ";
                    }

                    discardMessage += " " + dealtCard.AccessibleNameWithoutSelectionAndMofN + ". ";

                    // Has a card now been revealed? 
                    SetOnTopStateFollowingMove(dealtCard, true);

                    RefreshCardButtonMofNInRow(cardButtonClicked);

                    vm.DealtCards[dealtCard.PyramidRow][dealtCard.PyramidCardOriginalIndexInRow].Card = null;

                    cardButtonClicked.IsVisible = false;
                    cardButtonClicked.IsToggled = false;

                    PlaySound(true);
                }
            }
            else
            {
                // The clicked card was not removed. If another pyramid card was already selected, deselect it.
                if (cardAlreadySelected != null)
                {
                    cardButtonClicked.IsToggled = false;

                    cardAlreadySelected.IsToggled = false;

                    PlaySound(false);
                }
            }

            if (!string.IsNullOrEmpty(discardMessage))
            {
                // Barker Todo: Figure out how to announce the results of the operation without
                // the announcement conflicting with the default VoiceOver announcement relating
                // to focus moving.
                //MakeDelayedScreenReaderAnnouncementWithDelayTime(discardMessage, true, 2000);

                // Barker Todo: Still announce the available moves if necessary.
                if (automaticallyAnnounceMoves)
                {
                    var availableMoveAnnouncement = AnnounceAvailableMoves(false);
                    if (!string.IsNullOrEmpty(availableMoveAnnouncement))
                    {
                        MakeDelayedScreenReaderAnnouncementWithDelayTime(availableMoveAnnouncement, false, 3000);
                    }
                }
            }

            if (GameOver())
            {
                ShowEndOfGameDialog(false);
            }
        }

        private void HandleTripeaksPyramidCardClick(CardButton cardButtonClicked)
        {
            if (cardButtonClicked.Card == null)
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            var cardMoved = false;

            if (CardDeckUpturned.Card != null)
            {
                CollectionView? list;
                var dealtCard = FindDealtCardFromCard(cardButtonClicked.Card, false, out list);
                if (dealtCard != null)
                {
                    // Is the difference between the cards a value of 1 or 12? (A difference of 12 means
                    // that one of the cards is an Ace and one is a King.)
                    var difference = Math.Abs(CardDeckUpturned.Card.Rank - cardButtonClicked.Card.Rank);
                    if ((difference == 1) || (difference == 12))
                    {
                        // Has a card now been revealed? 
                        SetOnTopStateFollowingMove(dealtCard, true);

                        RefreshCardButtonMofNInRow(cardButtonClicked);

                        vm.DealtCards[dealtCard.PyramidRow][dealtCard.PyramidCardOriginalIndexInRow].Card = null;

                        cardButtonClicked.IsVisible = false;
                        cardButtonClicked.IsToggled = false;

                        // Move the clicked card to the upturned pile.
                        _deckUpturned.Add(cardButtonClicked.Card);

                        RefreshUpperCards();

                        // Barker Todo: Figure out how to announce the results of the operation without
                        if (automaticallyAnnounceMoves)
                        {
                            var availableMoveAnnouncement = AnnounceAvailableMoves(false);
                            if (!string.IsNullOrEmpty(availableMoveAnnouncement))
                            {
                                MakeDelayedScreenReaderAnnouncementWithDelayTime(availableMoveAnnouncement, false, 3000);
                            }
                        }

                        PlaySound(true);

                        if (GameOver())
                        {
                            ShowEndOfGameDialog(false);
                        }

                        cardMoved = true;
                    }
                }
            }

            if (!cardMoved)
            {
                // The clicked pryamid card is not adjacent to the Upturned card.
                cardButtonClicked.IsToggled = false;

                PlaySound(false);
            }
        }

        private void HandleClickOnUpturnedOrHigherObscuredCard(CardButton cardDeckUpturned)
        {
            // Is the most recently upturned card a King?
            if (cardDeckUpturned.Card != null)
            {
                var discardMessage = "";

                if (cardDeckUpturned.Card.Rank == 13)
                {
                    discardMessage = MainPage.MyGetString("Discarded") + " " +
                                        cardDeckUpturned.Card.GetCardAccessibleName();

                    // Move the King to the discard pile.
                    RemoveCardButtonCard(_deckUpturned, cardDeckUpturned.Card);

                    // The most recently upturned card is now empty.
                    if (cardDeckUpturned == CardDeckUpturned)
                    {
                        CardDeckUpturned.Card = null;
                    }
                    else
                    {
                        // The top of the other pile is the next most recently upturned card.
                        CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 1 ?
                                            _deckUpturned[_deckUpturned.Count - 2] : null);
                    }
                }
                else
                {
                    // The upturned card is not a King. Check it's a pair with an already selected pyramid card.
                    var vm = this.BindingContext as DealtCardViewModel;
                    if ((vm != null) && (vm.DealtCards != null))
                    {
                        CardButton? cardAlreadySelected = null;

                        // Is any card in the pyramid already selected?
                        var gridCards = CardPileGridPyramid.Children;

                        foreach (var gridCard in gridCards)
                        {
                            var card = gridCard as CardButton;
                            if ((card != null) && card.IsToggled)
                            {
                                cardAlreadySelected = card;

                                break;
                            }
                        }

                        var toggleSelectionStateOfUpturnedCard = false;

                        if ((cardAlreadySelected != null) && ((cardAlreadySelected.Card != null)))
                        {
                            string upturnedCardAccessibleName;
                            var removeCard = CanRemoveUpturnedCardAndPyramidCard(cardDeckUpturned, cardAlreadySelected,
                                                                                    out upturnedCardAccessibleName);
                            if (removeCard)
                            {
                                CollectionView? list;
                                var dealtCardAlreadySelected = FindDealtCardFromCard(cardAlreadySelected.Card, false, out list);
                                if (dealtCardAlreadySelected != null)
                                {
                                    cardAlreadySelected.IsToggled = false;

                                    discardMessage = MainPage.MyGetString("Discarded") + " " +
                                                        upturnedCardAccessibleName + " " +
                                                        MainPage.MyGetString("And") + " " +
                                                        cardAlreadySelected.CardPileAccessibleNameWithoutMofN;

                                    // Has a card now been revealed? 
                                    SetOnTopStateFollowingMove(dealtCardAlreadySelected, false);

                                    RefreshCardButtonMofNInRow(cardAlreadySelected);

                                    vm.DealtCards[dealtCardAlreadySelected.PyramidRow]
                                        [dealtCardAlreadySelected.PyramidCardOriginalIndexInRow].Card = null;

                                    cardAlreadySelected.IsVisible = false;

                                    // Make sure the Upturned card appears as not selected.
                                    cardDeckUpturned.RefreshVisuals();
                                }

                                // The most recently upturned card is now empty.
                                if (cardDeckUpturned == CardDeckUpturned)
                                {
                                    CardDeckUpturned.Card = null;

                                    CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 0 ?
                                                        _deckUpturned[_deckUpturned.Count - 1] : null);
                                }
                                else
                                {
                                    // The top of the other pile is the next most recently upturned card.
                                    CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 1 ?
                                                        _deckUpturned[_deckUpturned.Count - 2] : null);
                                }
                            }
                            else
                            {
                                // The clicked upturned card does not total 13 with the already selected pyramid card.
                                PlaySound(false);
                            }
                        }
                        else
                        {
                            toggleSelectionStateOfUpturnedCard = true;
                        }

                        if (toggleSelectionStateOfUpturnedCard)
                        {
                            // Toggle the selected state of the upturned card.
                            ToggleUpturnedCardSelection(cardDeckUpturned);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(discardMessage))
                {
                    // Barker Todo: Figure out how to announce the results of the operation without
                    // the announcement conflicting with the default VoiceOver announcement relating
                    // to focus moving.
                    //MakeDelayedScreenReaderAnnouncement(discardMessage, true);

                    // Barker Todo: Still announce the available moves if necessary.
                    if (automaticallyAnnounceMoves)
                    {
                        var availableMoveAnnouncement = AnnounceAvailableMoves(false);
                        if (!string.IsNullOrEmpty(availableMoveAnnouncement))
                        {
                            MakeDelayedScreenReaderAnnouncementWithDelayTime(availableMoveAnnouncement, false, 3000);
                        }
                    }

                    PlaySound(true);
                }
            }
        }

        // A pyramid card has been clicked. Remove the Upturned card if appropriate.
        private bool CanRemoveUpturnedCardAndPyramidCard(CardButton upturnedCard, CardButton cardButton, 
                                                            out string upturnedCardAccessibleName)
        {
            upturnedCardAccessibleName = "";

            if ((upturnedCard.Card == null) || (cardButton.Card == null))
            {
                return false;
            }

            bool canRemoveUpturnedCardAndPyramidCard = false;

            if (upturnedCard.Card.Rank + cardButton.Card.Rank == 13)
            {
                upturnedCard.IsToggled = false;

                upturnedCardAccessibleName = upturnedCard.CardPileAccessibleName;

                RemoveCardButtonCard(_deckUpturned, upturnedCard.Card);

                canRemoveUpturnedCardAndPyramidCard = true;
            }
            else
            {
                // Deselect the two cards because they don't total 13.
                upturnedCard.IsToggled = false;
                cardButton.IsToggled = false;
            }

            return canRemoveUpturnedCardAndPyramidCard;
        }

        // Set the property to indicate the Discard pile is not empty.
        private void RemoveCardButtonCard(List<Card> cardList, Card cardToRemove)
        {
            cardList.Remove(cardToRemove);
        }

        // Check if both the Upturned card and HigherObscured card should be removed.
        private bool MoveBothUpturnedCards(CardButton cardButtonClicked)
        {
            var continueCheckForMove = true;

            if ((CardDeckUpturned.Card == null) || (CardDeckUpturnedObscuredHigher.Card == null))
            {
                return continueCheckForMove;
            }

            var checkForMove = ((cardButtonClicked == CardDeckUpturned) && CardDeckUpturnedObscuredHigher.IsToggled) ||
                               ((cardButtonClicked == CardDeckUpturnedObscuredHigher) && CardDeckUpturned.IsToggled);

            if (checkForMove)
            {
                continueCheckForMove = false;

                if (CardDeckUpturned.Card.Rank + CardDeckUpturnedObscuredHigher.Card.Rank == 13)
                {
                    RemoveCardButtonCard(_deckUpturned, CardDeckUpturned.Card);
                    RemoveCardButtonCard(_deckUpturned, CardDeckUpturnedObscuredHigher.Card);

                    CardDeckUpturned.Card = null;

                    CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 0 ?
                                        _deckUpturned[_deckUpturned.Count - 1] : null);

                    // Make sure the cards appear as not selected.
                    if (cardButtonClicked != CardDeckUpturned)
                    {
                        CardDeckUpturned.IsToggled = false;
                        CardDeckUpturned.RefreshVisuals();
                    }

                    if (cardButtonClicked != CardDeckUpturnedObscuredHigher)
                    {
                        CardDeckUpturnedObscuredHigher.IsToggled = false;
                        CardDeckUpturnedObscuredHigher.RefreshVisuals();
                    }

                    PlaySound(true);
                }
                else
                {
                    CardDeckUpturnedObscuredHigher.IsToggled = false;
                    CardDeckUpturned.IsToggled = false;

                    PlaySound(false);
                }
            }

            return continueCheckForMove;
        }

        private void RefreshAccessibleCardCountInRow(CardButton cardButton, int pyramidRow)
        {
            Debug.WriteLine("RefreshAccessibleCardCountInRow: Refresh cards in row " + pyramidRow);

            if (cardButton.Card == null)
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            var cardButtonsUI = CardPileGridPyramid.Children;

            // Refresh all the CardButton UI on the affected row.
            var cardButtonUIRowStartIndex = GetCardButtonUIStartIndexForRow(pyramidRow);

            var maxCardCountOnRow = pyramidRow + 1;
            if (currentGameType == SolitaireGameType.Tripeaks)
            {
                switch (pyramidRow)
                {
                    case 0:
                        maxCardCountOnRow = 3;
                        break;

                    case 1:
                        maxCardCountOnRow = 6;
                        break;

                    case 2:
                        maxCardCountOnRow = 9;
                        break;

                    default:
                        maxCardCountOnRow = 10;
                        break;
                }
            }

            for (int rowIndex = 0; rowIndex < maxCardCountOnRow; ++rowIndex)
            {
                var nextCardButton = cardButtonsUI[cardButtonUIRowStartIndex + rowIndex] as CardButton;
                if ((nextCardButton != null) && nextCardButton.IsVisible)
                {
                    nextCardButton.RefreshCardButtonMofN();
                }
            }
        }

        // Adjust the M of N for all cards on a row.
        private void RefreshCardButtonMofNInRow(CardButton cardButton)
        {
            if (cardButton.Card == null)
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            DealtCard? dealtCard = null;

            if ((currentGameType == SolitaireGameType.Klondike) ||
                (currentGameType == SolitaireGameType.Bakersdozen) ||
                (currentGameType == SolitaireGameType.Spider) ||
                (currentGameType == SolitaireGameType.Pyramid))
            {
                CollectionView? list;
                dealtCard = FindDealtCardFromCard(cardButton.Card, false, out list);
            }
            else // Tripeaks.
            {
                dealtCard = FindAnyDealtCardFromCard(cardButton.Card);
            }

            if ((dealtCard != null) && ((dealtCard.Card != null)))
            {
                var cardButtonsUI = CardPileGridPyramid.Children;
                for (int i = 0; i < cardButtonsUI.Count; ++i)
                {
                    var cardButtonPyramid = cardButtonsUI[i] as CardButton;
                    if ((cardButtonPyramid != null) && (cardButtonPyramid.Card != null) &&
                        (cardButtonPyramid.Card == cardButton.Card))
                    {
                        --dealtCard.PyramidCardCurrentCountOfCardsOnRow;

                        var dealtCardsInRow = vm.DealtCards[dealtCard.PyramidRow];
                        if (dealtCardsInRow != null)
                        {
                            for (int j = 0; j < dealtCardsInRow.Count; ++j)
                            {
                                var dealtCardInRow = dealtCardsInRow[j];
                                if ((dealtCardInRow != null) && (dealtCardInRow != dealtCard))
                                {
                                    if (j > dealtCard.PyramidCardOriginalIndexInRow)
                                    {
                                        --dealtCardInRow.PyramidCardCurrentIndexInRow;
                                    }

                                    dealtCardInRow.PyramidCardCurrentCountOfCardsOnRow = dealtCard.PyramidCardCurrentCountOfCardsOnRow;
                                }
                            }
                        }

                        break;
                    }
                }

                RefreshAccessibleCardCountInRow(cardButton, dealtCard.PyramidRow);
            }
        }

        private int GetCardButtonUIStartIndexForRow(int pyramidRow)
        {
            var startIndex = 0;

            if (currentGameType == SolitaireGameType.Pyramid)
            {
                var rowCount = 1;

                for (int i = 0; i < pyramidRow; ++i)
                {
                    startIndex += rowCount;

                    ++rowCount;
                }
            }
            else if (currentGameType == SolitaireGameType.Tripeaks)
            {
                switch (pyramidRow)
                {
                    case 0:
                        startIndex = 0;
                        break;

                    case 1:
                        startIndex = 3;
                        break;

                    case 2:
                        startIndex = 9;
                        break;

                    default:
                        startIndex = 18;
                        break;
                }
            }

            return startIndex;
        }

        private CardButton? GetCardButtonFromPyramidDealtCard(DealtCard dealtCard, out int cardButtonPyramidIndex)
        {
            cardButtonPyramidIndex = 0;

            CardButton? cardButtonFound = null;

            if ((dealtCard != null) && (dealtCard.Card != null))
            {
                var cardButtons = CardPileGridPyramid.Children;
                if ((cardButtons != null) && (cardButtons.Count > 0))
                { 
                    for (int i = 0; i < cardButtons.Count; ++i)
                    {
                        var cardButton = (cardButtons[i] as CardButton);
                        if ((cardButton != null) && (cardButton.Card == dealtCard.Card))
                        {
                            cardButtonFound = cardButton;

                            cardButtonPyramidIndex = i;

                            break;
                        }
                    }
                }
            }

            return cardButtonFound;
        }

        // **************************************************************************************************
        //
        // VERY IMPORTANT!
        //
        // SetOnTopStateFollowingMove() is being called because a card in the pyramid has been discarded.
        // If the card has focus, then focus will move away from the card, and by default, move to wherever
        // MAUI wants to put it. Sometimes the target seems unintuitive, so we should explicitly move focus
        // to somewhere more predictable. Note that in test on my iPad, keyboard focus is not helpful here,
        // and attempting to set keyboard focus and have it drag VoiceOver focus with it, doesn't work.
        // (ie Calls to Focus() fail, and VoiceOver seems unaffected by the attempt.) As such, explicitly
        // call SemanticSetFocus() to make sure VoiceOver ends up somewhere predictable and helpful.

        // HOWEVER: There is some undeterminable delay between the call to SemanticSetFocus() and VoiceOver
        // moving to the target element and making the related announcement. This means that if a custom
        // announcement related to the cards being discarded is still in progress, that announcement gets
        // stopped. No good away to avoid this problem has yet been found. There are no known events 
        // relating to VoiceOver moving to the target element, or relating to screen reader audio finishing.
        // As such, simply at a longer-than-usual delay before beginning the custom announcement relating
        // to cards being discarded. This is very unfortunate, as at best it results in a long delay before
        // learning about the results of discard action (and even then, the announcement relating to the
        // target element will have started and truncated), and at worst the delay in starting the 
        // discard-related announcement is insufficient and that announcement still gets truncated.
        //
        // **************************************************************************************************

        private void SetOnTopStateFollowingMove(DealtCard dealtCard, bool moveFocus)
        {
            if (Application.Current == null)
            {
                return;
            }

            if (IsGameCollectionViewBased())
            {
                return;
            }

            //Debug.WriteLine("SetOnTopStateFollowingMove: " + dealtCard.AccessibleNameWithoutSelectionAndMofN);

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Consider moving focus for the screen reader to follow if necessary.
            if (moveFocus && (dealtCard.PyramidRow > 0))
            {
                var cardButtonsUI = CardPileGridPyramid.Children;
                if (cardButtonsUI.Count > 0)
                {
                    CardButton? previousPyramidCard = null;
                    for (int i = 0; i < cardButtonsUI.Count; ++i)
                    {
                        var pyramidCard = (cardButtonsUI[i] as CardButton);
                        if ((pyramidCard != null) && pyramidCard.IsVisible)
                        {
                            if ((pyramidCard.Card != null) && (pyramidCard.Card == dealtCard.Card))
                            {
                                break;
                            }

                            previousPyramidCard = pyramidCard;
                        }
                    }

                    if (previousPyramidCard != null)
                    {
                        Debug.WriteLine("SetOnTopStateFollowingMove: Set semantic focus to " + 
                            previousPyramidCard.CardPileAccessibleName);

#if WINDOWS
                        previousPyramidCard.Focus();
#else
                        previousPyramidCard.SetSemanticFocus();
#endif
                    }
                }
            }

            var rowRevealed = dealtCard.PyramidRow - 1;
            if (rowRevealed >= 0)
            {
                // First check whether the card above left is now revealed.
                var checkAboveLeft = (dealtCard.PyramidCardOriginalIndexInRow > 0);
                if (checkAboveLeft && (currentGameType == SolitaireGameType.Tripeaks))
                {
                    switch (dealtCard.PyramidRow)
                    {
                        case 0:
                            checkAboveLeft = false;
                            break;

                        case 1:

                            if ((dealtCard.PyramidCardOriginalIndexInRow == 2) ||
                                (dealtCard.PyramidCardOriginalIndexInRow == 4))
                            {
                                checkAboveLeft = false;
                            }

                            break;

                        case 2:

                            if ((dealtCard.PyramidCardOriginalIndexInRow == 3) ||
                                (dealtCard.PyramidCardOriginalIndexInRow == 6))
                            {
                                checkAboveLeft = false;
                            }

                            break;

                        default:
                            break;
                    }
                }

                if (checkAboveLeft)
                {
                    var cardToLeft = vm.DealtCards[dealtCard.PyramidRow][dealtCard.PyramidCardOriginalIndexInRow - 1];
                    if (cardToLeft.Card == null)
                    {
                        var indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow - 1;

                        switch (dealtCard.PyramidRow)
                        {
                            case 1:

                                if (dealtCard.PyramidCardOriginalIndexInRow > 2)
                                {
                                    indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow - 2;
                                }

                                if (dealtCard.PyramidCardOriginalIndexInRow > 4)
                                {
                                    indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow - 3;
                                }

                                break;

                            case 2:

                                if (dealtCard.PyramidCardOriginalIndexInRow > 3)
                                {
                                    indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow - 2;
                                }

                                if (dealtCard.PyramidCardOriginalIndexInRow > 6)
                                {
                                    indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow - 3;
                                }

                                break;

                            default:
                                break;
                        }

                        var cardAboveToLeft = vm.DealtCards[rowRevealed][indexOfCardInRowAbove] as DealtCard;
                        cardAboveToLeft.Open = true;

                        if (currentGameType == SolitaireGameType.Tripeaks)
                        {
                            int cardButtonPyramidIndex;
                            var cardButtonAboveToLeft = GetCardButtonFromPyramidDealtCard(cardAboveToLeft, out cardButtonPyramidIndex);
                            if (cardButtonAboveToLeft != null)
                            {
                                cardButtonAboveToLeft.IsFaceUp = true;

                                cardButtonAboveToLeft.BackgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                                            Colors.White : Colors.Black);

                                // Barker Todo: Is FaceDown used in the pyramid games?
                                cardAboveToLeft.FaceDown = false;
                            }
                        }

                        var cardButtonUI = FindPyramidCardButtonFromDealtCard(cardAboveToLeft);
                        if (cardButtonUI != null)
                        {
                            cardButtonUI.RefreshCardButtonMofN();
                        }
                    }
                }

                // Next check whether the card above right is now revealed.
                var checkAboveRight = false;
                if (currentGameType == SolitaireGameType.Pyramid)
                {
                    checkAboveRight = (dealtCard.PyramidCardOriginalIndexInRow < dealtCard.PyramidRow);
                }
                else if (currentGameType == SolitaireGameType.Tripeaks)
                {
                    checkAboveRight = true;

                    var maxIndexInRow = 0;
                    switch (dealtCard.PyramidRow)
                    {
                        case 0:
                            maxIndexInRow = 2;

                            checkAboveRight = false;
                            break;

                        case 1:
                            maxIndexInRow = 5;

                            if ((dealtCard.PyramidCardOriginalIndexInRow == 1) ||
                                (dealtCard.PyramidCardOriginalIndexInRow == 3))
                            {
                                checkAboveRight = false;
                            }

                            break;

                        case 2:
                            maxIndexInRow = 8;

                            if ((dealtCard.PyramidCardOriginalIndexInRow == 2) ||
                                (dealtCard.PyramidCardOriginalIndexInRow == 5))
                            {
                                checkAboveRight = false;
                            }

                            break;

                        default:
                            maxIndexInRow = 9;
                            break;
                    }

                    if (checkAboveRight)
                    {
                        checkAboveRight = (dealtCard.PyramidCardOriginalIndexInRow < maxIndexInRow);
                    }
                }

                if (checkAboveRight)
                {
                    var cardToRight = vm.DealtCards[dealtCard.PyramidRow][dealtCard.PyramidCardOriginalIndexInRow + 1];
                    if (cardToRight.Card == null)
                    {
                        var indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow;

                        switch (dealtCard.PyramidRow)
                        {
                            case 1:

                                if (dealtCard.PyramidCardOriginalIndexInRow > 3)
                                {
                                    indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow - 2;
                                }
                                else if (dealtCard.PyramidCardOriginalIndexInRow > 1)
                                {
                                    indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow - 1;
                                }


                                break;

                            case 2:

                                if (dealtCard.PyramidCardOriginalIndexInRow > 2)
                                {
                                    indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow - 1;
                                }

                                if (dealtCard.PyramidCardOriginalIndexInRow > 5)
                                {
                                    indexOfCardInRowAbove = dealtCard.PyramidCardOriginalIndexInRow - 2;
                                }

                                break;

                            default:
                                break;
                        }

                        var cardAboveToRight = vm.DealtCards[rowRevealed][indexOfCardInRowAbove] as DealtCard;
                        cardAboveToRight.Open = true;

                        if (currentGameType == SolitaireGameType.Tripeaks)
                        {
                            int cardButtonPyramidIndex;
                            var cardButtonAboveToRight = GetCardButtonFromPyramidDealtCard(cardAboveToRight, out cardButtonPyramidIndex);
                            if (cardButtonAboveToRight != null)
                            {
                                cardButtonAboveToRight.IsFaceUp = true;

                                cardButtonAboveToRight.BackgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                                            Colors.White : Colors.Black);

                                // Barker Todo: Is FaceDown used in the pyramid games?
                                cardAboveToRight.FaceDown = false;
                            }
                        }

                        var cardButtonUI = FindPyramidCardButtonFromDealtCard(cardAboveToRight);
                        if (cardButtonUI != null)
                        {
                            cardButtonUI.RefreshCardButtonMofN();
                        }
                    }
                }
            }

            // While we're here, check if the card being removed is a heading.
            if (vm.CardButtonsHeadingState)
            { 
                int cardButtonPyramidIndex;
                var cardButton = GetCardButtonFromPyramidDealtCard(dealtCard, out cardButtonPyramidIndex);
                if (cardButton != null)
                {
                    var innerButton = (Button)cardButton.FindByName("InnerButton");
                    if (innerButton != null)
                    {
                        var headingLevel = SemanticProperties.GetHeadingLevel(innerButton);
                        if (headingLevel != SemanticHeadingLevel.None)
                        {
                            Debug.WriteLine("Found existing heading: " + cardButton.CardPileAccessibleName);

                            var cardButtons = CardPileGridPyramid.Children;
                            if (cardButtons != null)
                            {
                                if ((cardButtonPyramidIndex >= 0) &&
                                    (cardButtonPyramidIndex < cardButtons.Count - 1))
                                {
                                    for (int i = cardButtonPyramidIndex + 1; i < cardButtons.Count; ++i)
                                    {
                                        var nextCardButton = cardButtons[i] as CardButton;
                                        if ((nextCardButton != null) && nextCardButton.IsVisible)
                                        {
                                            Debug.WriteLine("Set as Heading: " + nextCardButton.CardPileAccessibleName);

                                            nextCardButton.SetHeadingState(true);

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private CardButton? FindPyramidCardButtonFromDealtCard(DealtCard dealtCard)
        {
            CardButton? foundCardButton = null;

            if (dealtCard.Card != null)
            {
                var cardButtonsUI = CardPileGridPyramid.Children;
                for (int i = 0; i < cardButtonsUI.Count; ++i)
                {
                    var cardButtonPyramid = cardButtonsUI[i] as CardButton;
                    if ((cardButtonPyramid != null) && (cardButtonPyramid.Card != null) &&
                        (cardButtonPyramid.Card == dealtCard.Card))
                    {
                        foundCardButton = cardButtonPyramid;

                        break;
                    }
                }
            }

            return foundCardButton;
        }
    }
}