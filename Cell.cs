using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Conway
{
    public class Cell {
        public (int, int) _position;

        public Cell ((int, int) Position)
        {
            _position = Position;
        }

        /*
        public void CheckNeighbours ()
        {
            int _neighbours = 0;
            //Two dimensional loop checking eight tiles sideways and diagonally
            for (int _index = _position.Item1 - 1; _index < _position.Item1 + 1; _index++) {
                for (int _field = _position.Item2 - 1; _field < _position.Item2 + 1; _field++)
                {
                    if (!( (_index, _field) == _position) ) //Don't check yourself
                    {
                        //Query each tile for alive Cell's
                        //Todo: Make query originate from cell instead of global
                        foreach (Cell _cell in GameofLife.AliveCells)
                            if (_cell._position == (_index, _field)) _neighbours++;
                            else CheckNeighbours( (_index, _field) ); //Check if to-be resurrected
                    }
                }
            }

            //Todo: if Cell is over/undercrowded then suicide
            if (_neighbours < 2 || _neighbours > 3)
            {
                GameofLife.KillBuffer.Add(this);
            }
        }

        public void CheckNeighbours ((int, int) _gridLocation)
        {
            int _neighbours = 0;
            //Two dimensional loop checking eight tiles sideways and diagonally
            for (int _index = _gridLocation.Item1 - 1; _index < _gridLocation.Item1 + 1; _index++) {
                for (int _field = _gridLocation.Item2 - 1; _field < _gridLocation.Item2 + 1; _field++)
                {
                    if (!( (_index, _field) == _position) ) //Don't check yourself
                        foreach ((int, int) _checkedGridLocation in GameofLife.CheckedDeadCells) // \/
                            if (_checkedGridLocation == (_index, _field) ) goto CheckSkip; //Check if current grid is already checked
                        {
                            //Todo: Query each tile for alive Cell's
                            foreach (Cell _cell in GameofLife.AliveCells)
                                if (_cell._position == (_index, _field)) _neighbours++;
                        }
                    CheckSkip:
                    {
                    }
                }
            }
            if (_neighbours == 3)
                {
                    GameofLife.SpawnBuffer.Add(new Cell(_gridLocation)); //Add new entry to SpawnBuffer
                    GameofLife.CheckedDeadCells.Add(_gridLocation); //Add new entry to CheckedDeadCells
                }
        }
        */

    }
}