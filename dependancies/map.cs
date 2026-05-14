using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using q_game.dependancies.Th1nkWave;

//https://www.spritefusion.com/editor
//used to make maps        -might use it again someday idk
namespace q_game.dependancies
{


    public struct collision_chunk {
        private UInt64 collision;

        public bool this[byte x, byte y]
        {    //i think this is causing issues due to endianness cause it only breaks on intel CPUs but idk
            get => (collision & ((UInt64)1 << ((int)8*(int)y + (int)x))) > 0;
            set => collision = (collision & ~((UInt64)1 << ((int)8 * (int)y + (int)x))) | ((Unsafe.As<bool, UInt64>(ref value)) <<  ((int)8*(int) y + (int) x));
        }
    }
    
    public struct sprite_chunk
    {
        public UInt16[] tiles = new UInt16[64];

        public sprite_chunk() 
        {
            return;
        }

        public UInt16 this[byte x, byte y] 
        {
            get => tiles[x+y*8];
            set => tiles[x+y*8] = value;
        }

    }

    public struct map_chunk
    {
        public collision_chunk collision;
        public sprite_chunk[] spritelayers;
    }

    public struct json_tile {

        public byte id;
        public UInt16 x;
        public UInt16 y;
    }

    public struct json_layer {
        
        public json_tile[] tiles;
        public bool collision;
        public string name;
    }

    public struct map 
    {
        public Dictionary<(int,int), map_chunk> chunks;
        public UInt16 map_width;
        public UInt16 map_height;
        public Color SkyColor;

        public map(string map_path)
        {
            string tmp = File.ReadAllText($"{map_path}");
            var json = JsonObject.Parse(tmp);

            map_width = ushort.Parse(json["mapWidth"]?.ToString());
            map_height = ushort.Parse(json["mapHeight"]?.ToString());

            var layers = json["layers"] as JsonArray;
            chunks = new Dictionary<(int, int), map_chunk>();

            json_layer[] tmp_layers = new json_layer[layers.Count];

            for (int i = 0; i < tmp_layers.Length; i++) {
                var tmp_tiles = layers[i]["tiles"] as JsonArray;
                tmp_layers[i] = new json_layer() {
                    name = layers[i]["name"]?.ToString(),
                    collision = bool.Parse(layers[i]["collider"]?.ToString()),
                    tiles = new json_tile[tmp_tiles.Count]
                };

                for (int j = 0; j < tmp_tiles.Count; j++) {
                    tmp_layers[i].tiles[j] = new json_tile() {
                        id = byte.Parse(tmp_tiles[j]["id"]?.ToString()),
                        x = UInt16.Parse(tmp_tiles[j]["x"]?.ToString()),
                        y = UInt16.Parse(tmp_tiles[j]["y"]?.ToString()),
                    };

                    UInt16 Rx = (UInt16)(tmp_layers[i].tiles[j].x / 8);
                    UInt16 Ry = (UInt16)(tmp_layers[i].tiles[j].y / 8);

                    if (!chunks.ContainsKey((Rx, Ry))) {
                        
                        var spritelayers = new sprite_chunk[tmp_layers.Length];
                        for (int k = 0; k < spritelayers.Length; k++)
                            spritelayers[k] = new sprite_chunk(); 
 
                        chunks.Add((Rx, Ry), new map_chunk() { spritelayers = spritelayers });
                    }

                    map_chunk tmp_chunk = chunks[(Rx, Ry)];

                    byte lx = (byte)(tmp_layers[i].tiles[j].x % 8);
                    byte ly = (byte)(tmp_layers[i].tiles[j].y % 8);

                    if (tmp_layers[i].collision)
                        tmp_chunk.collision[lx, ly] = true;

                    tmp_chunk.spritelayers[i][lx, ly] = tmp_layers[i].tiles[j].id;

                    chunks[(Rx, Ry)] = tmp_chunk;
                }
            }
        }
    }
}
