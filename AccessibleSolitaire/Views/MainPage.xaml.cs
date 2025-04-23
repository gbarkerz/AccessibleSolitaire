using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Switch = Microsoft.Maui.Controls.Switch;

namespace Sa11ytaire4All
{
    public enum CardState
    {
        KingPlaceHolder,
        FaceDown,
        FaceUp
    }

    public enum Suit
    {
        NoSuit,
        Clubs,
        Diamonds,
        Hearts,
        Spades,
    }

    public partial class MainPage : ContentPage
    {
        static public MainPage? MainPageSingleton = null;

        // Barker: Remove the use of this static.
        static public bool ShowRankSuitLarge = false;

        private int cCardPiles = 7;

        private Shuffler? _shuffler;
        private List<Card> _deckRemaining = new List<Card>();
        private List<Card> _deckUpturned = new List<Card>();

        private int cTargetPiles = 4;
        private List<Card>[] _targetPiles = new List<Card>[4];

        private string[] localizedNumbers = new string[10];

        private Timer? timerFirstRunAnnouncement;

        // These setting are not bound to any UI.
        private bool allowSelectionByFaceDownCard = false;
        private bool playSoundSuccessfulMove = false;
        private bool playSoundUnsuccessfulMove = false;
        private bool playSoundOther = false;

        private MediaElement mainMediaElement;

        static public Dictionary<string, Color> suitColours;

        public MainPage()
        {
            MainPage.MainPageSingleton = this;

            this.BindingContext = new DealtCardViewModel();

            suitColours = new Dictionary<string, Color>();
            suitColours.Add("Default", Colors.Transparent);
            suitColours.Add("Black", Colors.Black);
            suitColours.Add("Dark Red", Colors.DarkRed);
            suitColours.Add("Dark Orange", Colors.DarkOrange);
            suitColours.Add("Dark Gold", Colors.DarkGoldenrod);
            suitColours.Add("Dark Green", Colors.DarkGreen);
            suitColours.Add("Dark Blue", Colors.DarkBlue);
            suitColours.Add("Dark Indigo", Color.FromArgb("#FF1F0954"));
            suitColours.Add("Dark Violet", Colors.DarkViolet);
            suitColours.Add("Red", Colors.Red);
            suitColours.Add("Orange", Colors.Orange);
            suitColours.Add("Dark Yellow", Color.FromArgb("#FF8B8000"));
            suitColours.Add("Green", Colors.Green);
            suitColours.Add("Blue", Colors.Blue);
            suitColours.Add("Indigo", Colors.Indigo);
            suitColours.Add("Violet", Colors.Violet);

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm != null)
            {
                vm.HighlightSelectedCardSet = (bool)Preferences.Get("HighlightSelectedCardSet", true);
                vm.MergeFaceDownCards = (bool)Preferences.Get("MergeFaceDownCards", true);
            }

            this.InitializeComponent();

            // Barker: Use of MediaElement originally worked, but after an update now crashes on Android.
            // So avoid use on Android until hopefully the crash goes away in a later updated.
#if IOS
            mainMediaElement = new CommunityToolkit.Maui.Views.MediaElement();
            mainMediaElement.ShouldShowPlaybackControls = false;
            mainMediaElement.IsVisible = false;

            InnerMainGrid.Children.Add(mainMediaElement);
#endif

            SetOrientationLayout();

            SetContainerAccessibleNames();

            CardPileGrid.SizeChanged += CardPileGrid_SizeChanged;

            for (int i = 0; i < 10; i++)
            {
                localizedNumbers[i] = MyGetString((i + 1).ToString());
            }

            for (int i = 0; i < cTargetPiles; ++i)
            {
                _targetPiles[i] = new List<Card>();
            }

            RestartGame(false /* screenReaderAnnouncement. */);

            DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;
        }

