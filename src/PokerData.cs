using System;
using System.Collections.Generic;

namespace code
{
    public static class PokerData
    {
        public enum RoundStage 
        {
            PREFLOP = 0,
            FLOP = 1,
            TURN = 2,
            RIVER = 3,
            SHOWDOWN = 4
        }

        public enum PlayerAction
        {
            FOLD = 0,
            CHECK = 1,
            CALL = 2,
            BET = 3,
            RAISE = 4
        }
    }
}