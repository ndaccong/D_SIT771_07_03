using static code.DataArrays;

public class Deck
{
    // public Card[] deck = new Card[52];
    public int[] deck = new int[52];
    
    public int[] FillDeck()
    {
        int i, j, n = 0;
        int suit = 0x8000;

        for (i = 0; i < 4; i++, suit >>= 1)
        {
            for (j = 0; j < 13; j++, n++)
            {
                deck[n] = PRIMES[j] | (j << 8) | suit | (1 << (16 + j));
            }
        }

        return deck;
    }

    public static int[] ShuffleDeck(int[] deck)
    {
        // FisherYates algorithm
        Random r = new Random();

        for (int i = deck.Length - 1; i > 0; i--)
        {
            int j = r.Next(i+1);
            int temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }

        return deck;
    }
}