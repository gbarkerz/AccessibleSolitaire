# Accessible Solitaire for iOS and Android

The Accessible Solitaire game is an exploration into building the most accessible solitaire game possible for iOS devices like iPhones and iPads, and Android phones. 

Today the game focuses on the experience for players who use zoom features or the TalkBack screen reader, but in the future I'd like to also explore how the game might be played with a switch device. The game can always be made more accessible, so please provide feedback to help me learn how I should prioritise its accessibility improvements.

Your feedback can make a real difference, so please let me know by messaging "Sa11ytaire Help" at Facebook, or e-mail gbarkerz@hotmail.com.

Some technical details relating to building the app are available at [Case Study: Building a Multi-Platform Accessible Solitaire Game](https://www.linkedin.com/pulse/case-study-building-multi-platform-accessible-solitaire-guy-barker-zwlxe).

Short videos demonstrating some of the ways that Accessible Solitaire can be played are referenced below. Please note that currently these videos do not have captions or audio descriptions.

[Playing Accessible Solitaire for Android with various zoom features](https://youtu.be/uy4U3ORwIh0)

[Playing Accessible Solitaire for Android with the Android TalkBack screen reader](https://youtu.be/6I04Rj7vMZA)

[Playing Accessible Solitaire for Android with Android Switch Access](https://youtu.be/Foa-Oz6LvCA)

[Playing Accessible Solitaire for Android with Android Voice Access](https://youtu.be/zEMFCMTLPx8)

## Playing the game

The goal of the game is to build up four piles of playing cards, one per suit, in the target card piles shown near the top right of the game. The piles must be built up in order, starting with an ace and ending with a king.

**Please note:** When moving a card, first tap the card to be moved and then tap the place where you'd like the card to go. Do not try dragging cards around the game.

Seven dealt card piles are shown along the bottom half of the game, with the left-most pile having only one card, and each pile then containing one more card, up to the right-most card pile which contains seven cards. Only the last card in each dealt card pile is shown face-up, and all the other cards in the pile are face-down. 

The last card in a pile can be moved to a target card pile if it continues the order of the cards being added to the target card pile. For example, a 2 of Hearts in a dealt card pile can be moved on top of an Ace of Hearts in the Hearts target card pile. An Ace is always the first card to be moved to a target card pile.

Face-up cards in dealt card piles can also be moved to lie on top of the last card in another dealt card pile if the card being moved is one number lower than the card on which it is being placed, and the cards are not the same colour. For example, a 3 of Clubs in one dealt card pile could be moved to lie over a 4 of Diamonds if the 4 of Diamonds is the last card in another dealt card pile.

The remaining playing cards in the deck are placed face-down near the top-left of the game. The top card in that pile can be tapped and either one, two, or three face-down cards in the pile will be turned face-up next to the pile. The number of cards turned up depends on your current game settings. The top-most card turned up can then be moved to either a target card pile or the end of a dealt card pile with the same rules as when moving dealt cards to either a target card pile or another dealt card pile. Once all cards have been turned up, tap on the button again to have all the remaining cards turned face-down to work through them again.

In addition to being able to move cards from the remaining card pile to the target card piles or dealt card piles, or from the dealt card piles to the target card piles or another dealt card pile, you may sometimes choose to move a card from a target card pile back down to a dealt card pile. Such a move is not common, but can be useful at times.

Cards are moved from the upturned card pile, or the dealt card piles, until all four target card piles have been built up and the game is won. If it is not possible to make any more moves which help to build up the target card piles, the game cannot be won and you would restart the game.

![A solitaire game in progress with various cards moved between the three main areas of the game.](/ReadmeScreenshots/AccessibleSolitaire_DefaultGame.jpg)

## Size of things shown in the game

The game has three settings which can help make the contents of cards easier to see.

### "Zoom level"

By default, the things shown in the game are sized such that all parts of the game fit onto the device screen. This means on phone screens, some things might be so small that they're not easy to see or tap. The size of things shown in the game can be changed by tapping the Menu button near the top-left of the game and tapping the “Settings” item. The game's settings contains a "Zoom Level" setting which can be increased to make everything bigger in the game.

Once everything is bigger, the entire game won’t fit on the screen and so the top part of the game containing the upturned cards and the target card piles can be scrolled to bring whatever’s of interest in that area into view. Also, the lower part of the game containing the dealt card piles can be scrolled separately from the top part, such that whatever dealt card piles are of interest can be brought into view.

![The game zoomed to 150%, showing portions of the two main areas of the game scrolled into view.](/ReadmeScreenshots/AccessibleSolitaire_Zoom150.jpg)

### "Card rank and suit display"

By default, the content of cards is similar to those of traditional playing cards, such that the main area shows a collection of small suit symbols. This means determining exactly which suit is being shown can be a challenge, given that the clubs and spade symbols are very similar, as are the diamonds and hearts symbols. The "Card rank and suit display" setting provides a way to have the card not show the traditional collection of symbols, and instead show one large number (or letter) to indicate the card's rank, and one large suit symbol.

![A game in progress, with all cards showing a large number or letter indicating the card's rank and a large suit symbol.](/ReadmeScreenshots/AccessibleSolitaire_ShowLargeRankSuit.jpg)

### "Show Zoom Card Button"

The "Show Zoom Card Button" setting provides a way to have a zoom button shown at the top right corner of all face-up cards. When that button is tapped, a large popup appears containing the associated card. To dismiss the popup, either tap its Close button, or tap outside the popup.

![A game in progress with all face-up cards showing a zoom icon at their top right corners.](/ReadmeScreenshots/AccessibleSolitaire_ShowZoomCardButton.jpg)

![A large popup window showing a 3 of Spades card, with the rest of the game greyed out in the background.](/ReadmeScreenshots/AccessibleSolitaire_PopupLight.jpg)

## Using the TalkBack screen reader

The TalkBack screen reader announces the name of whatever it encounters in the game. For example: 

- "Menu, Button"
- "Next card, Button"”
- "Upturned Cards"
- "Clubs pile"
- "Dealt card piles"

All cards in the upturned card piles or target card piles behave like switches, with a toggled state to indicate whether a card has been selected in preparation for moving it. As such, TalkBack will announce them as switches and either On or Off depending on their toggled state. For example, if the Ace of Clubs is shown in the Clubs target card piles, TalkBack will say either "OFF, Ace of Clubs, Switch" or "ON, Ace of Clubs, Switch".  

When TalkBack encounters a card in one of the dealt card piles, it announces the details of the card. The details always include the rank and suit of the card if the card is face-up, or the phrase "Face down" if the card is face-down. The details also include the position of the card in the dealt card pile, the count of cards in that pile, and whether the card is selected. For example: "10 of Diamonds, selected, 3 of 4". TalkBack might also announce which dealt card pile the card is in, as either its "dealt card pile" or "list pile".

TalkBack can be moved directly to an element in the game  by touching the element, or by swiping right or left to have TalkBack move to the next or previous element respectively. When navigating forward by swipe gesture, TalkBack will announce the container of the dealt card piles as: "Dealt card piles".

TalkBack also announces details in response to some specific actions that are taken in the game. For example:

- When selecting a card in a dealt card pile: "3 of Clubs, selected, 3 of 3 in dealt card pile 3".
- When a  card has been moved from a dealt card pile up to a target card pile, and the next card in the pile is turned face-up: "Moved Ace of Hearts, Revealed Jack of Diamonds, 4 of 4 in dealt card pile 5".
- When the Next Card button is tapped and two cards are turned over: "5 of Hearts, 6 of Spades on top".
- When the Next Card button is tapped and the pile becomes empty: “9 of spades, 3 of Diamonds on top. No cards left in remaining cards pile”.
- When the topmost upturned card is selected: “3 of Diamonds selected”.

![TalkBack's highlight at a partially obscured 8 of Clubs card in a dealt card pile, and TalkBack's caption showing the text, Revealed 8 of Clubs, 5 of 5 in dealt card pile 6](/ReadmeScreenshots/AccessibleSolitaire_TalkBackMoveBetweenDealtPiles.jpg)

![TalkBack's highlight at the 3 of Spades card in the Spades target pile, and TalkBack's caption showing the text, Moved 3 of Spades, Revealed Empty card pile in dealt card pile 3](/ReadmeScreenshots/AccessibleSolitaire_TalkBackMoveToTargetPile.jpg)

**Announcing the entire state of the game:** To have the current state of the entire game announced by TalkBack, go to the Settings page and check: “Show state announcement button”. When you then return to the game, a button called: "Screen reader announce game state" will follow the Menu button near the top-left of the game. When that button is tapped, TalkBack will announce the current state of the upturned cards, the target card piles, and the dealt card piles. For example:

"Top upturned card is 6 of Clubs, then 9 of Diamonds, then 4 of Clubs.  More cards are available to turn over. Target piles, 2 of Clubs, 2 of Diamonds, Ace of Hearts, Empty Spades Pile. Pile 1, 10 of Hearts to King of Spades, Pile 2, Empty, Pile 3, 8 of Clubs, 2 Cards Face-down, Pile 4, 10 of Spades to Jack of Diamonds, 2 Cards Face-down, Pile 5, 7 of Clubs, 4 Cards Face-down, Pile 6, 4 of Diamonds, 1 Card Face-down, Pile 7, 5 of Diamonds, 6 Cards Face-down."

![TalkBack's highlight around the "Screen reader announce game state" button, and TalkBack's caption showing the entire state of the game, including what cards are shown in the upturned cards pile, the target cards piles, and all dealt cards piles.](/ReadmeScreenshots/AccessibleSolitaire_TalkBackGameState.jpg)

**Please note:** TalkBack sometimes makes announcements that are not helpful and can be confusing. For example, various transitioning selection states of a card being moved before it announces the card's final selection state. Also when a card is being moved and TalkBack announces the position of a card in a pile and the count of cards in that pile, sometimes those details do not relate to the card of most interest during a move. Over time I hope to reduce the number of unwanted announcements made by TalkBack.  

**Please note:** When TalkBack is not running, a selected card in a dealt card pile can be tapped to deselect it. When TalkBack is running, a selected card does not respond when a TalkBack double-tap gesture is made on the card. To deselect that card, double-tap on another card.

## Colours shown in the game

The colours shown in the game depend on whether the **Android Dark Mode** setting is on when the game is started. 

![A game in progress at a zoom level of 150%, with all cards showing a large rank letter and suit symbol, using the app's dark mode colours.](/ReadmeScreenshots/AccessibleSolitaire_DarkZoom.jpg)

![A game in progress with all cards showing a zoom icon at their top right corners, using the app's dark mode colours.](/ReadmeScreenshots/AccessibleSolitaire_DarkShowZoomCard.jpg)

![A large popup window showing a 10 of Clubs card, using the app's dark mode colours.](/ReadmeScreenshots/AccessibleSolitaire_DarkPopup.jpg)

In addition, the game has a **"Card Appearance"** setting which can be used to lower the brightness of cards shown in the game.

![A game in progress with all cards having a grey background instead of the default white background.](/ReadmeScreenshots/AccessibleSolitaire_DarkenCards.jpg)

## Other Settings

### "Number of cards turned up" and "Empty Dealt Card Pile"

The game has two settings which can influence how challenging it is for the game to be won. One of those settings, "Number of cards turned up", controls how many cards are turned over from the remaining card pile when the Next Card button is tapped. If cards are turned up one at a time, the game is more likely to be won, but if the cards are turned up three at a time, you may find it less common for the game to be won. The other setting, "Empty Dealt Card Pile", controls what cards can be moved to empty dealt card piles. If this is set such that any card can be moved to an empty dealt card pile, the game is more likely to be won, and the game is less likely to be won if only Kings can be moved to an empty dealt card pile.

### "Screen Reader"

As mentioned above, the "Show state announcement button" can be used to have the TalkBack screen reader announce the entire state of the game. If TalkBack is not running, this button appears to do nothing when tapped. 

## New options introduced in Version 1.18

Accessible Solitaire V1.18 introduced two new options. 

### Select via face-down card

Often when selecting a partially obscured card in a dealt card pile, the card of interest is the lowest face-up card in the pile. Given that that card can be difficult to tap on due to the amount of card that's shown, the "Select via face-down card" option means a tap on any face-down card in a dealt card pile will select or deselect the nearest face-up card in the same pile. This will often make it less challenging to select the lowest face-up card in a dealt card pile.

### Highlight selected card set

Often when moving cards between dealt card piles, multiple cards are moved in a single move action. For example, in the following image the 6 of Hearts is selected in preparation for moving it over to a 7 of Spades in another dealt card pile. On top of the 6 of Hearts are four other cards, and those cards will be moved to the other dealt card pile when the 6 of Hearts is moved. By default, when the 6 of Hearts is selected, the visuals associated with all the cards lying on top of the 6 of Hearts are not changed, despite the fact that the next operation will impact all those cards. The "Highlight selected card set" option has the visuals for those related cards change to indicate they will be moved when the selected card is moved.

Note that the accessible name of the cards associated with the selected card always includes the phase: "in selected set", to indicate that the cards are impacted by the moving of the selected card in the same dealt card pile.

![A game in progress with a 6 of Hearts selected in a dealt card pile. Four cards lie above the 6 of Hearts, and their backgrounds show a gradient shading from white at the top left corner, down to light purple at the bottom right corner. TalkBack's highlight is around one of the partially shown card above the 6 of Hearts, and its caption is: "3 of Spades, in selected set, 6 of 7, in list Pile 6"](/ReadmeScreenshots/V1.18_SelectedSetLight.jpg)

The following image shows the new highlight for cards associated with the selected dealt card, when the cards show a large suit symbol and rank letter, and the Zoom Card button in their top right corners.

![A game in progress with all cards showing a large suit symbol and rank letter, and the Zoom Card button in their top right corner. One card is selected, and all associated cards have a gradient white-purple background.](/ReadmeScreenshots/V1.18_SelectedSetLightLarge.jpg)

The following image shows the new highlight for cards associated with the selected dealt card, when the Android Dark Mode setting was on when the Accessible Solitaire was started.

![A game in progress with all cards showing the app's Dark Mode colours. One card is selected, and all associated cards have a gradient black-yellow background.](/ReadmeScreenshots/V1.18_SelectedSetDark.jpg)

## New options introduced in Version 1.19

Accessible Solitaire V1.19 introduced the following option.

### Merge face-down cards

Given that all the face-down cards in the dealt card piles can occupy a lot of space in the game, you may prefer to have only one face-down card shown in each dealt card pile if the pile has any face-down cards. By choosing this option, only one face-down card is shown in each dealt card pile, and the total number of face-down cards in the associated pile is shown on that face-down card.  

Note: This option might be particularly interesting when playing the game with the TalkBack screen reader. By default when swiping to move TalkBack through a dealt card pile, TalkBack will move to each face-down card in turn before it reaches the face-up cards in the pile. By merging all the face-down cards, TalkBack will encounter the one face-down card shown, announce the count of face-down cards, then with the next swipe will move to the first face-up card in the pile. Note that when TalkBack moves to the lowest face-down card in the pile, it will always announce the count of face-down cards in that pile regardless of whether the face-down cards are merged. 

Also, when TalkBack announces the position of a card in a dealt card pile and the total count of cards in the pile, it will announce details as if the face-down cards are not merged. 

![A game in progress with the face-down cards in each dealt card pile merged into a single face-down card. The face-down card shows the count of face-down card in its associated pile. TalkBack's highlight is at the face-down card shown at dealt card pile 7. That face-down card shows a number 6, and above that card are two face-up cards. TalkBack's caption says: 6 Face-down, 1 of 8, In list Pile 7](/ReadmeScreenshots/V1.19_MergeFaceDownCards.jpg)

## Android Switch Access 

The Accessible Solitaire game can be played using the Android Switch Access feature. This might also be known as “Universal Switch” on some devices. For information relating to this Android feature, please visit [Switch Access](https://support.google.com/accessibility/android/topic/6151780).

### Tips on using Android Switch Access

When playing Accessible Solitaire with a switch, please consider the following:

1. At the Accessible Solitaire Settings page, turn on “Merge face-down cards”. This reduces the number of switch presses required before reaching dealt cards of interest.

2. At the Android Switch Settings set a “Default scan method” of “Group”. This reduces the number of switch presses required when moving to a group containing a card of interest.

3. When a card of interest is selected using a switch, the Android Switch “global menu” may appear, and another switch press is then required to tap the card. If you’d prefer a card to be tapped automatically when selected, at the Android Switch Settings select turn on the “Enable auto-select” option. (On some devices this may be called “Auto tap to select”.) Also consider reducing the delay before the auto-tap occurs after the card is selected.

![A game in progress with the face-down cards in each dealt card pile merged into a single face-down card. The Android Switch Access highlight surrounds the entire set of seven dealt card piles, with the topmost face-up cards in two of the piles protruding slightly below the bottom of the highlight.](/ReadmeScreenshots/AccessibleSolitaire_SwitchAccess.jpg)

## Android Voice Access 

**Important:** Development of the game-playing experience using Android Voice Access is in progress, and you may currently find the quality of the experience unsatisfactory.

Technically the Accessible Solitaire game can be played using the Android Voice Access feature. For information relating to this Android feature, please visit [Voice Access](https://support.google.com/accessibility/android/topic/6151842).

### Common phrases

Phrases that are commonly used in the game are as follows:

- The name of a card. For example: "three of diamonds".
- "next card", to turn over the next card in the remaining cards pile.
- "turn over cards", when the remaining card pile is empty and all upturned cards are to be turned back over.
- The name of an empty target card pile. For example: "clubs pile" or "hearts pile".
- The name of an empty dealt card pile. For example: "pile one" or "pile seven".

### Tips on using Android Voice Access

When playing Accessible Solitaire with speech, please consider the following:

1. To increase the likelihood of spoken card names being recognised by Android Voice Access, go to the Accessible Solitaire Settings page and turn on the "Speak card name to select it" option.

2. After speaking, check whether Android Voice Access correctly recognised the phrase you spoke. For example, it might incorrectly recognise the spoken phrase "ten of" as "turn off", in which case the intended card will not be selected. In that case, perhaps say "tap ten" followed by a slight pause, and then speak the remainder of the card name.

3. If a card or a button in the game cannot be selected or clicked using its name, use the Android Voice Access command "Show numbers" to have a number shown above the card or button, and then speak that number.

![A game in progress with the Ace of Clubs selected in dealt pile four. The Android Voice Access feature is displaying the following text at the top of the screen: "Tapped Ace of clubs"](/ReadmeScreenshots/AccessibleSolitaire_VoiceAccess.jpg)