        private void SetContainerAccessibleNames()
        {
            // Barker: VoiceOver navigation is not working as hoped. If the three Grids for the 
            // upturned cards, target cards, and dealt cards are given names and so exposed in 
            // the accessibility tree, VoiceOver won't navigate to their contained elements, 
            // even when swiping down when in Container navigation as set using the Rotor.
            // (Setting the Grids to be Headers instead doesn't help.) So for now, leave the 
            // Grids for the three main areas in the game out of the accessibility tree, and 
            // have the player move through everything. (Note that VoiceOver also doesn't move
            // to the Zoom cards buttons on the dealt cards.)

            // Barker Important Todo: This is a serious impact on the VoiceOver experience, so
            // continue to investigate how this can be improved.

            // Note: I tried all sorts of things to natively set in our ConfigureMauiHandlers,
            // Handler's PlatformView.accessibilityContainerType = semanticGroup; (Value of 4)
            // in the hope of manually setting the Grid's as containers which can be moved into.
            // But I had no luck in finding how to set the native views for the Grids.

            // Note: Switch Access: We also need this Android-only for iOS Switch Control. Say 
            // we DO have the groupings here, then iOS Switch Control moves between groups as 
            // expected. But when we press the switch and the Dealt Cards group is highlighted, 
            // the middle card in the group gets clicked, rather than a scan beginning through 
            // all the cards within the dealt card pile.

            // Note: Voice Access: We also need this Android-only for iOS Voice Control. Say 
            // we DO have the groupings here, then when we say "Show names", Voice Control shows 
            // the names of the three main groups, but there are no names shown for any element
            // inside the groups, and saying the name of a dealt card has no effect.

#if (ANDROID || WINDOWS)
            var resmgr = StringResources.ResourceManager;

            SemanticProperties.SetDescription(UpturnedCardsGrid, resmgr.GetString("UpturnedCards"));
            SemanticProperties.SetDescription(TargetPiles, resmgr.GetString("TargetCardPiles"));
            SemanticProperties.SetDescription(CardPileGrid, resmgr.GetString("DealtCardPiles"));

            SemanticProperties.SetDescription(CardPile1, resmgr.GetString("CardPile1"));
            SemanticProperties.SetDescription(CardPile2, resmgr.GetString("CardPile2"));
            SemanticProperties.SetDescription(CardPile3, resmgr.GetString("CardPile3"));
            SemanticProperties.SetDescription(CardPile4, resmgr.GetString("CardPile4"));
            SemanticProperties.SetDescription(CardPile5, resmgr.GetString("CardPile5"));
            SemanticProperties.SetDescription(CardPile6, resmgr.GetString("CardPile6"));
            SemanticProperties.SetDescription(CardPile7, resmgr.GetString("CardPile7"));
#endif
        }

        public void ShowZoomedCardPopup(Card card, bool checkFaceUp)
        {
            if (card == null)
            {
                return;
            }

            bool showPopup = true;

            // Only act on face-up cards if necessary.
            if (checkFaceUp)
            {
                // Find the face-up card on which the gesture occurred.
                CollectionView? list;
                var dealtCard = FindDealtCardFromCard(card, false, out list);
                if (dealtCard == null)
                {
                    // The card to be zoomed is face-down, so don't show it.
                    showPopup = false;
                }
            }

            if (showPopup)
            {
                var vm = this.BindingContext as DealtCardViewModel;
                if (vm != null)
                {
                    var popup = new CardPopup(card, vm);

                    this.ShowPopup(popup);
                }
            }
        }

        private bool tapGestureProcessingInProgress = false;

