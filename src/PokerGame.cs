using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SplashKitSDK;
using static code.PokerData;
using CSharpItertools.Interfaces;

namespace code
{
    public class PokerGame
    {
        public const int STACK = 200;
        public const int SB = 1;
        public const int BB = 2;

        public Player[] Players = [new HumanPlayer(PlayerType.HUMAN, SB, BB, STACK), new BotPlayer(PlayerType.BOT, SB, BB, STACK)];

        public int[] deck = new Deck().FillDeck();
        public int GameWinner { get; set; } = -1;

        public int CurrentDealer { get; set; } = 0;
        public int CurrentPlayer { get; set; } = 0;
        public RoundStage CurrentStage { get; set; } = RoundStage.PREFLOP;
        public List<int> CommunityCards { get; set; } = new List<int>();
        public int CurrentPot { get; set; } = 0;
        public int RoundWinner { get; set; } = -1;

        public bool StageHasAction { get; set; } = false;
        public int CurrentBet { get; set; } = 0;

        public PlayerAction LastAction { get; set; } = PlayerAction.CHECK;

        private Window window;

        private Bitmap tableBitmap;
        private Bitmap humanPlayerBitmap;
        private Bitmap botPlayerBitmap;

        private Bitmap foldButtonBitmap;
        private Bitmap callButtonBitmap;
        private Bitmap raiseButtonBitmap;
        private Bitmap checkButtonBitmap;
        private Bitmap betButtonBitmap;
        private Bitmap increaseButtonBitmap;
        private Bitmap decreaseButtonBitmap;

        public PokerGame(Window window)
        {
            this.window = window;

            tableBitmap = new Bitmap("table", "table.png");
            humanPlayerBitmap = new Bitmap("humanPlayer", "human_player.png");
            botPlayerBitmap = new Bitmap("botPlayer", "bot_player.png");
            foldButtonBitmap = new Bitmap("foldButton", "fold_button.png");
            callButtonBitmap = new Bitmap("callButton", "call_button.png");
            raiseButtonBitmap = new Bitmap("raiseButton", "raise_button.png");
            checkButtonBitmap = new Bitmap("checkButton", "check_button.png");
            betButtonBitmap = new Bitmap("betButton", "bet_button.png");
            increaseButtonBitmap = new Bitmap("increaseButton", "increase_button.png");
            decreaseButtonBitmap = new Bitmap("decreaseButton", "decrease_button.png");
        }

        public void ResetRound()
        {
            Console.WriteLine("\nNEW ROUND!");
            Deck.ShuffleDeck(this.deck);
            this.ResetPlayerStage();
            this.CurrentDealer ^= 1;
            Console.WriteLine($"This round's dealer is {this.Players[this.CurrentDealer].type}");
            Console.WriteLine($"Deck: {new Card(this.deck[0]).Rank}");
            this.CurrentPlayer = this.CurrentDealer ^ 1;
            this.CurrentStage = RoundStage.PREFLOP;
            this.CommunityCards.Clear();
            this.CurrentPot = 0;
            this.RoundWinner = -1;
            this.CurrentBet = 0;
            this.LastAction = PlayerAction.CHECK;

            this.Players[0].ResetStage();
            this.Players[1].ResetStage();
            this.StageHasAction = false;

            this.DealCards();

            // Bet SB and BB
            this.Players[this.CurrentPlayer].Bet(SB);
            this.DoAction(PlayerAction.BET, SB);
            this.SwitchTurn();
            this.Players[this.CurrentPlayer].Raise(BB);
            this.DoAction(PlayerAction.RAISE, BB);
            this.SwitchTurn();
        }

        public void NextStage()
        {
            this.ResetPlayerStage();
            this.CurrentBet = 0;
            this.CurrentStage = this.CurrentStage.Next();

            this.CurrentPlayer = this.CurrentDealer ^ 1;
            this.StageHasAction = false;
        }

        public bool IsGameEnd
        {
            get
            {
                if (this.Players[0].Stack == 0)
                {
                    this.GameWinner = 1;
                    return true;
                }
                else if (this.Players[1].Stack == 0)
                {
                    this.GameWinner = 0;
                    return true;
                }

                return false;
            }
        }

        public bool IsRoundEnd
        {
            get
            {
                if (this.LastAction == PlayerAction.FOLD)
                {
                    return true;
                }
                else if (this.CurrentStage == RoundStage.SHOWDOWN)
                {
                    return true;
                }

                return false;
            }

            set
            {
                this.IsRoundEnd = value;
            }
        }

