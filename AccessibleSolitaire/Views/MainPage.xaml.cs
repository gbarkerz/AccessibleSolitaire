using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Plugin.Maui.KeyListener;
using Sa11ytaire4All;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

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

        public string[] localizedNumbers = new string[13];

        // Barker Todo: Remove as much of the timer use as possible.
        private Timer? timerFirstRunAnnouncement;
        private Timer? timerPlayFirstDealSounds;
        private Timer? timerSetSuitColours;
        private Timer? timerDelayDealCards;
        private Timer? timerDelayCardSpin;
        private Timer? timerDelayScreenReaderAnnouncement;
        private Timer? timerDelayAttemptToMoveCard;
        private Timer? timerDeselectDealtCard;
        private Timer? timerSelectDealtCard;

        // These setting are not bound to any UI.
        private bool allowSelectionByFaceDownCard = false;
        private bool includeFacedownCardsInAnnouncement = false;
        private bool playSoundSuccessfulMove = false;
        private bool playSoundUnsuccessfulMove = false;
        private bool playSoundOther = false;
        private bool celebrationExperienceVisual = false;
        private bool celebrationExperienceAudio = false;
        private string celebrationExperienceAudioFile = "";
        private bool automaticallyAnnounceMoves = false;

        private MediaElement mainMediaElement = new MediaElement();

        public static Dictionary<string, Color> suitColours =
            new Dictionary<string, Color>
            {
                { "Default", Colors.Transparent },
                { "Black", Colors.Black },
                { "DarkRed", Colors.DarkRed },
                { "DarkOrange", Colors.DarkOrange },
                { "DarkGold", Colors.DarkGoldenrod },
                { "DarkGreen", Colors.DarkGreen },
                { "DarkBlue", Colors.DarkBlue },
                { "DarkIndigo", Color.FromArgb("#FF1F0954") },
                { "DarkViolet", Colors.DarkViolet },
                { "White", Colors.White },
                { "Yellow", Colors.Yellow },
                { "Pink", Color.FromArgb("#FFFF74A0") }, // Colors.Pink is too light. 
                { "Cyan", Colors.Cyan },
                { "LightBlue", Colors.LightBlue },
                { "LightGreen", Colors.LightGreen },
                { "LightCoral", Colors.LightCoral }
            };

        private KeyboardBehavior? keyboardBehavior;

        public MainPage()
        {
            MainPage.MainPageSingleton = this;

            // We must set this early, as it gets used before it gets set later in OnAppearing().
            currentGameType = (SolitaireGameType)Preferences.Get("CurrentGameType",
                                    Convert.ToInt32(SolitaireGameType.Klondike));

            this.BindingContext = new DealtCardViewModel();

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm != null)
            {
                vm.MergeFaceDownCards = MainPage.GetMergeFaceDownCardsSetting();
            }

            this.InitializeComponent();

            // Add all the buttons for a game of Pyramid Solitaire.
            AddPyramidButtons();
    
            if (mainMediaElement != null)
            {
                mainMediaElement.ShouldShowPlaybackControls = false;
                mainMediaElement.IsVisible = false;
            }

#if WINDOWS || ANDROID
            if (mainMediaElement != null)
            {
                InnerMainGrid.Children.Add(mainMediaElement);
            }
#endif

            SetOrientationLayout();

            SetContainerAccessibleNames();

            InnerMainGrid.SizeChanged += InnerMainPageGrid_SizeChanged;

            for (int i = 0; i < 13; i++)
            {
                localizedNumbers[i] = MyGetString((i + 1).ToString());
            }

            for (int i = 0; i < cTargetPiles; ++i)
            {
                _targetPiles[i] = new List<Card>();
            }

            DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;

            if (mainMediaElement != null)
            {
                mainMediaElement.MediaEnded += MainMediaElement_MediaEnded;
            }

            this.Unloaded += MainPage_Unloaded;

            // Initialize cross-platform keyboard behavior
            keyboardBehavior = new KeyboardBehavior();
            this.Behaviors.Add(keyboardBehavior);

#if IOS
            // On iOS, if the dealt card piles' CollectionViews have a SelectionMode of Single,
            // a selected card cannot be programmatically deselected. Apparently is we have a 
            // SelectionMode of None, a CollectionView does maintain whatever SelectedItem we
            // set, and yet it doesn't impact the behavior of the items.

            CardPile1.SelectionMode = SelectionMode.None;
            CardPile2.SelectionMode = SelectionMode.None;
            CardPile3.SelectionMode = SelectionMode.None;
            CardPile4.SelectionMode = SelectionMode.None;
            CardPile5.SelectionMode = SelectionMode.None;
            CardPile6.SelectionMode = SelectionMode.None;
            CardPile7.SelectionMode = SelectionMode.None;
            CardPile8.SelectionMode = SelectionMode.None;
            CardPile9.SelectionMode = SelectionMode.None;
            CardPile10.SelectionMode = SelectionMode.None;
            CardPile11.SelectionMode = SelectionMode.None;
            CardPile12.SelectionMode = SelectionMode.None;
            CardPile13.SelectionMode = SelectionMode.None;
