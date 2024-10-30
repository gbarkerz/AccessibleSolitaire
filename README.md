# Accessible Solitaire for Android

The Accessible Solitaire game for Android is an exploration into building the most accessible solitaire game possible for Android phones. Today the game focuses on the experience for players who use zoom features or the TalkBack screen reader, but in the future I'd like to also explore how the game might be played with a switch device. The game can always be made more accessible, so please provide feedback to help me learn how I should prioritise its accessibility improvements.

You feedback can make a real difference, so please let me know by messaging "Sa11ytaire Help" at Facebook, or e-mail gbarkerz@hotmail.com.

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_DefaultGame.jpg)

## Playing the game

The goal of the game is to build up four piles of playing cards, one per suit, in the target card piles shown near the top right of the game. The piles must be built up in order, starting with an ace and ending with a king.

**Please note:** When moving a card, first tap the card to be moved and then tap the place where you'd like the card to go. Do not try dragging cards around the game.

Seven dealt card piles are shown along the bottom half of the game, with the left-most pile having only one card, and each pile then containing one more card, up to the right-most card pile which contains seven cards. Only the last card in each dealt card pile is shown face-up, and all the other cards in the pile are face-down. 

The last card in a pile can be moved to a target card pile if it continues the order of the cards being added to the target card pile. For example, a 2 of Hearts in a dealt card pile can be moved on top of an Ace of Hearts in the Hearts target card pile. 

Face-up cards in dealt card piles can also be moved to lie on top of the last card in another dealt card pile if the card being moved is one number lower than the card on which it is being placed, and the cards are not the same colour. For example, a 3 of Clubs in one dealt card pile could be moved to lie over a 4 of Diamonds if the 4 of Diamonds  the last card in another dealt card pile.

The remaining playing cards in the deck are placed face-down near the top-left of the game. The top card in that pile can be tapped and either one, two, or three face-down cards in the pile will be turned face-up next to the pile. The number of cards turned up depends on your current game settings. The top-most of cards turned up can then be moved to either a target card pile or the end of a dealt card pile with the same rules as when moving dealt cards to either a target card pile or another dealt card pile. Once all cards have been turned up, tap on the button again to have all the remaining cards turned face-down to work through them again.

In addition to being able to move cards from the remaining card pile to the target card piles or dealt card piles, or from the dealt card piles to the target card piles or other dealt card piles, you may sometimes choose to move a card from the target card piles back down to a dealt card pile. Such a move is not common, but can be useful at times.

Cards are moved from the upturned card pile, or the dealt card piles, until all four target card piles have been built up and the game is won. If it is not possible to make any moves which help to build up the target card piles, the game would be restarted.

## Size of things shown in the game

The game has three settings which can help make the contents of cards easier to see.

### "Zoom level"

By default, the things shown in the game are sized such that all parts of the game fit onto the device screen. This means on phone screens, some things might be so small that they're not easy to see or tap. The size of things shown in the game can be changed by tapping the Menu button near the top-left of the game and tapping the “Settings” item. Settings contains a "Zoom Level" setting which can be increased to make everything bigger in the game.

Given that once everything is bigger the entire game won’t fit on the screen, the top part of the game containing the upturned cards and the target card piles can be scrolled to bring whatever’s of interest in that area into view. Also, the lower part of the game containing the dealt card piles can be scrolled separately from the top part, such that whatever dealt card piles are of interest can be brought into view.

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_Zoom150.jpg)

### "Card rank and suit display"

By default, the content of cards is similar to those of traditional playing cards, such that main area shows a collection of small suit symbols. This means determining exactly which suit is being shown can be a challenge, given that the clubs and spade symbols are very similar, as are the diamonds and hearts symbols. The "Card rank and suit display" setting provides a way to have the card not show the collection of symbols and instead show one large number (or letter) to indicate the card's rank, and one large suit symbol.

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_ShowLargeRankSuit.jpg)

### "Show Zoom Card Button"

The "Show Zoom Card Button" setting provides a way to have a zoom button shown at the top right corner of all cards. When that button is tapped, a large popup appears containing the associated card. To dismiss the popup, either tap its Close button, or tap outside the popup.

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_ShowZoomCardButton.jpg)

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_PopupLight.jpg)

## Using the TalkBack screen reader

The TalkBack screen reader will announce the name of whatever it’s encountered in the game. For example: 

- "Menu, Button"
- "Next card, Button"”
- "Upturned Cards"
- "Target card piles"
- "Dealt card piles"

