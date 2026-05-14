using System.Runtime.InteropServices;
using q_game.dependancies.Th1nkWave;
using q_game.dependancies;

namespace q_game;

public static class game {
    public static Random rnd;           
    public static monoprinter window;

    public static UInt32 BgCol = Utility.MakeColPack(0, 0, 0);
    public static UInt32 FgCol = Utility.MakeColPack(255, 255, 255);

    public static UInt16 CurrentLevel = 0;

    public static int width;
    public static int height; 

    public static async Task main() {
        rnd = new Random(100);
        
        width  = Console.WindowWidth  / 2 - 1;
        height = Console.WindowHeight - 1;
        
        window = new monoprinter((ushort)width, (ushort)height, new Color(255, 0, 0), (byte)0);
        
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                double hue    = (y + x) * 360.0 / (size * 2);
                var (r, g, b) = HslToRgb(hue / 2, 1.0, 0.55);
                window.SetPixel(x, y, Utility.MakeColPack((byte)r, (byte)g, (byte)b));
            }
        } //drawing the funky rainbow 
        
        window.DrawBox(FgCol, width / 2 - 8, height / 2 - 5, 17, 12);
        window.DrawBox(BgCol, width / 2 - 7, height / 2 - 4, 15, 10); 
        //dumb way to do outline but i couldnt be bothered to make an outline function for just this
        
        string t0 = $"res = {width}, {height}";
        string t1 = "This game was really rushed";
        string t2 = "  so sry in advance lol";
        string t3 = "press any button to start...";
        //didnt realise how small this text is now that the console is font size 10
        
        for (int i = 0; i < t0.Length; i++) window.SetCharacter(t0[i], FgCol, width - 14 + i, height / 2 - 2);
        for (int i = 0; i < t1.Length; i++) window.SetCharacter(t1[i], FgCol, width - 14 + i, height / 2);
        for (int i = 0; i < t2.Length; i++) window.SetCharacter(t2[i], FgCol, width - 14 + i, height / 2 + 1);
        for (int i = 0; i < t3.Length; i++) window.SetCharacter(t3[i], FgCol, width - 14 + i, height / 2 + 3);

        window.Render();
        window.fillCharacter(' ');
        Console.ReadKey(intercept: true);

        await RunGame();

        Console.ReadKey(intercept: true);
    }

    private static async Task RunGame() {
        map level = new map($"../../../maps/{CurrentLevel}.json");

        Graphics display = new Graphics(window, (UInt16)(width / 8), (UInt16)(height / 8), CurrentLevel);
        display.SetLevel(level);
        display.SetCamera(level.map_width / 2f, level.map_height / 2f);

        character player = new character(
            level.map_width  * 0.5f,   // tile coords * 8 / 2
            level.map_height * 4f,
            level, ref window, ref display 
        ); //started just adding refs to everything cause this code became a mess by the end

        player.OnLevelComplete += () => {
            string nextPath = $"../../../maps/{CurrentLevel + 1}.json";
            if (!File.Exists(nextPath)) {
                player.ResetToSpawn();
                return;
            }
            CurrentLevel++;
            map newLevel     = new map($"../../../maps/{CurrentLevel}.json");
            player.Level     = newLevel;
            player.SpawnX    = newLevel.map_width  * 0.5f;
            player.SpawnY    = newLevel.map_height * 4f;
            display.SetLevel(newLevel, CurrentLevel); 
            display.SetCamera(player.SpawnX / 8f, player.SpawnY / 8f);
            player.ResetToSpawn();
        };

        var cts = new CancellationTokenSource();

        Task renderTask = Task.Run(async () => {
            try {
                while (!cts.Token.IsCancellationRequested) {
                    if (display._currentLevel.HasValue) {
                        var l = display._currentLevel.Value;
                        display.DrawPlayArea(display._camX, display._camY, ref l);
                        display.DrawPlayer(player);
                        display.window.Render();
                    }
                    await Task.Delay(16, cts.Token);
                }
            } catch (TaskCanceledException) { }
        });

        Task physicsTask = Task.Run(async () => {
            try {
                while (!cts.Token.IsCancellationRequested) {
                    player.Tick();
                    //Console.Title = $"pos=({player.PosX:F1},{player.PosY:F1}) vel=({player.VelX:F2},{player.VelY:F2}) grounded={player.IsGrounded} cam=({display._camX:F1},{display._camY:F1})";
                    await Task.Delay(16, cts.Token);
                }
            } catch (TaskCanceledException) { }
        });

        while (true) {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Escape) break;
        }

        cts.Cancel();
        await Task.WhenAll(renderTask, physicsTask);
    }

    static (int R, int G, int B) HslToRgb(double h, double s, double l) {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = l - c / 2;

        double r, g, b;
        if      (h < 60)  { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else              { r = c; g = 0; b = x; }

        return (
            (int)Math.Round((r + m) * 255),
            (int)Math.Round((g + m) * 255),
            (int)Math.Round((b + m) * 255)
        );
    }
}
