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
    }

    static public bool IsZoomPopupOpen()
    {
        return zoomPopupIsVisible;
    }

    private void CardPopup_Closed(object? sender, CommunityToolkit.Maui.Core.PopupClosedEventArgs e)
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
        BindableProperty.Create(nameof(SuitColoursClubs), typeof(Color), typeof(CardPileCardSwitch));

    public Color SuitColoursClubs
    {
        get => (Color)GetValue(SuitColoursClubsProperty);
        set
        {
            SetValue(SuitColoursClubsProperty, value);

            this.OnPropertyChanged("SuitColoursClubs");
        }
    }

    public static readonly BindableProperty SuitColoursDiamondsProperty =
        BindableProperty.Create(nameof(SuitColoursDiamonds), typeof(Color), typeof(CardPileCardSwitch));

    public Color SuitColoursDiamonds
    {
        get => (Color)GetValue(SuitColoursDiamondsProperty);
        set
        {
            SetValue(SuitColoursDiamondsProperty, value);

            this.OnPropertyChanged("SuitColoursDiamonds");
        }
    }

    public static readonly BindableProperty SuitColoursHeartsProperty =
        BindableProperty.Create(nameof(SuitColoursHearts), typeof(Color), typeof(CardPileCardSwitch));

    public Color SuitColoursHearts
    {
        get => (Color)GetValue(SuitColoursHeartsProperty);
        set
        {
            SetValue(SuitColoursHeartsProperty, value);

            this.OnPropertyChanged("SuitColoursHearts");
        }
    }

    public static readonly BindableProperty SuitColoursSpadesProperty =
        BindableProperty.Create(nameof(SuitColoursSpades), typeof(Color), typeof(CardPileCardSwitch));

    public Color SuitColoursSpades
    {
        get => (Color)GetValue(SuitColoursSpadesProperty);
        set
        {
            SetValue(SuitColoursSpadesProperty, value);

            this.OnPropertyChanged("SuitColoursSpades");
        }
    }

    private void CloseButton_Clicked(object sender, EventArgs e)
    {
        this.Close();
    }
}
