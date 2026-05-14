using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


//this is unchanged from the original at 
//https://github.com/Th1nk-Wave/sec-saver/blob/main/sec-saver/dependancies/monoprinter.cs
namespace q_game.dependancies.Th1nkWave
{
    public static class ConsoleAPI
    {
        // CONSTANTS
        public const int STD_OUTPUT_HANDLE = -11;
        public const int FILE_SHARE_READ = 1;
        public const int FILE_SHARE_WRITE = 2;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const int CONSOLE_TEXTMODE_BUFFER = 1;
        public const Int32 INVALID_HANDLE_VALUE = -1;
        public const int FixedWidthTrueType = 54;
        public const int StandardOutputHandle = -11;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;

        // enums
        [Flags]
        public enum CharacterAttributesFlags : ushort
        {
            None = 0,
            FOREGROUND_BLUE = 0x0001,
            FOREGROUND_GREEN = 0x0002,
            FOREGROUND_RED = 0x0004,
            FOREGROUND_INTENSITY = 0x0008,
            BACKGROUND_BLUE = 0x0010,
            BACKGROUND_GREEN = 0x0020,
            BACKGROUND_RED = 0x0040,
            BACKGROUND_INTENSITY = 0x0080,
            COMMON_LVB_LEADING_BYTE = 0x0100,
            COMMON_LVB_TRAILING_BYTE = 0x0200,
            COMMON_LVB_GRID_HORIZONTAL = 0x0400,
            COMMON_LVB_GRID_LVERTICAL = 0x0800,
            COMMON_LVB_GRID_RVERTICAL = 0x1000,
            COMMON_LVB_REVERSE_VIDEO = 0x4000,
            COMMON_LVB_UNDERSCORE = 0x8000,
        }

        public enum CONSOLE_BUFFER_MODES
        {
            ENABLE_PROCESSED_INPUT = 0x0001,
            ENABLE_PROCESSED_OUTPUT = 0x0001,
            ENABLE_LINE_INPUT = 0x0002,
            ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
            ENABLE_ECHO_INPUT = 0x0004,
            ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
            ENABLE_WINDOW_INPUT = 0x0008,
            DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
            ENABLE_MOUSE_INPUT = 0x0010,
            ENABLE_LVB_GRID_WORLDWIDE = 0x0010,
            ENABLE_INSERT_MODE = 0x0020,
            ENABLE_QUICK_EDIT_MODE = 0x0040,
        }
        
