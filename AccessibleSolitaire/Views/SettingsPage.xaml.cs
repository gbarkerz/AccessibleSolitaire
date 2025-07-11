namespace Sa11ytaire4All.Views;

public partial class SettingsPage : ContentPage
{

    public SettingsPage()
	{
		InitializeComponent();

#if IOS || ANDROID
        MergeFaceDownCardsHeading.IsVisible = true;
        MergeFaceDownCardsGrid.IsVisible = true;
#endif

#if IOS
        VoiceOverScreenReaderWarning.IsVisible = true;
#endif

        var pickers = new Picker[]{ 
            SuitColoursClubsPicker,
            SuitColoursDiamondsPicker,
            SuitColoursHeartsPicker,
            SuitColoursSpadesPicker};

        foreach (var picker in pickers)
        {
            foreach (var colourName in MainPage.suitColours.Keys)
            {
                picker.Items.Add(MainPage.MyGetString(colourName));
            }
        }

        for (int i = 0; i < 6; ++i)
        {
            LongPressOnCardPicker.Items.Add(i == 0 ? "Never" : i.ToString());
        }

        var longPressZoomDuration = (int)Preferences.Get("LongPressZoomDuration", 2000);
        LongPressOnCardPicker.SelectedIndex = longPressZoomDuration / 1000;

        var currentColour = (string)Preferences.Get("SuitColoursClubs", "Default");
        SuitColoursClubsPicker.SelectedItem = MainPage.MyGetString(currentColour);

        currentColour = (string)Preferences.Get("SuitColoursDiamonds", "Default");
        SuitColoursDiamondsPicker.SelectedItem = MainPage.MyGetString(currentColour);

        currentColour = (string)Preferences.Get("SuitColoursHearts", "Default");
        SuitColoursHeartsPicker.SelectedItem = MainPage.MyGetString(currentColour);

        currentColour = (string)Preferences.Get("SuitColoursSpades", "Default");
        SuitColoursSpadesPicker.SelectedItem = MainPage.MyGetString(currentColour);

        var showRankSuitLarge = (bool)Preferences.Get("ShowRankSuitLarge", true);
        ShowRankSuitLargeSwitch.IsToggled = showRankSuitLarge;

        var showScreenReaderAnnouncementButtons = (bool)Preferences.Get("ShowScreenReaderAnnouncementButtons", false);
        ShowScreenReaderAnnouncementButtonsSwitch.IsToggled = showScreenReaderAnnouncementButtons;

        var addHintToTopmostCard = (bool)Preferences.Get("AddHintToTopmostCard", false);
        AddHintToTopmostCardSwitch.IsToggled = addHintToTopmostCard;

        var includeFacedownCardsInAnnouncement = (bool)Preferences.Get("IncludeFacedownCardsInAnnouncement", false);
        IncludeFacedownCardsInAnnouncementSwitch.IsToggled = includeFacedownCardsInAnnouncement;

        var ExtendDealtCardHitTarget = (bool)Preferences.Get("ExtendDealtCardHitTarget", false);
        ExtendDealtCardHitTargetSwitch.IsToggled = ExtendDealtCardHitTarget;

        var allowSelectionByFaceDownCard = (bool)Preferences.Get("AllowSelectionByFaceDownCard", true);
        AllowSelectionByFaceDownCardSwitch.IsToggled = allowSelectionByFaceDownCard;

        var mergeFaceDownCards = MainPage.GetMergeFaceDownCardsSetting();
        MergeFaceDownCardsSwitch.IsToggled = mergeFaceDownCards;

        var flipGameLayoutHorizontally = (bool)Preferences.Get("FlipGameLayoutHorizontally", false);
        FlipGameLayoutHorizontallySwitch.IsToggled = flipGameLayoutHorizontally;

        var playSoundSuccessfulMove = (bool)Preferences.Get("PlaySoundSuccessfulMove", false);
        PlaySoundSuccessfulMoveSwitch.IsToggled = playSoundSuccessfulMove;

        var playSoundUnsuccessfulMove = (bool)Preferences.Get("PlaySoundUnsuccessfulMove", false);
        PlaySoundUnsuccessfulMoveSwitch.IsToggled = playSoundUnsuccessfulMove;

        var playSoundOther = (bool)Preferences.Get("PlaySoundOther", false);
        PlaySoundOtherSwitch.IsToggled = playSoundOther;

        var celebrationExperienceVisual = (bool)Preferences.Get("CelebrationExperienceVisual", false);
        CelebrationExperienceVisualSwitch.IsToggled = celebrationExperienceVisual;

        var celebrationExperienceAudio = (bool)Preferences.Get("CelebrationExperienceAudio", false);
        CelebrationExperienceAudioSwitch.IsToggled = celebrationExperienceAudio;

        // Game playing options.

        for (int i = 1; i <= 3; ++i)
        {
            CardTurnCountPicker.Items.Add(i.ToString());
        }

        var cardTurnCount = (int)Preferences.Get("CardTurnCount", 1);
        CardTurnCountPicker.SelectedItem = cardTurnCount.ToString();

        var kingsOnlyToEmptyPile = (bool)Preferences.Get("KingsOnlyToEmptyPile", false);
        KingsOnlyToEmptyPileSwitch.IsToggled = kingsOnlyToEmptyPile;

        var keepGameAcrossSessions = (bool)Preferences.Get("KeepGameAcrossSessions", true);
        KeepGameAcrossSessionsSwitch.IsToggled = keepGameAcrossSessions;

#if WINDOWS
        this.Loaded += SettingsPage_Loaded;
#endif
    }

