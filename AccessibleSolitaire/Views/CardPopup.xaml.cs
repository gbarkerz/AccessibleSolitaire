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

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await this.CloseAsync();
    }
}