All cards in the upturned card piles or target card piles behave like switches, with a toggled state to indicate whether a card has been selected in preparation for moving it. As such, TalkBack will announce them as switches and either On or Off depending on their toggled state. For example, if the Ace of Clubs is shown in the Clubs target card piles, TalkBack will say either "OFF, Ace of Clubs, Switch" or "ON, Ace of Clubs, Switch".  

When TalkBack encounters a card in one of the dealt card piles, it announces the details of the card. The details always includes the rank and suit of the card if the card is face-up, or the phrase "Face down" is the card is face-down. The details also include the position of the card in the dealt card pile, the count of cards in that pile, and whether the card is selected. For example: "!0 od Diamonds, selected, 3 of 4". TalkBack might also announce which dealt card pile the card is in, as either its "dealt card pile" of "list pile".

TalkBack can be moved directly to an element in the game  by touching the element, or by swiping right or left to have TalkBack move to the next or previous element respectively. When navigating forward by swipe gesture, TalkBack will announce the container of the target card piles as: "Target Card Piles", and the container of the dealt card piles as: "Dealt card piles".

TalkBack also announces details in response to some specific actions that are taken in the game. For example:

- When selecting a card in a dealt card pile: "3 of Clubs, selected, 3 of 3 in dealt card pile 3".
- When a  card has been moved from a dealt card pile up to a target card pile, and the next card in the pile is turned face-up: "Moved Ace of Hearts, Revealed Jack of Diamonds, 4 of 4 in dealt card pile 5".
- When the Next Card button is tapped and two cards are turned over: "5 of Hearts, 6 of Spades on top".
- When the Next Card button is tapped and the pile becomes empty: “9 of spades, 3 of Diamonds on top. No cards left in remaining cards pile”.
- When the topmost upturned card is selected: “3 of Diamonds selected”.

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_TalkBackMoveBetweenDealtPiles.jpg)

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_TalkBackMoveToTargetPile.jpg)

**Announcing the entire state of the game:** To have the current state of the entire game announced by TalkBack, go to the Settings page and check: “Show state announcement button”. When you then return to the game, a button called: "Screen reader announce game state" will follow the Menu button near the top-left of the game. When that button is tapped, TalkBack will announce the current state of the upturned cards, the target card piles, and the dealt card piles. For example:

"Top upturned card is 6 of Clubs, then 9 of Diamonds, then 4 of Clubs.  More cards are available to turn over. Target piles, 2 of Clubs, 2 of Diamonds, Ace of Hearts, Empty Spades Pile. Pile 1, 10 of Hearts to King of Spades, Pile 2, Empty, Pile 3, 8 of Clubs, 2 Cards Face-down, Pile 4, 10 of Spades to Jack of Diamonds, 2 Cards Face-down, Pile 5, 7 of Clubs, 4 Cards Face-down, Pile 6, 4 of Diamonds, 1 Card Face-down, Pile 7, 5 of Diamonds, 6 Cards Face-down."

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_TalkBackGameState.jpg)

**Please note:** TalkBack sometimes makes announcements that are not helpful and can be confusing. For example, transitioning states of selection for a card being moved before it announces the card's final selection state. Also when a card is being moved and TalkBack announces the a card in a pile and the count of cards in that pile, when that card is not the one of most interest during a move. Over time I hope to reduce the number of unwanted announcements made by TalkBack.  

**Please note:** When TalkBack is not running, a selected card in a dealt card pile can be tapped to deselect it. When TalkBack is running, a selected card does not respond when a TalkBack double-tap gesture is made on the card. To deselect that card, double-tap on another card.

## Colours shown in the game

The colours shown in the game depend on whether the **Android Dark Mode** setting is on when the game is started. 

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_DarkZoom.jpg)

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_DarkShowZoomCard.jpg)

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_DarkPopup.jpg)

In addition, the game has a "Card Appearance" setting which can be used to lower the brightness of cards shown in the game.

![Todo add description](/ReadmeScreenshots/AccessibleSolitaire_DarkenCards.jpg)

## Other Settings

### "Number of cards turned up" and "Empty Dealt Card Pile"

The games has two settings which can influence how challenging it is for the game to be won. One of those settings, "Number of cards turned up", controls how many cards are turned on from the remaining card pile when the Next Card button is tapped. If cards are turned up one at a time, the game is more likely to be won, but if the cards are turned up three at a time, you may find it less common for the game to be won. The other setting, "Empty Dealt Card Pile", controls what cards can be moved to empty dealt card piles. If this is set such that any card can be moved to an empty dealt card pile, the game is more likely to be won, and the game is less likely to be won if only Kings can be moved to an empty dealt card pile.

### "Screen Reader"

As mentioned above, the "Show state announcement button" can be used to have the TalkBack screen reader announce the entire state of the game. If TalkBack is not running, this button appears to do nothing when tapped. 
