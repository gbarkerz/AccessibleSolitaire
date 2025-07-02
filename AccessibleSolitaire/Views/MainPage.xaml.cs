using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Plugin.Maui.KeyListener;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
        private bool playSoundSuccessfulMove = false;
        private bool playSoundUnsuccessfulMove = false;
        private bool playSoundOther = false;
        private bool celebrationExperienceVisual = false;
        private bool celebrationExperienceAudio = false;

        private MediaElement mainMediaElement = new MediaElement();

        public static Dictionary<string, Color> suitColours =
            new Dictionary<string, Color>
            {
                { "Default", Colors.Transparent },
                { "Black", Colors.Black },
                { "Dark Red", Colors.DarkRed },
                { "Dark Orange", Colors.DarkOrange },
                { "Dark Gold", Colors.DarkGoldenrod },
                { "Dark Green", Colors.DarkGreen },
                { "Dark Blue", Colors.DarkBlue },
                { "Dark Indigo", Color.FromArgb("#FF1F0954") },
                { "Dark Violet", Colors.DarkViolet },
                { "White", Colors.White },
                { "Yellow", Colors.Yellow },
                { "Pink", Color.FromArgb("#FFFF74A0") }, // Colors.Pink is too light. 
                { "Cyan", Colors.Cyan },
                { "Light Blue", Colors.LightBlue },
                { "Light Green", Colors.LightGreen },
                { "Light Coral", Colors.LightCoral }
            };

        private KeyboardBehavior? keyboardBehavior;

        public MainPage()
        {
            MainPage.MainPageSingleton = this;

            this.BindingContext = new DealtCardViewModel();

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm != null)
            {
                vm.MergeFaceDownCards = MainPage.GetMergeFaceDownCardsSetting();
            }

            this.InitializeComponent();

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

            CardPileGrid.SizeChanged += CardPileGrid_SizeChanged;

            for (int i = 0; i < 10; i++)
            {
                localizedNumbers[i] = MyGetString((i + 1).ToString());
            }

            for (int i = 0; i < cTargetPiles; ++i)
            {
                _targetPiles[i] = new List<Card>();
            }

            //DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;

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
#endif
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
#endif
        }

        public void ShowZoomedCardPopup(Card? card, bool checkFaceUp)
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

        private void OnTapGestureRecognizerTapped(object sender, TappedEventArgs args)
        {
            Debug.WriteLine("OnTapGestureRecognizerTapped: Enter OnTapGestureRecognizerTapped.");

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
                Debug.WriteLine("OnTapGestureRecognizerTapped: Tapped on card: " + dealtCard.AccessibleName);

                // Toggle the selection state of the card which was tapped on.
                if (list.SelectedItem == dealtCard)
                {
                    Debug.WriteLine("OnTapGestureRecognizerTapped: Delay deselection of: " + dealtCard.AccessibleName);

                    string announcement = "Deselected" + " " + dealtCard.AccessibleNameWithoutSelectionAndMofN;
                    MakeDelayedScreenReaderAnnouncement(announcement);

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
                    Debug.WriteLine("OnTapGestureRecognizerTapped: Selecting now: " + dealtCard.AccessibleName);

                    timerSelectDealtCard = new Timer(
                        new TimerCallback((s) => TimedDelaySelectDealtCard(list, dealtCard)),
                            null,
                            TimeSpan.FromMilliseconds(300),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
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
        private bool OptionKeepGameAcrossSessions = true;

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
                            TimeSpan.FromMilliseconds(Timeout.Infinite)); Dispatcher.Dispatch(() => {
                        ;
                    });
                }

                var previousMergeFaceDownCards = vm.MergeFaceDownCards;
                vm.MergeFaceDownCards = MainPage.GetMergeFaceDownCardsSetting();

                // Refresh the accessibility of all cards if necessary.
                if (vm.MergeFaceDownCards != previousMergeFaceDownCards)
                {
                    RefreshAllDealtCardPileCardIsInAccessibleTree();
                }

                vm.FlipGameLayoutHorizontally = (bool)Preferences.Get("FlipGameLayoutHorizontally", false);

                vm.ShowScreenReaderAnnouncementButtons = (bool)Preferences.Get("ShowScreenReaderAnnouncementButtons", false);
                ScreenReaderAnnouncementButtons.IsVisible = vm.ShowScreenReaderAnnouncementButtons;

                vm.ExtendDealtCardHitTarget = (bool)Preferences.Get("ExtendDealtCardHitTarget", false);

                // General game-playing options.

                OptionCardTurnCount = (int)Preferences.Get("CardTurnCount", 1);
                OptionKingsOnlyToEmptyPile = (bool)Preferences.Get("KingsOnlyToEmptyPile", false);
                OptionKeepGameAcrossSessions = (bool)Preferences.Get("KeepGameAcrossSessions", true);

                allowSelectionByFaceDownCard = (bool)Preferences.Get("AllowSelectionByFaceDownCard", true);

                playSoundSuccessfulMove = (bool)Preferences.Get("PlaySoundSuccessfulMove", false);
                playSoundUnsuccessfulMove = (bool)Preferences.Get("PlaySoundUnsuccessfulMove", false);
                playSoundOther = (bool)Preferences.Get("PlaySoundOther", false);

                celebrationExperienceVisual = (bool)Preferences.Get("CelebrationExperienceVisual", false);
                celebrationExperienceAudio = (bool)Preferences.Get("CelebrationExperienceAudio", false);

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

                    if (!OptionKeepGameAcrossSessions || !LoadSession())
                    {
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
            timerSetSuitColours?.Dispose();
            timerSetSuitColours = null;

            SetCardSuitColours(CardDeckUpturnedObscuredLower);
            SetCardSuitColours(CardDeckUpturnedObscuredHigher);
            SetCardSuitColours(CardDeckUpturned);

            SetCardSuitColours(TargetPileC);
            SetCardSuitColours(TargetPileD);
            SetCardSuitColours(TargetPileH);
            SetCardSuitColours(TargetPileS);
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
                    DisplayAlert(title, message, btnText);
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

        private bool gotPreviousOrientation = false;
        private bool previousOrientationPortrait;

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

            // Determine if we need to change layout orientation.
            var currentOrientationIsPortrait = IsPortrait();

            if (!gotPreviousOrientation || (currentOrientationIsPortrait != previousOrientationPortrait))
            {
                SetOrientationLayout();

                OriginalCardWidth = 0;
                OriginalCardHeight = 0;
            }

            previousOrientationPortrait = currentOrientationIsPortrait;
            gotPreviousOrientation = true;

            if (currentOrientationIsPortrait)
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
                    // Barker Todo: On Windows simply calling vm.DealtCards[i].Clear() here and then adding 
                    // the new cards can leave the suit colours in the new cards wrong. So it seems that
                    // something's up with the tint colour binding. However, explicitly removing all the
                    // existing cards in the pile before adding the new cards seems to avoid this. So do
                    // that for now, and investigate the correct fix.
#if WINDOWS
                    var previousCount = vm.DealtCards[i].Count;

                    for (int previousItemIndex = 0; previousItemIndex < previousCount; ++previousItemIndex)
                    {
                        vm.DealtCards[i].RemoveAt(0);
                    }
#else
                    vm.DealtCards[i].Clear();
#endif

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
            }

            SetCardButtonToggledSelectionState(TargetPileC, false);
            SetCardButtonToggledSelectionState(TargetPileD, false);
            SetCardButtonToggledSelectionState(TargetPileH, false);
            SetCardButtonToggledSelectionState(TargetPileS, false);
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

            timerDelayDealCards = new Timer(
                new TimerCallback((s) => TimedDelayDealCards()),
                    null,
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromMilliseconds(Timeout.Infinite));

            NextCardDeck.IsEmpty = false;

            if (screenReaderAnnouncement)
            {
                // We only play sounds if the app's ready to make sounds.
                if (playSoundOther)
                {
                    PlaySoundFile("deal.mp4");
                }

                var announcement = MainPage.MyGetString("GameRestarted");
                MakeDelayedScreenReaderAnnouncement(announcement);
            }

            ClearDealtCardPileSelections();
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
        }

        private void SetUpturnedCards()
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

        private void SetCardSuitColours(CardButton cardButton)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            if (cardButton.Card == null)
            {
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
            else
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
                // or target card pile, or another dealt card pile) is the card on which the 
                // moved card will lie is the last card in the destination dealt card pile.
                if (cardBelow.IsLastCardInPile)
                {
                    if ((cardBelow.Card != null) && (cardAbove.Card != null))
                    {
                        if (cardBelow.Card.Rank == cardAbove.Card.Rank + 1)
                        {
                            bool isBelowRed = ((cardBelow.Card.Suit == Suit.Diamonds) || (cardBelow.Card.Suit == Suit.Hearts));
                            bool isAboveRed = ((cardAbove.Card.Suit == Suit.Diamonds) || (cardAbove.Card.Suit == Suit.Hearts));

                            canMove = (isBelowRed != isAboveRed);
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
                Debug.WriteLine("RaiseNotificationEvent: " + announcement);

                SemanticScreenReader.Default.Announce(announcement);
            }
        }

        private async void ShowEndOfGameDialog()
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

            var answer = await DisplayAlert(
                MainPage.MyGetString("AccessibleSolitaire"),
                MainPage.MyGetString("QueryRestartWonGame"),
                MainPage.MyGetString("Yes"),
                MainPage.MyGetString("No"));

            if (answer)
            {
                // ***BUILD WARNINGS***
                //    warning CS4014: Because this call is not awaited, execution of the current method
                //      continues before the call is completed.
                //        Consider applying the 'await' operator to the result of the call.

                // Stop any target card piles from rotating.
                TargetPileC.RotateTo(0, 0);
                TargetPileD.RotateTo(0, 0);
                TargetPileH.RotateTo(0, 0);
                TargetPileS.RotateTo(0, 0);

                // Stop any running sounds.
                if ((mainMediaElement != null) && (mainMediaElement.Source != null))
                {
                    mainMediaElement.Source = null;
                }

                RestartGame(true /* screenReaderAnnouncement. */);
            }
        }


        private int countOfSpinningCards;

        private void StartCelebrating()
        {
            if (celebrationExperienceAudio)
            {
                PlaySoundFile("cromptonspinningmule.mp4");
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
                switch (countOfSpinningCards)
                {
                    case 0:
                        TargetPileC.RelRotateTo(3600, 10000);
                        break;

                    case 1:
                        TargetPileD.RelRotateTo(3600, 10000);
                        break;

                    case 2:
                        TargetPileH.RelRotateTo(3600, 10000);
                        break;

                    case 3:
                        TargetPileS.RelRotateTo(3600, 10000);
                        break;

                    default:
                        timerDelayCardSpin?.Dispose();
                        timerDelayCardSpin = null;

                        break;
                }

                ++countOfSpinningCards;
            });
        }

        private void StateAnnouncementButton_Clicked(object sender, EventArgs e)
        {
            SentrySdk.CaptureMessage("Accessible Solitaire: Button Clicked: StateAnnouncementButton", SentryLevel.Info);

            var announcementRemainingCards = AnnounceStateRemainingCards(false);

            var announceStateTargetPiles = AnnounceStateTargetPiles(false);

            var announcementDealtCards = AnnounceStateDealtCardPiles(false);

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

        private void AvailableMovesAnnouncementButton_Clicked(object sender, EventArgs e)
        {
            SentrySdk.CaptureMessage("Accessible Solitaire: Button Clicked: AvailableMovesAnnouncementButton", SentryLevel.Info);

            AnnounceAvailableMoves();
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

        public string AnnounceStateRemainingCards(bool makeAnnouncement)
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

            if (makeAnnouncement)
            {
                MakeDelayedScreenReaderAnnouncement(stateMessage);
            }

            return stateMessage;
        }

        public string AnnounceStateTargetPiles(bool makeAnnouncement)
        {
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
                MakeDelayedScreenReaderAnnouncement(stateMessage);
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

            string empty = MyGetString("Empty");
            string pile = MyGetString("Pile");
            string to = MyGetString("Card");
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
                    stateMessage += " " + to + " " + (vm.DealtCards[i][indexLastFaceUp] as DealtCard).AccessibleNameWithoutSelectionAndMofN;
                }

                stateMessage += ", ";

                if (cFaceDown > 0)
                {                    stateMessage += cFaceDown + " " +
                        (cFaceDown > 1 ? cards : to) + " " + facedown;

                    stateMessage += (i < cCardPiles - 1 ? ", " : ".");
                }
            }

            if (makeAnnouncement)
            {
                MakeDelayedScreenReaderAnnouncement(stateMessage);
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

        private void PlaySoundFile(string soundFilename)
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

                    SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: F1", SentryLevel.Info);

                    ShowKeyboardShortcuts();
                    e.Handled = true;
                    break;

                case KeyboardKeys.F6:

                    SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: F6", SentryLevel.Info);

                    // Check if Shift is pressed for backward navigation
                    // Since we can't easily detect modifiers, we'll handle both directions

                    // Modifiers is a bitfield here, and we'll move backwards if the Shift key,
                    // and only the Shift key, is pressed.
                    HandleF6(e.Modifiers == KeyboardModifiers.Shift); // Forward by default
                    e.Handled = true;
                    break;

                case KeyboardKeys.H:

                    SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: H", SentryLevel.Info);

                    LaunchHelp();
                    e.Handled = true;
                    break;

                case KeyboardKeys.N:

                    SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: N", SentryLevel.Info);

                    PerformNextCardAction();
                    e.Handled = true;
                    break;

                case KeyboardKeys.R:

                    SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: R", SentryLevel.Info);

                    QueryRestartGame();
                    e.Handled = true;
                    break;

                case KeyboardKeys.M:

                    SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: M", SentryLevel.Info);

                    AnnounceAvailableMoves();
                    e.Handled = true;
                    break;

                case KeyboardKeys.U:

                    SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: U", SentryLevel.Info);

                    AnnounceStateRemainingCards(true);
                    e.Handled = true;
                    break;

                case KeyboardKeys.T:

                    SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: T", SentryLevel.Info);

                    AnnounceStateTargetPiles(true);
                    e.Handled = true;
                    break;

                case KeyboardKeys.D:

                    SentrySdk.CaptureMessage("Accessible Solitaire: Key Down: D", SentryLevel.Info);

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
    }
}
