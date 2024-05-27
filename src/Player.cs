using System;
using System.Collections.Generic;
using CSharpItertools;
using CSharpItertools.Interfaces;
using static code.DataArrays;
using static code.PokerData;
using SplashKitSDK;

namespace code
{
    public enum PlayerType
    {
        HUMAN = 0,
        BOT = 1
    }

    public abstract class Player
    {
        public PlayerAction action = PlayerAction.CHECK;
        public int actionAmount = 0;
        public PlayerType type;

        public int SB { get; set; } = 1;
        public int BB { get; set; } = 2;
        public int Stack { get; set; } = 200;

        public int CurrentPot { get; set; } = 0;
        public int OpponentStack { get; set; } = 0;
        public int CurrentStageTableBetAmount { get; set; } = 0;
        public int CurrentStagePlayerBetAmount { get; set; } = 0;
        public int CurrentStageOpponentBetAmount { get; set; } = 0;
        public bool StageHasAction { get; set; } = false;

        public int[] PlayerCards { get; set; } = new int[2];
        public List<int> CommunityCards { get; set; } = new List<int>();

        public PlayerAction inputAction { get; set; }
        public int inputActionAmount { get; set; } = 0;
        public bool HasInputAction { get; set; } = false;

        public int[] deck = new Deck().FillDeck();

        protected int[] RemainingCards
        { 
            get
            {
                List<int> knownCards = this.PlayerCards.ToList();
                knownCards.AddRange(this.CommunityCards.ToList());

                List<int> deckList = deck.ToList();

                int[] remainingCards = deckList.ToArray().Except(knownCards.ToArray()).ToArray();

                return remainingCards;
            }
        }

        public Player(PlayerType type, int sb, int bb, int stack)
        {
            this.type = type;
            this.SB = sb;
            this.BB = bb;
            this.Stack = stack;
        }

        public void ResetStage()
        {
            this.CurrentStageTableBetAmount = 0;
            this.CurrentStagePlayerBetAmount = 0;
            this.CurrentStageOpponentBetAmount = 0;
            this.StageHasAction = false;
            this.HasInputAction = false;
        }

        public void Fold()
        {
            this.action = PlayerAction.FOLD;
            this.actionAmount = 0;
            this.StageHasAction = true;
        }

        public void Check()
        {
            this.action = PlayerAction.CHECK;
            this.actionAmount = 0;
            this.StageHasAction = true;
        }

        public void Call()
        {
            int amountToCall = this.CurrentStageTableBetAmount - this.CurrentStagePlayerBetAmount;

            this.action = PlayerAction.CALL;
            this.actionAmount = this.CurrentStageTableBetAmount;
            this.Stack -= amountToCall;
            this.StageHasAction = true;
        }

        public void Bet(int amount)
        {
            this.action = PlayerAction.BET;
            this.actionAmount = amount;
            this.Stack -= amount;
            this.StageHasAction = true;
        }
        
        public void Raise(int amount)
        {
            int amountToRaise = amount - this.CurrentStagePlayerBetAmount;

            this.action = PlayerAction.RAISE;
            this.actionAmount = amount;
            this.Stack -= amountToRaise;
            this.StageHasAction = true;
        }

        public PlayerAction[] ActionList
        {
            get
            {
                List<PlayerAction> actionList = new List<PlayerAction>();
                actionList.Add(PlayerAction.FOLD);

                if (this.CurrentStageTableBetAmount == 0)
                {
                    actionList.Add(PlayerAction.CHECK);
                    if (this.Stack >= this.SB)
                    {
                        actionList.Add(PlayerAction.BET);
                    }
                } 
                else
                {
                    actionList.Add(PlayerAction.CALL);
                    if (this.Stack + this.CurrentStagePlayerBetAmount >= this.CurrentStageTableBetAmount)
                    {
                        actionList.Add(PlayerAction.RAISE);
                    }
                }

                return actionList.ToArray();
            }
        }
        
        public virtual void HandleInput() {}
        public abstract void MakeAction();
    }
    
    public class HumanPlayer : Player
    {
        private PlayerType playerType = PlayerType.HUMAN;

        public HumanPlayer(PlayerType playerType, int sb, int bb, int stack) : base(playerType, sb, bb, stack) {}