        public bool IsStageEnd
        {
            get
            {
                if (!this.StageHasAction)
                {
                    return false;
                }
                else if (this.LastAction == PlayerAction.FOLD)
                {
                    return true;
                }
                else if (this.Players[0].StageHasAction && this.Players[1].StageHasAction && this.Players[0].CurrentStagePlayerBetAmount == this.Players[1].CurrentStagePlayerBetAmount)
                {
                    return true;
                }

                return false;
            }
        }

        public void ResetPlayerStage()
        {
            this.Players[0].ResetStage();
            this.Players[1].ResetStage();
            this.PlayerUpdateOpponentInfo();
        }

        public void PlayerUpdateOpponentInfo()
        {
            this.Players[0].OpponentStack = this.Players[1].Stack;
            this.Players[0].CurrentStageOpponentBetAmount = this.Players[1].CurrentStagePlayerBetAmount;
            this.Players[0].CurrentPot = this.CurrentPot;
            this.Players[1].OpponentStack = this.Players[0].Stack;
            this.Players[1].CurrentStageOpponentBetAmount = this.Players[0].CurrentStagePlayerBetAmount;
            this.Players[1].CurrentPot = this.CurrentPot;            
        }

        public void PlayerGetCommunityCards()
        {
            this.Players[0].CommunityCards = this.CommunityCards;
            this.Players[1].CommunityCards = this.CommunityCards;
        }

        public void DealCards()
        {
            switch (this.CurrentStage)
            {
                case RoundStage.PREFLOP:
                {
                    this.Players[this.CurrentDealer].PlayerCards[0] = this.deck[0];
                    this.Players[this.CurrentDealer ^ 1].PlayerCards[0] = this.deck[1];
                    this.Players[this.CurrentDealer].PlayerCards[1] = this.deck[2];
                    this.Players[this.CurrentDealer ^ 1].PlayerCards[1] = this.deck[3];

                    Console.WriteLine($"Dealt: {new Card(this.Players[0].PlayerCards[0]).Rank}");
                    Console.WriteLine($"Dealt: {new Card(this.Players[0].PlayerCards[1]).Rank}");
                    break;
                }
                case RoundStage.FLOP:
                {
                    this.CommunityCards.Add(this.deck[4]);
                    this.CommunityCards.Add(this.deck[5]);
                    this.CommunityCards.Add(this.deck[6]);
                    this.PlayerGetCommunityCards();
                    break;
                }
                case RoundStage.TURN:
                {
                    this.CommunityCards.Add(this.deck[7]);
                    this.PlayerGetCommunityCards();
                    break;
                }
                case RoundStage.RIVER:
                {
                    this.CommunityCards.Add(this.deck[8]);
                    this.PlayerGetCommunityCards();
                    break;
                }
                case RoundStage.SHOWDOWN:
                    break;
            }

            Console.WriteLine("\nDone dealing cards!");
        }

        public void SwitchTurn()
        {
            this.CurrentPlayer = this.CurrentPlayer ^ 1;
            this.Players[0].HasInputAction = false;
            Console.WriteLine($"\n{this.Players[this.CurrentPlayer].type}'s turn...");
        }

        public void DetermineRoundWinner()
        {
            if (this.LastAction == PlayerAction.FOLD)
            {
                this.RoundWinner = this.CurrentPlayer ^ 1;
            }
            else
            {
                List<int> humanHand = this.Players[0].PlayerCards.ToList();
                List<int> botHand = this.Players[1].PlayerCards.ToList();
                        
                humanHand.AddRange(this.CommunityCards);
                botHand.AddRange(this.CommunityCards);

                int compareResult = Evaluator.CompareHand(humanHand.ToArray(), botHand.ToArray());
                switch (compareResult)
                {
                    case 1:
                        this.RoundWinner = 0;
                        break;
                    case 0:
                        this.RoundWinner = -1;
                        break;
                    case -1:
                        this.RoundWinner = 1;
                        break;
                }
            }
        }

