using CommunityToolkit.Maui.Views;
using Sa11ytaire4All.Source;

namespace Sa11ytaire4All.Views;

public partial class CardPopup : Popup
{
    public CardPopup(Card card)
    {
        this.BindingContext = card;

        InitializeComponent();
    }

    private void CloseButton_Clicked(object sender, EventArgs e)
    {
        this.Close();
    }
}