        // public override async Task HandleInput()
        public override void HandleInput()
        {
            Point2D clickPoint = SplashKit.MousePosition();
            if (this.ActionList.Contains(PlayerAction.FOLD) && clickPoint.X > 649 && clickPoint.X < 649 + 123 && clickPoint.Y > 622 && clickPoint.Y < 622 + 73)
            {
                this.inputAction = PlayerAction.FOLD;
                this.inputActionAmount = 0;
                this.HasInputAction = true;
            }

            if (this.ActionList.Contains(PlayerAction.CHECK) && clickPoint.X > 792 && clickPoint.X < 792 + 123 && clickPoint.Y > 622 && clickPoint.Y < 622 + 73)
            {
                this.inputAction = PlayerAction.CHECK;
                this.inputActionAmount = 0;
                this.HasInputAction = true;
            }

            if (this.ActionList.Contains(PlayerAction.CALL) && clickPoint.X > 792 && clickPoint.X < 792 + 123 && clickPoint.Y > 622 && clickPoint.Y < 622 + 73)
            {
                this.inputAction = PlayerAction.CALL;
                this.HasInputAction = true;
            }

            if (this.ActionList.Contains(PlayerAction.BET) && clickPoint.X > 935 && clickPoint.X < 935 + 123 && clickPoint.Y > 622 && clickPoint.Y < 622 + 73)
            {
                this.inputAction = PlayerAction.BET;
                this.HasInputAction = true;
            }

            if (this.ActionList.Contains(PlayerAction.RAISE) && clickPoint.X > 935 && clickPoint.X < 935 + 123 && clickPoint.Y > 622 && clickPoint.Y < 622 + 73)
            {
                this.inputAction = PlayerAction.RAISE;
                this.HasInputAction = true;
            }

            if ((this.ActionList.Contains(PlayerAction.BET) || this.ActionList.Contains(PlayerAction.RAISE)) && clickPoint.X > 967 && clickPoint.X < 967 + 59 && clickPoint.Y > 492 && clickPoint.Y < 492 + 55)
            {
                this.inputActionAmount += 1;
            }

            if ((this.ActionList.Contains(PlayerAction.BET) || this.ActionList.Contains(PlayerAction.RAISE)) && clickPoint.X > 967 && clickPoint.X < 967 + 59 && clickPoint.Y > 552 && clickPoint.Y < 552 + 55)
            {
                this.inputActionAmount -= 1;
            }
        }

        public override void MakeAction()
        {
            string actionToShow = String.Empty;
            foreach (PlayerAction action in this.ActionList)
            {
                actionToShow += action.ToString() + " - " + (int)action + " ";
            }

            Console.WriteLine($"Choose action: {actionToShow}");

            if (this.HasInputAction)
            {
                switch (this.inputAction)
                {
                    case PlayerAction.FOLD:
                        this.Fold();
                        break;
                    case PlayerAction.CHECK:
                        this.Check();
                        break;
                    case PlayerAction.CALL:
                        this.Call();
                        break;
                    case PlayerAction.BET:
                        // int inputBetAmount = int.Parse(Console.ReadLine());
                        this.Bet(this.inputActionAmount);
                        break;
                    case PlayerAction.RAISE:
                        // int inputRaiseAmount = int.Parse(Console.ReadLine());
                        this.Raise(this.inputActionAmount);
                        break;
                }
            }
        }
    }

    public class BotPlayer : Player
    {
        private PlayerType playerType = PlayerType.BOT;
        private const double PLAYABLE_HAND = 0.25f;
        private const double ALLOWABLE_DIFF = 0.5f;
        private const double GREEDY_MULTIPLIER = 1.0f;
        private const double BET_ACTIVATE_MULTIPLIER = 2.5f;
        private const double RAISE_ACTIVATE_MULTIPLIER = 6f;

        private int Wins { get; set; } = 0;
        private int Draws { get; set; } = 0;
        private int Losses { get; set; } = 0;

        private readonly IItertools itertools = new Itertools();

        public BotPlayer(PlayerType playerType, int sb, int bb, int stack) : base(playerType, sb, bb, stack) {}

        private void GetProbs()
        {
            int numCommunityCardsLeft = 5 - this.CommunityCards.Count;

            this.Wins = 0;
            this.Draws = 0;
            this.Losses = 0;

            IEnumerable<IEnumerable<int>> opponentCardsList = itertools.Combinations(this.RemainingCards, 2);

            if (numCommunityCardsLeft == 5)
            {
                opponentCardsList = opponentCardsList.Shuffle().Take(30);
            }

            foreach (var opponentCards in opponentCardsList)
            {
                
                var watch = System.Diagnostics.Stopwatch.StartNew();
                
                List<int> knownCards = this.PlayerCards.ToList();
                knownCards.AddRange(this.CommunityCards.ToList());
                knownCards.AddRange(opponentCards);

                int[] remainingCards = deck.Except(knownCards.ToArray()).ToArray();

                if (numCommunityCardsLeft > 0)
                {
                    var possibleRemainingCommnunityCards = itertools.Combinations(remainingCards, numCommunityCardsLeft);
                    

                    if (numCommunityCardsLeft == 5)
                    {
                        possibleRemainingCommnunityCards = possibleRemainingCommnunityCards.Shuffle().Take(1000);
                    }

                    
                    foreach (IEnumerable<int> remainingCommnunityCards in possibleRemainingCommnunityCards)
                    {
                        List<int> playerHand = this.PlayerCards.ToList();
                        List<int> opponentHand = opponentCards.ToList();
                        
                        playerHand.AddRange(this.CommunityCards);
                        playerHand.AddRange(remainingCommnunityCards);
                        opponentHand.AddRange(this.CommunityCards);
                        opponentHand.AddRange(remainingCommnunityCards);

                        int compareResult = Evaluator.CompareHand(playerHand.ToArray(), opponentHand.ToArray());
                        switch (compareResult)
                        {
                            case 1:
                                this.Wins++;
                                break;
                            case -1:
                                this.Losses++;
                                break;
                            case 0:
                                this.Draws++;
                                break;
                        }
                    }

                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Console.WriteLine($"Run time: {elapsedMs} ms.");
                }
                else
                {
                    List<int> playerHand = this.PlayerCards.ToList();
                    List<int> opponentHand = opponentCards.ToList();
                    playerHand.AddRange(this.CommunityCards);
                    opponentHand.AddRange(this.CommunityCards);

                    int compareResult = Evaluator.CompareHand(playerHand.ToArray(), opponentHand.ToArray());
                    switch (compareResult)
                    {
                        case 1:
                            this.Wins++;
                            break;
                        case 0:
                            this.Draws++;
                            break;
                        case -1:
                            this.Losses++;
                            break;
                    }
                }
            }
        }

