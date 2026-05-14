using System.Runtime.CompilerServices;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

/*
LICENSE : MIT
DESCRIPTION: this is a low level console graphics library that is designed to be used to display text to the console screen extreemly quickly.
             this code is more generally a stipped down version of inprobitas graphics library without font size functionality, unlike
             the original library, this library relies on actual characters to display font rather than drawing pixels.
CREATOR: https://github.com/Th1nk-Wave

im not insisting this but if you could, please try to keep credits intact wherever you place this file. 
 */

using static q_game.dependancies.Th1nkWave.ConsoleAPI;
using static q_game.dependancies.Th1nkWave.Utility;


namespace q_game.dependancies.Th1nkWave
{
    public struct Color
    {
        public UInt32 packed;
        public Color(byte r, byte g, byte b)  
        {
            packed = ((UInt32)r << 8) | ((UInt32)g << 16) | ((UInt32)b << 24) | (UInt32)255;
        }
        public Color(byte r, byte g, byte b, byte a)
        {
            packed = ((UInt32)r << 8) | ((UInt32)g << 16) | ((UInt32)b << 24) | (UInt32)a;
        }

        public byte r => (byte)(packed >> 8);
        public byte g => (byte)(packed >> 16 );
        public byte b => (byte)(packed >> 24);
        public byte a => (byte)(packed);

        public static UInt32 BlendColors(UInt32 bgColor, UInt32 fgColor)
        {
            // Extract components of the background color
            byte bgR = (byte)(bgColor >> 8);
            byte bgG = (byte)(bgColor >> 16);
            byte bgB = (byte)(bgColor >> 24);
            byte bgA = (byte)(bgColor); // Not really needed if we assume bgA is opaque (255)

            // Extract components of the foreground color
            byte fgR = (byte)(fgColor >> 8);
            byte fgG = (byte)(fgColor >> 16);
            byte fgB = (byte)(fgColor >> 24);
            byte fgA = (byte)(fgColor);

            // Calculate alpha as a fraction
            float alpha = fgA / 255f;

            // Perform alpha blending for each color channel
            byte blendedR = (byte)((fgR * alpha) + (bgR * (1 - alpha)));
            byte blendedG = (byte)((fgG * alpha) + (bgG * (1 - alpha)));
            byte blendedB = (byte)((fgB * alpha) + (bgB * (1 - alpha)));

            // Assemble the blended color into a single UInt32 value with bgA as the final alpha
            return ((UInt32)blendedB << 24) | ((UInt32)blendedG << 16) | ((UInt32)blendedR << 8) | bgA;
        }
    }
    public class monoprinter
    {
        // buffers
        private UInt32[] ColorBuffer;
        private UInt32[] CharacterColorBuffer;
        private char[] CharacterBuffer;
        

        private UInt32[] LastColorBuffer;
        private UInt32[] LastCharacterColorBuffer;
        private char[] LastCharacterBuffer;


        // misc variables
        public byte _CompressionFactor;
        private UInt16 _Width = 100;
        private UInt16 _Height = 100;
        public UInt16 Width
        {
            get { return _Width; }
            set { _Width = value; }
        }
        public UInt16 Height
        {
            get { return _Height; }
            set { _Height = value; }
        }
        public uint BGFiller;

        // handles
        private IntPtr Hwindow;

        public monoprinter(UInt16 width, UInt16 height, Color ClearColor, byte CompressionFactor)
        {
            // set vars
            _Width = width;
            _Height = height;
            BGFiller = ClearColor.packed;
            _CompressionFactor = CompressionFactor;

            // initialise buffers
            ColorBuffer = new UInt32[Width * Height]; Populate(ColorBuffer, 1000000u);
            LastColorBuffer = new UInt32[Width * Height]; Populate(LastColorBuffer, 0u);

            CharacterColorBuffer = new UInt32[Width * Height * 2]; Populate(CharacterColorBuffer, new Color(255,255,255).packed);
            LastCharacterColorBuffer = new UInt32[Width * Height * 2]; Populate(LastCharacterColorBuffer, 0u);

            CharacterBuffer = new char[Width * Height * 2]; Populate(CharacterBuffer, ' ');
            LastCharacterBuffer = new char[Width * Height * 2]; Populate(LastCharacterBuffer, ' ');

            // get window handle
            Hwindow = GetStdHandle(STD_OUTPUT_HANDLE);
            // set vitual
            SetVirtual(Hwindow);
            SetWindowSize(Hwindow, Width, Height);


        }

