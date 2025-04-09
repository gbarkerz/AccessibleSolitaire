# Accessible Solitaire for iOS and Android

The Accessible Solitaire game is an exploration into building the most accessible solitaire game possible for iOS devices like iPhones and iPads, and Android phones. 

Today the game focuses on the experiences for players who use zoom features, screen readers, switch devices, or speech to play their games. The game can always be made more accessible, so please provide feedback to help me learn how I should prioritise its accessibility improvements.

Your feedback can make a real difference, so please let me know by messaging "Sa11ytaire Help" at Facebook, or e-mail gbarkerz@hotmail.com.

Some technical details relating to building the Android version of the app are available at [Case Study: Building a Multi-Platform Accessible Solitaire Game](https://www.linkedin.com/pulse/case-study-building-multi-platform-accessible-solitaire-guy-barker-zwlxe) and [Accessible Solitaire for iOS and Android: Reaching the Starting Line](https://www.linkedin.com/pulse/accessible-solitaire-ios-android-reaching-starting-line-guy-barker-8ad9e).

Short videos demonstrating some of the ways that Accessible Solitaire can be played are referenced below. Please note that currently these videos do not have captions or audio descriptions.

**iOS**

[Playing Accessible Solitaire on iOS with Switch Control](https://youtu.be/pRBqJvEHXN4)

[Playing Accessible Solitaire on iOS with Voice Control](https://youtu.be/S_9veYTN_48)

[Playing Accessible Solitaire on iOS with the iOS VoiceOver screen reader](https://youtu.be/dMB6RmiMEfE)

**Android**

[Playing Accessible Solitaire on Android with various zoom features](https://youtu.be/uy4U3ORwIh0)

[Playing Accessible Solitaire on Android with the Android TalkBack screen reader](https://youtu.be/6I04Rj7vMZA)

[Playing Accessible Solitaire on Android with Android Switch Access](https://youtu.be/Foa-Oz6LvCA)

## Playing the game

The goal of the game is to build up four piles of playing cards, one per suit, in the target card piles shown near the top right of the game. The piles must be built up in order, starting with an ace and ending with a king.

**Please note:** When moving a card, first tap the card to be moved and then tap the place where you'd like the card to go. Do not try dragging cards around the game.

**Please note:** The "Next card" button is used to turn over cards from a pile of face-down cards near the top left corner of the app. The button does not show any text on it, rather it shows a picture of the Herbi cartoon character wearing a scarf. 

Seven dealt card piles are shown along the bottom half of the game, with the left-most pile having only one card, and each pile then containing one more card, up to the right-most card pile which contains seven cards. Only the last card in each dealt card pile is shown face-up, and all the other cards in the pile are face-down. 

The last card in a pile can be moved to a target card pile if it continues the order of the cards being added to the target card pile. For example, a 2 of Hearts in a dealt card pile can be moved on top of an Ace of Hearts in the Hearts target card pile. An Ace is always the first card to be moved to a target card pile.

Face-up cards in dealt card piles can also be moved to lie on top of the last card in another dealt card pile if the card being moved is one number lower than the card on which it is being placed, and the cards are not the same colour. For example, a 3 of Clubs in one dealt card pile could be moved to lie over a 4 of Diamonds if the 4 of Diamonds is the last card in another dealt card pile.

The remaining playing cards in the deck are placed face-down near the top-left of the game. The top card in that pile can be tapped and either one, two, or three face-down cards in the pile will be turned face-up next to the pile. The number of cards turned up depends on your current game settings. The top-most card turned up can then be moved to either a target card pile or the end of a dealt card pile with the same rules as when moving dealt cards to either a target card pile or another dealt card pile. Once all cards have been turned up, tap on the button again to have all the remaining cards turned face-down to work through them again.

In addition to being able to move cards from the remaining card pile to the target card piles or dealt card piles, or from the dealt card piles to the target card piles or another dealt card pile, you may sometimes choose to move a card from a target card pile back down to a dealt card pile. Such a move is not common, but can be useful at times.

Cards are moved from the upturned card pile, or the dealt card piles, until all four target card piles have been built up and the game is won. If it is not possible to make any more moves which help to build up the target card piles, the game cannot be won and you would restart the game.

Figure 1: A solitaire game running on iOS with various cards moved between the three main areas of the game.
![iOS: A solitaire game in progress with various cards moved between the three main areas of the game.](/ReadmeScreenshots/AccessibleSolitaire_DefaultGame_iOS.jpg)

Figure 2: A solitaire game running on Android with various cards moved between the three main areas of the game.
![Android: A solitaire game in progress with various cards moved between the three main areas of the game.](/ReadmeScreenshots/AccessibleSolitaire_DefaultGame.jpg)

## Differences between Version 1 and Version 2 of Accessible Solitaire

Based on feedback, three features that were available in Version 1 have been removed for Version 2. These features were: in-app zoom of two areas of the app, buttons on each card to show a zoomed version of the card, and a setting to make all parts of the game darker. Also, based on feedback, two features have been added to version 2. These features are a long press on a card to show a zoomed popup of the card, and a setting to change the colours used for the four playing card suits. Also in Version 2, screen readers consider the playing cards shown in the remaining cards pile and target card piles to be buttons rather than switches.  

## Size of things shown in the game

The game has three features which can help make the contents of cards easier to see.

### Card rank and suit display

A setting in the app can be used to have the content of playing cards be similar to those of traditional playing cards, such that the main area shows a collection of small suit symbols. A collection of small symbols can make determining exactly which suit is being shown  challenge, given that the clubs and spade symbols are very similar, as are the diamonds and hearts symbols. A such, a "Card rank and suit display" setting provides a way to have the card not show the traditional collection of symbols, and instead show one large number (or letter) to indicate the card's rank, and one large suit symbol. By default, the "Card rank and suit display" setting is on.

Figure 3: A solitaire game running on iOS, with all cards showing a large number or letter indicating the card's rank and a large suit symbol.
![iOS: A game in progress, with all cards showing a large number or letter indicating the card's rank and a large suit symbol.](/ReadmeScreenshots/AccessibleSolitaire_ShowLargeRankSuit_iOS.jpg)

Figure 4: A solitaire game running on Android, with all cards showing a large number or letter indicating the card's rank and a large suit symbol.
![Android: A game in progress, with all cards showing a large number or letter indicating the card's rank and a large suit symbol.](/ReadmeScreenshots/AccessibleSolitaire_ShowLargeRankSuit.jpg)

### Show a playing card in a zoomed popup

If a player places their finger on a playing card of interest, and leaves their finger on the card for a specific time, a zoomed version of the card will popup. To dismiss the popup, either tap its Close button, or tap outside the popup. The time required for the popup to appear when a finger is on the card can be adjusted with the "Press and hold on card to zoom" setting, and this setting can also be used to never show the zoom popup.

Figure 5: A solitaire game running on iOS, with a large popup window showing a 3 of Spades card, with the rest of the game greyed out in the background.
![iOS: A large popup window showing a 3 of Spades card, with the rest of the game greyed out in the background.](/ReadmeScreenshots/AccessibleSolitaire_PopupLight_iOS.jpg)

Figure 6: A solitaire game running on Android, with a large popup window showing a 3 of Spades card, with the rest of the game greyed out in the background.
![Android: A large popup window showing a 3 of Spades card, with the rest of the game greyed out in the background.](/ReadmeScreenshots/AccessibleSolitaire_PopupLight.jpg)

### Changes the colours used for suits 

By default, the colours of suits are the traditional playing card colours, those being black clubs, red diamonds, red hearts, and black spades. Having the same colours used for different suits can make it a challenge to differentiate clubs from spades, and diamonds from hearts. So a setting can be used to change the colours used for each suit.  

Figure 7: A solitaire game running on iOS, with the following suit colours: Clubs black, Diamonds red, Hearts dark red, Spades dark blue.
![iOS: Playing cards with the following suit colours: Clubs black, Diamonds red, Hearts dark red, Spades dark blue.](/ReadmeScreenshots/V2_SuitColours_RedBlack.jpg)

Figure 8: A solitaire game running on iOS, with the following suit colours: Clubs dark green, Diamonds dark gold, Hearts dark indigo, Spades dark gold.
![iOS: Playing cards with the following suit colours: Clubs dark green, Diamonds dark gold, Hearts dark indigo, Spades dark gold.](/ReadmeScreenshots/V2_SuitColours_Light.jpg)

Figure 9: A solitaire game running on iOS with the app's dark colour theme, with the following suit colours: Clubs violet, Diamonds dark violet, Hearts dark red, Spades blue.
![iOS: Playing cards with the following suit colours: Clubs violet, Diamonds dark violet, Hearts dark red, Spades blue.](/ReadmeScreenshots/V2_SuitColours_Dark.jpg)

## Using a switch device (iOS Switch Control or Android Switch Access)

The Accessible Solitaire game can be played using iOS Switch Control or Android Switch Access. For more information on these features, please visit [Use Switch Control to navigate your iPhone, iPad or iPod touch](https://support.apple.com/119835) or [Switch Access](https://support.google.com/accessibility/android/topic/6151780).

### Tips on using a switch device

When playing Accessible Solitaire with a switch, please consider the following:

1. At the Accessible Solitaire Settings page, turn on “Merge face-down cards”. This reduces the number of switch presses required before reaching dealt cards of interest.

2. Android: Set the Android Switch "Default scan method" setting to "Group". This reduces the number of switch presses required when moving to a group containing a card of interest.

3. When a card of interest is selected using a switch, the a menu of actions may appear, and another switch press is then required to tap the card. If you’d prefer a card to be tapped automatically when selected, change the device's Switch settings to have the card tapped with a single click of the switch. Also consider reducing the delay before the auto-tap occurs after the card is selected.

Figure 10: A solitaire game running on iOS, with Switch Control faint highlighting 6 of the 7 dealt card piles and strong highlighting the entire fourth dealt card pile.
![iOS: A game in progress with the face-down cards in each dealt card pile merged into a single face-down card. A faint dashed highlight surrounds the first 6 dealt card piles, and a stronger highlight surrounds the cards in the fourth dealt cards pile.](/ReadmeScreenshots/AccessibleSolitaire_SwitchAccess_iOS.jpg)

Figure 11: A solitaire game running on Android, with Switch Access highlighting the entire set of the 7 dealt card piles.
![Android: A game in progress with the face-down cards in each dealt card pile merged into a single face-down card. The Android Switch Access highlight surrounds the entire set of seven dealt card piles, with the topmost face-up cards in two of the piles protruding slightly below the bottom of the highlight.](/ReadmeScreenshots/AccessibleSolitaire_SwitchAccess.jpg)

## Using speech (iOS Voice Control only) 

**Important:** At the time of writing this, Android Voice Access cannot be used to play the Accessible Solitaire game.

For more information on the iOS Voice Control feature, please visit: [Use Voice Control on your iPhone, iPad or iPod touch](https://support.apple.com/111778).

### Common phrases

Phrases that are commonly used in the game are: "tap", followed by one of the these:

- The name of a card. For example: "three of diamonds".
- "next card", to turn over the next card in the remaining cards pile.
- "turn over cards", when the remaining card pile is empty and all upturned cards are to be turned back over.
- The name of an empty target card pile. For example: "clubs pile" or "hearts pile".
- The name of an empty dealt card pile. For example: "pile one" or "pile seven".

Figure 12: A solitaire game running on iOS, with Voice Control showing names by all interactable elements.
![iOS: A game in progress with iOS Voice Controls showing names by all interactable controls. For example, "Diamonds" by the Diamonds target card pile, and "4" by the 4 of Spades.](/ReadmeScreenshots/AccessibleSolitaire_VoiceAccess_iOS.jpg)

## Using a screen reader (iOS VoiceOver or Android TalkBack)

A screen reader announces the name of whatever it encounters in the game. For example: 

- "Menu, Button"
- "Next card, Button"

All cards in the upturned card piles or target card piles behave like buttons, whose names include the word "selected" if the card has been selected in readiness to move the card. For example, if the Ace of Clubs is shown in the Clubs target card piles, a screen reader will say either "Ace of Clubs, button" or "Ace of Clubs, selected, button".  

When the screen reader encounters a card in one of the dealt card piles, it announces the details of the card. The details always include the rank and suit of the card if the card is face-up, or the phrase "Face down" if the card is face-down. The details also include the position of the card in the dealt card pile, the count of cards in that pile, and whether the card is selected. For example: "10 of Diamonds, selected, 3 of 4". The screen reader might also announce which dealt card pile the card is in, as either its "dealt card pile" or "list pile".

The screen reader can be moved directly to an element in the game by touching the element, or by swiping right or left to have TalkBack move to the next or previous element respectively. When navigating forward by swipe gesture, TalkBack will announce the container of the dealt card piles as: "Dealt card piles".

The screen reader also announces details in response to some specific actions that are taken in the game. For example:

- When selecting a card in a dealt card pile: "3 of Clubs, selected, 3 of 3 in dealt card pile 3".
- When a card has been moved from a dealt card pile up to a target card pile, and the next card in the pile is turned face-up: "Moved Ace of Hearts, Revealed Jack of Diamonds, 4 of 4 in dealt card pile 5".
- When the Next Card button is tapped and two cards are turned over: "5 of Hearts, 6 of Spades on top".
- When the Next Card button is tapped and the pile becomes empty: “9 of spades, 3 of Diamonds on top. No cards left in remaining cards pile”.
- When the topmost upturned card is selected: “3 of Diamonds selected”.

**Announcing the entire state of the game:** To have the current state of the entire game announced by the screen reader, go to the Settings page and check: “Show state announcement button”. When you then return to the game, a button called: "Screen reader announce game state" will follow the Menu button near the top-left of the game. When that button is tapped, the screen reader will announce the current state of the upturned cards, the target card piles, and the dealt card piles. For example:

"Top upturned card is 6 of Clubs, then 9 of Diamonds, then 4 of Clubs.  More cards are available to turn over. Target piles, 2 of Clubs, 2 of Diamonds, Ace of Hearts, Empty Spades Pile. Pile 1, 10 of Hearts to King of Spades, Pile 2, Empty, Pile 3, 8 of Clubs, 2 Cards Face-down, Pile 4, 10 of Spades to Jack of Diamonds, 2 Cards Face-down, Pile 5, 7 of Clubs, 4 Cards Face-down, Pile 6, 4 of Diamonds, 1 Card Face-down, Pile 7, 5 of Diamonds, 6 Cards Face-down."

**Extend dealt card hit targets:** This setting only applies using a portrait screen orientation. When turned on, the hit target for each of the topmost dealt cards extends across the width of the screen. The hit target is represented by a solid colour, and the dealt card appears in the middle of the area. Extending the hit target area may make it quicker to find the topmost card in each pile when moving your finger over the screen while using a screen reader.

**Note:** When using the Android TalkBack screen reader, the screen reader will move to the three containers associated with the upturned cards pile, the target card piles, and the dealt card piles, and announce their names in addition to moving to the cards contained inside those groups. When using the iOS VoiceOver screen reader, the screen reader only moves to the cards, and does not move to the three containers.

**TIP:** When using the iOS VoiceOver screen reader, turn off the VoiceOver"Text Recognition" setting to prevent VoiceOver attempting to recognise any text shown on the cards and announcing that text in addition to the name of the card.

Figure 13: A solitaire game running on iOS, with VoiceOver highlighting the 3 of Spades and its caption showing an announcement relating to a move.
![iOS: VoiceOver's highlight at a partially obscured 3 of Spades card in a dealt card pile, and VocieOver's caption showing the text, Moved 2 of Hearts, revealed 8 of Clubs, 1 of 1 in dealt card pile 5](/ReadmeScreenshots/AccessibleSolitaire_VoiceOverMoveBetweenDealtPiles.jpg)

Figure 14: A solitaire game running on iOS, with VoiceOver highlighting the Jack of Spades and its caption showing an announcement relating to a move.
![iOS: VoiceOver's highlight at the Jack of Spades card in a dealt card pile, and VoiceOver's caption showing the text, Moved King of Clubs to Empty Dealt card pile 2](/ReadmeScreenshots/AccessibleSolitaire_VoiceOverMoveToTargetPile.jpg)

Figure 15: A solitaire game running on iOS, with VoiceOver announcing the entire state of the game.
![iOS: VoiceOver's highlight around the "Screen reader announce game state" button, and VoiceOver's caption showing the entire state of the game, including what cards are shown in the upturned cards pile, the target cards piles, and all dealt cards piles.](/ReadmeScreenshots/AccessibleSolitaire_VoiceOverGameState.jpg)

Figure 16: A solitaire game running on Android, with TalkBack highlighting the 8 of Clubs and its caption showing an announcement relating to a move.
![Android: TalkBack's highlight at a partially obscured 8 of Clubs card in a dealt card pile, and TalkBack's caption showing the text, Revealed 8 of Clubs, 5 of 5 in dealt card pile 6](/ReadmeScreenshots/AccessibleSolitaire_TalkBackMoveBetweenDealtPiles.jpg)

Figure 17: A solitaire game running on Android, with TalkBack highlighting the 3 of Spades and its caption showing an announcement relating to a move.
![Android: TalkBack's highlight at the 3 of Spades card in the Spades target pile, and TalkBack's caption showing the text, Moved 3 of Spades, Revealed Empty card pile in dealt card pile 3](/ReadmeScreenshots/AccessibleSolitaire_TalkBackMoveToTargetPile.jpg)

Figure 18: A solitaire game running on Android, with TalkBack announcing the entire state of the game.
![Android: TalkBack's highlight around the "Screen reader announce game state" button, and TalkBack's caption showing the entire state of the game, including what cards are shown in the upturned cards pile, the target cards piles, and all dealt cards piles.](/ReadmeScreenshots/AccessibleSolitaire_TalkBackGameState.jpg)

**Please note:** The screen readers sometimes makes announcements that are not helpful and can be confusing. For example, various transitioning selection states of a card being moved before it announces the card's final selection state. Also when a card is being moved and the screen reader announces the position of a card in a pile and the count of cards in that pile, sometimes those details do not relate to the card of most interest during a move. Over time I hope to reduce the number of unwanted announcements made by the screen reader.  

**Please note:** When the screen reader is not running, a selected card in a dealt card pile can be tapped to deselect it. When the screen reader is running, a selected card does not respond when a  double-tap gesture is made on the card. To deselect that card, double-tap on another card.

## Colours shown in the game

The colours shown in the game depend on whether the device's ** Dark Mode** setting is on when the game is started. 

## Other Settings

### "Number of cards turned up" and "Empty Dealt Card Pile"

The game has two settings which can influence how challenging it is for the game to be won. One of those settings, "Number of cards turned up", controls how many cards are turned over from the remaining card pile when the Next Card button is tapped. If cards are turned up one at a time, the game is more likely to be won, but if the cards are turned up three at a time, you may find it less common for the game to be won. The other setting, "Empty Dealt Card Pile", controls what cards can be moved to empty dealt card piles. If this is set such that any card can be moved to an empty dealt card pile, the game is more likely to be won, and the game is less likely to be won if only Kings can be moved to an empty dealt card pile.

### Merge face-down cards

Given that all the face-down cards in the dealt card piles can occupy a lot of space in the game, you may prefer to have only one face-down card shown in each dealt card pile if the pile has any face-down cards. By choosing this option, only one face-down card is shown in each dealt card pile, and the total number of face-down cards in the associated pile is shown on that face-down card.  

Note: This option might be particularly interesting when playing the game with the screen reader or switch device. By default when swiping to move the screen reader through a dealt card pile, the screen reader will move to each face-down card in turn before it reaches the face-up cards in the pile. By merging all the face-down cards, the screen reader will encounter the one face-down card shown, announce the count of face-down cards, then with the next swipe will move to the first face-up card in the pile. Note that when the screen reader moves to the lowest face-down card in the pile, it will always announce the count of face-down cards in that pile regardless of whether the face-down cards are merged. Similarly, merging the face-down cards also means that when a switch device is being used to scan through all the cards, it takes less time to reach the face-up cards of interest.

Not also that when the screen reader announces the position of a card in a dealt card pile and the total count of cards in the pile, it will announce details as if the face-down cards are not merged. 

Figure 19: A solitaire game running on Android with the face-down cards in each dealt card pile merged into a single face-down card.
![Android: A game in progress with the face-down cards in each dealt card pile merged into a single face-down card. The face-down card shows the count of face-down card in its associated pile. TalkBack's highlight is at the face-down card shown at dealt card pile 7. That face-down card shows a number 6, and above that card are two face-up cards. TalkBack's caption says: 6 Face-down, 1 of 8, In list Pile 7](/ReadmeScreenshots/V1.19_MergeFaceDownCards.jpg)

Figure 20: A solitaire game running on iOS with the face-down cards in each dealt card pile merged into a single face-down card.
![iOS: A game in progress with the face-down cards in each dealt card pile merged into a single face-down card. The face-down card shows the count of face-down card in its associated pile. VoiceOver's highlight is at the face-down card shown at dealt card pile 7. That face-down card shows a number 6, and above that card are two face-up cards. VoiceOver's caption says: 6 Face-down, 1 of 8](/ReadmeScreenshots/V1.19_MergeFaceDownCards_iOS.jpg)

### Flip layout horizontally

The "Flip layout" setting flips the layout of all things in the app horizontally. This might be helpful for players who prefer the Next Card button to be near the top right of the screen rather than the top left.

Figure 21: A solitaire game running on iOS with the layout of the game flipped horizontally.
![iOS: A game in progress with the target card piles shown at the left half of the top of the screen, ordered Spades, Hearts, Diamonds then Clubs. To the right of those piles is the remaining cards pile, the Next Cards button, and then the Menu button. Beneath those areas are the dealt cards piles, with the seventh pile at the left sides of the piles, and the first pile at the right side of the screen.](/ReadmeScreenshots/V2_Flip_Layout.jpg)

### Select via face-down card

Often when selecting a partially obscured card in a dealt card pile, the card of interest is the lowest face-up card in the pile. Given that that card can be difficult to tap on due to the amount of card that's shown, the "Select via face-down card" option means a tap on any face-down card in a dealt card pile will select or deselect the nearest face-up card in the same pile. This will often make it less challenging to select the lowest face-up card in a dealt card pile. The option is on by default.

### Sounds

This setting is currently only available on iOS, and can be used to have sounds played following specific events happening in the game.

### Highlight selected card set

Often when moving cards between dealt card piles, multiple cards are moved in a single move action. For example, in the following image the 6 of Hearts is selected in preparation for moving it over to a 7 of Spades in another dealt card pile. On top of the 6 of Hearts are four other cards, and those cards will be moved to the other dealt card pile when the 6 of Hearts is moved. By default, when the 6 of Hearts is selected, the visuals associated with all the cards lying on top of the 6 of Hearts are not changed, despite the fact that the next operation will impact all those cards. The "Highlight selected card set" option has the visuals for those related cards change to indicate they will be moved when the selected card is moved.

Note that the names of the cards associated with the selected card always includes the phase: "in selected set" when announced by the screen reader, to indicate that the cards are impacted by the moving of the selected card in the same dealt card pile.

Figure 22: A solitaire game running on Android with a dealt card selected and all cards on top of that selected card also highlighted.
![Android: A game in progress with a 6 of Hearts selected in a dealt card pile. Four cards lie above the 6 of Hearts, and their backgrounds are light purple. TalkBack's highlight is around one of the partially shown card above the 6 of Hearts, and its caption is: "3 of Spades, in selected set, 6 of 7, in list Pile 6"](/ReadmeScreenshots/V1.18_SelectedSetLight.jpg)

Figure 23: A solitaire game running on iOS with a dealt card selected and all cards on top of that selected card also highlighted.
![iOS: A game in progress with a 6 of Hearts selected in a dealt card pile. Four cards lie above the 6 of Hearts, and their backgrounds are light purple](/ReadmeScreenshots/V1.18_SelectedSetLight_iOS.jpg)

The following image shows the new highlight for cards associated with the selected dealt card, when the device's Dark Mode setting was on when the Accessible Solitaire was started.

Figure 24: A solitaire game running on iOS showing the app's Dark Mode colours, with a dealt card selected and all cards on top of that selected card also highlighted.
![iOS: A game in progress with all cards showing the app's Dark Mode colours. One card is selected, and all associated cards have a gradient black-yellow background.](/ReadmeScreenshots/V1.18_SelectedSetDark_iOS.jpg)

Figure 25: A solitaire game running on Android showing the app's Dark Mode colours, with a dealt card selected and all cards on top of that selected card also highlighted.
![Android: A game in progress with all cards showing the app's Dark Mode colours. One card is selected, and all associated cards have a gradient black-yellow background.](/ReadmeScreenshots/V1.18_SelectedSetDark.jpg)