        public void PayWinner()
        {
            if (this.RoundWinner == -1)
            {
                this.Players[0].Stack += this.CurrentPot / 2;
                this.Players[1].Stack += this.CurrentPot / 2;
                Console.WriteLine("Draw");
                SplashKit.DrawText("Round result: DRAW", Color.Black, window.Width / 2 - 150, window.Height / 2 - 80);
                window.Refresh(60);
                SplashKit.Delay(5000);
            }
            else
            {
                this.Players[this.RoundWinner].Stack += this.CurrentPot;
                Console.WriteLine($"{this.Players[this.RoundWinner].type} has won this round!");
                SplashKit.DrawText($"{this.Players[this.RoundWinner].type} has won this round! POT: {this.CurrentPot}", Color.Black, window.Width / 2 - 150, window.Height / 2 - 80);
                window.Refresh(60);
                SplashKit.Delay(5000);
            }
        }

        public bool DoAction(PlayerAction action, int actionAmount)
        {
            switch (action)
            {
                case PlayerAction.FOLD:
                {
                    Console.WriteLine($"{this.Players[this.CurrentPlayer].type} has folded.");
                    this.StageHasAction = true;
                    this.LastAction = PlayerAction.FOLD;
                    return true;
                }
                case PlayerAction.CHECK:
                {
                    Console.WriteLine($"{this.Players[this.CurrentPlayer].type} has checked.");
                    break;
                }
                case PlayerAction.CALL:
                {
                    Console.WriteLine($"{this.Players[this.CurrentPlayer].type} has called.");
                    this.CurrentPot += this.CurrentBet - this.Players[this.CurrentPlayer].CurrentStagePlayerBetAmount;
                    break;
                }
                case PlayerAction.BET:
                {
                    Console.WriteLine($"{this.Players[this.CurrentPlayer].type} has bet {actionAmount}.");
                    this.CurrentPot += actionAmount;
                    this.CurrentBet = actionAmount;
                    this.Players[0].CurrentStageTableBetAmount = this.CurrentBet;
                    this.Players[1].CurrentStageTableBetAmount = this.CurrentBet;
                    break;
                }
                case PlayerAction.RAISE:
                {
                    Console.WriteLine($"{this.Players[this.CurrentPlayer].type} has raised to {actionAmount}.");
                    this.CurrentPot += actionAmount - this.Players[this.CurrentPlayer].CurrentStagePlayerBetAmount;
                    this.CurrentBet = actionAmount;
                    this.Players[0].CurrentStageTableBetAmount = this.CurrentBet;
                    this.Players[1].CurrentStageTableBetAmount = this.CurrentBet;
                    break;
                }
            }

            this.Players[this.CurrentPlayer].CurrentStagePlayerBetAmount = this.CurrentBet;
            this.PlayerUpdateOpponentInfo();
            this.StageHasAction = true;
            this.LastAction = action;

            return true;
        }

        // public void RunGame()
        // {
        //     SplashKit.ProcessEvents();
        //     this.ResetRound();
        //     this.Draw();
        //     while (!this.IsRoundEnd)
        //     {
        //         SplashKit.ProcessEvents();
        //         this.DealCards();
        //         // Console.WriteLine("\nDone dealing cards");
        //         this.Draw();
        //         while (!this.IsStageEnd)
        //         {
        //             SplashKit.ProcessEvents();
        //             this.Players[this.CurrentPlayer].MakeAction();
        //             this.DoAction(this.Players[this.CurrentPlayer].action, this.Players[this.CurrentPlayer].actionAmount);
        //             if (this.LastAction != PlayerAction.FOLD && this.LastAction != PlayerAction.CALL)
        //             {
        //                 this.SwitchTurn();
        //             }
        //             this.Draw();
        //         }
        //         if (this.LastAction != PlayerAction.FOLD)
        //         {
        //             Console.WriteLine($"\nEND OF {this.CurrentStage}");
        //             this.NextStage();
        //             this.Draw();
        //         }
        //     }

        //     this.DetermineRoundWinner();
        //     this.PayWinner();
        //     this.Draw();

        //     Console.ReadLine();
        // }

        public bool IsFirstRound { get; set; } = true;

