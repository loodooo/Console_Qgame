using System.Runtime.InteropServices;
using q_game.dependancies.Th1nkWave;
using SixLabors.ImageSharp;      //dont even know if im using this anymore....
using SixLabors.ImageSharp.PixelFormats;      //but eh ill just keep it and hope nothing breaks

namespace q_game;

class Program {
      //restarting this whole project on wednesday afternoon... its due thursday morning
      
      //12 hours of ~straight programming later and its finished at 8:17 .... the lesson where i have to show this starts at 8:45
      //      ; yh i was like 15 mins late to the lesson lol,
      //       ALSO CAME FIRST PLACE !!! like 15 votes 
      [DllImport("kernel32.dll", ExactSpelling = true)]
      public static extern bool SetConsoleDisplayMode(IntPtr c_handle,int flag_dw, ref win32_coord _coord);
      
      [StructLayout(LayoutKind.Sequential)]
      public struct win32_coord {
            public short X, Y;
      }
      
      static async Task Main() {
            win32_coord dummy_coord = new(); //still dont know why this cord is needed.. maybe its modifing it but idrk or care so yh
            nint C_handle = ConsoleAPI.GetStdHandle(-11);
            Console.CursorVisible = false;
            
            ConsoleAPI.SetCurrentFont(C_handle, "Consolas", 10);
            
            SetConsoleDisplayMode(C_handle, 1, ref dummy_coord);
            // this only works on older windows console, this aint a issue on college computers
            // but this will not function properly on modern windows console
            Thread.Sleep(200);
            //Console.ReadKey();      //to allow manual fullscreen when not on a older console
            await game.main();
      }
} 
