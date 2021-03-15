using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Conway
{
    public class Cell {
        public (int, int) _position;
        Chunk chunk;

        public Cell ((int, int) Position)
        {
            _position = Position;
            chunk = Chunk.GetChunk(Position);
            chunk.AddCell(this);
        }

    }
}