        // STRUCTS AND TYPES
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };
        [StructLayout(LayoutKind.Explicit)]
        public struct CHAR_UNION
        {
            [FieldOffset(0)] public ushort UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }
        [StructLayout(LayoutKind.Explicit)]
        public struct CHAR_INFO
        {
            [FieldOffset(0)] public CHAR_UNION Char;
            [FieldOffset(2)] public short Attributes;
        }
        public struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }
        public struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public CharacterAttributesFlags wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FontInfo
        {
            internal int cbSize;
            internal int FontIndex;
            internal short FontWidth;
            public short FontSize;
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.wc, SizeConst = 32)]
            public string FontName;
        }


        // Kernel32 functions
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", EntryPoint = "WriteConsoleOutputW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool WriteConsoleOutput(
            IntPtr hConsoleOutput,
            [MarshalAs(UnmanagedType.LPArray), In] CHAR_INFO[] lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpWriteRegion);

        [DllImport("Kernel32.dll")]
        public static extern IntPtr CreateConsoleScreenBuffer(
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr securityAttributes,
            UInt32 flags,
            IntPtr screenBufferData
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleActiveScreenBuffer(IntPtr hBuf);

        [DllImport("kernel32.dll")]
        public static extern bool ReadConsoleOutput(
            IntPtr hConsoleOutput,
            [MarshalAs(UnmanagedType.LPArray), Out] CHAR_INFO[] lpBuffer,
            COORD dwBufferSize,
            COORD dwReadCoord,
            ref SMALL_RECT lpReadRegion
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleScreenBufferInfo(
            IntPtr hConsoleOutput,
            ref CONSOLE_SCREEN_BUFFER_INFO ConsoleScreenBufferInfo
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FillConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            char cCharacter,
            uint nLength,
            COORD dwWriteCoord,
            out uint lpNumberOfCharsWritten
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FillConsoleOutputAttribute(
            IntPtr hConsoleOutput,
            ushort wAttribute,
            uint nLength,
            COORD dwWriteCoord,
            out uint lpNumberOfAttrsWritten
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorPosition(
            IntPtr hConsoleOutput,
            COORD dwCursorPosition
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(
            IntPtr hConsoleHandle,
            uint dwMode
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(
            IntPtr hConsoleHandle,
            out uint lpMode
        );

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleWindowInfo(
            IntPtr hConsoleOutput,
            bool bAbsolute,
            [In] ref SMALL_RECT lpConsoleWindow
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern COORD GetLargestConsoleWindowSize(
            IntPtr hConsoleOutput
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferSize(
            IntPtr hConsoleOutput,
            COORD dwSize
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            string lpCharacter,
            uint nLength,
            COORD dwWriteCoord,
            out uint lpNumberOfCharsWritten
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsole(
            IntPtr hConsoleOutput,
            string lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved
        );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, UInt32 uFlags);

        public static void cls(IntPtr hConsole)
        {
            COORD coordScreen = new COORD(0, 0);    // home for the cursor
            UInt32 cCharsWritten = 0;
            CONSOLE_SCREEN_BUFFER_INFO csbi = new CONSOLE_SCREEN_BUFFER_INFO();
            UInt32 dwConSize = 0;

            // Get the number of character cells in the current buffer.
            if (!GetConsoleScreenBufferInfo(hConsole, ref csbi))
            {
                return;
            }

            dwConSize = (UInt32)(csbi.dwSize.X) * (UInt32)(csbi.dwSize.Y);

            // Fill the entire screen with blanks.
            if (!FillConsoleOutputCharacter(hConsole,        // Handle to console screen buffer
                                            ' ',             // Character to write to the buffer
                                            dwConSize,       // Number of cells to write
                                            coordScreen,     // Coordinates of first cell
                                            out cCharsWritten)) // Receive number of characters written
            {
                return;
            }

            // Get the current text attribute.
            if (!GetConsoleScreenBufferInfo(hConsole, ref csbi))
            {
                return;
            }

            // Set the buffer's attributes accordingly.
            if (!FillConsoleOutputAttribute(hConsole,         // Handle to console screen buffer
                                            (ushort)(CharacterAttributesFlags.BACKGROUND_RED | CharacterAttributesFlags.BACKGROUND_GREEN | CharacterAttributesFlags.BACKGROUND_BLUE), // Character attributes to use
                                            dwConSize,        // Number of cells to set attribute
                                            coordScreen,      // Coordinates of first cell
                                            out cCharsWritten))  // Receive number of characters written
            {
                return;
            }

            // Put the cursor at its home coordinates.
            SetConsoleCursorPosition(hConsole, coordScreen);
        }

        public static void SetVirtual(IntPtr hConsole)
        {
            uint mode = 0;
            GetConsoleMode(hConsole, out mode);
            SetConsoleMode(hConsole, mode | (uint)CONSOLE_BUFFER_MODES.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }

        public static FontInfo[] SetCurrentFont(IntPtr hConsole, string font, short fontSize = 0)
        {
            FontInfo before = new FontInfo
            {
                cbSize = Marshal.SizeOf<FontInfo>()
            };

            if (GetCurrentConsoleFontEx(hConsole, false, ref before))
            {

                FontInfo set = new FontInfo
                {
                    cbSize = Marshal.SizeOf<FontInfo>(),
                    FontIndex = 0,
                    FontFamily = FixedWidthTrueType,
                    FontName = font,
                    FontWeight = 400,
                    FontSize = fontSize > 0 ? fontSize : before.FontSize
                };

                // Get some settings from current font.
                if (!SetCurrentConsoleFontEx(hConsole, false, ref set))
                {
                    var ex = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(ex);
                }

                FontInfo after = new FontInfo
                {
                    cbSize = Marshal.SizeOf<FontInfo>()
                };
                GetCurrentConsoleFontEx(hConsole, false, ref after);

                return new[] { before, set, after };
            }
            else
            {
                var er = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(er);
            }
        }
        public static void SetWindowSize(IntPtr hConsole)
        {
            COORD MaxSize = GetLargestConsoleWindowSize(hConsole);
            SetConsoleScreenBufferSize(hConsole, MaxSize);

            SMALL_RECT size = new SMALL_RECT();
            size.Left = 1;//-20;
            size.Top = 1;//-20;
            size.Right = (Int16)(MaxSize.X - 1);
            size.Bottom = (Int16)(MaxSize.Y - 1);
            SetConsoleWindowInfo(hConsole, true, ref size);

            IntPtr Wnd = GetConsoleWindow();
            SetWindowPos(Wnd, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER);



            //Console.WriteLine($"{MaxSize.X}, {MaxSize.Y}");

        }
        public static void SetWindowSize(IntPtr hConsole, UInt16 Width, UInt16 Height)
        {
            COORD MaxSize = new COORD((short)(Width*2+1),(short)(Height+1));
            SetConsoleScreenBufferSize(hConsole, MaxSize);

            SMALL_RECT size = new SMALL_RECT();
            size.Left = 1;//-20;
            size.Top = 1;//-20;
            size.Right = (Int16)(MaxSize.X - 1);
            size.Bottom = (Int16)(MaxSize.Y - 1);
            SetConsoleWindowInfo(hConsole, true, ref size);

            IntPtr Wnd = GetConsoleWindow();
            //SetWindowPos(Wnd, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER);



            //Console.WriteLine($"{MaxSize.X}, {MaxSize.Y}");

        }
    }
}