        public double CalculateEV(int pot, int table_bet_amount, int player_bet_amount)
        {
            this.GetProbs();


            double wp = Convert.ToDouble(this.Wins) / Convert.ToDouble(this.Wins + this.Draws + this.Losses) * (1 - PLAYABLE_HAND);
            double dp = Convert.ToDouble(this.Draws) / Convert.ToDouble(this.Wins + this.Draws + this.Losses);
            double lp = Convert.ToDouble(this.Losses) / Convert.ToDouble(this.Wins + this.Draws + this.Losses);

            double ev = wp * Convert.ToDouble(pot) + dp * Convert.ToDouble(pot + table_bet_amount - player_bet_amount) / 2 - lp * Convert.ToDouble(table_bet_amount - player_bet_amount);

            Console.WriteLine($"Wins: {this.Wins} - Draws: {this.Draws} - Losses: {this.Losses}");
            Console.WriteLine($"POT: {pot}, table_bet_amount: {table_bet_amount}, wp: {wp}, dp: {dp}, lp: {lp}, * {wp * pot}");
            Console.WriteLine($"EV: {ev}");

            return ev;
        }
        
        public override void MakeAction()
        {
            double ev = CalculateEV(this.CurrentPot, this.CurrentStageTableBetAmount, this.CurrentStagePlayerBetAmount);

            switch (this.ActionList)
            {
                case [PlayerAction.FOLD, PlayerAction.CHECK, PlayerAction.BET]:
                {
                    int betAmount = (int)Math.Round(ev / 2);
                    if (betAmount > BET_ACTIVATE_MULTIPLIER * this.SB)
                    {
                        this.action = PlayerAction.BET;
                        this.actionAmount = betAmount;
                    }
                    else
                    {
                        this.action = PlayerAction.CHECK;
                        this.actionAmount = 0;
                    }
                    break;
                }
                case [PlayerAction.FOLD, PlayerAction.CHECK]:
                {
                    this.action = PlayerAction.CHECK;
                    this.actionAmount = 0;
                    break;
                }
                case [PlayerAction.FOLD, PlayerAction.CALL, PlayerAction.RAISE]:
                {
                    int betAmount = (int)Math.Round(ev / 2);
                    if (ev < ALLOWABLE_DIFF * this.SB)
                    {
                        this.action = PlayerAction.FOLD;
                        this.actionAmount = 0;
                    }
                    else if (betAmount > RAISE_ACTIVATE_MULTIPLIER * this.SB)
                    {
                        this.action = PlayerAction.RAISE;
                        this.actionAmount = Math.Min(Math.Max(this.CurrentStageTableBetAmount + betAmount, this.CurrentStageTableBetAmount * 2), this.Stack + this.CurrentStagePlayerBetAmount);
                    }
                    else
                    {
                        this.action = PlayerAction.CALL;
                        this.actionAmount = this.CurrentStageTableBetAmount;
                    }
                    break;
                }
                case [PlayerAction.FOLD, PlayerAction.CALL]:
                {
                    if (ev < - ALLOWABLE_DIFF * this.SB)
                    {
                        this.action = PlayerAction.FOLD;
                        this.actionAmount = 0;
                    }
                    else
                    {
                        this.action = PlayerAction.CALL;
                        this.actionAmount = this.CurrentStageTableBetAmount;
                    }
                    break;
                }
            }

            switch (this.action)
            {
                case PlayerAction.FOLD:
                    this.Fold();
                    break;
                case PlayerAction.CHECK:
                    this.Check();
                    break;
                case PlayerAction.CALL:
                    this.Call();
                    break;
                case PlayerAction.BET:
                    this.Bet(this.actionAmount);
                    break;
                case PlayerAction.RAISE:
                    this.Raise(this.actionAmount);
                    break;
            }
        }
    }
}
