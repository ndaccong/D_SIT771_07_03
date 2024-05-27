using SplashKitSDK;

public class Card
{
    public int CardInt { get; set; }
    public readonly char[] ranks = new char[] {'2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A'};

    public double X { get; set; }
    public double Y { get; set; }
    
    private Bitmap cardFront = new Bitmap("cardFront", "cardFront.png");
    private Bitmap cardBack = new Bitmap("cardBack", "cardBack.png");
    private Bitmap cardSuitClub = new Bitmap("suitClub", "suitClub.png");
    private Bitmap cardSuitDiamond = new Bitmap("suitDiamond", "suitDiamond.png");
    private Bitmap cardSuitHeart = new Bitmap("suitHeart", "suitHeart.png");
    private Bitmap cardSuitSpade = new Bitmap("suitSpade", "suitSpade.png");

    public double Width { 
        get
        {
            return this.cardFront.Width;
        }
    }
    public double Height { 
        get
        {
            return this.cardFront.Height;
        }
    }
    
    public bool IsRevealed { get; set; } = true;

    public char Rank
    {
        get
        {
            return ranks[(this.CardInt >> 8) & 0xF];
        }
    }

    public char Suit
    {
        get
        {
            char suit;

            if ((this.CardInt & 0x8000) == 0x8000)
            {
                suit = 'c';
            }
            else if ((this.CardInt & 0x4000) == 0x4000)
            {
                suit = 'd';
            }
            else if ((this.CardInt & 0x2000) == 0x2000)
            {
                suit = 'h';
            }
            else
            {
                suit = 's';
            }

            return suit;
        }
    }

    private string suitName;

    public string Name
    {
        get
        {  
            switch (this.Suit)
            {
                case 'c':
                    suitName = "Clubs";
                    break;
                case 'd':
                    suitName = "Diamonds";
                    break;
                case 'h':
                    suitName = "Hearts";
                    break;
                case's':
                    suitName = "Spades";
                    break;
            }

            return this.Rank + " of  " + suitName;
        }
    }

    public Card(int card_int)
    {
        this.CardInt = card_int;
    }

    public void Draw()
    {
        SplashKit.LoadFont("Arial", "arial.ttf");

        if (this.IsRevealed)
        {
            cardFront.Draw(X, Y);

            Color cardColor = new Color();

            switch (this.Suit)
            {
                case 'c':
                {
                    cardColor = Color.Black;
                    SplashKit.DrawBitmap(cardSuitClub, this.X + this.Width / 2 - cardSuitClub.Width / 2, this.Y + this.Height / 2 - 2);
                    break;
                }
                case 'd':
                {
                    cardColor = Color.DarkRed;
                    SplashKit.DrawBitmap(cardSuitDiamond, this.X + this.Width / 2 - cardSuitDiamond.Width / 2, this.Y + this.Height / 2 - 2);
                    break;
                }
                case 'h':
                {
                    cardColor = Color.DarkRed;
                    SplashKit.DrawBitmap(cardSuitHeart, this.X + this.Width / 2 - cardSuitHeart.Width / 2, this.Y + this.Height / 2 - 2);
                    break;
                }
                case 's':
                {
                    cardColor = Color.Black;
                    SplashKit.DrawBitmap(cardSuitSpade, this.X + this.Width / 2 - cardSuitSpade.Width / 2, this.Y + this.Height / 2 - 2);
                    break;
                }
            }

            SplashKit.DrawText(this.Rank.ToString(), cardColor, "Arial", 24, this.X + this.Width / 2 - 7, this.Y + this.Height / 2 - 26);
        }
        else
        {
            cardBack.Draw(X, Y);
        }
    }
}