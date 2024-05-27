using System;
using System.Collections.Generic;
using System.Text;
using static code.DataArrays;
using static code.PokerData;

namespace code
{
    public static class Evaluator
    {
        public static uint HashFunction(uint u)
        {
            uint a, b, r;
            u += 0xe91aaa35;
            u ^= u >> 16;
            u += u << 8;
            u ^= u >> 4;
            b  = (u >> 8) & 0x1ff;
            a  = (u + (u << 2)) >> 19;
            r  = a ^ HASH_ADJUST[b];
            return r;
        }

        public static ushort Eval5Cards(int c1, int c2, int c3, int c4, int c5)
        {
            uint q = (uint)(c1 | c2 | c3 | c4 | c5) >> 16;
            ushort s;
    
            // check for flushes and straight flushes
            if ((c1 & c2 & c3 & c4 & c5 & 0xf000) != 0)
            {
                return FLUSHES[q];
            }

            // check for straights and high card hands
            s = UNIQUE5[q];
            if (s != 0) {
                return s;
            }
            
            // other hands
            return HASH_VALUES[HashFunction((uint)((c1 & 0xff) * (c2 & 0xff) * (c3 & 0xff) * (c4 & 0xff) * (c5 & 0xff)))];
        }

        public static ushort Eval5Hand(int[] hand)
        {
            int c1, c2, c3, c4, c5;

            c1 = hand[0];
            c2 = hand[1];
            c3 = hand[2];
            c4 = hand[3];
            c5 = hand[4];

            return Eval5Cards(c1, c2, c3, c4, c5);
        }

        public static ushort Eval7Cards(int[] hand)
        {
            int i, j;
            ushort q, best = 9999;
            int[] subhand = new int[5];

	        for (i = 0; i < 21; i++)
	        {       
		        for ( j = 0; j < 5; j++ )
			    subhand[j] = hand[PERM7[i,j]];
		        q = Eval5Hand(subhand);
		        if (q < best)
			        best = q;
	        }
	        
            return best;
        }

        public static int CompareHand(int[] hand1, int[] hand2)
        {
            int score1 = Eval7Cards(hand1);
            int score2 = Eval7Cards(hand2);

            if (score1 > score2)
            {
                return -1;
            }
            else if (score1 < score2)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}