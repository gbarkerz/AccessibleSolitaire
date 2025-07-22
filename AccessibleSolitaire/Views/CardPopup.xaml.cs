using CommunityToolkit.Maui.Views;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using System.ComponentModel;

namespace Sa11ytaire4All.Views;

public partial class CardPopup : Popup, INotifyPropertyChanged
{
    static private bool zoomPopupIsVisible = false;

    public CardPopup(Card card, DealtCardViewModel vm)
    {
        this.BindingContext = this;

        this.Card = card;

        this.SuitColoursClubs = vm.SuitColoursClubs;
        this.SuitColoursDiamonds = vm.SuitColoursDiamonds;
        this.SuitColoursHearts = vm.SuitColoursHearts;
        this.SuitColoursSpades = vm.SuitColoursSpades;

        InitializeComponent();

        // Barker Todo: Find a more appropriate way of determining whether the popup is displayed.
        zoomPopupIsVisible = true;

        this.Closed += CardPopup_Closed;

        this.Loaded += CardPopup_Loaded;
    }

    private void CardPopup_Loaded(object? sender, EventArgs e)
    {
        CloseButton.Focus();
    }

    static public bool IsZoomPopupOpen()
    {
        return zoomPopupIsVisible;
    }

    private void CardPopup_Closed(object? sender, EventArgs e)
    {
        zoomPopupIsVisible = false;
    }

    public static readonly BindableProperty CardProperty =
        BindableProperty.Create(nameof(Card), typeof(Card), typeof(DealtCard));

    public Card Card
    {
        get { return (Card)GetValue(CardProperty); }
        set
        {
            if (Card != value)
            {
                SetValue(CardProperty, value);

                OnPropertyChanged("Card");
            }
        }
    }


    public static readonly BindableProperty SuitColoursClubsProperty =
        BindableProperty.Create(nameof(SuitColoursClubs), typeof(Color), typeof(CardButton));

    public Color? SuitColoursClubs
    {
        get => (Color)GetValue(SuitColoursClubsProperty);
        set
        {
            SetValue(SuitColoursClubsProperty, value);

            this.OnPropertyChanged("SuitColoursClubs");
        }
    }

    public static readonly BindableProperty SuitColoursDiamondsProperty =
        BindableProperty.Create(nameof(SuitColoursDiamonds), typeof(Color), typeof(CardButton));

    public Color? SuitColoursDiamonds
    {
        get => (Color)GetValue(SuitColoursDiamondsProperty);
        set
        {
            SetValue(SuitColoursDiamondsProperty, value);

            this.OnPropertyChanged("SuitColoursDiamonds");
        }
    }

    public static readonly BindableProperty SuitColoursHeartsProperty =
        BindableProperty.Create(nameof(SuitColoursHearts), typeof(Color), typeof(CardButton));

    public Color? SuitColoursHearts
    {
        get => (Color)GetValue(SuitColoursHeartsProperty);
        set
        {
            SetValue(SuitColoursHeartsProperty, value);

            this.OnPropertyChanged("SuitColoursHearts");
        }
    }

    public static readonly BindableProperty SuitColoursSpadesProperty =
        BindableProperty.Create(nameof(SuitColoursSpades), typeof(Color), typeof(CardButton));

    public Color? SuitColoursSpades
    {
        get => (Color)GetValue(SuitColoursSpadesProperty);
        set
        {
            SetValue(SuitColoursSpadesProperty, value);

            this.OnPropertyChanged("SuitColoursSpades");
        }
    }

    public static readonly BindableProperty CardPopupTintColourProperty =
        BindableProperty.Create(nameof(CardPopupTintColour), typeof(Color), typeof(CardPopup));

    public Color? CardPopupTintColour
    {
        get => GetCardPopupColourTint();
    }

    private Color? GetCardPopupColourTint()
    {
        Color? suitColor = null;

        if (this.Card == null)
        {
            // No Card supplied, so perhaps this is an empty target card pile.
            if (this.AutomationId == null)
            {
                return null;
            }

            switch (this.AutomationId)
            {
                case "TargetPileC":
                    suitColor = this.SuitColoursClubs;
                    break;

                case "TargetPileD":
                    suitColor = this.SuitColoursDiamonds;
                    break;

                case "TargetPileH":
                    suitColor = this.SuitColoursHearts;
                    break;

                case "TargetPileS":
                    suitColor = this.SuitColoursSpades;
                    break;

                default:
                    break;
            }
        }
        else
        {
            switch (this.Card.Suit)
            {
                case Suit.Clubs:
                    suitColor = this.SuitColoursClubs;
                    break;

                case Suit.Diamonds:
                    suitColor = this.SuitColoursDiamonds;
                    break;

                case Suit.Hearts:
                    suitColor = this.SuitColoursHearts;
                    break;

                case Suit.Spades:
                    suitColor = this.SuitColoursSpades;
                    break;

                default:
                    if (Application.Current != null)
                    {
                        suitColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                            Colors.LightGrey : Colors.Grey);
                    }

                    break;
            }
        }

        return suitColor;
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await this.CloseAsync();
    }
}
