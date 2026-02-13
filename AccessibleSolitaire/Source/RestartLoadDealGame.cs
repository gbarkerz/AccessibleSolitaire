using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;

// This file contains most of code relating to restarting a game, reloading an earlier game,
// or dealing out the cards.

namespace Sa11ytaire4All
{
    // IMPORTANT: According to the MAU docs...
    // "CollectionView will throw an exception if its ItemsSource is updated off the UI thread."
    // So always add/remove items from the datasource on the UI thread.

    public partial class MainPage : ContentPage
    {
        // The dealt card pile grid has completed loading after the app has started. 
        // Attempt to load the previous session for the current game type. This is
        // called on the UI thread.
        private void CardPileGrid_Loaded(object? sender, EventArgs e)
        {
            // We're here when the app is first started, and when returning to the MainPage after
            // visiting the Settings page or the Help content. We only need to take action here
            // in response to the app starting.
            if (firstAppAppearanceSinceStarting)
            {
                firstAppAppearanceSinceStarting = false;

                LoadPreviousSession();

                SetRemainingCardUIVisibility();
            }
        }

        // This is called on the UI thread.
        private async void LoadPreviousSession()
        {
            // This shouldn't take too long, so it's done on the UI thread.
            var loadSucceeded = LoadSession();
            if (loadSucceeded)
            {
                // Data was loaded without a problem, so verify the card count seems as expected.
                if (loadSucceeded && LoadedCardCountUnexpected())
                {
                    loadSucceeded = false;
                }
            }

            if (loadSucceeded)
            {
                LoadAllGamesPausedState();

                if (!IsGameCollectionViewBased())
                {
                    DealPyramidCardsPostprocess(false);
                }

                RefreshUpperCards();

                SetNowAsStartOfCurrentGameSessionIfAppropriate();

                // We can now proceed with loading the card images on a background thread.
                CardPackImagesLoad();
            }
            else
            {
                ClearAllPiles();

                // We'll load card images below RestartGame();
                RestartGame(false /* screenReaderAnnouncement. */);

                if (playSoundOther)
                {
                    timerPlayFirstDealSounds = new Timer(
                    new TimerCallback((s) => TimedDelayMakeFirstDealSounds()),
                        null,
                        TimeSpan.FromMilliseconds(500),
                        TimeSpan.FromMilliseconds(Timeout.Infinite));
                }
            }
        }

        private void AddOnePackToRemainingCards()
        {
            for (int rank = 1; rank <= 13; ++rank)
            {
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    if (suit == Suit.NoSuit)
                    {
                        continue;
                    }

                    _deckRemaining.Add(new Card { Rank = rank, Suit = suit });
                }
            }
        }

        public void RestartGame(bool screenReaderAnnouncement)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            ClearCardButtonSelections(true);

            _deckUpturned.Clear();

            CardDeckUpturned.Card = null;
            CardDeckUpturnedObscuredLower.Card = null;
            CardDeckUpturnedObscuredHigher.Card = null;

            SetUpturnedCardsVisuals();

            _deckRemaining.Clear();

            AddOnePackToRemainingCards();

            // Spider Solitaire uses two packs.
            if (currentGameType == SolitaireGameType.Spider)
            {
                AddOnePackToRemainingCards();

                SetSpiderDiscardedSequenceDetails("0");
            }

            _shuffler = new Shuffler();
            _shuffler.Shuffle(_deckRemaining);

            if (currentGameType == SolitaireGameType.Bakersdozen)
            {
                MoveBakersdozenKingsAroundDealtCard();
            }

            var countPiles = GetGameCardPileCount();

            // Barker Todo: On Windows simply calling vm.DealtCards[i].Clear() here and then adding 
            // the new cards can leave the suit colours in the new cards wrong. So it seems that
            // something's up with the tint colour binding. However, explicitly removing all the
            // existing cards in the pile before adding the new cards seems to avoid this. So do
            // that for now, and investigate the correct fix.

