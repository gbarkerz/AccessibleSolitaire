using Sa11ytaire4All.Source;

namespace Sa11ytaire4All
{
    public partial class MainPage : ContentPage
    {
        private void PrepareCardsForRoyalParade()
        {
            var countAcesFound = 0;

            // Don't use Aces index here, as the index will change as the Aces are removed from the collection.
            var cardAces = new Card[4];

            var index2 = -1;
            var index3 = -1;
            var index4 = -1;

            // Find all the Aces, and a 2, a 3 and a 4.
            for (int i = _deckRemaining.Count - 1; i >= 0; --i)
            {
                if (_deckRemaining[i] != null)
                { 
                    if ((countAcesFound < 4) &&
                        (_deckRemaining[i].Rank == 1))
                    {
                        cardAces[countAcesFound] = _deckRemaining[i];

                        ++countAcesFound;
                    }
                    else if ((index2 == -1) && (_deckRemaining[i].Rank == 2))
                    {
                        index2 = i;
                    }
                    else if ((index3 == -1) && (_deckRemaining[i].Rank == 3))
                    {
                        index3 = i;
                    }
                    else if ((index4 == -1) && (_deckRemaining[i].Rank == 4))
                    {
                        index4 = i;
                    }

                    if ((countAcesFound == 4) &&
                        (index2 != -1) &&
                        (index3 != -1) &&
                        (index4 != -1))
                    {
                        break;
                    }
                }
            }

            // Now swap a 2, a 3, and a 4, such that they lie at the start of the
            // first, second, and third rows respectively. Note that the way the
            // code works here, we'll never have a 2, 3, or 4 at the start of any 
            // of the rows. The can be changed if feedback prompts it.

            if (_deckRemaining[0].Rank != 2)
            {
                SwapCard(0, index2);
            }

            if (_deckRemaining[8].Rank != 3)
            {
                SwapCard(8, index3);
            }

            if (_deckRemaining[16].Rank != 4)
            {
                SwapCard(16, index4);
            }

            // Next remove the Aces.
            for (int i = 3; i >= 0; --i)
            {
                _deckRemaining.Remove(cardAces[i]);
            }
        }

        private void SwapCard(int indexCardFirst, int indexCardSecond)
        {
            Card tempCard = new Card();
            tempCard.Suit = _deckRemaining[indexCardFirst].Suit;
            tempCard.Rank = _deckRemaining[indexCardFirst].Rank;

            _deckRemaining[indexCardFirst].Suit = _deckRemaining[indexCardSecond].Suit;
            _deckRemaining[indexCardFirst].Rank = _deckRemaining[indexCardSecond].Rank;

            _deckRemaining[indexCardSecond].Suit = tempCard.Suit;
            _deckRemaining[indexCardSecond].Rank = tempCard.Rank;
        }
    }
}