#endif
        }

        private void Current_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
        {
            Debug.WriteLine("Current_MainDisplayInfoChanged: e.DisplayInfo.Orientation " + 
                e.DisplayInfo.Orientation.ToString());

            MainPage.MainPageSingleton?.SetOrientationLayout();
        }

        private void MainMediaElement_MediaEnded(object? sender, EventArgs e)
        {
            if (mainMediaElement == null)
            {
                return;
            }

            // Barker Todo: On Android, the media element can be accessed outside of the app 
            // by swiping down from the top of the screen while the app's running. Figure out
            // how to prevent that. In the meantime, prevent that standalone media UI from 
            // actually being able to play audio by nulling out the MediaElement source once 
            // media play has completed.

            Debug.WriteLine("MainMediaElement_MediaEnded called.");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                mainMediaElement.Source = null;
            });
        }

        private void MainPage_Unloaded(object? sender, EventArgs e)
        {
            if (mainMediaElement == null)
            {
                return;
            }

            // Release the MediaElement to prevent the standalone media element being available
            // after the ap has been closed.
            mainMediaElement.Handler?.DisconnectHandler();
        }

        public static bool GetMergeFaceDownCardsSetting()
        {
            // Don't merge face down cards on Windows.
            var mergeFaceDownCardsSetting = false;

#if IOS || ANDROID
            mergeFaceDownCardsSetting = (bool)Preferences.Get("MergeFaceDownCards", true);
#endif
            return mergeFaceDownCardsSetting;
        }

        // Barker Todo: Until adding the Baker's Dozen game, we always considered there 
        // to be 7 dealt card piles, regardless of what game was current. Continue to 
        // that here, except for returning 13 when the current game really is Baker's Dozen.
        // Consider more closely exactly how this count is used, and how it relates to 
        // the Pyramid and Tri Pakes games.
        private int GetCardPileCount()
        {
            return (currentGameType != SolitaireGameType.Bakersdozen ? cCardPiles : 13);
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
            var resmgr = Strings.StringResources.ResourceManager;

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
            SemanticProperties.SetDescription(CardPile8, resmgr.GetString("CardPile8"));
            SemanticProperties.SetDescription(CardPile9, resmgr.GetString("CardPile9"));
            SemanticProperties.SetDescription(CardPile10, resmgr.GetString("CardPile10"));
            SemanticProperties.SetDescription(CardPile11, resmgr.GetString("CardPile11"));
            SemanticProperties.SetDescription(CardPile12, resmgr.GetString("CardPile12"));
            SemanticProperties.SetDescription(CardPile13, resmgr.GetString("CardPile13"));
#endif
        }

        public async void ShowZoomedCardPopup(Card? card, bool checkFaceUp)
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
                var dealtCard = this.BindingContext as DealtCard;
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

                    await this.ShowPopupAsync(popup);
                }
            }
        }

        // Sentry reported two different crashes: 
        //   collectionView:viewForSupplementaryElementOfKind:atIndexPath: > Attempted to dereference null pointer.
        //   System.NullReferenceException: void GroupableItemsViewController<ReorderableItemsView>.UpdateDefaultSupplementaryView()
        // which it felt were related to rapid touch input on the dealt cards. It suggested preventing re-entrancy 
        // in the Tapped handler, but the code was already doing that. Until this is better understood, attempt to 
        // prevent the two crashes by ignoring a tap if it's less than 500ms since the previous tap. Hopefully that
        // will give the app a chance to respond to the tap (including performing whatever selection, deselection,
        // or change to CollectionView contents is appropriate) before handling the next tap. This will certainly
        // be noticeable to users using touch input without a screen reader, but seems unlikely to input using 
        // screen reader, voice input, or switch input. It's the users of AT who are the main priority for this app.
        private DateTime timePreviousDealtCardTap = DateTime.Now;

        private bool tapGestureProcessingInProgress = false;

        private Card mostRecentlyTappedCard = new Card();

        private void OnTapGestureRecognizerTapped(object sender, TappedEventArgs args)
        {
            //Debug.WriteLine("OnTapGestureRecognizerTapped: Enter OnTapGestureRecognizerTapped.");

            var timeNow = DateTime.Now;
            if (timeNow - timePreviousDealtCardTap < TimeSpan.FromMilliseconds(500))
            {
                Debug.WriteLine("OnTapGestureRecognizerTapped: Ignore rapid touch tap.");

                return;
            }

            timePreviousDealtCardTap = timeNow;

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
                Debug.WriteLine("OnTapGestureRecognizerTapped: Ignore tap gesture while earlier tap gesture is in progress.");

                return;
            }

            tapGestureProcessingInProgress = true;

            // Find the card on which the tap occurred.
            CollectionView? list;
            var dealtCard = FindDealtCardFromCard(tappedCard, allowSelectionByFaceDownCard, out list);
            if ((dealtCard != null) && (list != null))
            {
                //Debug.WriteLine("OnTapGestureRecognizerTapped: Tapped on card: " + dealtCard.AccessibleName);

                // Toggle the selection state of the card which was tapped on.
                if (list.SelectedItem == dealtCard)
                {
                    //Debug.WriteLine("OnTapGestureRecognizerTapped: Delay deselection of: " + dealtCard.AccessibleName);

                    string announcement = MyGetString("Deselected") + " " + dealtCard.AccessibleNameWithoutSelectionAndMofN;
                    MakeDelayedScreenReaderAnnouncement(announcement, false);

                    // On Android, deselecting a card inside the tap handler seems not to work.
                    // So delay the deselection a little until we're out of the tap handler.
                    timerDeselectDealtCard = new Timer(
                        new TimerCallback((s) => TimedDelayDeselectDealtCard(list)),
                            null,
                            TimeSpan.FromMilliseconds(300),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                }
                else
                {
                    //Debug.WriteLine("OnTapGestureRecognizerTapped: Selecting now: " + dealtCard.AccessibleName);

                    if ((dealtCard != null) && (dealtCard.Card != null))
                    {
                        // TESTING: Note which dealt card was tapped on.
                        mostRecentlyTappedCard.Rank = dealtCard.Card.Rank;
                        mostRecentlyTappedCard.Suit = dealtCard.Card.Suit;

                        timerSelectDealtCard = new Timer(
                            new TimerCallback((s) => TimedDelaySelectDealtCard(list, dealtCard)),
                                null,
                                TimeSpan.FromMilliseconds(300),
                                TimeSpan.FromMilliseconds(Timeout.Infinite));
                    }
                }
            }

            tapGestureProcessingInProgress = false;
        }


        private void TimedDelayDeselectDealtCard(CollectionView list)
        {
            timerDeselectDealtCard?.Dispose();
            timerDeselectDealtCard = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DeselectAllCardsFromDealtCardPile(list);
            });
        }

        private void TimedDelaySelectDealtCard(CollectionView list, DealtCard dealtCard)
        {
            timerSelectDealtCard?.Dispose();
            timerSelectDealtCard = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                list.SelectedItem = dealtCard;
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
                for (int i = 0; i < GetCardPileCount(); i++)
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

        public DealtCard? FindAnyDealtCardFromCard(Card card)
        {
            DealtCard? dealtCard = null;

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                    {
                        var pileCard = (DealtCard?)vm.DealtCards[i][j];
                        if ((pileCard != null) && (pileCard.Card == card))
                        {
                            dealtCard = pileCard;
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

        public DealtCard? FindDealtCardFromCard(Card card, bool findNearestFaceUpCard, out CollectionView? list)
        {
            DealtCard? dealtCard = null;
            DealtCard? nearestFaceUpDealtCard = null;

            list = null;

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                for (int i = 0; i < GetCardPileCount(); i++)
                {
                    for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                    {
                        var pileCard = vm.DealtCards[i][j];

                        // Is this card face-down?
                        if (pileCard.FaceDown)
                        {
                            // If we're not interested in face-down cards, move onto the next pile.
                            if (!findNearestFaceUpCard && (currentGameType == SolitaireGameType.Klondike))
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
        private bool OptionAutoCompleteGame = false;

        private bool firstAppAppearanceSinceStarting = true;
        private bool firstSizeHandlingSinceStarting = false;

        // We always set the suit colours on startup.
        private bool InitialSetSuitColours = true;

        protected override void OnAppearing()
        {
            Debug.WriteLine("OnAppearing: START");

            var timeOnAppearingStart = DateTime.Now;

            base.OnAppearing();

            // Accessibility-related options.

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                currentGameType = (SolitaireGameType)Preferences.Get("CurrentGameType", 
                                                        Convert.ToInt32(SolitaireGameType.Klondike));

                vm.CurrentGameType = currentGameType;

                SetRemainingCardUIVisibility();

                if ((currentGameType == SolitaireGameType.Klondike) ||
                    (currentGameType == SolitaireGameType.Bakersdozen))
                {
                    CardPileGrid.IsVisible = true;
                    CardPileGridPyramid.IsVisible = false;
                }
                else
                {
                    CardPileGrid.IsVisible = false;
                    CardPileGridPyramid.IsVisible = true;
                }

                RefreshDealtCardPilesIsInAccessibleTree();

                PauseResumeButton.IsVisible = (bool)Preferences.Get("ShowPauseResumeButton", false);

                // If the Pause/Resume button is not visible, make sure no game is currently paused.
                if (!PauseResumeButton.IsVisible)
                {
                    vm.GamePausedKlondike = false;
                    vm.GamePausedPyramid = false;
                    vm.GamePausedTripeaks = false;
                    vm.GamePausedBakersdozen = false;

                    Preferences.Set("GamePausedKlondike", false);
                    Preferences.Set("GamePausedPyramid", false);
                    Preferences.Set("GamePausedTripeaks", false);
                    Preferences.Set("GamePausedBakersdozen", false);
                    
                    SetPauseResumeButtonState();
                }

                var longPressZoomDuration = (int)Preferences.Get("LongPressZoomDuration", 2000);
                vm.LongPressZoomDuration = longPressZoomDuration;

                var previousColoursClubs = vm.SuitColoursClubs;
                var previousColoursDiamonds = vm.SuitColoursDiamonds;
                var previousColoursHearts = vm.SuitColoursHearts;
                var previousColoursSpades = vm.SuitColoursSpades;

                try
                {
                    var suitColourNameClubs = (string)Preferences.Get("SuitColoursClubs", "Default");
                    vm.SuitColoursClubs = DetermineSuitColour(suitColourNameClubs, Colors.Black, Colors.White);

                    var suitColourNameDiamonds = (string)Preferences.Get("SuitColoursDiamonds", "Default");
                    vm.SuitColoursDiamonds = DetermineSuitColour(suitColourNameDiamonds,
                                                Color.FromRgb(0xD0, 0x00, 0x00), Colors.Red);

                    var suitColourNameHearts = (string)Preferences.Get("SuitColoursHearts", "Default");
                    vm.SuitColoursHearts = DetermineSuitColour(suitColourNameHearts,
                                                Color.FromRgb(0xD0, 0x00, 0x00), Colors.Red);

                    var suitColourNameSpades = (string)Preferences.Get("SuitColoursSpades", "Default");
                    vm.SuitColoursSpades = DetermineSuitColour(suitColourNameSpades, Colors.Black, Colors.White);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MainPage Attempt to handle suit colours: " + ex.Message);

                    if (Application.Current != null) // To prevent build warning.
                    {
                        if (Application.Current.RequestedTheme != AppTheme.Dark)
                        {
                            vm.SuitColoursClubs = Colors.Black;
                            vm.SuitColoursDiamonds = Colors.Red;
                            vm.SuitColoursHearts = Colors.Red;
                            vm.SuitColoursSpades = Colors.Black;
                        }
                        else
                        {
                            vm.SuitColoursClubs = Colors.White;
                            vm.SuitColoursDiamonds = Color.FromRgb(0xD0, 0x00, 0x00);
                            vm.SuitColoursHearts = Color.FromRgb(0xD0, 0x00, 0x00);
                            vm.SuitColoursSpades = Colors.White;
                        }
                    }
                }

                // Barker: Remove use of this static.

                var previousShowRankSuitLarge = MainPage.ShowRankSuitLarge;
                MainPage.ShowRankSuitLarge = (bool)Preferences.Get("ShowRankSuitLarge", true);

                // Refresh the visuals on all cards if necessary.
                if (MainPage.ShowRankSuitLarge != previousShowRankSuitLarge)
                {
                    // Make sure all required images are loaded.
                    // Barker: Move this to a background thread.
                    LoadAllCardImages();

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
                        new TimerCallback((s) => TimedDelaySetSuitColours()),
                            null,
                            TimeSpan.FromMilliseconds(1000),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                }

                var previousMergeFaceDownCards = vm.MergeFaceDownCards;
                vm.MergeFaceDownCards = MainPage.GetMergeFaceDownCardsSetting();

                vm.FlipGameLayoutHorizontally = (bool)Preferences.Get("FlipGameLayoutHorizontally", false);

                // Screen reader-related settings.

                vm.ShowScreenReaderAnnouncementButtons = (bool)Preferences.Get("ShowScreenReaderAnnouncementButtons", false);
                ScreenReaderAnnouncementButtons.IsVisible = vm.ShowScreenReaderAnnouncementButtons;

                automaticallyAnnounceMoves = (bool)Preferences.Get("AutomaticallyAnnounceMoves", false);

                var previousAddHintToTopmostCard = vm.AddHintToTopmostCard;
                vm.AddHintToTopmostCard = (bool)Preferences.Get("AddHintToTopmostCard", false);

                includeFacedownCardsInAnnouncement = (bool)Preferences.Get("IncludeFacedownCardsInAnnouncement", false);

                var previousExtendDealtCardHitTarget = vm.ExtendDealtCardHitTarget;
                vm.ExtendDealtCardHitTarget = (bool)Preferences.Get("ExtendDealtCardHitTarget", false);

                // Refresh the dealt card dimensions is necessary.
                if ((vm.MergeFaceDownCards != previousMergeFaceDownCards) ||
                    (vm.ExtendDealtCardHitTarget != previousExtendDealtCardHitTarget))
                {
                    RefreshAllCardVisuals();
                }

                // Refresh the accessibility of all cards if necessary.
                if ((vm.AddHintToTopmostCard != previousAddHintToTopmostCard) ||
                    (vm.MergeFaceDownCards != previousMergeFaceDownCards))
                {
                    RefreshAllDealtCardPileCardIsInAccessibleTree();
                }

                // Change the heading state of the target card piles if necessary.
                var previousCardButtonsHeadingLevel = vm.CardButtonsHeadingState;
                vm.CardButtonsHeadingState = (bool)Preferences.Get("CardButtonsHeadingState", false);

                if (firstAppAppearanceSinceStarting || (previousCardButtonsHeadingLevel != vm.CardButtonsHeadingState))
                {
                    SetCardButtonsHeadingState(vm.CardButtonsHeadingState);
                }

                // General game-playing options.

                OptionCardTurnCount = (int)Preferences.Get("CardTurnCount", 1);
                OptionKingsOnlyToEmptyPile = (bool)Preferences.Get("KingsOnlyToEmptyPile", false);

                OptionAutoCompleteGame = (bool)Preferences.Get("AutoCompleteGame", false);

                allowSelectionByFaceDownCard = (bool)Preferences.Get("AllowSelectionByFaceDownCard", true);

                playSoundSuccessfulMove = (bool)Preferences.Get("PlaySoundSuccessfulMove", false);
                playSoundUnsuccessfulMove = (bool)Preferences.Get("PlaySoundUnsuccessfulMove", false);
                playSoundOther = (bool)Preferences.Get("PlaySoundOther", false);

                celebrationExperienceVisual = (bool)Preferences.Get("CelebrationExperienceVisual", false);
                celebrationExperienceAudio = (bool)Preferences.Get("CelebrationExperienceAudio", false);
                celebrationExperienceAudioFile = (string)Preferences.Get("CelebrationExperienceAudioFile", "brasscelebration");

                // Barker Todo: It seems when the page reappears after dismissing the Settings page,
                // the MediaElement behaves as though it's not longer attached beneath the Main page.
                // So re-add it here, but figure this out! (Hopefully the fact the same element's 
                // been added multiple times isn't going to cause problems in the meantime.)
#if IOS
                if (playSoundSuccessfulMove || playSoundUnsuccessfulMove || playSoundOther ||
                    celebrationExperienceAudio || celebrationExperienceVisual)
                {
                    if (mainMediaElement != null)
                    {
                        InnerMainGrid.Children.Add(mainMediaElement);
                    }
                }
#endif
                // First-run message.

                var showFirstRunMessage = (bool)Preferences.Get("ShowFirstRunMessage", true);
                if (showFirstRunMessage)
                {
                    Preferences.Set("ShowFirstRunMessage", false);

                    var delay = 1000;

#if WINDOWS
                    // Barker Todo: In testing, a delay of 1 second on Windows leads to an exception
                    // relating to there being no root element when trying to display the first run 
                    // message. So for now, add a hacky 5 second delay instead.
                    delay = 5000;

                    // Note: An attempt to replace the use of this timer with a Dispatcher.Dispatch() still
                    // resulted in a crash on Windows. So stick with the timer until this is understood.
#endif
                    timerFirstRunAnnouncement = new Timer(
                        new TimerCallback((s) => TimedDelayShowFirstRunMessage()),
                            null,
                            TimeSpan.FromMilliseconds(delay),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                }

                // We wait to play the first sounds in the app until the UI is appearing.
                if (firstAppAppearanceSinceStarting)
                {
                    firstAppAppearanceSinceStarting = false;

                    // Don't load the card images until the sessions been loaded.
                    ReadyToLoadCardImages = false;

                    Debug.WriteLine("OnAppearing: Loading previous session.");

                    CardPileGrid.Loaded += CardPileGrid_Loaded;
                }
            }

            var timeInOnAppearing = (DateTime.Now - timeOnAppearingStart).TotalMilliseconds;

            Debug.WriteLine("OnAppearing: DONE timeInOnAppearing ms " + timeInOnAppearing);
        }

        private void CardPileGrid_Loaded(object? sender, EventArgs e)
        {
            timerDelayLoadPreviousSession = new Timer(
                        new TimerCallback((s) => TimedDelayLoadPreviousSession()),
                            null,
                            TimeSpan.FromMilliseconds(2000),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        private Timer? timerDelayLoadPreviousSession;

        private void TimedDelayLoadPreviousSession()
        {
            timerDelayLoadPreviousSession?.Dispose();
            timerDelayLoadPreviousSession = null;

            ReadyToLoadCardImages = false;

            LoadPreviousSession();

            // Make sure all the required card images get loaded up now.
            CardPackImagesLoad();
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

            Debug.WriteLine("BackgroundImagesLoad: ReadyToLoadCardImages at start " + ReadyToLoadCardImages);

            // Wait in this background thread until all cards have been dealt.
            while (!ReadyToLoadCardImages)
            {
                Debug.WriteLine("BackgroundImagesLoad: ReadyToLoadCardImages waiting...");

                Thread.Sleep(200);
            }

            Debug.WriteLine("BackgroundImagesLoad: ReadyToLoadCardImages load cards now.");

            // Ok, all the dealt card sources are ready, so begin loading the images.
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
            for (int i = 0; i < GetCardPileCount(); i++)
            {
                Debug.WriteLine("LoadAllCardImages: Pile " + 1 + ", count " + vm.DealtCards[i].Count);

                for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                {
                    var pileCard = vm.DealtCards[i][j];

                    string? cardImageSourceName = null;

                    if ((pileCard != null) && (pileCard.Card != null))
                    {
                        Debug.WriteLine("LoadAllCardImages: Get image name for Rank " +
                            pileCard.Card.Rank + ", Suit " + pileCard.Card.Suit);

                        TryToAddCardImageWithPictureToDictionary(pileCard);

                        Debug.WriteLine("LoadAllCardImages: Image name " + cardImageSourceName);
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

                    pileCard.RefreshCardImageSources();

                    // Give the UI a chance to catch up.
                    await Task.Delay(100);
                }
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

        // This is called on a background thread.
        private async void LoadPreviousSession()
        {
            // Don't load the card images until the sessions been loaded.
            ReadyToLoadCardImages = false;

            var loadSucceeded = await LoadSession();

            //Thread.Sleep(10000);

            //loadSucceeded = false;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (loadSucceeded)
                {
                    if (loadSucceeded && LoadedCardCountUnexpected())
                    {
                        loadSucceeded = false;
                    }
                }

                ResizeDealtCard(true);

                if (loadSucceeded)
                {
                    LoadAllGamesPausedState();

                    if ((currentGameType != SolitaireGameType.Klondike) &&
                        (currentGameType != SolitaireGameType.Bakersdozen))
                    {
                        DealPyramidCardsPostprocess(false);
                    }

                    RefreshUpperCards();

                    SetNowAsStartOfCurrentGameSessionIfAppropriate();
                }
                else
                {
                    ClearAllPiles();

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
            });

            Debug.WriteLine("LoadPreviousSession: Set ReadyToLoadCardImages true.");

            // We can now proceed with loading the card images.
            ReadyToLoadCardImages = true;
        }

        public bool ReadyToLoadCardImages = false;

        private void SetCardButtonsHeadingState(bool isHeading)
        {
            CardDeckUpturned.SetHeadingState(isHeading);

            TargetPileC.SetHeadingState(isHeading);
            TargetPileD.SetHeadingState(isHeading);
            TargetPileH.SetHeadingState(isHeading);
            TargetPileS.SetHeadingState(isHeading);

            if ((currentGameType != SolitaireGameType.Klondike) &&
                (currentGameType != SolitaireGameType.Bakersdozen))
            {
                var vm = this.BindingContext as DealtCardViewModel;
                if ((vm != null) && (vm.DealtCards != null))
                {
                    for (int i = 0; i < GetCardPileCount(); i++)
                    {
                        for (int j = 0; j < vm.DealtCards[i].Count; ++j)
                        {
                            var dealtCard = vm.DealtCards[i][j];
                            if ((dealtCard != null) && (dealtCard.Card != null))
                            {
                                // We've found the first dealt card on this row, so find the associated CardButton.
                                int cardButtonPyramidIndex;
                                var cardButton = GetCardButtonFromPyramidDealtCard(dealtCard, out cardButtonPyramidIndex);
                                if (cardButton != null)
                                {
                                    // Now set the first card in the row to have the required Heading state.
                                    cardButton.SetHeadingState(isHeading);
                                }

                                // We don't need to consider other cards on this row, as they can never have been headings yet.
                                break;
                            }
                        }
                    }
                }

            }
        }

        private void RefreshAllCardVisuals()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                // Refresh all the cards to show the required visuals.
                if ((currentGameType == SolitaireGameType.Klondike) ||
                    (currentGameType == SolitaireGameType.Bakersdozen))
                {
                    for (int i = 0; i < GetCardPileCount(); i++)
                    {
                        for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                        {
                            var pileCard = vm.DealtCards[i][j];

                            pileCard.RefreshVisuals();
                        }
                    }

                    TargetPileC.RefreshVisuals();
                    TargetPileD.RefreshVisuals();
                    TargetPileH.RefreshVisuals();
                    TargetPileS.RefreshVisuals();

                    CardDeckUpturnedObscuredLower.RefreshVisuals();
                }
                else
                {
                    var pyramidCards = CardPileGridPyramid.Children;

                    foreach (var pyramidCard in pyramidCards)
                    {
                        var card = pyramidCard as CardButton;
                        if ((card != null) && (card.Card != null))
                        {
                            card.RefreshVisuals();
                        }
                    }
                }

                CardDeckUpturnedObscuredHigher.RefreshVisuals();
                CardDeckUpturned.RefreshVisuals();
            }
        }

        private Color? DetermineSuitColour(string suitColourName, Color DefaultLight, Color DefaultDark)
        {
            // Avoid build warnings re: Application.Current being null.
            if (Application.Current == null)
            {
                return null;
            }

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

        private void TimedDelaySetSuitColours()
        {
            Debug.WriteLine("TimedDelaySetSuitColours: START");

            timerSetSuitColours?.Dispose();
            timerSetSuitColours = null;

            Debug.WriteLine("TimedDelaySetSuitColours: SetCardSuitColours for upturned cards.");

            SetCardSuitColours(CardDeckUpturnedObscuredLower);
            SetCardSuitColours(CardDeckUpturnedObscuredHigher);
            SetCardSuitColours(CardDeckUpturned);

            SetCardSuitColours(TargetPileC);
            SetCardSuitColours(TargetPileD);
            SetCardSuitColours(TargetPileH);
            SetCardSuitColours(TargetPileS);

            Debug.WriteLine("TimedDelaySetSuitColours: About to dispatch.");

            Dispatcher.Dispatch(() =>
            {
                Debug.WriteLine("TimedDelaySetSuitColours: About to call RefreshAllCardVisuals.");

                RefreshAllCardVisuals();

                Debug.WriteLine("TimedDelaySetSuitColours: DONE on dispatched thread.");
            });

            Debug.WriteLine("TimedDelaySetSuitColours: DONE on calling thread.");
        }

        private void TimedDelayMakeFirstDealSounds()
        {
            timerPlayFirstDealSounds?.Dispose();
            timerPlayFirstDealSounds = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlaySoundFile("deal.mp4");
            });
        }

        private void TimedDelayShowFirstRunMessage()
        {
            timerFirstRunAnnouncement?.Dispose();
            timerFirstRunAnnouncement = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var title = MyGetString("AccessibleSolitaire");
                var message = MyGetString("FirstRunMessage");

#if WINDOWS
                message += "\r\n\r\n"+ MyGetString("FirstRunMessageWindows");
#endif
                var btnText = MyGetString("OK");

                try
                {
                    DisplayAlertAsync(title, message, btnText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ShowFirstRunMessage: " + ex.Message);
                }
            });
        }

        private bool cardButtonStateInProgress = false;

        // Make sure all CardButtons showing a card are enabled.
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
                    CardButton.IsEnabled = true;                    // Make sure the contained Switch is visible.
                    var cardButton = CardButton.FindByName("CardButton") as Microsoft.Maui.Controls.Switch;
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

        static public bool IsPortrait()
        {
            var isPortrait = (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait);

#if WINDOWS
            isPortrait = false;

            if (MainPage.MainPageSingleton != null)
            {
                var grid = MainPage.MainPageSingleton.InnerMainGrid;
                if (grid != null)
                {
                    if ((grid.Bounds.Width > 0) && (grid.Bounds.Height > 0))
                    {
                        isPortrait = (grid.Bounds.Height > grid.Bounds.Width);
                    }
                }
            }

            // Barker Todo: The portrait layout of the CollectionViews isn't working yet,
            // so for now, only present landscape orientation.
            isPortrait = false;
#endif
            return isPortrait;
        }

        private void InnerMainPageGrid_SizeChanged(object? sender, EventArgs e)
        {
            Debug.WriteLine("InnerMainPageGrid_SizeChanged: Set all sizes and layout.");

            // The number of rows in the main area may need to be updated based on the current game.
            SetOrientationLayout();

            ResizeDealtCard(false);
        }

        private void ClearTargetPileButtons()
        {
            TargetPileC.Card = null;
            TargetPileD.Card = null;
            TargetPileH.Card = null;
            TargetPileS.Card = null;
        }

        // Assume all dealt card piles have been cleared before this is called.
        private async void DealCards()
        {
            ReadyToLoadCardImages = false;

            int cardIndex = 0;

            Debug.WriteLine("Deal, start with " + _deckRemaining.Count + " cards.");

            var countPiles = (currentGameType == SolitaireGameType.Tripeaks ? 4 : GetCardPileCount());

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
                    else // Klondike, Pyramid.
                    {
                        rowCardCount = (i + 1);
                    }

                    for (int j = 0; j < rowCardCount; j++)
                    {
                        var card = new DealtCard();

                        if (j == 0)
                        {
                            card.CountFaceDownCardsInPile = i;
                        }

                        card.Card = _deckRemaining[cardIndex];

                        var cardEnabled = (j == i) ||
                                            ((vm.CurrentGameType != SolitaireGameType.Klondike) &&
                                             (vm.CurrentGameType != SolitaireGameType.Bakersdozen));

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

                        if ((currentGameType != SolitaireGameType.Klondike) && 
                            (currentGameType != SolitaireGameType.Bakersdozen))
                        {
                            // All of these are zero-based.
                            card.PyramidRow = i;
                            card.PyramidCardOriginalIndexInRow = j;
                            card.PyramidCardCurrentIndexInRow = j;

                            card.PyramidCardCurrentCountOfCardsOnRow = rowCardCount;
                        }

                        ++cardIndex;

                        vm.DealtCards[i].Add(card);

                        // Give the UI a chance to catch up.
                        await Task.Delay(100);
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

            if ((currentGameType != SolitaireGameType.Klondike) && 
                (currentGameType != SolitaireGameType.Bakersdozen))
            {
                var postprocessSuccess = DealPyramidCardsPostprocess(true);
                if (!postprocessSuccess)
                {
                    Debug.WriteLine("DealCards: DealPyramidCardsPostprocess failed.");
                }
            }

            Debug.WriteLine("DealCards: Set ReadyToLoadCardImages true.");

            // We can now proceed with loading the card images.
            ReadyToLoadCardImages = true;
        }

        private void ClearUpturnedPileButton()
        {
            SetUpturnedCardsVisuals();
        }

        private void SetCardButtonToggledSelectionState(CardButton cardButton, bool isSelected)
        {
            if (cardButton.IsToggled != isSelected)
            {
                cardButton.IsToggled = isSelected;

                cardButton.RefreshVisuals();
            }
        }

        private void ClearCardButtonSelections(bool includeUpturnedCard)
        {
            if (includeUpturnedCard)
            {
                SetCardButtonToggledSelectionState(CardDeckUpturned, false);

                if ((currentGameType != SolitaireGameType.Klondike) && 
                    (currentGameType != SolitaireGameType.Bakersdozen))
                {
                    SetCardButtonToggledSelectionState(CardDeckUpturnedObscuredHigher, false);
                }
            }

            SetCardButtonToggledSelectionState(TargetPileC, false);
            SetCardButtonToggledSelectionState(TargetPileD, false);
            SetCardButtonToggledSelectionState(TargetPileH, false);
            SetCardButtonToggledSelectionState(TargetPileS, false);
        }

        public void RestartGame(bool screenReaderAnnouncement)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Don't load the card images until the cards have all been re-dealt.
            ReadyToLoadCardImages = false;

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

            if (currentGameType == SolitaireGameType.Bakersdozen)
            {
                MoveBakersdozenKingsAroundDealtCard();
            }

            var countPiles = (currentGameType == SolitaireGameType.Tripeaks ? 4 : GetCardPileCount());

            // Barker Todo: On Windows simply calling vm.DealtCards[i].Clear() here and then adding 
            // the new cards can leave the suit colours in the new cards wrong. So it seems that
            // something's up with the tint colour binding. However, explicitly removing all the
            // existing cards in the pile before adding the new cards seems to avoid this. So do
            // that for now, and investigate the correct fix.

            if ((currentGameType == SolitaireGameType.Klondike) ||
                (currentGameType == SolitaireGameType.Bakersdozen))
            {
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
                }
            }
            else
            {
                ClearPyramidCards();
            }

            ReadyToLoadCardImages = false;

            timerDelayDealCards = new Timer(
                new TimerCallback((s) => TimedDelayDealCards()),
                    null,
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromMilliseconds(Timeout.Infinite));

            // Make sure all the required card images get loaded up now.
            CardPackImagesLoad();

            NextCardDeck.State = NextCardPileState.Active;

            if (screenReaderAnnouncement)
            {
                // We only play sounds if the app's ready to make sounds.
                if (playSoundOther)
                {
                    PlaySoundFile("deal.mp4");
                }

                var announcement = MainPage.MyGetString("GameRestarted");
                MakeDelayedScreenReaderAnnouncement(announcement, false);
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

            Debug.WriteLine("Accessible Solitaire: Zero time spent playing this game.");
        }

        private void MoveBakersdozenKingsAroundDealtCard()
        {
            // Find all the Kings in the dealt cards.
            for (int i = 0; i < _deckRemaining.Count; ++i)
            {
                var currentCard = _deckRemaining[i];
                if (currentCard.Rank == 13)
                {
                    // Swap this King with what will be the bottom card in the dealt card.
                    var indexOfCardToSwap = 4 * (i / 4);
                    if (i != indexOfCardToSwap)
                    {
                        var cardToSwap = _deckRemaining[indexOfCardToSwap];

                        // Check for the card at the bottom of the pile already being a King.
                        // We know we can get 4 Kings in a single pile.
                        while (cardToSwap.Rank == 13)
                        {
                            ++indexOfCardToSwap;

                            cardToSwap = _deckRemaining[indexOfCardToSwap];
                        }

                        Card tempCard = new Card();
                        tempCard.Suit = cardToSwap.Suit;
                        tempCard.Rank = cardToSwap.Rank;

                        _deckRemaining[indexOfCardToSwap].Suit = currentCard.Suit;
                        _deckRemaining[indexOfCardToSwap].Rank = currentCard.Rank;

                        _deckRemaining[i].Suit = tempCard.Suit;
                        _deckRemaining[i].Rank = tempCard.Rank;
                    }
                }
            }
        }

        private void TimedDelayDealCards()
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

        // Toggle the pasued state of the current game.
        private void PauseResumeButton_Click(object sender, EventArgs e)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            var currentGameIsPaused = IsCurrentGamePaused();
            Debug.WriteLine("PauseResumeButton_Click: Current paused state " + currentGameIsPaused);

            string announcement = "";

            if (currentGameIsPaused)
            {
                Debug.WriteLine("PauseResumeButton_Click: Set now as current start time for game session.");

                // Note that we must toggle the game state before calling SaveCurrentTimeSpentPlayingStateIfAppropriate();
                ToggleCurrentGamePausedState();

                // The current game is paused, and is being resumed. So set the start time 
                // for the  in-progress session to now, as we resume the session.
                SetNowAsStartOfCurrentGameSessionIfAppropriate();

                announcement = MyGetString("GameHasResumed");
            }
            else
            {
                Debug.WriteLine("PauseResumeButton_Click: Persist game session time.");

                // The current game is not paused, so we'll be pausing it here. So persist 
                // the current time spent playing the current session of the game.
                SaveCurrentTimeSpentPlayingStateIfAppropriate();

                ToggleCurrentGamePausedState();

                announcement = MyGetString("GameIsNowPaused");
            }

            MakeDelayedScreenReaderAnnouncement(announcement, false);

            // Now change the visuals relating to the current paused state of the game.
            SetPauseResumeButtonState();
        }

        private void ToggleCurrentGamePausedState()
        {
            Debug.WriteLine("ToggleCurrentGamePausedState: Toggle game state.");

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            switch (currentGameType)
            {
                case SolitaireGameType.Klondike:
                    vm.GamePausedKlondike = !vm.GamePausedKlondike;
                    Preferences.Set("GamePausedKlondike", vm.GamePausedKlondike);

                    Debug.WriteLine("PauseResumeButton_Click: Klondike paused game state " + vm.GamePausedKlondike);
                    break;

                case SolitaireGameType.Pyramid:
                    vm.GamePausedPyramid = !vm.GamePausedPyramid;
                    Preferences.Set("GamePausedPyramid", vm.GamePausedPyramid);

                    Debug.WriteLine("PauseResumeButton_Click: Pyramid paused game state " + vm.GamePausedPyramid);
                    break;

                case SolitaireGameType.Tripeaks:
                    vm.GamePausedTripeaks = !vm.GamePausedTripeaks;
                    Preferences.Set("GamePausedTripeaks", vm.GamePausedTripeaks);

                    Debug.WriteLine("PauseResumeButton_Click: Tripeaks paused game state " + vm.GamePausedTripeaks);
                    break;

                case SolitaireGameType.Bakersdozen:
                    vm.GamePausedBakersdozen = !vm.GamePausedBakersdozen;
                    Preferences.Set("GamePausedBakersdozen", vm.GamePausedBakersdozen);

                    Debug.WriteLine("PauseResumeButton_Click: Bakersdozen paused game state " + vm.GamePausedBakersdozen);
                    break;

                default:
                    break;
            }
        }

        // Barker Todo: Replace this explicit button state setting with binding at some point.
        private void SetPauseResumeButtonState()
        {
            if (Application.Current == null)
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            var resmgr = Strings.StringResources.ResourceManager;

            if (IsCurrentGamePaused())
            {
                SemanticProperties.SetDescription(PauseResumeButton, resmgr.GetString("ResumeGame"));

                PauseResumeButton.Source = "resumegame.png";

                MainPageGrid.BackgroundColor = Colors.DarkGray;
            }
            else
            {
                SemanticProperties.SetDescription(PauseResumeButton, resmgr.GetString("PauseGame"));

                PauseResumeButton.Source = "pausegame.png";

                MainPageGrid.BackgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                                        Colors.Green : Color.FromRgb(0x00, 0x40, 0x00));
            }
        }

        public static bool IsCurrentGamePaused()
        {
            if (Application.Current == null)
            {
                return false;
            }

            if (MainPageSingleton == null)
            {
                return false;
            }

            var vm = MainPageSingleton.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return false;
            }

            var currentGameIsPaused = false;

            switch (currentGameType)
            {
                case SolitaireGameType.Klondike:

                    Debug.WriteLine("IsCurrentGamePaused: Current Klondike paused state " + currentGameIsPaused);

                    currentGameIsPaused = vm.GamePausedKlondike;
                    break;

                case SolitaireGameType.Pyramid:

                    Debug.WriteLine("IsCurrentGamePaused: Current Pyramid paused state " + currentGameIsPaused);

                    currentGameIsPaused = vm.GamePausedPyramid;
                    break;

                case SolitaireGameType.Tripeaks:

                    Debug.WriteLine("IsCurrentGamePaused: Current Tripeaks paused state " + currentGameIsPaused);

                    currentGameIsPaused = vm.GamePausedTripeaks;
                    break;

                case SolitaireGameType.Bakersdozen:

                    Debug.WriteLine("IsCurrentGamePaused: Current Bakersdozen paused state " + currentGameIsPaused);

                    currentGameIsPaused = vm.GamePausedBakersdozen;
                    break;

                default:
                    break;
            }

            return currentGameIsPaused;
        }

        public static async void ShowGameIsPausedMessage()
        {
            if (MainPageSingleton == null)
            {
                return;
            }

            var title = MyGetString("AccessibleSolitaire");
            var message = MyGetString("GameIsPaused");
            var btnText = MyGetString("OK");

            await MainPageSingleton.DisplayAlertAsync(title, message, btnText);
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
                SetUpturnedCards();
            }

            // Couldn't seem to get the binding to work for the suit colours, so set them explicitly here. 
            SetCardSuitColours(CardDeckUpturned);
            SetCardSuitColours(CardDeckUpturnedObscuredHigher);
            SetCardSuitColours(CardDeckUpturnedObscuredLower);

            CardDeckUpturned.RefreshVisuals();
            CardDeckUpturnedObscuredHigher.RefreshVisuals();
            CardDeckUpturnedObscuredLower.RefreshVisuals();
        }

        private void SetUpturnedCards()
        {
            CardDeckUpturned.IsEnabled = true;

            CardDeckUpturned.Card = (_deckUpturned.Count > 0 ? 
                                        _deckUpturned[_deckUpturned.Count - 1] : null);

            CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 1 ?
                                                    _deckUpturned[_deckUpturned.Count - 2] : null);

            if (currentGameType == SolitaireGameType.Klondike)
            {
                CardDeckUpturnedObscuredLower.Card = (_deckUpturned.Count > 2 ?
                                                        _deckUpturned[_deckUpturned.Count - 3] : null);
            }
        }

        private void SetCardSuitColours(CardButton cardButton)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            var setSuitColour = true;

            if ((cardButton.Card == null) && !string.IsNullOrEmpty(cardButton.AutomationId))
            {
                setSuitColour = false;

                string pileId = cardButton.AutomationId.Replace("TargetPile", "");
                switch (pileId)
                {
                    case "C":
                        cardButton.SuitColoursClubsSwitch = vm.SuitColoursClubs;
                        break;

                    case "D":
                        cardButton.SuitColoursDiamondsSwitch = vm.SuitColoursDiamonds;
                        break;

                    case "H":
                        cardButton.SuitColoursHeartsSwitch = vm.SuitColoursHearts;
                        break;

                    case "S":
                        cardButton.SuitColoursSpadesSwitch = vm.SuitColoursSpades;
                        break;
                }
            }

            if (setSuitColour && (cardButton.Card != null))
            {
                switch (cardButton.Card.Suit)
                {
                    case Suit.Clubs:
                        cardButton.SuitColoursClubsSwitch = vm.SuitColoursClubs;
                        break;

                    case Suit.Diamonds:
                        cardButton.SuitColoursDiamondsSwitch = vm.SuitColoursDiamonds;
                        break;

                    case Suit.Hearts:
                        cardButton.SuitColoursHeartsSwitch = vm.SuitColoursHearts;
                        break;

                    case Suit.Spades:
                        cardButton.SuitColoursSpadesSwitch = vm.SuitColoursSpades;
                        break;
                }
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

        private bool CanMoveCardToDealtCardPile(DealtCard? cardBelow, DealtCard? cardAbove)
        {
            bool canMove = false;

            if ((cardBelow != null) && (cardAbove != null))
            {
                // A card can only be moved to a dealt card pile (from the upturned card pile,
                // or target card pile, or another dealt card pile) if the card on which the 
                // moved card will lie is the last card in the destination dealt card pile.
                if (cardBelow.IsLastCardInPile)
                {
                    if ((cardBelow.Card != null) && (cardAbove.Card != null))
                    {
                        if (cardBelow.Card.Rank == cardAbove.Card.Rank + 1)
                        {
                            // Suit doesn't matter in the Baker's Dozen game.
                            if (currentGameType != SolitaireGameType.Bakersdozen)
                            {
                                bool isBelowRed = ((cardBelow.Card.Suit == Suit.Diamonds) || (cardBelow.Card.Suit == Suit.Hearts));
                                bool isAboveRed = ((cardAbove.Card.Suit == Suit.Diamonds) || (cardAbove.Card.Suit == Suit.Hearts));

                                canMove = (isBelowRed != isAboveRed);
                            }
                            else
                            {
                                canMove = true;
                            }
                        }
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
                //Debug.WriteLine("RaiseNotificationEvent: " + announcement);

                SemanticScreenReader.Default.Announce(announcement);
            }
        }

        private DateTime timeStartOfThisKlondikeSession;
        private DateTime timeStartOfThisPyramidSession;
        private DateTime timeStartOfThisTripeaksSession;
        private DateTime timeStartOfThisBakersdozenSession;

        private async void ShowEndOfGameDialog(bool gameWasAutoCompleted)
        {
            // It's possible that a screen reader announcement for the QueryRestartWonGame window
            // will get stomped on by a delayed announcement relating to the last move in the game.
            // To prevent this, dispose of the timer delaying the move announcement before showing
            // the window.
            if (timerDelayScreenReaderAnnouncement != null)
            {
                Debug.WriteLine("ShowEndOfGameDialog: Dispose of timerDelayScreenReaderAnnouncement.");

                timerDelayScreenReaderAnnouncement?.Dispose();
                timerDelayScreenReaderAnnouncement = null;
            }

            StartCelebrating();

            var message1 = MainPage.MyGetString("QueryRestartWonGame");

            var nameOfCurrentGame = "";

            switch (currentGameType)
            {
                case SolitaireGameType.Klondike:
                    nameOfCurrentGame = MainPage.MyGetString("KlondikeSolitaire");
                    break;

                case SolitaireGameType.Pyramid:
                    nameOfCurrentGame = MainPage.MyGetString("PyramidSolitaire");
                    break;

                case SolitaireGameType.Tripeaks:
                    nameOfCurrentGame = MainPage.MyGetString("TripeaksSolitaire");
                    break;

                case SolitaireGameType.Bakersdozen:
                    nameOfCurrentGame = MainPage.MyGetString("BakersdozenSolitaire");
                    break;
            }

            message1 = string.Format(message1, nameOfCurrentGame);

            if (gameWasAutoCompleted)
            {
                message1 += MainPage.MyGetString("GameWasAutoCompleted") + " ";
            }

            var message2 = MainPage.MyGetString("QueryRestartWonGame1");

            var messageTimeString = MainPage.MyGetString("QueryRestartWonGameTime");

            TimeSpan timeSpentPlayingCurrent = TimeSpan.Zero;

            var messageTime = "";

            if (currentGameType == SolitaireGameType.Klondike)
            {
                timeSpentPlayingCurrent = DateTime.Now - timeStartOfThisKlondikeSession;
            }
            else if (currentGameType == SolitaireGameType.Pyramid)
            {
                timeSpentPlayingCurrent = DateTime.Now - timeStartOfThisPyramidSession;
            }
            else if (currentGameType == SolitaireGameType.Tripeaks)
            {
                timeSpentPlayingCurrent = DateTime.Now - timeStartOfThisTripeaksSession;
            }
            else if (currentGameType == SolitaireGameType.Bakersdozen)
            {
                timeSpentPlayingCurrent = DateTime.Now - timeStartOfThisBakersdozenSession;
            }

            Debug.WriteLine("Accessible Solitaire: Time spent currently playing this game " +
                timeSpentPlayingCurrent.TotalSeconds);

           int secondsSpentPlayingPrevious = 0;

            if (currentGameType == SolitaireGameType.Klondike)
            {
                secondsSpentPlayingPrevious = (int)Preferences.Get("KlondikeSessionDuration", 0);
            }
            else if (currentGameType == SolitaireGameType.Pyramid)
            {
                secondsSpentPlayingPrevious = (int)Preferences.Get("PyramidSessionDuration", 0);
            }
            else if (currentGameType == SolitaireGameType.Tripeaks)
            {
                secondsSpentPlayingPrevious = (int)Preferences.Get("TripeaksSessionDuration", 0);
            }
            else if (currentGameType == SolitaireGameType.Bakersdozen)
            {
                secondsSpentPlayingPrevious = (int)Preferences.Get("BakersDozenSessionDuration", 0);
            }

            if (secondsSpentPlayingPrevious > 0)
            {
                var timeSpentPlayingPrevious = TimeSpan.FromSeconds(secondsSpentPlayingPrevious);

                Debug.WriteLine("Accessible Solitaire: Time spent previously playing this game " +
                    timeSpentPlayingPrevious.TotalSeconds);

                timeSpentPlayingCurrent += timeSpentPlayingPrevious;
            }

            Debug.WriteLine("Accessible Solitaire: TOTAL Time spent currently playing this game " +
                timeSpentPlayingCurrent.TotalSeconds);

            var spentMinutes = timeSpentPlayingCurrent.Minutes;
            var spentSeconds = timeSpentPlayingCurrent.Seconds;

            messageTime = string.Format(messageTimeString, spentMinutes.ToString(), spentSeconds.ToString());

            var fullMessage = message1 + messageTime + message2;

            var answer = await DisplayAlertAsync(
                MainPage.MyGetString("AccessibleSolitaire"),
                fullMessage,
                MainPage.MyGetString("Yes"),
                MainPage.MyGetString("No"));

            if (answer)
            {
                StopCelebratoryActions();

                RestartGame(true /* screenReaderAnnouncement. */);
            }
        }

        private void StopCelebratoryActions()
        {
            // ***BUILD WARNINGS***
            //    warning CS4014: Because this call is not awaited, execution of the current method
            //      continues before the call is completed.
            //        Consider applying the 'await' operator to the result of the call.

#pragma warning disable CS4014

            // Stop any target card piles from rotating.
            if ((currentGameType == SolitaireGameType.Klondike) ||
                (currentGameType == SolitaireGameType.Bakersdozen))
            {
                TargetPileC.RotateToAsync(0, 0);
                TargetPileD.RotateToAsync(0, 0);
                TargetPileH.RotateToAsync(0, 0);
                TargetPileS.RotateToAsync(0, 0);
            }
            else if (currentGameType == SolitaireGameType.Pyramid)
            {
                NextCardDeck.RotateToAsync(0, 0);
                CardDeckUpturnedObscuredHigher.RotateToAsync(0, 0);
                CardDeckUpturned.RotateToAsync(0, 0);
            }
            else if (currentGameType == SolitaireGameType.Tripeaks)
            {
                NextCardDeck.RotateToAsync(0, 0);
                CardDeckUpturned.RotateToAsync(0, 0);
            }

#pragma warning restore CS4014

            // Stop any running sounds.
            if ((mainMediaElement != null) && (mainMediaElement.Source != null))
            {
                mainMediaElement.Source = null;
            }
        }

        private int countOfSpinningCards;

        private void StartCelebrating()
        {
            if (celebrationExperienceAudio)
            {
                var celebrationSound = celebrationExperienceAudioFile;
                if (string.IsNullOrEmpty(celebrationSound))
                {
                    celebrationSound = "brasscelebration";
                }

                PlaySoundFile(celebrationSound + ".mp4");
            }

            if (celebrationExperienceVisual)
            {
                countOfSpinningCards = 0;

                timerDelayCardSpin = new Timer(
                    new TimerCallback((s) => TimedDelayCardSpin()), null, 0, 200);
            }
        }

        private void TimedDelayCardSpin()
        {
            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if ((currentGameType == SolitaireGameType.Klondike) ||
                    (currentGameType == SolitaireGameType.Bakersdozen))
                {
                    switch (countOfSpinningCards)
                    {
                        case 0:
                            TargetPileC.RelRotateToAsync(3600, 10000);
                            break;

                        case 1:
                            TargetPileD.RelRotateToAsync(3600, 10000);
                            break;

                        case 2:
                            TargetPileH.RelRotateToAsync(3600, 10000);
                            break;

                        case 3:
                            TargetPileS.RelRotateToAsync(3600, 10000);
                            break;

                        default:
                            timerDelayCardSpin?.Dispose();
                            timerDelayCardSpin = null;

                            break;
                    }
                }
                else if (currentGameType == SolitaireGameType.Pyramid)
                {
                    switch (countOfSpinningCards)
                    {
                        case 0:
                            NextCardDeck.RelRotateToAsync(3600, 10000);
                            break;

                        case 1:
                            CardDeckUpturnedObscuredHigher.RelRotateToAsync(3600, 10000);
                            break;

                        case 2:
                            CardDeckUpturned.RelRotateToAsync(3600, 10000);
                            break;

                        default:
                            timerDelayCardSpin?.Dispose();
                            timerDelayCardSpin = null;

                            break;
                    }
                }
                else if (currentGameType == SolitaireGameType.Tripeaks)
                {
                    switch (countOfSpinningCards)
                    {
                        case 0:
                            NextCardDeck.RelRotateToAsync(3600, 10000);
                            break;

                        case 1:
                            CardDeckUpturned.RelRotateToAsync(3600, 10000);
                            break;

                        default:
                            timerDelayCardSpin?.Dispose();
                            timerDelayCardSpin = null;

                            break;
                    }
                }

                ++countOfSpinningCards;
            });
        }

        private void StateAnnouncementButton_Clicked(object sender, EventArgs e)
        {
            //SentrySdk.CaptureMessage("Accessible Solitaire: Button Clicked: StateAnnouncementButton", SentryLevel.Info);

            var announcementRemainingCards = "";
            
            if (currentGameType != SolitaireGameType.Bakersdozen)
            {
                AnnounceStateRemainingCards(false);
            }

            var announceStateTargetPiles = AnnounceStateTargetPiles(false);

            var announcementDealtCards = AnnounceStateDealtCardPiles(false);

            var fullAnnouncement = announcementRemainingCards + " " +
                                    announceStateTargetPiles + " " +
                                    announcementDealtCards;

            // On iOS, VoiceOver always seems to announce the name of the button after it's been clicked,
            // and that interupts the game state announcement. So for now, delay the start of the 
            // announcement to avoid that interuption. Barker Todo: Investigate this unexpected 
            // announcement of the button name.
            MakeDelayedScreenReaderAnnouncement(fullAnnouncement, false);

            Debug.WriteLine("State announcement: " + fullAnnouncement);
        }
        
        private void AvailableMovesAnnouncementButton_Clicked(object sender, EventArgs e)
        {
            //SentrySdk.CaptureMessage("Accessible Solitaire: Button Clicked: AvailableMovesAnnouncementButton", SentryLevel.Info);

            AnnounceAvailableMoves(true);
        }

        private void PyramidOpenCardsAnnouncementButton_Clicked(object sender, EventArgs e)
        {
            if (currentGameType == SolitaireGameType.Bakersdozen)
            {
                AnnounceBakersdozenOpenCards(true);
            }
            else
            {
                AnnouncePyramidOpenCards(true);
            }
        }

        private void MakeDelayedScreenReaderAnnouncement(string? announcement, bool appendAvailableMoves)
        {
            if (string.IsNullOrEmpty(announcement))
            {
                return;
            }

            if (timerDelayScreenReaderAnnouncement == null)
            {
                timerDelayScreenReaderAnnouncement = new Timer(
                    new TimerCallback((s) => MakeScreenReaderAnnouncement(announcement, appendAvailableMoves)),
                        announcement,
                        TimeSpan.FromMilliseconds(200),
                        TimeSpan.FromMilliseconds(Timeout.Infinite));
            }
        }

        private void MakeDelayedScreenReaderAnnouncementWithDelayTime(string? announcement, bool appendAvailableMoves, int delay)
        {
            if (string.IsNullOrEmpty(announcement))
            {
                return;
            }

            if (timerDelayScreenReaderAnnouncement == null)
            {
                timerDelayScreenReaderAnnouncement = new Timer(
                    new TimerCallback((s) => MakeScreenReaderAnnouncement(announcement, appendAvailableMoves)),
                        announcement,
                        TimeSpan.FromMilliseconds(delay),
                        TimeSpan.FromMilliseconds(Timeout.Infinite));
            }
        }

        public double OriginalCardWidth { get => originalCardWidth; set => originalCardWidth = value; }
        public double OriginalCardHeight { get => originalCardHeight; set => originalCardHeight = value; }

        private void MakeScreenReaderAnnouncement(object announcement, bool appendAvailableMoves)
        {
            timerDelayScreenReaderAnnouncement?.Dispose();
            timerDelayScreenReaderAnnouncement = null;

            var fullAnnouncement = (string?)announcement;
            if (!String.IsNullOrEmpty(fullAnnouncement))
            {
                if (automaticallyAnnounceMoves && appendAvailableMoves)
                {
                    var availableMoveAnnouncement = AnnounceAvailableMoves(false);
                    if (!string.IsNullOrEmpty(availableMoveAnnouncement))
                    {
                        fullAnnouncement += " " + availableMoveAnnouncement;
                    }
                }

                Debug.WriteLine("MakeScreenReaderAnnouncement: " + fullAnnouncement);

                // Always run this on the UI thread.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RaiseNotificationEvent(fullAnnouncement);
                });
            }
        }

        public string AnnounceStateRemainingCards(bool makeAnnouncement)
        {
            string stateMessage = "";

            if ((currentGameType == SolitaireGameType.Klondike) ||
                (currentGameType == SolitaireGameType.Bakersdozen))
            {
                stateMessage = AnnounceStateRemainingCardsKlondike();
            }
            else if ((currentGameType == SolitaireGameType.Pyramid) ||
                     (currentGameType == SolitaireGameType.Tripeaks))
            {
                stateMessage = AnnounceStateRemainingCardsPyramid();
            }

            if (makeAnnouncement)
            {
                MakeDelayedScreenReaderAnnouncement(stateMessage, false);
            }

            return stateMessage;
        }

        public string AnnounceStateRemainingCardsKlondike()
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

        public string AnnounceStateRemainingCardsPyramid()
        {
            string stateMessage = "";

            if (currentGameType == SolitaireGameType.Pyramid)
            {
                if (_deckUpturned.Count > 0)
                {
                    stateMessage += MyGetString("UpturnedCardIs") + " " +
                        _deckUpturned[_deckUpturned.Count - 1].GetCardAccessibleName();
                }

                if (_deckUpturned.Count > 1)
                {
                    stateMessage += ", " + MyGetString("TopOfWastePileIs") + " " +
                        _deckUpturned[_deckUpturned.Count - 2].GetCardAccessibleName();
                }
            }
            else if (currentGameType == SolitaireGameType.Tripeaks)
            {
                if (_deckUpturned.Count > 0)
                {
                    stateMessage += MyGetString("TopOfWastePileIs") + " " +
                        _deckUpturned[_deckUpturned.Count - 1].GetCardAccessibleName();
                }
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

        public string AnnounceStateTargetPiles(bool makeAnnouncement)
        {
            if ((currentGameType != SolitaireGameType.Klondike) &&
                (currentGameType != SolitaireGameType.Bakersdozen))
            {
                return "";
            }

            string stateMessage = MyGetString("TargetCardPiles") + ", ";

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

            if (makeAnnouncement)
            {
                MakeDelayedScreenReaderAnnouncement(stateMessage, false);
            }

            return stateMessage;
        }

        public static string MyGetString(string stringId)
        {
            var requiredString = Strings.StringResources.ResourceManager.GetString(stringId);
            if (requiredString == null)
            {
                return "";
            }

            return requiredString;
        }

        public string AnnounceStateDealtCardPiles(bool makeAnnouncement)
        {
            string stateMessage = "";

            if (currentGameType == SolitaireGameType.Klondike)
            {
                stateMessage = AnnounceStateDealtCardPilesKlondike();
            }
            else if (currentGameType == SolitaireGameType.Bakersdozen)
            {
                stateMessage = AnnounceStateDealtCardPilesBakersDozen();
            }
            else if ((currentGameType == SolitaireGameType.Pyramid) ||
                     (currentGameType == SolitaireGameType.Tripeaks))
            {
                stateMessage = AnnounceStateDealtCardPilesPyramid();
            }

            if (makeAnnouncement)
            {
                MakeDelayedScreenReaderAnnouncement(stateMessage, false);
            }

            return stateMessage;
        }

        public string AnnounceStateDealtCardPilesKlondike()
        {
            string stateMessage = "";

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return "";
            }

            string empty = MyGetString("Empty");
            string pile = MyGetString("Pile");
            string to = MyGetString("To");
            string card = MyGetString("Card");
            string cards = MyGetString("Cards");
            string facedown = MyGetString("FaceDown");

            var countPiles = GetCardPileCount();

            for (int i = 0; i < countPiles; i++)
            {
                stateMessage += pile + " " + (i + 1) + ", ";

                int cFaceDown = 0;
                int indexLastFaceUp = -1;

                var faceDownMessage = "";

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
                            if (includeFacedownCardsInAnnouncement)
                            {
                                var dealtCard = (vm.DealtCards[i][j] as DealtCard);
                                if ((dealtCard != null) && (dealtCard.Card != null))
                                {
                                    faceDownMessage += MyGetString("FaceDown") + " " +
                                                        dealtCard.Card.GetCardAccessibleName() + ", ";
                                }
                            }
                            else
                            {
                                ++cFaceDown;
                            }
                        }
                        else
                        {
                            indexLastFaceUp = j;
                        }
                    }
                }

                if ((indexLastFaceUp != -1) && (indexLastFaceUp != vm.DealtCards[i].Count - 1))
                {
                    stateMessage += " " + to + " " + (vm.DealtCards[i][indexLastFaceUp] as DealtCard).AccessibleNameWithoutSelectionAndMofN;
                }

                stateMessage += ", ";

                if (!string.IsNullOrEmpty(faceDownMessage))
                {
                    stateMessage += faceDownMessage;
                }
                else if (cFaceDown > 0)
                {
                    stateMessage += cFaceDown + " " +
                        (cFaceDown > 1 ? cards : card) + " " + facedown;

                    stateMessage += (i < countPiles - 1 ? ", " : ".");
                }
            }

            return stateMessage;
        }

        public string AnnounceStateDealtCardPilesBakersDozen()
        {
            string stateMessage = "";

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return "";
            }

            string empty = MyGetString("Empty");
            string pile = MyGetString("Pile");
            string card = MyGetString("Card");
            string cards = MyGetString("Cards");

            var countPiles = GetCardPileCount();

            for (int i = 0; i < countPiles; i++)
            {
                stateMessage += pile + " " + (i + 1) + ", ";

                for (int j = vm.DealtCards[i].Count - 1; j >= 0; j--)
                {
                    if ((vm.DealtCards[i][j] as DealtCard).CardState == CardState.KingPlaceHolder)
                    {
                        stateMessage += empty;
                    }
                    else
                    {
                        stateMessage += (vm.DealtCards[i][j] as DealtCard).AccessibleNameWithoutSelectionAndMofN;
                    }

                    if ((i < countPiles - 1) | (j > 0))
                    {
                        stateMessage += ", ";
                    }
                }
            }

            stateMessage += ".";

            return stateMessage;
        }

        public string AnnounceStateDealtCardPilesPyramid()
        {
            string stateMessage = "";

            // Work up from the last row in the pyramid.
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return "";
            }

            var maxRowIndex = (currentGameType == SolitaireGameType.Pyramid ? 6 : 4);

            for (int i = maxRowIndex; i >= 0; i--)
            {
                if (vm.DealtCards[i].Count > 0)
                {
                    stateMessage += MyGetString("Row") + " " + (i + 1) + ", ";

                    for (int j = 0; j < vm.DealtCards[i].Count; j++)
                    {
                        var dealtCard = vm.DealtCards[i][j];
                        if ((dealtCard != null) && (dealtCard.Card != null))
                        {
                            stateMessage += dealtCard.Card.GetCardAccessibleName() + ", ";
                        }
                    }
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
            if (mainMediaElement == null)
            {
                return;
            }

            // Only play a sound if the relevant setting is turned on.
            if ((success && playSoundSuccessfulMove) ||
                (!success && playSoundUnsuccessfulMove))
            {
                mainMediaElement.Source = MediaSource.FromResource(
                                            success ? "successmove.mp4" : "notsuccessmove.mp4");

                Debug.WriteLine("PlaySound: " + mainMediaElement.Source.ToString());

                mainMediaElement.Play();
            }
        }

        public void PlaySoundFile(string soundFilename)
        {
            if (mainMediaElement == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(soundFilename))
            {
                return;
            }

            try
            {
                mainMediaElement.Source = MediaSource.FromResource(soundFilename);

                Debug.WriteLine("PlaySoundFile: " + mainMediaElement.Source.ToString());

                mainMediaElement.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PlaySoundFile: " + ex.Message);
            }
        }

        private async void TouchBehavior_LongPressCompleted(object sender, CommunityToolkit.Maui.Core.LongPressCompletedEventArgs e)
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

                                await this.ShowPopupAsync(popup);
                            }
                        }
                    }
                }
            }
        }

        private string Sa11ytaireHelpPage =
            "https://accessiblesolitaire.wordpress.com/2025/07/17/accessible-solitaire-for-ios-android-and-windows";

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
            var answer = await DisplayAlertAsync(
                MainPage.MyGetString("AccessibleSolitaire"),
                MainPage.MyGetString("QueryRestartGame"),
                MainPage.MyGetString("Yes"),
                MainPage.MyGetString("No"));

            if (answer)
            {
                Shell.Current.FlyoutIsPresented = false;

                MainPage.MainPageSingleton?.RestartGame(true);
            }
        }

        private void MenuFlyoutItemShowZoomPopup_Clicked(object sender, EventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            if (menuFlyoutItem != null)
            {
                var dealtCard = menuFlyoutItem.BindingContext as DealtCard;
                if ((dealtCard != null) && (dealtCard.Card != null))
                {
                    ShowZoomedCardPopup(dealtCard.Card, true);
                }
            }
        }        
        
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            
            // Subscribe to keyboard events
            if (keyboardBehavior != null)
            {
                keyboardBehavior.KeyDown += OnKeyDown;
                keyboardBehavior.KeyUp += OnKeyUp;
            }
        }

        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            base.OnNavigatedFrom(args);
            
            // Unsubscribe from keyboard events
            if (keyboardBehavior != null)
            {
                keyboardBehavior.KeyDown -= OnKeyDown;
                keyboardBehavior.KeyUp -= OnKeyUp;
            }
        }

        private void OnKeyDown(object? sender, KeyPressedEventArgs e)
        {
            // Handle key down events for cross-platform keyboard shortcuts
            switch (e.Keys)
            {
                case KeyboardKeys.F1:

                    //SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: F1", SentryLevel.Info);

                    ShowKeyboardShortcuts();
                    e.Handled = true;
                    break;

                case KeyboardKeys.F6:

                    //SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: F6", SentryLevel.Info);

                    // Check if Shift is pressed for backward navigation
                    // Since we can't easily detect modifiers, we'll handle both directions

                    // Modifiers is a bitfield here, and we'll move backwards if the Shift key,
                    // and only the Shift key, is pressed.
                    HandleF6(e.Modifiers == KeyboardModifiers.Shift); // Forward by default
                    e.Handled = true;
                    break;

                case KeyboardKeys.H:

                    //SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: H", SentryLevel.Info);

                    LaunchHelp();
                    e.Handled = true;
                    break;

                case KeyboardKeys.N:

                    //SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: N", SentryLevel.Info);

                    PerformNextCardAction();
                    e.Handled = true;
                    break;

                case KeyboardKeys.R:

                    //SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: R", SentryLevel.Info);

                    QueryRestartGame();
                    e.Handled = true;
                    break;

                case KeyboardKeys.M:

                    //SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: M", SentryLevel.Info);

                    AnnounceAvailableMoves(true);
                    e.Handled = true;
                    break;

                case KeyboardKeys.U:

                    //SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: U", SentryLevel.Info);

                    AnnounceStateRemainingCards(true);
                    e.Handled = true;
                    break;

                case KeyboardKeys.T:

                    //SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: T", SentryLevel.Info);

                    AnnounceStateTargetPiles(true);
                    e.Handled = true;
                    break;

                case KeyboardKeys.D:

                    //SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: D", SentryLevel.Info);

                    AnnounceStateDealtCardPiles(true);
                    e.Handled = true;
                    break;

                case KeyboardKeys.LeftArrow:
                case KeyboardKeys.RightArrow:

                    e.Handled = false;

                    if (MainPage.MainPageSingleton != null)
                    {
                        MainPage.MainPageSingleton.MoveToNearbyDealtCardPile(e.Keys == KeyboardKeys.RightArrow);

                        e.Handled = true;
                    }

                    break;

                case KeyboardKeys.UpArrow:
                case KeyboardKeys.DownArrow:

                    e.Handled = false;

                    if ((MainPage.MainPageSingleton != null) && (MainPage.currentGameType == SolitaireGameType.Pyramid))
                    {
                        MainPage.MainPageSingleton.MoveBetweenPyramidRow(e.Keys == KeyboardKeys.UpArrow);

                        e.Handled = true;
                    }

                    break;
            }
        }

        private void OnKeyUp(object? sender, KeyPressedEventArgs e)
        {
            // Handle key up events if needed
        }

        private void DeselectAllCardsFromDealtCardPile(CollectionView dealtCardPile)
        {
            //Debug.WriteLine("DeselectAllCardsFromDealtCardPile: " + dealtCardPile.AutomationId);

            dealtCardPile.SelectedItem = null;
        }

        private bool RemoveDealtCardFromDealtCardList(List<Card> collection, DealtCard? dealtCard)
        {
            bool removedOk = false;

            if ((dealtCard == null) || (dealtCard.Card == null))
            {
                return false;
            }

            if ((collection == null) || (collection.Count == 0) || !collection.Contains(dealtCard.Card))
            {
                return false;
            }

            // Barker Todo: A NullReferenceException has been thrown when VoiceOver is running,
            // and when rapidly selecting/deselecting dealt cards and moving an Ace from a dealt 
            // card pile up to the target card pile. Until this is understood, catch any exceptions.
            try
            {
                collection.Remove(dealtCard.Card);

                removedOk = true;
            }
            catch (Exception ex)
            {
                // Based on a known exception during an attempt to remove a card, and a report
                // of symptoms from a user, assume the card HAS been removed from the collection.

                var debugString = "RemoveCardFromDealtCardList: " +
                                    "Attempt to remove card from list: " +
                                    ex;

                Debug.WriteLine(debugString);

                //SentrySdk.CaptureMessage(debugString, SentryLevel.Debug);
            }

            return removedOk;
        }

        private bool RemoveDealtCardFromDealtCardCollection(ObservableCollection<DealtCard> collection, DealtCard? dealtCard)
        {
            bool removedOk = false;

            if ((dealtCard == null) || (dealtCard.Card == null))
            {
                return false;
            }

            if ((collection == null) || (collection.Count == 0) || !collection.Contains(dealtCard))
            {
                return false;
            }

            try
            {
                collection.Remove(dealtCard);

                removedOk = true;
            }
            catch (Exception ex)
            {
                // Based on a known exception during an attempt to remove a card, and a report
                // of symptoms from a user, assume the card HAS been removed from the collection.

                var debugString = "RemoveDealtCardFromDealtCardCollection: " +
                                    "Attempt to remove dealt card from collection: " +
                                    ex;

                Debug.WriteLine(debugString);

                //SentrySdk.CaptureMessage(debugString, SentryLevel.Debug);
            }

            return removedOk;
        }

        public double GetCardPileGridWidth()
        {
            if (CardPileGrid == null)
            {
                return 0;
            }

            // Barker: Based the returned width on the MainPageGrid to avoid the situation where
            // the CardPileGrid width changes in response to contained card piles width changing,
            // which in turn leads to the contained cards wanting to changing their own width again.
            return MainPageGrid.Width;
        }

        public double GetCardPileGridHeight()
        {
            if (CardPileGrid == null)
            {
                return 0;
            }

            return CardPileGrid.Height;
        }
    }
}
