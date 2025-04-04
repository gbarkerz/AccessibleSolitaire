using System.ComponentModel;
using System.Diagnostics;
using Sa11ytaire4All.Source;

using Switch = Microsoft.Maui.Controls.Switch;

namespace Sa11ytaire4All.Views;

public partial class CardPileCardSwitch : ContentView, INotifyPropertyChanged
{
    public CardPileCardSwitch()
	{
        BindingContext = this;

        InitializeComponent();
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

    public string CardPileAccessibleName
    {
        get
        {
            string cardPileAccessibleName;

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

    public static readonly BindableProperty SuitColoursClubsSwitchProperty =
        BindableProperty.Create(nameof(SuitColoursClubsSwitch), typeof(Color), typeof(CardPileCardSwitch));

    public Color SuitColoursClubsSwitch
    {
        get => (Color)GetValue(SuitColoursClubsSwitchProperty);
        set
        {
            SetValue(SuitColoursClubsSwitchProperty, value);

            this.OnPropertyChanged("SuitColoursClubsSwitch");
        }
    }

    public static readonly BindableProperty SuitColoursDiamondsSwitchProperty =
        BindableProperty.Create(nameof(SuitColoursDiamondsSwitch), typeof(Color), typeof(CardPileCardSwitch));

    public Color SuitColoursDiamondsSwitch
    {
        get => (Color)GetValue(SuitColoursDiamondsSwitchProperty);
        set
        {
            SetValue(SuitColoursDiamondsSwitchProperty, value);

            this.OnPropertyChanged("SuitColoursDiamondsSwitch");
        }
    }

    public static readonly BindableProperty SuitColoursHeartsSwitchProperty =
        BindableProperty.Create(nameof(SuitColoursHeartsSwitch), typeof(Color), typeof(CardPileCardSwitch));

    public Color SuitColoursHeartsSwitch
    {
        get => (Color)GetValue(SuitColoursHeartsSwitchProperty);
        set
        {
            SetValue(SuitColoursHeartsSwitchProperty, value);

            this.OnPropertyChanged("SuitColoursHeartsSwitch");
        }
    }

    public static readonly BindableProperty SuitColoursSpadesSwitchProperty =
        BindableProperty.Create(nameof(SuitColoursSpadesSwitch), typeof(Color), typeof(CardPileCardSwitch));

    public Color SuitColoursSpadesSwitch
    {
        get => (Color)GetValue(SuitColoursSpadesSwitchProperty);
        set
        {
            SetValue(SuitColoursSpadesSwitchProperty, value);

            this.OnPropertyChanged("SuitColoursSpadesSwitch");
        }
    }

    public static readonly BindableProperty LongPressZoomDurationProperty =
        BindableProperty.Create(nameof(LongPressZoomDuration), typeof(int), typeof(CardPileCardSwitch));

    public int LongPressZoomDuration
    {
        get => (int)GetValue(LongPressZoomDurationProperty);
        set
        {
            SetValue(LongPressZoomDurationProperty, value);

            this.OnPropertyChanged("LongPressZoomDuration");
        }
    }

    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(CardPileCardSwitch));

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set
        {
            SetValue(IsToggledProperty, value);

            this.OnPropertyChanged("IsToggled");
        }
    }

    public void SetToggledState(bool toggledState)
    {
        var cardSwitch = this.FindByName("CardSwitch") as Button;
        if (cardSwitch == null)
        {
            return;
        }

        Debug.WriteLine("SetToggledState: " + cardSwitch.AutomationId);

        IsToggled = toggledState;
    }

    public void RefreshVisuals()
    {
        if (this.card != null)
        {
            this.OnPropertyChanged("BackgroundColor");
            this.OnPropertyChanged("CardPileImage");
            this.OnPropertyChanged("PictureCardPileImage");
        }
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

    private void CardSwitch_Clicked(object sender, EventArgs e)
    {
        var cardSwitch = sender as Button;
        if (cardSwitch == null)
        {
            return;
        }

        Debug.WriteLine("CardSwitch Clicked: " + cardSwitch.AutomationId);

        if (CardPopup.IsZoomPopupOpen())
        {
            Debug.WriteLine("Zoom Popup is open, so ignore tap on card.");

            return;
        }

        MainPage.MainPageSingleton?.CardPileCardSelected(cardSwitch);

        var accessibleName = SemanticProperties.GetDescription(cardSwitch);

        string announcement =
            accessibleName + " " +
            MainPage.MyGetString(IsToggled ? "Selected" : "Unselected");

        SemanticScreenReader.Default.Announce(announcement);
    }
}
