using q_game.dependancies.Th1nkWave;

namespace q_game.dependancies;

public class character {
    public static int ANIMATION_FRAMES = 1; //unused
    public Color[,] sprite;
    public int size = 7;
    public float PosX, PosY;
    public float SpawnX, SpawnY;
    public map Level;
    public string SpriteSheetPath = $"../../../sprites/player.png";

    public float VelX = 0f;
    public float VelY = 0f;
    public bool IsGrounded = false;

    const float WALK_ACCEL     = 0.35f;
    const float MAX_WALK_SPEED = 5f;
    const float FRICTION       = 0.85f;//higher friction  = less friction
    const float AIR_FRICTION   = 0.85f;
    const float JUMP_FORCE     = -4.5f;
    const float GRAVITY        = 0.30f;
    const float MAX_FALL_SPEED = 10f;
    const float JUMP_CUT       = 0.35f;

    bool _jumpHeldLastFrame = false;
    bool _jumpConsumed      = false;

    public Graphics display;

    public event Action OnLevelComplete;

    public character(float PosX, float PosY, map level, ref monoprinter m, ref Graphics display) {
        this.PosX    = PosX;
        this.PosY    = PosY;
        this.SpawnX  = PosX;
        this.SpawnY  = PosY;
        this.Level   = level;
        this.display = display;
        sprite = m.GetPixelMap(SpriteSheetPath);
    }

    private bool IsSolid(float tileX, float tileY) {
        if (tileX < 0 || tileY < 0 || tileX >= Level.map_width || tileY >= Level.map_height)
            return true;

        int tx = (int)tileX;
        int ty = (int)tileY;

        int chunkX = tx / 8;
        int chunkY = ty / 8;

        if (!Level.chunks.TryGetValue((chunkX, chunkY), out map_chunk chunk))
            return false;

        return chunk.collision[(byte)(tx % 8), (byte)(ty % 8)];//i hate intel
    }                            //little endian breaks the collision 

    private (float pos, bool hit) ResolveAxis(float pos, float vel, bool horizontal) {
        float next    = pos + vel;
        float step    = MathF.Sign(vel) * 0.125f;
        float current = pos;

        float probe0 = horizontal ? PosY / 8f          : PosX / 8f;
        float probe1 = horizontal ? (PosY + size) / 8f : (PosX + size) / 8f;

        while (MathF.Sign(next - current) == MathF.Sign(vel)) {
            float candidate = current + step;

            if (MathF.Sign(next - candidate) != MathF.Sign(vel))
                candidate = next;

            float edgeTile = horizontal
                ? (vel > 0 ? (candidate + size) / 8f : candidate / 8f)
                : (vel > 0 ? (candidate + size) / 8f : candidate / 8f);

            bool blocked =
                IsSolid(horizontal ? edgeTile : probe0, horizontal ? probe0 : edgeTile) ||
                IsSolid(horizontal ? edgeTile : probe1, horizontal ? probe1 : edgeTile);

            if (blocked)
                return (current, true);

            current = candidate;
            if (current == next) break;
        }

        return (next, false);
    }

    public void ResetToSpawn() {
        PosX          = SpawnX;
        PosY          = SpawnY;
        VelX          = 0f;
        VelY          = 0f;
        IsGrounded    = false;
        _jumpConsumed = false;
    }

    public bool CanMove(UInt16 x, UInt16 y, ref map level) {
        if (level.chunks[(x / 8, y / 8)].collision[(byte)(x % 8), (byte)(y % 8)])
            return false;
        return true;
    }

    public async Task MovementHandler() {
        while (true) {
            Tick();
            await Task.Delay(16);
        }
    }

    public void Tick() {
        bool left      = input.IsKeyDown((int)Keys.A) || input.IsKeyDown((int)Keys.Left);
        bool right     = input.IsKeyDown((int)Keys.D) || input.IsKeyDown((int)Keys.Right);
        bool jumpPress = input.IsKeyDown((int)Keys.Space) || input.IsKeyDown((int)Keys.W) || input.IsKeyDown((int)Keys.Up);

        if (right) VelX += WALK_ACCEL;
        if (left)  VelX -= WALK_ACCEL;

        if (VelX >  MAX_WALK_SPEED) VelX =  MAX_WALK_SPEED;
        if (VelX < -MAX_WALK_SPEED) VelX = -MAX_WALK_SPEED;

        VelX *= IsGrounded ? FRICTION : AIR_FRICTION;
        if (MathF.Abs(VelX) < 0.05f) VelX = 0f;

        if (jumpPress && !_jumpHeldLastFrame && IsGrounded && !_jumpConsumed) {
            VelY          = JUMP_FORCE;
            IsGrounded    = false;
            _jumpConsumed = true;
        }

        if (!jumpPress && VelY < 0f)
            VelY *= JUMP_CUT;

        _jumpHeldLastFrame = jumpPress;

        VelY += GRAVITY;
        if (VelY > MAX_FALL_SPEED) VelY = MAX_FALL_SPEED;

        if (VelX != 0f) {
            var (newX, hitX) = ResolveAxis(PosX, VelX, horizontal: true);
            PosX = newX;
            if (hitX) VelX = 0f;
        }

        {
            var (newY, hitY) = ResolveAxis(PosY, VelY, horizontal: false);

            if (hitY && VelY > 0f) {
                PosY          = newY;
                VelY          = 0f;
                IsGrounded    = true;
                _jumpConsumed = false;
            } else if (hitY && VelY < 0f) {
                PosY = newY;
                VelY = 0f;
            } else {
                PosY       = newY;
                IsGrounded = false;
            }
        }

        if (PosX > Level.map_width * 8f-16)
            OnLevelComplete?.Invoke();

        if (PosY >= Level.map_height*8-8)
            ResetToSpawn();

        display.SetCamera(PosX / 8f, PosY / 8f);
    }

    public async Task Move(float x, float y) {
        Tick();
        await Task.CompletedTask;
    }
}