        private void OnTapGestureRecognizerTapped(object sender, TappedEventArgs args)
        {
            Debug.WriteLine("Accessible Solitaire: Enter OnTapGestureRecognizerTapped.");

            // BARKER TODO: If the Zoom Popup is visible now, it probably means that a tap has been released
            // after a long press. So do nothing here in that case. We don't want a card to be selected as
            // part of a long press.

            var tappedCard = args.Parameter as Card;
            if (tappedCard == null)
            {
                return;
            }

            // Do not react to a tap on a card if we've still processing an earlier tap.
            if (tapGestureProcessingInProgress)
            {
                Debug.WriteLine("Accessible Solitaire: Ignore tap gesture while earlier tap gesture is in progress.");

                return;
            }

            tapGestureProcessingInProgress = true;

            // Find the card on which the tap occurred.
            CollectionView? list;
            var dealtCard = FindDealtCardFromCard(tappedCard, allowSelectionByFaceDownCard, out list);
            if ((dealtCard != null) && (list != null))
            {
                Debug.WriteLine("Tapped on card: " + dealtCard.AccessibleName);

                // Toggle the selection state of the card which was tapped on.
                if (list.SelectedItem == dealtCard)
                {
                    // On Android, deselecting a card inside the tap handler seems not to work.
                    // So delay the deselection a little until we're out of the tap handler.
                    timerDeselectDealtCard = new Timer(
                        new TimerCallback((s) => DelayDeselectDealtCard(list)),
                            null,
                            TimeSpan.FromMilliseconds(200),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                }
                else
                {
                    list.SelectedItem = dealtCard;
                }
            }

            tapGestureProcessingInProgress = false;
        }

        private Timer? timerDeselectDealtCard;

        private void DelayDeselectDealtCard(CollectionView list)
        {
            timerDeselectDealtCard?.Dispose();
            timerDeselectDealtCard = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                list.SelectedItem = null;
            });
        }

        public static CollectionView? FindCollectionView(DealtCard dealtCard)
        {
            if (MainPageSingleton == null)
            {
                return null;
            }

            return MainPageSingleton.FindCollectionViewFromDealtCard(dealtCard);
        }

        private CollectionView? FindCollectionViewFromDealtCard(DealtCard dealtCard)
        {
            CollectionView? collectionView = null;

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                for (int i = 0; i < cCardPiles; i++)
                {
                    for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                    {
                        var pileCard = vm.DealtCards[i][j];
                        if (pileCard == dealtCard)
                        {
                            collectionView = (CollectionView)CardPileGrid.FindByName("CardPile" + (i + 1));

                            break;
                        }
                    }
                }
            }

            return collectionView;
        }

        private DealtCard? FindDealtCardFromCard(Card card, bool findNearestFaceUpCard, out CollectionView? list)
        {
            DealtCard? dealtCard = null;
            DealtCard? nearestFaceUpDealtCard = null;

            list = null;

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                for (int i = 0; i < cCardPiles; i++)
                {
                    for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                    {
                        var pileCard = vm.DealtCards[i][j];

                        // Is this card face-down?
                        if (pileCard.FaceDown)
                        {
                            // If we're not interested in face-down cards, move onto the next pile.
                            if (!findNearestFaceUpCard)
                            {
                                break;
                            }
                        }
                        else
                        {
                            // Track the nearest face-up card in case we need to return it later in the 
                            // event that the tap was on a face-down card in the same dealt card pile.
                            nearestFaceUpDealtCard = pileCard;
                        }

                        if (pileCard.Card == card)
                        {
                            // We've found the card that was tapped on.
                            list = (CollectionView)CardPileGrid.FindByName("CardPile" + (i + 1));

                            dealtCard = (pileCard.FaceDown ? nearestFaceUpDealtCard : pileCard);

                            break;
                        }
                    }

                    if (dealtCard != null)
                    {
                        break;
                    }
                }
            }

            return dealtCard;
        }

        // General game-playing options.
        private int OptionCardTurnCount = 1;
        private bool OptionKingsOnlyToEmptyPile = false;

        private bool firstAppAppearanceSinceStarting = true;

        // We always set the suit colours on startup.
        private bool InitialSetSuitColours = true;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Accessibility-related options.

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                var longPressZoomDuration = (int)Preferences.Get("LongPressZoomDuration", 2000);
                vm.LongPressZoomDuration = longPressZoomDuration;

                //if ((OriginalCardWidth == 0) && (OriginalCardHeight == 0))
                //{
                //    // Barker: On iOS, the app apparently can hang trying to relayout the app.
                //    // As such, make all the game UI invisible while the CardWidth and CardHeight
                //    // properties are adjusted, and make evreything visible again shortly afterwards.
                //    SetCollectionViewsVisibility(false);

                //    UpperGrid.IsVisible = false;
                //    UpperGrid.IsVisible = false;

                //    vm.CardWidth = OriginalCardWidth;
                //    vm.CardHeight = OriginalCardHeight;

                //    timerRelayoutApp = new Timer(
                //        new TimerCallback((s) => DelayedRelayoutApp()),
                //            null,
                //            TimeSpan.FromMilliseconds(500),
                //            TimeSpan.FromMilliseconds(Timeout.Infinite));
                //}

                var previousColoursClubs = vm.SuitColoursClubs;
                var previousColoursDiamonds = vm.SuitColoursDiamonds;
                var previousColoursHearts = vm.SuitColoursHearts;
                var previousColoursSpades = vm.SuitColoursSpades;

                try
                {
                    var suitColourNameClubs = (string)Preferences.Get("SuitColoursClubs", "Default");
                    vm.SuitColoursClubs = DetermineSuitColour(suitColourNameClubs, Colors.Black, Colors.White);

                    var suitColourNameDiamonds = (string)Preferences.Get("SuitColoursDiamonds", "Default");
                    vm.SuitColoursDiamonds = DetermineSuitColour(suitColourNameDiamonds, Colors.Red, Colors.Red);

                    var suitColourNameHearts = (string)Preferences.Get("SuitColoursHearts", "Default");
                    vm.SuitColoursHearts = DetermineSuitColour(suitColourNameHearts, Colors.Red, Colors.Red);

                    var suitColourNameSpades = (string)Preferences.Get("SuitColoursSpades", "Default");
                    vm.SuitColoursSpades = DetermineSuitColour(suitColourNameSpades, Colors.Black, Colors.White);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MainPage Attempt to handle suit colours: " + ex.Message);

                    vm.SuitColoursClubs = Colors.Black;
                    vm.SuitColoursDiamonds = Colors.Red;
                    vm.SuitColoursHearts = Colors.Red;
                    vm.SuitColoursSpades = Colors.Black;
                }

                // Barker: Remove use of this static.

                var previousShowRankSuitLarge = MainPage.ShowRankSuitLarge;
                MainPage.ShowRankSuitLarge = (bool)Preferences.Get("ShowRankSuitLarge", true);

                // Refresh the visuals on all cards if necessary.
                if (MainPage.ShowRankSuitLarge != previousShowRankSuitLarge)
                {
                    RefreshAllCardVisuals();
                }

                // If we don't set the colours here, the default colours show initially.
                if (InitialSetSuitColours ||
                    (previousColoursClubs != vm.SuitColoursClubs) ||
                    (previousColoursDiamonds != vm.SuitColoursDiamonds) ||
                    (previousColoursHearts != vm.SuitColoursHearts) ||
                    (previousColoursSpades != vm.SuitColoursSpades))
                {
                    InitialSetSuitColours = false;

                    timerSetSuitColours = new Timer(
                        new TimerCallback((s) => DelayedTimerSetSuitColours()),
                            null,
                            TimeSpan.FromMilliseconds(1000),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                }

                var previousMergeFaceDownCards = vm.MergeFaceDownCards;
                vm.MergeFaceDownCards = (bool)Preferences.Get("MergeFaceDownCards", true);

                // Refresh the accessibility of all cards if necessary.
                if (vm.MergeFaceDownCards != previousMergeFaceDownCards)
                {
                    RefreshAllDealtCardPileCardIsInAccessibleTree();
                }

                vm.HighlightSelectedCardSet = (bool)Preferences.Get("HighlightSelectedCardSet", true);

                vm.FlipGameLayoutHorizontally = (bool)Preferences.Get("FlipGameLayoutHorizontally", false);

                var showStateAnnouncementButton = (bool)Preferences.Get("ShowStateAnnouncementButton", false);
                StateAnnouncementButton.IsVisible = showStateAnnouncementButton;

                vm.ExtendDealtCardHitTarget = (bool)Preferences.Get("ExtendDealtCardHitTarget", false);

                // General game-playing options.

                OptionCardTurnCount = (int)Preferences.Get("CardTurnCount", 1);
                OptionKingsOnlyToEmptyPile = (bool)Preferences.Get("KingsOnlyToEmptyPile", false);

                allowSelectionByFaceDownCard = (bool)Preferences.Get("AllowSelectionByFaceDownCard", true);

                playSoundSuccessfulMove = (bool)Preferences.Get("PlaySoundSuccessfulMove", false);
                playSoundUnsuccessfulMove = (bool)Preferences.Get("PlaySoundUnsuccessfulMove", false);
                playSoundOther = (bool)Preferences.Get("PlaySoundOther", false);

                // First-run message.

                var showFirstRunMessage = (bool)Preferences.Get("ShowFirstRunMessage", true);
                if (showFirstRunMessage)
                {
                    Preferences.Set("ShowFirstRunMessage", false);

                    timerFirstRunAnnouncement = new Timer(
                        new TimerCallback((s) => ShowFirstRunMessage()),
                            null,
                            TimeSpan.FromMilliseconds(1000),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                }

                // We wait to play the first sounds in the app until the UI is appearing.
                if (firstAppAppearanceSinceStarting)
                {
                    firstAppAppearanceSinceStarting = false;

                    timerPlayFirstDealSounds = new Timer(
                        new TimerCallback((s) => MakeFirstDealSounds()),
                            null,
                            TimeSpan.FromMilliseconds(500),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                }
            }
        }

        private void RefreshAllCardVisuals()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                // Refresh all the face-up cards to show the required visuals.
                for (int i = 0; i < cCardPiles; i++)
                {
                    for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                    {
                        var pileCard = vm.DealtCards[i][j];

                        pileCard.RefreshVisuals();
                    }
                }

                CardDeckUpturnedObscuredLower.RefreshVisuals();
                CardDeckUpturnedObscuredHigher.RefreshVisuals();
                CardDeckUpturned.RefreshVisuals();

                TargetPileC.RefreshVisuals();
                TargetPileD.RefreshVisuals();
                TargetPileH.RefreshVisuals();
                TargetPileS.RefreshVisuals();
            }
        }

        private Color DetermineSuitColour(string suitColourName, Color DefaultLight, Color DefaultDark)
        {
            Color suitColour;

            if (suitColourName == "Default")
            {
                suitColour = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                        DefaultLight : DefaultDark);
            }
            else
            {
                suitColour = suitColours[suitColourName];
            }

            return suitColour;
        }

        private Timer timerSetSuitColours;

        private void DelayedTimerSetSuitColours()
        {
            timerSetSuitColours?.Dispose();
            timerSetSuitColours = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SetCardSuitColours(CardDeckUpturnedObscuredLower);
                SetCardSuitColours(CardDeckUpturnedObscuredHigher);
                SetCardSuitColours(CardDeckUpturned);

                SetCardSuitColours(TargetPileC);
                SetCardSuitColours(TargetPileD);
                SetCardSuitColours(TargetPileH);
                SetCardSuitColours(TargetPileS);
            });
        }

        private void MakeFirstDealSounds()
        {
            timerPlayFirstDealSounds?.Dispose();
            timerPlayFirstDealSounds = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlaySoundFile("deal.mp4");
            });
        }

        private Timer? timerPlayFirstDealSounds;

        //private Timer? timerRelayoutApp;

        //private void DelayedRelayoutApp()
        //{
        //    timerRelayoutApp?.Dispose();
        //    timerRelayoutApp = null;

        //    // Always run this on the UI thread.
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        UpperGrid.IsVisible = true;
        //        UpperGrid.IsVisible = true;

        //        SetCollectionViewsVisibility(true);
        //    });
        //}

        private void ShowFirstRunMessage()
        {
            timerFirstRunAnnouncement?.Dispose();
            timerFirstRunAnnouncement = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var title = MyGetString("Sa11ytaire");
                var message = MyGetString("FirstRunMessage");
                var btnText = MyGetString("OK");

                DisplayAlert(title, message, btnText);
            });
        }

        private bool cardButtonStateInProgress = false;

        // Make sure all CardButtones showing a card are enabled.
        private void CheckAllCardButtonState()
        {
            if (cardButtonStateInProgress)
            {
                return;
            }

            cardButtonStateInProgress = true;

            // Any CardButton showing a card should be enabled.
            CheckCardButtonState(CardDeckUpturnedObscuredLower);
            CheckCardButtonState(CardDeckUpturnedObscuredHigher);
            CheckCardButtonState(CardDeckUpturned);

            CheckCardButtonState(TargetPileC);
            CheckCardButtonState(TargetPileD);
            CheckCardButtonState(TargetPileH);
            CheckCardButtonState(TargetPileS);

            cardButtonStateInProgress = false;
        }

        private void CheckCardButtonState(CardButton CardButton)
        {
            if (CardButton.Card != null)
            {
                // Always run this on the UI thread.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CardButton.IsEnabled = true;

                    // Make sure the contained Switch is visible.
                    var cardButton = CardButton.FindByName("CardButton") as Switch;
                    if (cardButton != null)
                    {
                        cardButton.IsVisible = true;
                    }

                    // Now set the appropriate accessible name on the CardButton.
                    var accessibleName = CardButton.CardPileAccessibleName;
                    SemanticProperties.SetDescription(CardButton, accessibleName);
                });
            }
        }

        private double originalCardWidth = 0;
        private double originalCardHeight = 0;

        private void CardPileGrid_SizeChanged(object? sender, EventArgs e)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            // On iOS the topmost Grid containing the app UI also contains the system's status and navigation
            // base, which we do not want to include here. So base the app's UI sizing on the inner main Grid
            // which does not contain those bars.
            var mainContentGrid = InnerMainGrid;

            if (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
            {
                vm.CardHeight = mainContentGrid.Height / 9;
                vm.CardWidth = mainContentGrid.Width / 5;
            }
            else
            {
                vm.CardHeight = mainContentGrid.Height / 3;
                vm.CardWidth = mainContentGrid.Width / 7;
            }

            bool refreshAllCardVisuals = false;

            if (vm.CardWidth > 0)
            {
                if (OriginalCardWidth == 0)
                {
                    OriginalCardWidth = vm.CardWidth;

                    refreshAllCardVisuals = true;
                }

                vm.CardWidth = OriginalCardWidth;
            }

            if (vm.CardHeight > 0)
            {
                if (OriginalCardHeight == 0)
                {
                    OriginalCardHeight = vm.CardHeight;

                    refreshAllCardVisuals = true;
                }

                vm.CardHeight = OriginalCardHeight;
            }

            if (refreshAllCardVisuals)
            {
                RefreshAllCardVisuals();
            }
        }

        private void ClearTargetPileButtons()
        {
            TargetPileC.Card = null;
            TargetPileD.Card = null;
            TargetPileH.Card = null;
            TargetPileS.Card = null;
        }

        private async void DealCards()
        {
            int cardIndex = 0;

            Debug.WriteLine("Deal, start with " + _deckRemaining.Count + " cards.");

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                for (int i = 0; i < cCardPiles; i++)
                {
                    vm.DealtCards[i].Clear();

                    for (int j = 0; j < (i + 1); j++)
                    {
                        var card = new DealtCard();

                        if (j == 0)
                        {
                            card.CountFaceDownCardsInPile = i;
                        }

                        card.Card = _deckRemaining[cardIndex];

                        // EnableCard() sets FaceDown and CardState.
                        EnableCard(card, (j == i));

                        // These indices are always zero-based.
                        card.CurrentCardIndexInDealtCardPile = j;
                        card.CurrentDealtCardPileIndex = i;

                        card.IsLastCardInPile = (j == i);

                        ++cardIndex;

                        vm.DealtCards[i].Add(card);
                    }

                    // Give the UI a chance to catch up.
                    await Task.Delay(10);
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
        }

        private void ClearUpturnedPileButton()
        {
            SetUpturnedCardsVisuals();
        }

        private void ClearCardButtonSelections(bool includeUpturnedCard)
        {
            if (includeUpturnedCard)
            {
                CardDeckUpturned.IsToggled = false;

#if WINDOWS
                CardDeckUpturned.RefreshVisuals();
#endif
            }

            TargetPileC.IsToggled = false;
            TargetPileD.IsToggled = false;
            TargetPileH.IsToggled = false;
            TargetPileS.IsToggled = false;
        }

        public void RestartGame(bool screenReaderAnnouncement)
        {
            ClearCardButtonSelections(true);

            _deckUpturned.Clear();

            CardDeckUpturned.Card = null;
            CardDeckUpturnedObscuredLower.Card = null;
            CardDeckUpturnedObscuredHigher.Card = null;

            SetUpturnedCardsVisuals();

            _deckRemaining.Clear();

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

            _shuffler = new Shuffler();
            _shuffler.Shuffle(_deckRemaining);

            // Barker: On my old iPod touch, the first dealing of the cards
            // has the lowest first face-down on each pile a very small height. I've not 
            // yet figured out why this is happening. The item height converter, 
            // CardInAccessibleTreeToHeightRequestConverter(), doesn't seem to have a path
            // where that can happen. Restarting the game always fixes the problem. So for
            // now, add a delay to increase the likelihood of the relevant UI being
            // initialised as required before the cards are dealt.

            // Barker Important Todo: Figure out what's really happening and remove the timer.
            timerDelayDealCards = new Timer(
                new TimerCallback((s) => DelayDealCards()),
                    null,
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromMilliseconds(Timeout.Infinite));

            NextCardDeck.IsEmpty = false;

            if (screenReaderAnnouncement)
            {
                // We only play sounds if the app's ready to make sounds.
                PlaySoundFile("deal.mp4");

                var announcement = MainPage.MyGetString("GameRestarted");
                MakeDelayedScreenReaderAnnouncement(announcement);
            }

            ClearDealtCardPileSelections();
        }

        private Timer? timerDelayDealCards;

        private void DelayDealCards()
        {
            timerDelayDealCards?.Dispose();
            timerDelayDealCards = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DealCards();
            });
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }

        private int GetTargetPileIndex(CardButton CardButton)
        {
            int index = -1;

            string pileId = CardButton.AutomationId.Replace("TargetPile", "");
            switch (pileId)
            {
                case "C":
                    index = 0;
                    break;
                case "D":
                    index = 1;
                    break;
                case "H":
                    index = 2;
                    break;
                case "S":
                    index = 3;
                    break;
            }

            return index;
        }

        private void SetUpturnedCardsVisuals()
        {
            if (_deckUpturned.Count == 0)
            {
                CardDeckUpturned.Card = null;
                CardDeckUpturnedObscuredHigher.Card = null;
                CardDeckUpturnedObscuredLower.Card = null;
            }
            else
            {
                CardDeckUpturned.IsEnabled = true;
                CardDeckUpturned.Card = _deckUpturned[_deckUpturned.Count - 1];

                CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 1 ?
                                                        _deckUpturned[_deckUpturned.Count - 2] : null);

                CardDeckUpturnedObscuredLower.Card = (_deckUpturned.Count > 2 ?
                                                        _deckUpturned[_deckUpturned.Count - 3] : null);

                // Couldn't seem to get the binding to work for the suit colours, so set them explicitly here. 
                SetCardSuitColours(CardDeckUpturned);
                SetCardSuitColours(CardDeckUpturnedObscuredHigher);
                SetCardSuitColours(CardDeckUpturnedObscuredLower);
            }
        }

        private void SetCardSuitColours(CardButton cardButton)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if (vm != null)
            {
                cardButton.SuitColoursClubsSwitch = vm.SuitColoursClubs;
                cardButton.SuitColoursDiamondsSwitch = vm.SuitColoursDiamonds;
                cardButton.SuitColoursHeartsSwitch = vm.SuitColoursHearts;
                cardButton.SuitColoursSpadesSwitch = vm.SuitColoursSpades;
            }
        }

        // Barker: Use the approved method of getting the items source.
        private ObservableCollection<DealtCard>? GetListSource(CollectionView? list)
        {
            if (list == null)
            {
                return null;
            }

            ObservableCollection<DealtCard>? col = null;

            int index = int.Parse(list.AutomationId.Replace("CardPile", ""));
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                col = vm.DealtCards[index - 1];
            }

            return col;
        }

        private void EnableCard(DealtCard card, bool enable)
        {
            card.FaceDown = !enable;
            card.CardState = (enable ? CardState.FaceUp : CardState.FaceDown);
        }

        private bool CanMoveCardToDealtCardPile(DealtCard cardBelow, DealtCard cardAbove)
        {
            bool canMove = false;

            if ((cardBelow != null) && (cardAbove != null))
            {
                // A card can only be moved to a dealt card pile (from the upturned card pile,
                // or target card pile, or another dealt card pile) is the card on which the 
                // moved card will lie is the last card in the destination dealt card pile.
                if (cardBelow.IsLastCardInPile)
                {
                    if (cardBelow.Card.Rank == cardAbove.Card.Rank + 1)
                    {
                        bool isBelowRed = ((cardBelow.Card.Suit == Suit.Diamonds) || (cardBelow.Card.Suit == Suit.Hearts));
                        bool isAboveRed = ((cardAbove.Card.Suit == Suit.Diamonds) || (cardAbove.Card.Suit == Suit.Hearts));

                        canMove = (isBelowRed != isAboveRed);
                    }
                }

                // Check whether we can move the card to an empty pile if necessary.
                // NO! Don't include moves to empty card piles, as that fills the announcement.
                //if (!canMove && checkEmptyPile && (cardBelow.CardState == CardState.KingPlaceHolder))
                //{
                //    if (!optionKingsOnlyToEmptyPile || (cardAbove.Card.Rank == 13))
                //    {
                //        canMove = true;
                //    }
                //}
            }

            return canMove;
        }

        public void RaiseNotificationEvent(string? announcement)
        {
            if (announcement != null)
            {
                Debug.WriteLine("RaiseNotificationEvent: " + announcement);

                SemanticScreenReader.Default.Announce(announcement);
            }
        }

        private async void ShowEndOfGameDialog()
        {
            var answer = await DisplayAlert(
                MainPage.MyGetString("Sa11ytaire"),
                MainPage.MyGetString("QueryRestartWonGame"),
                MainPage.MyGetString("Yes"),
                MainPage.MyGetString("No"));

            if (answer)
            {
                RestartGame(true /* screenReaderAnnouncement. */);
            }
        }

        private void StateAnnouncementButton_Clicked(object sender, EventArgs e)
        {
            var announcementRemainingCards = AnnounceStateRemainingCards();

            var announceStateTargetPiles = AnnounceStateTargetPiles();

            var announcementDealtCards = AnnounceStateDealtCardPiles();

            var fullAnnouncement = announcementRemainingCards + " " +
                                    announceStateTargetPiles + " " +
                                    announcementDealtCards;

            // On iOS, VoiceOver always seems to announce the name of the button after it's been clicked,
            // and that interupts the game state announcement. So for now, delay the start of the 
            // announcement to avoid that interuption. Barker Todo: Investigate this unexpected 
            // announcement of the button name.
            MakeDelayedScreenReaderAnnouncement(fullAnnouncement);

            Debug.WriteLine("State announcement: " + fullAnnouncement);
        }

        private void MakeDelayedScreenReaderAnnouncement(string? announcement)
        {
            if (announcement == null)
            {
                return;
            }
            
            if (timerDelayScreenReaderAnnouncement == null)
            {
                timerDelayScreenReaderAnnouncement = new Timer(
                    new TimerCallback((s) => MakeScreenReaderAnnouncement(announcement)),
                        announcement,
                        TimeSpan.FromMilliseconds(200),
                        TimeSpan.FromMilliseconds(Timeout.Infinite));
            }
        }

        private Timer? timerDelayScreenReaderAnnouncement;

        public double OriginalCardWidth { get => originalCardWidth; set => originalCardWidth = value; }
        public double OriginalCardHeight { get => originalCardHeight; set => originalCardHeight = value; }

        private void MakeScreenReaderAnnouncement(object announcement)
        {
            Debug.WriteLine("MakeScreenReaderAnnouncement: " + announcement);

            timerDelayScreenReaderAnnouncement?.Dispose();
            timerDelayScreenReaderAnnouncement = null;

            var fullAnnouncement = (string?)announcement;
            if (!String.IsNullOrEmpty(fullAnnouncement))
            {
                // Always run this on the UI thread.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RaiseNotificationEvent(fullAnnouncement);
                });
            }
        }

        public string AnnounceStateRemainingCards()
        {
            string stateMessage = "";

            if (_deckUpturned.Count > 0)
            {
                stateMessage += MyGetString("TopUpturnedCardIs") + " " +
                    _deckUpturned[_deckUpturned.Count - 1].GetCardAccessibleName();
            }

            if (_deckUpturned.Count > 1)
            {
                stateMessage += ", " + MyGetString("Then") + " " +
                    _deckUpturned[_deckUpturned.Count - 2].GetCardAccessibleName();
            }

            if (_deckUpturned.Count > 2)
            {
                stateMessage += ", " + MyGetString("Then") + " " +
                    _deckUpturned[_deckUpturned.Count - 3].GetCardAccessibleName();
            }

            if (_deckUpturned.Count > 0)
            {
                stateMessage += ". ";
            }

            if (_deckUpturned.Count == 0)
            {
                stateMessage = MyGetString("ThereAreNoUpturnedCards") + ".";
            }

            if (_deckRemaining.Count > 0)
            {
                stateMessage += " " + MyGetString("MoreCardsAreAvailable") + ".";
            }

            return stateMessage;
        }

        public string AnnounceStateTargetPiles()
        {
            string stateMessage = MyGetString("TargetPiles") + ", ";

            string empty = MyGetString("Empty");
            string pile = MyGetString("Pile");

            stateMessage +=
                (TargetPileC.Card == null ?
                    empty + " " + MyGetString("Clubs") + " " + pile : TargetPileC.Card.GetCardAccessibleName()) + ", " +
                (TargetPileD.Card == null ?
                    empty + " " + MyGetString("Diamonds") + " " + pile : TargetPileD.Card.GetCardAccessibleName()) + ", " +
                (TargetPileH.Card == null ?
                    empty + " " + MyGetString("Hearts") + " " + pile : TargetPileH.Card.GetCardAccessibleName()) + ", " +
                (TargetPileS.Card == null ?
                    empty + " " + MyGetString("Spades") + " " + pile : TargetPileS.Card.GetCardAccessibleName()) + ".";

            return stateMessage;
        }

        public static string MyGetString(string stringId)
        {
            var requiredString = StringResources.ResourceManager.GetString(stringId);
            if (requiredString == null)
            {
                return "";
            }

            return requiredString;
        }

        public string AnnounceStateDealtCardPiles()
        {
            string stateMessage = "";

            string empty = MyGetString("Empty");
            string pile = MyGetString("Pile");
            string to = MyGetString("To");
            string card = MyGetString("Card");
            string cards = MyGetString("Cards");
            string facedown = MyGetString("FaceDown");

            for (int i = 0; i < cCardPiles; i++)
            {
                stateMessage += pile + " " + (i + 1) + ", ";

                int cFaceDown = 0;
                int indexLastFaceUp = -1;

                var vm = this.BindingContext as DealtCardViewModel;
                if ((vm == null) || (vm.DealtCards == null))
                {
                    break;
                }

                for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                {
                    if (j == vm.DealtCards[i].Count - 1)
                    {
                        if ((vm.DealtCards[i][j] as DealtCard).CardState == CardState.KingPlaceHolder)
                        {
                            stateMessage += empty;
                        }
                        else
                        {
                            stateMessage += (vm.DealtCards[i][j] as DealtCard).AccessibleNameWithoutSelectionAndMofN;
                        }
                    }
                    else
                    {
                        if ((vm.DealtCards[i][j] as DealtCard).FaceDown)
                        {
                            ++cFaceDown;
                        }
                        else
                        {
                            indexLastFaceUp = j;
                        }
                    }
                }

                if ((indexLastFaceUp != -1) && (indexLastFaceUp != vm.DealtCards[i].Count - 1))
                {
                    stateMessage += " " + to + " " + (vm.DealtCards[i][indexLastFaceUp] as DealtCard).AccessibleName;
                }

                stateMessage += ", ";

                if (cFaceDown > 0)
                {
                    stateMessage += cFaceDown + " " +
                        (cFaceDown > 1 ? cards : card) + " " + facedown;

                    stateMessage += (i < cCardPiles - 1 ? ", " : ".");
                }
            }

            return stateMessage;
        }

        private void VerticalStackLayout_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BackgroundColor")
            {
                var verticalStackLayout = sender as VerticalStackLayout;
                if (verticalStackLayout == null)
                {
                    return;
                }

                if (verticalStackLayout.BackgroundColor == null)
                {
                    return;
                }

                var dealtCard = verticalStackLayout.BindingContext as DealtCard;
                if (dealtCard != null)
                {
                    SetInSelectedSet(dealtCard);
                }

                return;
            }
        }

        private void SetInSelectedSet(DealtCard dealtCard)
        {
            if (dealtCard == null)
            {
                return;
            }

            // Change the selection state of all face-up cards above this card in the same dealt card pile.
            var foundSelectedCard = false;

            // Get the CollectionView for the dealt card pile containing the dealt card of interest.
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                var dealtPileCollectionView = vm.DealtCards[dealtCard.CurrentDealtCardPileIndex];

                for (int i = 0; i < dealtPileCollectionView.Count; ++i)
                {
                    var nextCard = dealtPileCollectionView[i];

                    if (foundSelectedCard)
                    {
                        nextCard.InSelectedSet = dealtCard.CardSelected;
                    }
                    else if (nextCard == dealtCard)
                    {
                        foundSelectedCard = true;
                    }
                }
            }
        }

        private void PlaySound(bool success)
        {
#if IOS
            // Only play a sound if the relevant setting is turned on.
            if ((success && playSoundSuccessfulMove) ||
                (!success && playSoundUnsuccessfulMove))
            {
                mainMediaElement.Source = MediaSource.FromResource(
                                            success ? "successmove.mp4" : "notsuccessmove.mp4");

                mainMediaElement.Play();
            }
#endif
        }

        private void PlaySoundFile(string soundFilename)
        {
#if IOS
            if (string.IsNullOrEmpty(soundFilename))
            {
                return;
            }

            if (playSoundOther)
            {
                try
                {
                    mainMediaElement.Source = MediaSource.FromResource(soundFilename);

                    mainMediaElement.Play();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("PlaySoundFile: " + ex.Message);
                }
            }
#endif
        }

        private void TouchBehavior_LongPressCompleted(object sender, CommunityToolkit.Maui.Core.LongPressCompletedEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                var bindingContext = border.BindingContext;
                if (bindingContext != null)
                {
                    var dealtCard = bindingContext as DealtCard;
                    if (dealtCard != null)
                    {
                        if ((dealtCard.Card != null) && (dealtCard.Card.Suit != Suit.NoSuit))
                        {
                            var vm = this.BindingContext as DealtCardViewModel;
                            if (vm != null)
                            {
                                var popup = new CardPopup(dealtCard.Card, vm);

                                this.ShowPopup(popup);
                            }
                        }
                    }
                }
            }
        }

        private string Sa11ytaireHelpPage =
            "https://www.facebook.com/permalink.php?story_fbid=pfbid036UmuvcKvmqBjkk2m2DRFNx7F7EpNwBJbPgTunUrvPnWebpkHuJikxpibtYijoP6tl&id=61564996280215";

        public async void LaunchHelp()
        {
            try
            {
                // Open the Sa11ytaireHelp page on Facebook.
                Uri uri = new Uri(Sa11ytaireHelpPage);

                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to open browser: " + ex.Message);

                // An unexpected error occurred. Perhaps no browser is available on the device.
            }
        }

        public async void QueryRestartGame()
        {
            var answer = await DisplayAlert(
                MainPage.MyGetString("Sa11ytaire"),
                MainPage.MyGetString("QueryRestartGame"),
                MainPage.MyGetString("Yes"),
                MainPage.MyGetString("No"));

            if (answer)
            {
                Shell.Current.FlyoutIsPresented = false;

                MainPage.MainPageSingleton?.RestartGame(true);
            }
        }
    }
}