        public void Update()
        {
            if (this.IsFirstRound)
            {
                this.ResetRound();
                this.IsFirstRound = false;
            }

            if (this.IsRoundEnd)
            {
                this.DetermineRoundWinner();
                this.PayWinner();

                if (this.IsGameEnd)
                {
                    SplashKit.DrawText($"{this.GameWinner} IS THE WINNER!", Color.Black, "Arial", 40, window.Width / 2 - 100, window.Height / 2 - 80);
                    window.Refresh(60);
                    SplashKit.Delay(10000);
                }
                else
                {
                    this.ResetRound();
                }
            }
            else
            {
                if (this.IsStageEnd)
                {
                    if (this.LastAction == PlayerAction.FOLD)
                    {
                        this.IsRoundEnd = true;
                    }
                    else
                    {
                        Console.WriteLine($"\nEND OF {this.CurrentStage}");
                        this.NextStage();
                        this.DealCards();
                    }
                }
                else
                {
                    if (this.CurrentPlayer == 0)
                    {
                        if (!this.Players[0].HasInputAction) {}
                        else
                        {
                            this.Players[this.CurrentPlayer].MakeAction();
                            this.DoAction(this.Players[this.CurrentPlayer].action, this.Players[this.CurrentPlayer].actionAmount);
                            if (this.IsStageEnd) {}
                            else
                            {
                                this.SwitchTurn();
                            }
                        }
                    }
                    else
                    {
                        this.Players[this.CurrentPlayer].MakeAction();
                        this.DoAction(this.Players[this.CurrentPlayer].action, this.Players[this.CurrentPlayer].actionAmount);
                        if (this.IsStageEnd) {}
                        else
                        {
                            this.SwitchTurn();
                        }
                    }
                }
            }
        }

        public void HandleInput()
        {
            if (SplashKit.MouseClicked(MouseButton.LeftButton))
            {
                this.Players[0].HandleInput();
            }
        }

        public void Draw()
        {
            window.Clear(Color.White);
            
            // Draw table and players
            tableBitmap.Draw(window.Width / 2 - tableBitmap.Width / 2, window.Height / 2 - tableBitmap.Height / 2);

            humanPlayerBitmap.Draw(window.Width / 2 - humanPlayerBitmap.Width / 2, window.Height / 2 + tableBitmap.Height / 2 - 10);
            botPlayerBitmap.Draw(window.Width / 2 - botPlayerBitmap.Width / 2, window.Height / 2 - tableBitmap.Height / 2 - botPlayerBitmap.Height - 3);

            SplashKit.DrawText("STACK: " + this.Players[0].Stack, Color.Black, 400, 700);
            SplashKit.DrawText("STACK: " + this.Players[1].Stack, Color.Black, 400, 94);

            if (this.CurrentDealer == 0)
            {
                SplashKit.DrawText("DEALER", Color.Black, 400, 616);
            }
            else if (this.CurrentDealer == 1)
            {
                SplashKit.DrawText("DEALER", Color.Black, 400, 10);
            }

            this.DrawHumanPlayerActionList();
            
            SplashKit.DrawText("STACK: " + this.Players[0].Stack, Color.Black, 400, 700);
            SplashKit.DrawText("STACK: " + this.Players[1].Stack, Color.Black, 400, 94);

            SplashKit.DrawText("POT: " + this.CurrentPot, Color.Black, window.Width / 2 - 20, window.Height / 2 - 100);

            this.DrawPlayerLastAction();
            this.DrawPlayerHand();
            this.DrawCommunityCards();

            window.Refresh(60);
        }