    private void SettingsPage_Loaded(object? sender, EventArgs e)
    {
        // Always set focus to the first control in the Settings page. If we don't set focus here,
        // focus can be left in the MainPage UI, and tabbing moves focus through the various areas
        // of the game before moving to the Settings page UI.
        CardTurnCountPicker.Focus();
    }

    private void CloseButton_Clicked(object sender, EventArgs e)
    {
        if (LongPressOnCardPicker != null)
        {
            var selectedIndex = LongPressOnCardPicker.SelectedIndex;
            if (selectedIndex >= 0)
            {
                Preferences.Set("LongPressZoomDuration", selectedIndex * 1000);
            }
        }

        var suitColoursArray = MainPage.suitColours.ToArray();

        var selectedSuitIndex = SuitColoursClubsPicker.SelectedIndex;
        if ((selectedSuitIndex >= 0) && (selectedSuitIndex < MainPage.suitColours.Count)) 
        {
            Preferences.Set("SuitColoursClubs", suitColoursArray[selectedSuitIndex].Key);
        }

        selectedSuitIndex = SuitColoursDiamondsPicker.SelectedIndex;
        if ((selectedSuitIndex >= 0) && (selectedSuitIndex < MainPage.suitColours.Count))
        {
            Preferences.Set("SuitColoursDiamonds", suitColoursArray[selectedSuitIndex].Key);
        }

        selectedSuitIndex = SuitColoursHeartsPicker.SelectedIndex;
        if ((selectedSuitIndex >= 0) && (selectedSuitIndex < MainPage.suitColours.Count))
        {
            Preferences.Set("SuitColoursHearts", suitColoursArray[selectedSuitIndex].Key);
        }

        selectedSuitIndex = SuitColoursSpadesPicker.SelectedIndex;
        if ((selectedSuitIndex >= 0) && (selectedSuitIndex < MainPage.suitColours.Count))
        {
            Preferences.Set("SuitColoursSpades", suitColoursArray[selectedSuitIndex].Key);
        }

        var showRankSuitLarge = ShowRankSuitLargeSwitch.IsToggled;
        Preferences.Set("ShowRankSuitLarge", showRankSuitLarge);

        var showScreenReaderAnnouncementButtons = ShowScreenReaderAnnouncementButtonsSwitch.IsToggled;
        Preferences.Set("ShowScreenReaderAnnouncementButtons", showScreenReaderAnnouncementButtons);

        var addHintToTopmostCard = AddHintToTopmostCardSwitch.IsToggled;
        Preferences.Set("AddHintToTopmostCard", addHintToTopmostCard);

        var includeFacedownCardsInAnnouncement = IncludeFacedownCardsInAnnouncementSwitch.IsToggled;
        Preferences.Set("IncludeFacedownCardsInAnnouncement", includeFacedownCardsInAnnouncement);

        var ExtendDealtCardHitTarget = ExtendDealtCardHitTargetSwitch.IsToggled;
        Preferences.Set("ExtendDealtCardHitTarget", ExtendDealtCardHitTarget);

        var allowSelectionByFaceDownCard = AllowSelectionByFaceDownCardSwitch.IsToggled;
        Preferences.Set("AllowSelectionByFaceDownCard", allowSelectionByFaceDownCard);

        var mergeFaceDownCards = MergeFaceDownCardsSwitch.IsToggled;
        Preferences.Set("MergeFaceDownCards", mergeFaceDownCards);

        var flipGameLayoutHorizontally = FlipGameLayoutHorizontallySwitch.IsToggled;
        Preferences.Set("FlipGameLayoutHorizontally", flipGameLayoutHorizontally);

        var playSoundSuccessfulMove = PlaySoundSuccessfulMoveSwitch.IsToggled;
        Preferences.Set("PlaySoundSuccessfulMove", playSoundSuccessfulMove);

        var playSoundUnsuccessfulMove = PlaySoundUnsuccessfulMoveSwitch.IsToggled;
        Preferences.Set("PlaySoundUnsuccessfulMove", playSoundUnsuccessfulMove);

        var playSoundOther = PlaySoundOtherSwitch.IsToggled;
        Preferences.Set("PlaySoundOther", playSoundOther);

        var celebrationExperienceVisual = CelebrationExperienceVisualSwitch.IsToggled;
        Preferences.Set("CelebrationExperienceVisual", celebrationExperienceVisual);

        var celebrationExperienceAudio = CelebrationExperienceAudioSwitch.IsToggled;
        Preferences.Set("CelebrationExperienceAudio", celebrationExperienceAudio);

        // Game playing options.

        var cardTurnCountValue = (int)CardTurnCountPicker.SelectedIndex;
        Preferences.Set("CardTurnCount", cardTurnCountValue + 1);

        var kingsOnlyToEmptyPile = KingsOnlyToEmptyPileSwitch.IsToggled;
        Preferences.Set("KingsOnlyToEmptyPile", kingsOnlyToEmptyPile);

        var keepGameAcrossSessions = KeepGameAcrossSessionsSwitch.IsToggled;
        Preferences.Set("KeepGameAcrossSessions", keepGameAcrossSessions);

        Navigation.PopModalAsync();
    }
}