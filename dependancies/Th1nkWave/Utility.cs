using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace q_game.dependancies.Th1nkWave
{
    public static class Utility
    {
        public static void Populate<T>(T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            } }

        //-made by ash: idk why this wasnt a thing origanaly
        public static UInt32 MakeColPack(byte r, byte g, byte b) 
        {
            return ((UInt32)r << 8) | ((UInt32)g << 16) | ((UInt32)b << 24) | (UInt32)255;
        }
        public static UInt32 MakeColPack(byte r, byte g, byte b, byte a) 
        {
            return ((UInt32)r << 8) | ((UInt32)g << 16) | ((UInt32)b << 24) | (UInt32)a;
        }
        

    }
}
