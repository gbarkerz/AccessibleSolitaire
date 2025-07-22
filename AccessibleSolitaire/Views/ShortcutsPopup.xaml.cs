using CommunityToolkit.Maui.Views;

namespace Sa11ytaire4All.Views;

public partial class ShortcutsPopup : Popup
{
    public ShortcutsPopup()
    {
        InitializeComponent();
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await this.CloseAsync();
    }
}
