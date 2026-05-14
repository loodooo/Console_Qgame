using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using q_game.dependancies.Th1nkWave;
using Color = q_game.dependancies.Th1nkWave.Color;

namespace q_game.dependancies;

public class Graphics {
    public monoprinter window;
    public UInt16 VeiwWidth, VeiwHeight;
    public UInt16[,,] drawBuffer;    //should just use the monoprinter buffers
    public Dictionary<UInt16, Color[,]> sprites;
    public byte SpriteSize = 8;
    public string SpriteSheetPath;

    internal map? _currentLevel;    //rly doesnt need to be nullable
    internal float _camX, _camY;    //it will crash if its null anyways lol
    public bool _running = false;

    public Graphics(monoprinter Window, UInt16 VeiwWidth, UInt16 VeiwHeight, int level) {
        this.window = Window;
        this.VeiwWidth = VeiwWidth;
        this.VeiwHeight = VeiwHeight;
        drawBuffer = new UInt16[VeiwWidth, VeiwHeight, 8];
        sprites = new Dictionary<UInt16, Color[,]>();
        UpdateSpriteSheet(level);
    }

    public void UpdateSpriteSheet(int level) {
        SpriteSheetPath = $"../../../sprites/{level}.png";
        sprites.Clear();
    }
    
    public void SetLevel(map level) => SetLevel(level, 0);
    public void SetLevel(map level, int levelIndex) {
        _currentLevel = level;
        UpdateSpriteSheet(levelIndex);
    }

    public void DrawPlayer(character player) {
        int screenPixelX = (int)((player.PosX / 8f - _camX) * 8 + (VeiwWidth  * 8) / 2f);
        int screenPixelY = (int)((player.PosY / 8f - _camY) * 8 + (VeiwHeight * 8) / 2f) + 2;
        window.DrawImg(player.sprite, screenPixelX, screenPixelY);
    }

    public Color[,] GetSprite(UInt16 id) {
        using Bitmap bmp = new Bitmap(SpriteSheetPath);

        int spritesPerRow = bmp.Width / 8;
        int spriteX = (id % spritesPerRow) * 8;
        int spriteY = (id / spritesPerRow) * 8;

        if (spriteX + 8 > bmp.Width || spriteY + 8 > bmp.Height)
            return new Color[8, 8];

        BitmapData bmpData = bmp.LockBits(
            new Rectangle(spriteX, spriteY, 8, 8),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );

        byte bytesPerPixel = 4;
        int stride    = bmp.Stride;
        int byteCount = stride * 8;
        byte[] pixels = new byte[byteCount];

        Marshal.Copy(bmpData.Scan0, pixels, 0, byteCount);

        Color[,] output = new Color[8, 8];
        for (int y = 0; y < 8; y++) {
            for (int x = 0; x < 8; x++) {
                int index = (y * stride) + (x * bytesPerPixel);
                output[y, x] = new Color(pixels[index + 2], pixels[index + 1], pixels[index + 0], pixels[index + 3]);
            }
        }

        bmp.UnlockBits(bmpData);
        return output;
    }

    public async Task RenderLoop() {
        _running = true;
        while (_running) {
            if (_currentLevel.HasValue) {
                var level = _currentLevel.Value;
                DrawPlayArea(_camX, _camY, ref level);
                window.Render();
            }
            await Task.Delay(16);    //~62 tps
        }
    }

    public void StopRenderLoop() => _running = false;

    public void SetCamera(float x, float y) {
        _camX = x;
        _camY = y;
    }   //using floats is kinda pointless cause i ran out of time
        //to actualy have non tile coords for camera pos and player rendering

    public void DrawPlayArea(float CamPosX, float CamPosY, ref map CurrentLevel) {
        int TLX = (int)MathF.Floor(CamPosX - VeiwWidth  / 2f);
        int TLY = (int)MathF.Floor(CamPosY - VeiwHeight / 2f);

        int layerCount = CurrentLevel.chunks.Count > 0
            ? CurrentLevel.chunks.Values.First().spritelayers.Length
            : 0;

        UInt16[,,] drawBuffer = new UInt16[layerCount, VeiwWidth, VeiwHeight];

        for (int screenY = 0; screenY < VeiwHeight; screenY++) {
            for (int screenX = 0; screenX < VeiwWidth; screenX++) {
                int worldTileX = TLX + screenX;
                int worldTileY = TLY + screenY;

                if (worldTileX < 0 || worldTileY < 0 || worldTileX >= CurrentLevel.map_width || worldTileY >= CurrentLevel.map_height) {
                    for (int layer = 0; layer < layerCount; layer++)
                        drawBuffer[layer, screenX, screenY] = 0;
                    continue;
                }

                int chunkX = worldTileX / 8;
                int chunkY = worldTileY / 8;

                byte subX = (byte)(worldTileX % 8);
                byte subY = (byte)(worldTileY % 8);

                map_chunk chunk;
                bool hasChunk = false;

                if (CurrentLevel.chunks.TryGetValue((chunkX, chunkY), out map_chunk foundChunk)) {
                    chunk    = foundChunk;
                    hasChunk = true;
                } else chunk = default;

                for (int layer = 0; layer < layerCount; layer++) {
                    drawBuffer[layer, screenX, screenY] = hasChunk
                        ? chunk.spritelayers[layer][subX, subY]
                        : (UInt16)0;
                }
            }
        }

        window.fill(CurrentLevel.SkyColor.packed);

        for (int layer = layerCount - 1; layer >= 0; layer--) {
            for (int i = 0; i < VeiwWidth; i++) {
                for (int j = 0; j < VeiwHeight; j++) {
                    UInt16 tileID = drawBuffer[layer, i, j];
                    if (tileID == 0) continue;    //holy indentation
                    if (!sprites.ContainsKey(tileID))
                        sprites.Add(tileID, GetSprite(tileID));
                    window.DrawImg(sprites[tileID], i * 8, j * 8);
                }
            }
        }
    }
}