        public bool RGBisclose(UInt32 col1, UInt32 col2)
        {
            // Extract components of col1
            byte c1R = (byte)(col1 >> 8);
            byte c1G = (byte)(col1 >> 16);
            byte c1B = (byte)(col1 >> 24);

            // Extract components of col2
            byte c2R = (byte)(col2 >> 8);
            byte c2G = (byte)(col2 >> 16);
            byte c2B = (byte)(col2 >> 24);

            bool Rclose = Math.Abs((int)c1R - (int)c2R) <= _CompressionFactor;
            bool Gclose = Math.Abs((int)c1G - (int)c2G) <= _CompressionFactor;
            bool Bclose = Math.Abs((int)c1B - (int)c2B) <= _CompressionFactor;
            return (Rclose && Gclose && Bclose);
        }
        public void Render()
        {
            // update
            string optiRend = "";
            UInt32 OldCol = UInt32.MaxValue;
            UInt32 OldCharacterCol = UInt32.MaxValue;
            int Blanks = 0;
            StringBuilder RenderSTR = new StringBuilder();
            bool jumping = true;

            for (UInt16 ypos = 0; ypos < _Height; ypos++)
            {
                RenderSTR.Append($"\x1b[{ypos + 1};0H");
                for (UInt16 xpos = 0; xpos < _Width*2; xpos++)
                {
                    UInt32 col = ColorBuffer[xpos/2 + ypos * _Width];
                    UInt32 LastFcol = LastColorBuffer[xpos/2 + ypos * _Width];

                    UInt32 CharacterCol = CharacterColorBuffer[xpos + ypos * _Width*2];
                    UInt32 LastCharacterCol = LastCharacterColorBuffer[xpos + ypos * _Width*2];

                    char Character = CharacterBuffer[xpos + ypos * _Width * 2];
                    char LastCharacter = LastCharacterBuffer[xpos + ypos * _Width * 2];

                    bool BGcolUpdate = col != LastFcol;
                    bool FGcolUpdate = CharacterCol != LastCharacterCol;
                    bool CharacterUpdate = Character != LastCharacter;
                    bool NeedsUpdate = BGcolUpdate || FGcolUpdate || CharacterUpdate;


                    if (NeedsUpdate)
                    {
                        if (jumping)
                        {
                            // set cursor position
                            RenderSTR.Append($"\x1b[{ypos + 1};{xpos + 1}H");
                            jumping = false;
                        }


                        if (BGcolUpdate || CharacterUpdate)
                        {
                            if ((col == OldCol || RGBisclose(col, OldCol)) && !jumping)
                            {
                                // do nothing
                            }
                            else
                            {
                                RenderSTR.Append($"\x1b[48;2;{(byte)(col >> 8)};{(byte)(col >> 16)};{(byte)(col >> 24)}m");
                                OldCol = col;
                            }
                        }

                        // for this pixel
                        if (FGcolUpdate || CharacterUpdate)
                        {
                            if ((CharacterCol == OldCharacterCol || RGBisclose(CharacterCol,OldCharacterCol)) && !jumping)
                            {
                                // if character colour is the same as beffore, dont bother changing it.
                            }
                            else
                            {
                                RenderSTR.Append($"\x1b[38;2;{(byte)(CharacterCol >> 8)};{(byte)(CharacterCol >> 16)};{(byte)(CharacterCol >> 24)}m");
                                OldCharacterCol = CharacterCol;
                            }
                        }

                        RenderSTR.Append(Character);


                    }
                    else
                    {
                        jumping = true;
                    }
                }
            }
            optiRend = RenderSTR.ToString();
            ColorBuffer.CopyTo(LastColorBuffer, 0);
            CharacterColorBuffer.CopyTo(LastCharacterColorBuffer, 0);
            CharacterBuffer.CopyTo(LastCharacterBuffer, 0);

            // render
            uint charsWritten = 0;
            nint reserved = 0;
            WriteConsole(Hwindow, optiRend, (uint)optiRend.Length, out charsWritten, reserved);
            //Console.WriteLine($"written {charsWritten}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, UInt32 col)
        {
            // check if pixel is out of bounds
            if (x < 0 || y < 0) { return; }
            if (x >= _Width || y >= _Height) { return; }


            // avoid expensive blending if a color is not transparrent
            if ((byte)col < 255)
            {
                if ((byte)col > 4)
                {
                    ColorBuffer[x + y * _Width] = Color.BlendColors(ColorBuffer[x + y * _Width], col);
                }
            }
            else
            {
                ColorBuffer[x + y * _Width] = col;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixelInBounds(int x, int y, UInt32 col) // same as SetPixel but with less checks
        {
            // avoid expensive blending if a color is not transparrent
            if ((byte)col < 255)
            {
                if ((byte)col > 4)
                {
                    ColorBuffer[x + y * _Width] = Color.BlendColors(ColorBuffer[x + y * _Width], col);
                }
                else
                {
                    return;
                }
            }
            else
            {
                ColorBuffer[x + y * _Width] = col;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void fill(UInt32 col)
        {
            Populate(ColorBuffer, col);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void fillCharacter(char c)
        {
            Populate(CharacterBuffer, c);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCharacter(char c, uint col, uint x, uint y)
        {
            // check if character is out of bounds
            if (x < 0 || y < 0) { return; }
            if (x >= (_Width)*2 || y >= _Height-1) { return; }

            // avoid expensive blending if a color is not transparrent
            if ((byte)col < 255)
            {
                if ((byte)col > 4)
                {
                    CharacterColorBuffer[x + y * _Width] = Color.BlendColors(ColorBuffer[x + y * _Width], col);
                }
            }
            else
            {
                CharacterColorBuffer[x + y * _Width] = col;
            }

            CharacterBuffer[x + y * _Width] = c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCharacter(char c, uint col, int x, int y)
        {
            // check if character is out of bounds
            if (x < 0 || y < 0) { return; }
            if (x >= (_Width) * 2 || y >= _Height - 1) { return; }

            CharacterColorBuffer[x + y * _Width * 2] = col;

            CharacterBuffer[x + y * _Width * 2] = c;
        }

        //-made by ash:  ive been lied to, think-wave said this was in this version of monoprinter
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawBox(UInt32 col, int x, int y, int width, int height) {
            
            int ENDX = x+width;
            int ENDY = y+height;
            for (int i = y; i < ENDY; i++) 
            {
                for (int j = x; j < ENDX; j++) 
                {
                    SetPixel(j, i, col);
                }
            }
        }

        //-made by ash:  unlike the inprobitas version of this, secsaver isnt designed to use images
        //also this function is basicaly copied in Graphics.cs, could have made a universal function for the 2
        //but nah, didnt have time
        public Color[,] GetPixelMap(string FilePath) {
            
            using Bitmap bitmap = new Bitmap(FilePath);

            // lock bits is faster than reading all (mainly for larger pngs tho)
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );

            byte bytesPerPixel = 4; // R, G, B, A
            int stride = bitmapData.Stride;
            int byteCount = stride * bitmap.Height;
            byte[] pixels = new byte[byteCount];

            Marshal.Copy(bitmapData.Scan0, pixels, 0, byteCount);   
            //ps this is unmanaged memory

            Color[,] output = new Color[bitmap.Height,bitmap.Width];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int index = (y * stride) + (x * bytesPerPixel);

                    // its stored as bgra in memory (neat cache optimisations)
                    output[y, x] = new Color(pixels[index + 2], pixels[index + 1], pixels[index + 0], pixels[index + 3]);
                }
            }
            Marshal.Copy(pixels, 0, bitmapData.Scan0, byteCount);

            bitmap.UnlockBits(bitmapData);

            return output;
        }

        //-made by ash:  lambda functions for overloads are for ... normal people
        public void DrawImg(string FilePath, int x, int y) {
            Color[,] img = GetPixelMap(FilePath);   //  index = [y,x]
                                                    //          [i,j]
            DrawImg(img, x, y);
        }
      
        //-made by ash:  should be y,x not x,y for cache optimisations but idc anymore
        public void DrawImg(Color[,] img, int x, int y) {
            for (int i = 0; i < img.GetLength(0); i++) {
                for (int j = 0; j < img.GetLength(1); j++) {
                    SetPixel(x + j, y + i, img[i, j].packed);
                }
            }
        }
    }
}
