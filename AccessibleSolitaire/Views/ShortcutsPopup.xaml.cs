using CommunityToolkit.Maui.Views;

namespace Sa11ytaire4All.Views;

public partial class ShortcutsPopup : Popup
{
    public ShortcutsPopup()
    {
        InitializeComponent();

        this.Loaded += ShortcutsPopup_Loaded;
    }

    private void ShortcutsPopup_Loaded(object? sender, EventArgs e)
    {
        CloseButton.Focus();
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await this.CloseAsync();
    }
}
