using System;
using SplashKitSDK;

namespace code
{
    public class Program
    {
        public static void Main()
        {
            Window gameWindow = new Window("PokerGame", 1080, 720);
            PokerGame pokerGame = new PokerGame(gameWindow);

            while (!pokerGame.IsGameEnd & !gameWindow.CloseRequested)
            {
                SplashKit.ProcessEvents();
                pokerGame.Draw();
                pokerGame.HandleInput();
                pokerGame.Update();

            }
        }
    }
}