            for (int i = 0; i < countPiles; i++)
            {
#if WINDOWS
                var previousCount = vm.DealtCards[i].Count;

                for (int previousItemIndex = 0; previousItemIndex < previousCount; ++previousItemIndex)
                {
                    vm.DealtCards[i].RemoveAt(0);
                }
#else
                vm.DealtCards[i].Clear();
#endif
                if ((currentGameType == SolitaireGameType.Pyramid) ||
                    (currentGameType == SolitaireGameType.Tripeaks))
                {
                    ClearPyramidCards();
                }
            }

            DealCards();

            NextCardDeck.State = NextCardPileState.Active;

            if (screenReaderAnnouncement)
            {
                // We only play sounds if the app's ready to make sounds.
                if (playSoundOther)
                {
                    PlaySoundFile("deal.mp4");
                }

                // Some game's can take a while to start, so delay the "Game restarted" announcement.
                // Barker todo: Move the announcement to be nearer to where the restart work is done.
                timerGameRestartedAnnouncement = new Timer(
                    new TimerCallback((s) => TimedDelayGameRestartedAnnouncement()),
                        null,
                        TimeSpan.FromMilliseconds(2000),
                        TimeSpan.FromMilliseconds(Timeout.Infinite));
            }

            ClearDealtCardPileSelections();

            // Whenever a game is restarted, reset the data relating to the time spent playing.
            if (currentGameType == SolitaireGameType.Klondike)
            {
                timeStartOfThisKlondikeSession = DateTime.Now;

                Preferences.Set("KlondikeSessionDuration", 0);
            }
            else if (currentGameType == SolitaireGameType.Pyramid)
            {
                timeStartOfThisPyramidSession = DateTime.Now;

                Preferences.Set("PyramidSessionDuration", 0);
            }
            else if (currentGameType == SolitaireGameType.Tripeaks)
            {
                timeStartOfThisTripeaksSession = DateTime.Now;

                Preferences.Set("TripeaksSessionDuration", 0);
            }
            else if (currentGameType == SolitaireGameType.Bakersdozen)
            {
                timeStartOfThisBakersdozenSession = DateTime.Now;

                Preferences.Set("BakersdozenSessionDuration", 0);
            }
            else if (currentGameType == SolitaireGameType.Spider)
            {
                timeStartOfThisSpiderSession = DateTime.Now;

                Preferences.Set("SpiderSessionDuration", 0);
            }

            Debug.WriteLine("Accessible Solitaire: Zero time spent playing this game.");

