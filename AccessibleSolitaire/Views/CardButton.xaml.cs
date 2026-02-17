using System.ComponentModel;
using System.Diagnostics;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;

namespace Sa11ytaire4All.Views;

public partial class CardButton : ContentView, INotifyPropertyChanged
{
    public CardButton()
    {
        BindingContext = this;

        InitializeComponent();

#if WINDOWS
        // Barker todo: Figure out why the Button Click handler doesn't get called on Windows.
        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += (s, e) =>
        {
            HandleCardButtonClick(InnerButton);
        };
        
        this.GestureRecognizers.Add(tapGestureRecognizer);
#endif
    }

    public void SetHeadingState(bool isHeading)
    {
        // Note: The heading level must be set on the InnerButton, not directly on the CardButton.
        var innerButton = (Button)this.FindByName("InnerButton");
        if (innerButton != null)
        {
            var headingLevel = (isHeading ? SemanticHeadingLevel.Level2 : SemanticHeadingLevel.None);
            SemanticProperties.SetHeadingLevel(innerButton, headingLevel);
        }
    }

    private Card? card = null;
    public Card? Card
    {
        get
        {
            return this.card;
        }
        set
        {
            // Refresh here even when the value is null to ensure the target piles' initial visuals appear.
            if ((this.card != value) || (value == null))
            {
                this.card = value;

                this.OnPropertyChanged("Card");
                this.OnPropertyChanged("CardPileAccessibleName");

                // Whenever the card changes, we must update the bound contained pictures.
                this.OnPropertyChanged("CardPileImage");
                this.OnPropertyChanged("PictureCardPileImage");
            }
        }
    }

    public void RefreshCardButtonMofN()
    {
        if (this.card != null)
        {
            Debug.WriteLine("CardButton: RefreshCardButtonMofN " + this.card.GetCardAccessibleName());
        }
        else
        {
            Debug.WriteLine("CardButton: RefreshCardButtonMofN NULL card");
        }

        if (this.Card != null)
        {
            OnPropertyChanged("CardPileAccessibleName");
        }
    }

