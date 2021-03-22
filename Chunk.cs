using System.Collections.Generic;
using System;

namespace Conway
{
    class Chunk
    {
        public (int, int) Position;
        private List<Cell> Cells;
        public static List<Chunk> Chunks = new List<Chunk>();

        Chunk( (int, int) position)
        {
            Cells = new List<Cell>();
            foreach (Chunk chunk in Chunks)
            {
                if (chunk.Position == position)
                    throw(new Exception()); //Do not allow multiple chunks
                else Position = position;
            }
        }

        public static Chunk GetChunk( (int X, int Y) position)
        {
            (int, int) _position = //Translate cell position to chunk position
                ((int)Math.Floor((double)(position.X / GameofLife.ChunkSize)),
                 (int)Math.Floor((double)(position.Y / GameofLife.ChunkSize)));
            foreach (Chunk chunk in Chunks)
                { if (chunk.Position == _position) return chunk; }
            return new Chunk(position);
        }
        public static Chunk GetChunk( (int X, int Y) position, bool isChunkTransform)
        {
            foreach (Chunk chunk in Chunks)
                { if (chunk.Position == position) return chunk; }
            return new Chunk(position);
        }

        public void AddCell(Cell cell) { Cells.Add(cell); }
        public List<Cell> GetCells() { return Cells; }
        public List<Cell> GetCells((int, int) exclude)
        {
            List<Cell> _excludedCells = new List<Cell>();
            foreach (Cell cell in Cells)
            {
                if (cell._position != exclude)
                    _excludedCells.Add(cell);
            } return _excludedCells;
        }

    }
}