            // We can now proceed with loading the card images on a background thread.
            CardPackImagesLoad();
        }

        private int GetGameCardPileCount()
        {
            int count = 0;

            switch (currentGameType)
            {
                case SolitaireGameType.Klondike:
                    count = 7;
                    break;

                case SolitaireGameType.Bakersdozen:
                    count = 13;
                    break;

                case SolitaireGameType.Spider:
                    count = 10;
                    break;

                case SolitaireGameType.Pyramid:
                    count = 7;
                    break;

               case SolitaireGameType.Tripeaks:
                    count = 4;
                    break;

                default:
                    Debug.WriteLine("GetGameCardPileCount: Unexpected game type.");
                    break;
            }

            return count;
        }

        // A game is being restarted by the user, or a previous session cannot be loaded
        // when the app is started or switching between games, so deal out the cards now.
        // Assume all dealt card piles have been cleared before this is called.

        // This will add cards to the CollectionView datasources, and so muct be called on the UI thread.
        private async void DealCards()
        {
            int cardIndex = 0;

            Debug.WriteLine("Deal, start with " + _deckRemaining.Count + " cards.");

            var countPiles = GetGameCardPileCount();

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                for (int i = 0; i < countPiles; i++)
                {
                    var rowCardCount = 0;

                    if (currentGameType == SolitaireGameType.Tripeaks)
                    {
                        if (i == countPiles - 1)
                        {
                            rowCardCount = 10;
                        }
                        else
                        {
                            rowCardCount = 3 * (i + 1);
                        }
                    }
                    else if (currentGameType == SolitaireGameType.Bakersdozen)
                    {
                        rowCardCount = 4;
                    }
                    else if (currentGameType == SolitaireGameType.Spider)
                    {
                        // The first 5 rows have a 6th card.
                        if (i < 4)
                        {
                            rowCardCount = 6;
                        }
                        else
                        {
                            rowCardCount = 5;
                        }
                    }
                    else // Klondike, Pyramid.
                    {
                        rowCardCount = (i + 1);
                    }

                    for (int j = 0; j < rowCardCount; j++)
                    {
                        var card = new DealtCard();

                        if (j == 0)
                        {
                            card.CountFaceDownCardsInPile = (currentGameType != SolitaireGameType.Spider ?
                                                                i : rowCardCount - 1);
                        }

                        card.Card = _deckRemaining[cardIndex];

                        var cardEnabled = false;

                        if (((currentGameType == SolitaireGameType.Klondike) && (j == i)) ||
                            ((currentGameType == SolitaireGameType.Spider) && (j == rowCardCount - 1)) ||
                            !IsGameCollectionViewBased())
                        {
                            cardEnabled = true;
                        }

                        // EnableCard() sets FaceDown and CardState.
                        EnableCard(card, cardEnabled);

                        // These indices are always zero-based.
                        card.CurrentCardIndexInDealtCardPile = j;
                        card.CurrentDealtCardPileIndex = i;

                        card.IsLastCardInPile = cardEnabled;

                        if (currentGameType == SolitaireGameType.Bakersdozen)
                        {
                            card.FaceDown = false;
                            card.CardState = CardState.FaceUp;
                            card.IsLastCardInPile = (j == rowCardCount - 1);
                        }

                        if (!IsGameCollectionViewBased())
                        {
                            // All of these are zero-based.
                            card.PyramidRow = i;
                            card.PyramidCardOriginalIndexInRow = j;
                            card.PyramidCardCurrentIndexInRow = j;

                            card.PyramidCardCurrentCountOfCardsOnRow = rowCardCount;
                        }

                        ++cardIndex;

                        vm.DealtCards[i].Add(card);
                    }
                }
            }

            _deckRemaining.RemoveRange(0, cardIndex);

            Debug.WriteLine("Left with " + _deckRemaining.Count + " cards remaining.");

            for (int i = 0; i < cTargetPiles; ++i)
            {
                _targetPiles[i].Clear();
            }

            ClearTargetPileButtons();
            ClearUpturnedPileButton();

            if (!IsGameCollectionViewBased())
            {
                var postprocessSuccess = DealPyramidCardsPostprocess(true);
                if (!postprocessSuccess)
                {
                    Debug.WriteLine("DealCards: DealPyramidCardsPostprocess failed.");
                }
            }
        }

        // This is called on the UI thread.
        private async void ChangeGameType(SolitaireGameType targetGameType)
        {
            Preferences.Set("ChangeGameType: targetGameType ", targetGameType.ToString());

            if (((targetGameType == SolitaireGameType.Klondike) ||
                    (targetGameType == SolitaireGameType.Bakersdozen) ||
                    (targetGameType == SolitaireGameType.Spider)) &&
                ((currentGameType != SolitaireGameType.Klondike) ||
                    (currentGameType != SolitaireGameType.Bakersdozen) ||
                    (currentGameType != SolitaireGameType.Spider)))
            {
                // Barker Todo: The CollectionViews' containing grid is -1 height here because it's 
                // yet to appear. Explicitly size the first CollectionView to be the height we know
                // the CollectionViews and their containing grid will ultimately be. Remove this at
                // some point once it's understood where the correct fix should go.

                if (!MainPage.IsPortrait())
                {
                    var pileHeight = (2 * InnerMainGrid.Height) / 3;
                    if (pileHeight > 0)
                    {
                        //CardPile1.HeightRequest = pileHeight;
                        //CardPile2.HeightRequest = pileHeight;
                        //CardPile3.HeightRequest = pileHeight;
                        //CardPile4.HeightRequest = pileHeight;
                        //CardPile5.HeightRequest = pileHeight;
                        //CardPile6.HeightRequest = pileHeight;
                        //CardPile7.HeightRequest = pileHeight;
                        //CardPile8.HeightRequest = pileHeight;
                        //CardPile9.HeightRequest = pileHeight;
                        //CardPile10.HeightRequest = pileHeight;
                        //CardPile11.HeightRequest = pileHeight;
                        //CardPile12.HeightRequest = pileHeight;
                        //CardPile13.HeightRequest = pileHeight;
                    }
                }
            }

            StopCelebratoryActions();

            // Save the current game so that we can reload it and continue it later.
            SaveSession();

            PrepareForGameTypeChange(currentGameType, targetGameType);

            // Barker Todo: Remove currentGameType now that we have vm.CurrentGameType.
            currentGameType = targetGameType;

            Preferences.Set("CurrentGameType", Convert.ToInt32(currentGameType));

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm != null)
            {
                vm.CurrentGameType = currentGameType;
            }

            Preferences.Set("ChangeGameType: currentGameType now ", currentGameType.ToString());

            var isGameCollectionViewBased = IsGameCollectionViewBased();

            CardDeckUpturnedObscuredLower.IsVisible = isGameCollectionViewBased;

            TargetPiles.IsVisible = isGameCollectionViewBased;

            CardPileGrid.IsVisible = isGameCollectionViewBased;
            CardPileGridPyramid.IsVisible = !isGameCollectionViewBased;

            ClearAllPiles();

            var loadSucceeded = LoadSession();

            ChangeGameLoadSessionPostprocess(loadSucceeded);

            // We can now proceed with loading the card images on a background thread.
            CardPackImagesLoad();
        }

        public bool LoadSession()
        {
            Debug.WriteLine("LoadSession START");

            var timeStartLoadSession = DateTime.Now;

            bool loadedSession = false;

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return false;
            }

            var preferenceSuffix = "";

            if (currentGameType == SolitaireGameType.Klondike)
            {
                // The Klondide data being loaded has no prefix.
                preferenceSuffix = "";
            }
            else if (currentGameType == SolitaireGameType.Pyramid)
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
            else
            {
                Debug.WriteLine("LoadSession: Unknown current game type.");

                return false;
            }

            Debug.WriteLine("LoadSession: currentGameType " + currentGameType);

            // If we can't find the associated data, then this might be the first time 
            // this game type is being played. If so, an exception is thrown below and
            // we'll move along to deal out the cards for a new game.
            try
            {
                // Cards in the face-down remaining cards pile.
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

                // The potentially three upturned cards.
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

                // The four target card piles.
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

                // The dealt card piles.
                for (var i = 0; i < GetGameCardPileCount(); ++i)
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

                // Set a few unpersisted dealt card properties based on the loaded data.
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
                        }
                    }
                }

                loadedSession = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadSession: " + ex);
            }

            Debug.WriteLine("LoadSession: DONE " + loadedSession +
                ", duration " + (DateTime.Now - timeStartLoadSession).TotalMilliseconds);

            return loadedSession;
        }

        private Timer? timerPackImagesLoad;

        private async void CardPackImagesLoad()
        {
            timerPackImagesLoad = new Timer(
                new TimerCallback((s) => BackgroundLoadCardImagesAfterDealtCardReady()),
                    null,
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        public static Dictionary<string, ImageSource> PackImageSourcesLarge = new Dictionary<string, ImageSource>();

        private void BackgroundLoadCardImagesAfterDealtCardReady()
        {
            timerPackImagesLoad?.Dispose();
            timerPackImagesLoad = null;

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            Debug.WriteLine("BackgroundImagesLoad: START.");

            var timeImageLoadStart = DateTime.Now;

            LoadAllCardImages();

            Debug.WriteLine("BackgroundImagesLoad: Done time (ms) = " +
                (DateTime.Now - timeImageLoadStart).TotalMilliseconds);
        }

        private void LoadAllCardImages()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            Debug.WriteLine("LoadAllCardImages: START.");

            var emptyCardName = "emptydealtcardpile.png";
            PackImageSourcesLarge.TryAdd(emptyCardName, ImageSource.FromFile(emptyCardName));

            var darkemptyCardName = "darkemptyCardName.png";
            PackImageSourcesLarge.TryAdd(darkemptyCardName, ImageSource.FromFile(darkemptyCardName));

            var timeImageLoadStart = DateTime.Now;

            // Ok, all the dealt card sources are ready, so begin loading the images.
            for (int i = 0; i < GetGameCardPileCount(); i++)
            {
                Debug.WriteLine("LoadAllCardImages: Pile " + i + ", count " + vm.DealtCards[i].Count);

                for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                {
                    var pileCard = vm.DealtCards[i][j];

                    if ((pileCard != null) && (pileCard.Card != null))
                    {
                        Debug.WriteLine("LoadAllCardImages: Get image name for Rank " +
                            pileCard.Card.Rank + ", Suit " + pileCard.Card.Suit);

                        TryToAddCardImageWithPictureToDictionary(pileCard);
                    }
                }
            }

            Debug.WriteLine("LoadAllCardImages: Done time (ms) = " +
                (DateTime.Now - timeImageLoadStart).TotalMilliseconds);
        }

        private void TryToAddCardImageWithPictureToDictionary(DealtCard pileCard)
        {
            if ((pileCard == null) || (pileCard.Card == null))
            {
                return;
            }

            var cardImageSourceName = pileCard.GetFaceupDealtCardImageSourceName();

            Debug.WriteLine("TryToAddCardImageWithPictureToDictionary: Card image name " + cardImageSourceName);

            TryToAddCardImageToDictionary(cardImageSourceName, pileCard);

            // Check if we also need the picture card image too.
            if (!ShowRankSuitLarge && (pileCard.Card.Rank > 10))
            {
                cardImageSourceName = pileCard.GetFaceupPictureDealtCardImageSourceName();

                TryToAddCardImageToDictionary(cardImageSourceName, pileCard);
            }
        }

        private async void TryToAddCardImageToDictionary(string? imageSourceName, DealtCard pileCard)
        {
            if (!string.IsNullOrEmpty(imageSourceName))
            {
                if (PackImageSourcesLarge.TryAdd(
                        imageSourceName,
                        ImageSource.FromFile(imageSourceName)))
                {
                    Debug.WriteLine("TryToAddCardImageToDictionary: Ready to use " + imageSourceName);
                }

                pileCard.RefreshCardImageSources();

                // Give the UI a chance to catch up.
                await Task.Delay(100);
            }
        }

        private int CountCards()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return 0;
            }

            var cardCount = _deckRemaining.Count + _deckUpturned.Count;

            Debug.WriteLine("CountCards: cardCount remaining " + cardCount);

            foreach (var targetPile in _targetPiles)
            {
                cardCount += targetPile.Count;
            }

            Debug.WriteLine("CountCards: cardCount including target piles " + cardCount);

            foreach (var dealtCardPile in vm.DealtCards)
            {
                // Don't include empty pile placeholders in the count.
                if (dealtCardPile.Count > 0)
                {
                    var topDealtCardInPile = dealtCardPile[dealtCardPile.Count - 1];
                    if (topDealtCardInPile.CardState != CardState.KingPlaceHolder)
                    {
                        cardCount += dealtCardPile.Count;
                    }
                }
            }

            Debug.WriteLine("LoadSession: FULL cardCount " + cardCount);

            return cardCount;
        }
    }
}
