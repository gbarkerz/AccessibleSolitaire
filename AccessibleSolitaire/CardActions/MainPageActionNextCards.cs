using Sa11ytaire4All.Source;
using Sa11ytaire4All.Views;

namespace Sa11ytaire4All
{
    public sealed partial class MainPage : ContentPage
    {
        private DateTime timeOfMostRecentNextCardClick = DateTime.Now;

        // Turn over the next card in the Remaining Card pile.
        private void NextCard_Click(object sender, EventArgs e)
        {
            PerformNextCardAction();
        }

        public void PerformNextCardAction()
        { 
            // Barker Todo: Figure out why we get two rapid clicks when invoked by TalkBack.
            var timeSinceMostRecentNextCardClick = DateTime.Now - timeOfMostRecentNextCardClick;
            if (timeSinceMostRecentNextCardClick.TotalMilliseconds < 500)
            {
                return;
            }

            timeOfMostRecentNextCardClick = DateTime.Now;

            ClearAllSelections(true);

            var screenReaderAnnouncement = "";

            var soundFilename = "";

            // Can we turn over at least one card?
            if (_deckRemaining.Count > 0)
            {
                // Yes, so how many cards can we turn over? Never try to turn over more cards
                // than remain in the remaining cards pile.
                var maxCountCardsToTurn = (_deckRemaining.Count >= 3 ? 3 : _deckRemaining.Count);
                
                // Respect the player's selection in the Settings.
                var countCardsToTurn = Math.Min(OptionCardTurnCount, maxCountCardsToTurn);

                // We only ever turn over one card in Pyramid solitaire.
                if (currentGameType == SolitaireGameType.Pyramid)
                {
                    countCardsToTurn = 1;
                }

                soundFilename = (countCardsToTurn > 1 ? "movecards.mp4" : "movecard.mp4");

                // Turn over each card in turn.
                for (int i = 0; i < countCardsToTurn; ++i)
                {
                    Card card = _deckRemaining[_deckRemaining.Count - 1];

                    _deckRemaining.Remove(card);
                    _deckUpturned.Add(card);

                    screenReaderAnnouncement += card.GetCardAccessibleName() + (i < countCardsToTurn - 1 ? ", " : " ");
                }

                screenReaderAnnouncement += MainPage.MyGetString("OnTop") + ". " +
                         (_deckRemaining.Count == 0 ? MainPage.MyGetString("NoCardLeft") + " " : "");
            }
            else
            {
                var someCardsLeft = _deckUpturned.Count > 0;

                if (someCardsLeft)
                {
                    soundFilename = "restack.mp4";
                }

                // There are no cards left to turn over, so move all the upturned cards back to 
                // the Remaining Cards pile.
                while (_deckUpturned.Count > 0)
                {
                    Card card = _deckUpturned[_deckUpturned.Count - 1];

                    _deckUpturned.Remove(card);
                    _deckRemaining.Add(card);
                }

                ClearUpturnedPileButton();

                screenReaderAnnouncement += MainPage.MyGetString(
                                                someCardsLeft ? "AllUpturnedCardsTurnedBack" : "NoNextCardsLeft");
            }

            if (playSoundOther)
            {
                PlaySoundFile(soundFilename);
            }

            SetUpturnedCardsVisuals();

            NextCardDeck.State = GetNextCardPileState();

            MakeDelayedScreenReaderAnnouncement(screenReaderAnnouncement, true);
        }

        private NextCardPileState GetNextCardPileState()
        {
            NextCardPileState state = NextCardPileState.Active;

            if (_deckRemaining.Count == 0)
            {
                state = (CardDeckUpturned.Card == null ?
                            NextCardPileState.Finished :
                            NextCardPileState.Empty);
            }

            return state;
        }

        private DateTime timePreviousClickUpturnedButton = DateTime.Now;

        private void CardDeckUpturned_Clicked(CardButton cardDeckUpturned)
        {
            if ((DateTime.Now - timePreviousClickUpturnedButton).TotalMilliseconds < 500)
            {
                return;
            }

            timePreviousClickUpturnedButton = DateTime.Now;

            if (cardDeckUpturned.Card == null)
            {
                // There is no upturned card in the upturned card pile. So 
                // make sure the empty upturned card is not left selected.
                SetCardButtonToggledSelectionState(CardDeckUpturned, false);

                return;
            }

            if (currentGameType == SolitaireGameType.Pyramid)
            {
                HandleClickOnUpturnedOrHigherObscuredCard(cardDeckUpturned);

                if (GameOver())
                {
                    ShowEndOfGameDialog();
                }
            }
            else
            {
                // Always deselect all dealt cards and the target card piles 
                // when the upturned card is selected.
                ClearAllSelections(false);

                ToggleUpturnedCardSelection(cardDeckUpturned);
            }
        }

        private void ToggleUpturnedCardSelection(CardButton cardDeckUpturned)
        {
            // Change the selection state of the upturned card.
            SetCardButtonToggledSelectionState(cardDeckUpturned, !cardDeckUpturned.IsToggled);

            cardDeckUpturned.RefreshVisuals();

            if (cardDeckUpturned.Card != null)
            {
                string selectionAnnouncement =
                    cardDeckUpturned.Card.GetCardAccessibleName() + " " +
                        MainPage.MyGetString(cardDeckUpturned.IsToggled ? "Selected" : "Unselected");

                MakeDelayedScreenReaderAnnouncement(selectionAnnouncement, false);
            }
        }
    }
}
