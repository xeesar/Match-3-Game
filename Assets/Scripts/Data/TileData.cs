using Assets.Scripts.Enum;
using System;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class TileData
    {
        public TileTypeEnum tileType;

        public int Row { get; set; }
        public int Column { get; set; }
    }
}