        public void DrawHumanPlayerActionList()
        {
            if (this.CurrentPlayer == 0)
            {
                if (this.Players[0].ActionList.Contains(PlayerAction.FOLD))
                {
                    foldButtonBitmap.Draw(window.Width / 2 + humanPlayerBitmap.Width / 2 + 40, window.Height / 2 + tableBitmap.Height / 2 + 20);
                }

                if (this.Players[0].ActionList.Contains(PlayerAction.CHECK))
                {
                    checkButtonBitmap.Draw(window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + 20, window.Height / 2 + tableBitmap.Height / 2 + 20);
                }
                else if (this.Players[0].ActionList.Contains(PlayerAction.CALL))
                {
                    callButtonBitmap.Draw(window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + 20, window.Height / 2 + tableBitmap.Height / 2 + 20);
                }

                if (this.Players[0].ActionList.Contains(PlayerAction.BET))
                {
                    betButtonBitmap.Draw(window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40, window.Height / 2 + tableBitmap.Height / 2 + 20);
                    increaseButtonBitmap.Draw(window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40 + betButtonBitmap.Width / 2 - increaseButtonBitmap.Width / 2, window.Height / 2 + tableBitmap.Height / 2 + 20 - 130);
                    // Console.WriteLine($"Button X = {window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40 + betButtonBitmap.Width / 2 - increaseButtonBitmap.Width / 2}, Y = {window.Height / 2 + tableBitmap.Height / 2 + 20 - 130}, Width = {increaseButtonBitmap.Width}, Height = {increaseButtonBitmap.Height}");
                    SplashKit.DrawText(this.Players[0].inputActionAmount.ToString(), Color.Black, "Arial", 24, window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40 + betButtonBitmap.Width / 2 - 5, window.Height / 2 + tableBitmap.Height / 2 + 20 - 90);
                    decreaseButtonBitmap.Draw(window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40 + betButtonBitmap.Width / 2 - decreaseButtonBitmap.Width / 2, window.Height / 2 + tableBitmap.Height / 2 + 20 - 70);
                    // Console.WriteLine($"Button X = {window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40 + betButtonBitmap.Width / 2 - increaseButtonBitmap.Width / 2}, Y = {window.Height / 2 + tableBitmap.Height / 2 + 20 - 70}, Width = {increaseButtonBitmap.Width}, Height = {increaseButtonBitmap.Height}");
                }
                else if (this.Players[0].ActionList.Contains(PlayerAction.RAISE))
                {
                    raiseButtonBitmap.Draw(window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40, window.Height / 2 + tableBitmap.Height / 2 + 20);
                    increaseButtonBitmap.Draw(window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40 + betButtonBitmap.Width / 2 - increaseButtonBitmap.Width / 2, window.Height / 2 + tableBitmap.Height / 2 + 20 - 130);
                    SplashKit.DrawText(this.Players[0].inputActionAmount.ToString(), Color.Black, "Arial", 24, window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40 + betButtonBitmap.Width / 2 - 5, window.Height / 2 + tableBitmap.Height / 2 + 20 - 90);
                    decreaseButtonBitmap.Draw(window.Width / 2 + humanPlayerBitmap.Width / 2 + 40 + foldButtonBitmap.Width + callButtonBitmap.Width + 40 + betButtonBitmap.Width / 2 - decreaseButtonBitmap.Width / 2, window.Height / 2 + tableBitmap.Height / 2 + 20 - 70);
                }
            }
        }

        public void DrawPlayerLastAction()
        {
            if (this.Players[0].StageHasAction)
            {
                SplashKit.DrawText($"{this.Players[0].action}: {this.Players[0].actionAmount}", Color.Black, 350, 550);
            }

            if (this.Players[1].StageHasAction)
            {
                SplashKit.DrawText($"{this.Players[1].action}: {this.Players[1].actionAmount}", Color.Black, 350, 200);
            }
        }

        public void DrawPlayerHand()
        {
            Card humanCard0 = new Card(this.Players[0].PlayerCards[0]);
            Card humanCard1 = new Card(this.Players[0].PlayerCards[1]);
            Card botCard0 = new Card(this.Players[1].PlayerCards[0]);
            Card botCard1 = new Card(this.Players[1].PlayerCards[1]);

            double card0_X = window.Width / 2 - humanCard0.Width - 10; 
            double card1_X = window.Width / 2 + 10;

            humanCard0.X = card0_X;
            botCard0.X = card0_X;
            humanCard1.X = card1_X;
            botCard1.X = card1_X;

            double humanCard_Y = window.Height / 2 + tableBitmap.Height / 2 - humanCard0.Width - 60;
            humanCard0.Y = humanCard_Y;
            humanCard1.Y = humanCard_Y;

            double botCard_Y = window.Height / 2 - tableBitmap.Height / 2 + 40;
            botCard0.Y = botCard_Y;
            botCard1.Y = botCard_Y;

            if (this.CurrentStage != RoundStage.SHOWDOWN)
            {
                botCard0.IsRevealed = false;
                botCard1.IsRevealed = false;
            }
            else
            {
                botCard0.IsRevealed = true;
                botCard1.IsRevealed = true;
            }

            humanCard0.Draw();
            humanCard1.Draw();
            botCard0.Draw();
            botCard1.Draw();
        }

        public void DrawCommunityCards()
        {
            double X = 360;
            double Y = 315;

            for (int i = 0; i < this.CommunityCards.Count; i++)
            {
                Card card = new Card(this.CommunityCards[i]);
                card.X = X + i * card.Width + 20;
                card.Y = Y;

                card.Draw();
            }
        }
    }
}
    