    public string CardPileAccessibleNameWithoutMofN
    {
        get
        {
            string cardPileAccessibleName = "";

            if ((MainPage.currentGameType == SolitaireGameType.Pyramid) && string.IsNullOrEmpty(this.AutomationId))
            {
                if ((this != null) && this.IsVisible && (this.Card != null) && (MainPage.MainPageSingleton != null))
                {
                    var dealtCardViewModel = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
                    if ((dealtCardViewModel != null) && (dealtCardViewModel.DealtCards != null))
                    {
                        CollectionView? list;
                        var dealtCard = MainPage.MainPageSingleton.FindDealtCardFromCard(this.Card, false, out list);
                        if (dealtCard != null)
                        {
                            var rowCards = dealtCardViewModel.DealtCards[dealtCard.PyramidRow];
                            if (rowCards != null)
                            {
                                cardPileAccessibleName = this.Card.GetCardAccessibleName();

                                if (this.IsToggled)
                                {
                                    cardPileAccessibleName += " " + MainPage.MyGetString("Selected");
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine("*** Unexpected empty accessible name. ***");
                        }
                    }
                }
            }

            return cardPileAccessibleName;
        }
    }

    public string CardPileAccessibleName
    {
        get
        {
            string cardPileAccessibleName = "";

            if (string.IsNullOrEmpty(this.AutomationId))
            {
                if ((this != null) && this.IsVisible && (this.Card != null) && (MainPage.MainPageSingleton != null))
                {
                    var dealtCardViewModel = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
                    if ((dealtCardViewModel != null) && (dealtCardViewModel.DealtCards != null))
                    {
                        DealtCard? dealtCard = null;

                        if (MainPage.currentGameType == SolitaireGameType.Tripeaks)
                        {
                            dealtCard = MainPage.MainPageSingleton.FindAnyDealtCardFromCard(this.Card);
                        }
                        else
                        {
                            CollectionView? list;
                            dealtCard = MainPage.MainPageSingleton.FindDealtCardFromCard(this.Card, false, out list);
                        }

                        if (dealtCard != null)
                        {
                            var rowCards = dealtCardViewModel.DealtCards[dealtCard.PyramidRow];
                            if (rowCards != null)
                            {
                                var name = "";

                                if (this.IsFaceUp)
                                {
                                    name = this.Card.GetCardAccessibleName() + (dealtCard.Open ? ", " +
                                                        MainPage.MyGetString("Open") : "");
                                }
                                else
                                {
                                    name = MainPage.MyGetString("FaceDown");
                                }

                                cardPileAccessibleName = name +
                                                    ", " + MainPage.MyGetString("Row") + " " + (dealtCard.PyramidRow + 1) +
                                                    ", " + (dealtCard.PyramidCardCurrentIndexInRow + 1) +
                                                    " " + MainPage.MyGetString("Of") + " " + dealtCard.PyramidCardCurrentCountOfCardsOnRow;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("*** Unexpected empty accessible name. ***");
                        }
                    }
                }
            }
            else
            {
                // Is this card pile empty?
                if (this.Card == null)
                {
                    // We'll load up a suit-specific localized string which indicates
                    // that no card is in this pile.
                    string suitResourceKey;

                    switch (this.AutomationId)
                    {
                        case "TargetPileC":
                            suitResourceKey = "ClubsPile";
                            break;

                        case "TargetPileD":
                            suitResourceKey = "DiamondsPile";
                            break;

                        case "TargetPileH":
                            suitResourceKey = "HeartsPile";
                            break;

                        case "TargetPileS":
                            suitResourceKey = "SpadesPile";
                            break;

                        case "CardDeckUpturned":

                            if (MainPage.currentGameType == SolitaireGameType.Pyramid)
                            {
                                suitResourceKey = "NoCardUpturnedCard";
                            }
                            else
                            {
                                suitResourceKey = "NoCard";
                            }

                            break;

                        case "CardDeckUpturnedObscuredHigher":

                            if (MainPage.currentGameType == SolitaireGameType.Pyramid)
                            {
                                suitResourceKey = "NoCardWastePile";
                            }
                            else
                            {
                                suitResourceKey = "NoCard";
                            }

                            break;

                        default:
                            // This must be one of the upturned cards.
                            suitResourceKey = "NoCard";
                            break;
                    }

                    cardPileAccessibleName = MainPage.MyGetString(suitResourceKey);
                }
                else
                {
                    // There is a card in this pile, so simply get the card's friendly name.
                    cardPileAccessibleName = this.Card.GetCardAccessibleName();

                    if (MainPage.currentGameType == SolitaireGameType.Pyramid)
                    {
                        if (this.AutomationId == "CardDeckUpturned")
                        {
                            cardPileAccessibleName += " " + MainPage.MyGetString("Upturned");
                        }
                        else if (this.AutomationId == "CardDeckUpturnedObscuredHigher")
                        {
                            cardPileAccessibleName += " " + MainPage.MyGetString("OnWastePile");
                        }
                    }
                }
            }

            if (this.IsToggled)
            {
                cardPileAccessibleName += " " + MainPage.MyGetString("Selected");
            }

            return cardPileAccessibleName;
        }
    }

    public ImageSource? CardPileImage
    {
        get
        {
            if (Application.Current == null)
            {
                return null;
            }

            string cardAsset;

            bool largeVersionAvailable = true;
            bool darkVersionAvailable = true;

            // Is this card pile empty?
            if (this.Card == null)
            {
                switch (this.AutomationId)
                {
                    case "TargetPileC":
                        cardAsset = "emptyclubs";

                        // There are no dark versions of the empty target card pile images.
                        darkVersionAvailable = false;

                        break;

                    case "TargetPileD":
                        cardAsset = "emptydiamonds";

                        darkVersionAvailable = false;

                        break;

                    case "TargetPileH":
                        cardAsset = "emptyhearts";

                        darkVersionAvailable = false;

                        break;

                    case "TargetPileS":
                        cardAsset = "emptyspades";

                        darkVersionAvailable = false;

                        break;

                    default:
                        cardAsset = "EmptyDealtCardPile";
                        break;
                }

                // There are no large versions of the empty card images.
                largeVersionAvailable = false;
            }
            else
            {
                switch (Card.Suit)
                {
                    case Suit.Clubs:
                        cardAsset = "Clubs";
                        break;

                    case Suit.Diamonds:
                        cardAsset = "Diamonds";
                        break;

                    case Suit.Hearts:
                        cardAsset = "Hearts";
                        break;

                    default:
                        cardAsset = "Spades";
                        break;
                }

                switch (Card.Rank)
                {
                    case 1:
                        cardAsset += "1";
                        break;

                    case 2:
                        cardAsset += "2";
                        break;

                    case 3:
                        cardAsset += "3";
                        break;

                    case 4:
                        cardAsset += "4";
                        break;

                    case 5:
                        cardAsset += "5";
                        break;

                    case 6:
                        cardAsset += "6";
                        break;

                    case 7:
                        cardAsset += "7";
                        break;

                    case 8:
                        cardAsset += "8";
                        break;

                    case 9:
                        cardAsset += "9";
                        break;

                    case 10:
                        cardAsset += "10";
                        break;

                    case 11:
                        cardAsset += "11";
                        break;

                    case 12:
                        cardAsset += "12";
                        break;

                    default:
                        cardAsset += "13";
                        break;
                }
            }

            if (string.IsNullOrEmpty(cardAsset))
            {
                return "";
            }

            var imageFileName = cardAsset.ToLower();

            // Many of the images in the project are .svg files. They get converted to .png by MAUI.
            // While Android can reference the original .svg here, iOS cannot. So reference instead
            // the .png equivalent, as both Android and iOS are happy with that.
            imageFileName += ".png";

            if (largeVersionAvailable && MainPage.ShowRankSuitLarge)
            {
                imageFileName = "large" + imageFileName;
            }

            if (darkVersionAvailable && (Application.Current.RequestedTheme == AppTheme.Dark))
            {
                imageFileName = "dark" + imageFileName;
            }

            var imageSource = ImageSource.FromFile(imageFileName);

            return imageSource;
        }
    }

    public ImageSource? PictureCardPileImage
    {
        get
        {
            if (Application.Current == null)
            {
                return null;
            }

            if (MainPage.ShowRankSuitLarge)
            {
                return null;
            }

            string cardAsset = "";

            if (this.Card != null)
            {
                switch (this.Card.Suit)
                {
                    case Suit.Clubs:
                    case Suit.Spades:
                        cardAsset = "black";
                        break;

                    case Suit.Diamonds:
                    case Suit.Hearts:
                        cardAsset = "red";
                        break;

                    default:
                        cardAsset = "";
                        break;
                }

                switch (this.Card.Rank)
                {
                    case 11:
                        cardAsset += "jack";
                        break;

                    case 12:
                        cardAsset += "queen";
                        break;

                    case 13:
                        cardAsset += "king";
                        break;

                    default:
                        cardAsset = "";
                        break;
                }
            }

            if (string.IsNullOrEmpty(cardAsset))
            {
                return "";
            }

            var imageFileName = cardAsset.ToLower() + ".png";

            if (Application.Current.RequestedTheme == AppTheme.Dark)
            {
                imageFileName = "dark" + imageFileName;
            }

            var imageSource = ImageSource.FromFile(imageFileName);

            return imageSource;
        }
    }

    public static readonly BindableProperty LongPressZoomDurationProperty =
        BindableProperty.Create(nameof(LongPressZoomDuration), typeof(int), typeof(CardButton));

    public int LongPressZoomDuration
    {
        get => (int)GetValue(LongPressZoomDurationProperty);
        set
        {
            if ((int)GetValue(LongPressZoomDurationProperty) != value)
            {
                SetValue(LongPressZoomDurationProperty, value);

                this.OnPropertyChanged("LongPressZoomDuration");
            }
        }
    }

    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(CardButton));

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set
        {
            if ((bool)GetValue(IsToggledProperty) != value)
            {
                SetValue(IsToggledProperty, value);

                this.OnPropertyChanged("IsToggled");
                this.OnPropertyChanged("CardPileAccessibleName");
            }
        }
    }


    public static readonly BindableProperty IsFaceUpProperty =
        BindableProperty.Create(nameof(IsFaceUp), typeof(bool), typeof(CardButton));

    public bool IsFaceUp
    {
        get => (bool)GetValue(IsFaceUpProperty);
        set
        {
            if ((bool)GetValue(IsFaceUpProperty) != value)
            {
                SetValue(IsFaceUpProperty, value);

                this.OnPropertyChanged("IsFaceUp");
            }
        }
    }

    public void RefreshAccessibleName()
    {
        if (this.Card != null)
        {
            OnPropertyChanged("CardPileAccessibleName");
        }
    }

    public void RefreshVisuals()
    {
        if (this.card != null)
        {
            Debug.WriteLine("CardButton: RefreshVisuals " + this.card.GetCardAccessibleName());
        }
        else
        {
            Debug.WriteLine("CardButton: RefreshVisuals NULL card");
        }

        this.OnPropertyChanged("Card");
        this.OnPropertyChanged("CardPileAccessibleName");

        this.OnPropertyChanged("BackgroundColor");
        this.OnPropertyChanged("CardPileImage");
        this.OnPropertyChanged("PictureCardPileImage");

        this.OnPropertyChanged("IsFaceUp"); 
    }

    private void TouchBehavior_LongPressCompleted(object sender, CommunityToolkit.Maui.Core.LongPressCompletedEventArgs e)
    {
        if ((MainPage.MainPageSingleton != null) && (this.Card != null))
        {
            // If the popup is already up, do nothing here.
            if (!CardPopup.IsZoomPopupOpen())
            {
                MainPage.MainPageSingleton.ShowZoomedCardPopup(this.Card, false);
            }
        }
    }

    private DateTime datetimePreviousClickOnCardButton = DateTime.Now;

    private void CardButton_Clicked(object sender, EventArgs e)
    {
        // Barker: I've had an unexpected repeated click on a card following a click with VoiceOver.
        // So ignore a fast quick repeat click.
        var msSincePreviousClick = (DateTime.Now - datetimePreviousClickOnCardButton).TotalMilliseconds;
        Debug.WriteLine("CardButton_Clicked: MS since previous click " + msSincePreviousClick);
        if (msSincePreviousClick < 200)
        {
            return;
        }

        datetimePreviousClickOnCardButton = DateTime.Now;

        var button = sender as Button;
        if (button == null)
        {
            return;
        }

        HandleCardButtonClick(button);
    }

    private void HandleCardButtonClick(Button button)
    {
        if (MainPage.IsCurrentGamePaused())
        {
            MainPage.ShowGameIsPausedMessage();

            return;
        }

        //Debug.WriteLine("CardButton Clicked: " + button.AutomationId);

        if (CardPopup.IsZoomPopupOpen())
        {
            Debug.WriteLine("Zoom Popup is open, so ignore tap on card.");

            return;
        }

        MainPage.MainPageSingleton?.CardButtonClicked(button);
    }

    private static Card? focusedCardButtonCard = null;

    public static Card? FocusedCardButtonCard { get => focusedCardButtonCard; set => focusedCardButtonCard = value; }

    private void CardButtonInnerButton_Focused(object sender, FocusEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var cardButton = button.BindingContext as CardButton;
            if ((cardButton != null) && ((cardButton.Card != null)))
            {
                //Debug.WriteLine("Fococused CardButton: " + cardButton.Card.GetCardAccessibleName());

                CardButton.FocusedCardButtonCard = card;
            }
        }
    }

    private void CardButtonInnerButton_Unfocused(object sender, FocusEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var cardButton = button.BindingContext as CardButton;
            if ((cardButton != null) && ((cardButton.Card != null)))
            {
                //Debug.WriteLine("Unfocused CardButton: " + cardButton.Card.GetCardAccessibleName());

                CardButton.FocusedCardButtonCard = null;
            }
        }
    }
}