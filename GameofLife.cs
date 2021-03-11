using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System;

namespace Conway
{
    public class GameofLife : Game
    {
        private int GridSize; //The size of the grid
        private int _cellScale; //How large the Cell's are
        private Texture2D _aliveCellTexture; //The texture Cell's render with
        public static List<Cell> AliveCells; //Buffer of all alive Cell's
        public static List<Cell> SpawnBuffer; //Cell's awaiting to be created
        public static List<Cell> KillBuffer; //Cell's to be terminated
        public static List<(int, int)> CheckedDeadCells;
        private static (int, int) lastSummonedCell = (0, 0);
        List<(int, int)> _blacklistedCells = new List<(int, int)>(); //List of already checked cells; For use in resurrecting


        private Stopwatch DeltaTime = new Stopwatch();
        private Random _random = new Random();
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //Return an alive Cell at the grid or else null
        //Cell isAlive( (int, int) position ) { return new Thread( () => isAliveThreaded( (1,1))); }
        
        Cell isAlive( (int, int) position) {
            Cell cell = null;
            for (int index = 0; index < AliveCells.Count; index++) {
                if (AliveCells[index]._position == position) { cell = AliveCells[index]; break; }
            } return cell;
        }

        //Return neighbors of a cell or empty array if none
        Cell[] GridNeighbours ( (int, int) position)
        {
            List<Cell> _nearCells = new List<Cell>();
            (int, int)[] _neighbours = new (int, int)[8];
            { //Add each grid to check
                _neighbours[0] = (position.Item1 - 1, position.Item2 - 1);
                _neighbours[1] = (position.Item1 + 0, position.Item2 - 1);
                _neighbours[2] = (position.Item1 + 1, position.Item2 - 1);
                _neighbours[3] = (position.Item1 - 1, position.Item2 + 0);
                _neighbours[4] = (position.Item1 + 1, position.Item2 + 0);
                _neighbours[5] = (position.Item1 - 1, position.Item2 + 1);
                _neighbours[6] = (position.Item1 + 0, position.Item2 + 1);
                _neighbours[7] = (position.Item1 + 1, position.Item2 + 1);
            }
            foreach ( (int, int) _grid in _neighbours) {
                if (isAlive(_grid) != null) _nearCells.Add(isAlive(_grid));
                //else Resurrect(_grid);
            }
            return _nearCells.ToArray();
        }
        
        void Resurrect( (int, int) position)
        {
            if (isAlive(position) != null) return;
            int _aliveNeighbours = 0;
            (int, int)[] _neighbours = new (int, int)[8];
            { //Add each grid to check
                _neighbours[0] = (position.Item1 - 1, position.Item2 - 1);
                _neighbours[1] = (position.Item1 + 0, position.Item2 - 1);
                _neighbours[2] = (position.Item1 + 1, position.Item2 - 1);
                _neighbours[3] = (position.Item1 - 1, position.Item2 + 0);
                _neighbours[4] = (position.Item1 + 1, position.Item2 + 0);
                _neighbours[5] = (position.Item1 - 1, position.Item2 + 1);
                _neighbours[6] = (position.Item1 + 0, position.Item2 + 1);
                _neighbours[7] = (position.Item1 + 1, position.Item2 + 1);
            }
            foreach ( (int, int) _grid in _neighbours) if (isAlive(_grid) != null) _aliveNeighbours++;
            if (_aliveNeighbours == 3) SpawnBuffer.Add(new Cell(position)); Console.WriteLine("Summoned new cell at {0} {1}", position, isAlive(position));
        }

        public GameofLife()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _cellScale = 6;
            GridSize = 100;
            AliveCells = new List<Cell>();
            SpawnBuffer = new List<Cell>();
            KillBuffer = new List<Cell>();
            CheckedDeadCells = new List<(int, int)> ();

            //Create the 'aircraft carrier'
            /*AliveCells.Add(new Cell( (5, 5) ) );
            AliveCells.Add(new Cell( (6, 5) ) );
            AliveCells.Add(new Cell( (5, 6) ) );
            AliveCells.Add(new Cell( (8, 7) ) );
            AliveCells.Add(new Cell( (8, 8) ) );
            AliveCells.Add(new Cell( (7, 8) ) );
            AliveCells.Add(new Cell( (6, 6) ) );*/

            //Create a replicator
            AliveCells.Add(new Cell( (3, 3) ) );
            AliveCells.Add(new Cell( (4, 3) ) );
            AliveCells.Add(new Cell( (5, 3) ) );
            AliveCells.Add(new Cell( (3, 4) ) );
            AliveCells.Add(new Cell( (5, 4) ) );
            AliveCells.Add(new Cell( (3, 5) ) );
            AliveCells.Add(new Cell( (4, 5) ) );
            AliveCells.Add(new Cell( (5, 5) ) );

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 920;
            _graphics.ApplyChanges();
            IsMouseVisible = true;
            /*IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1);
            _graphics.SynchronizeWithVerticalRetrace = false;*/

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            /* \/Create the cell texture\/ */
            _aliveCellTexture = new Texture2D(GraphicsDevice, _cellScale, _cellScale); //Declare texture
            Color[] _textureColor = new Color[_cellScale * _cellScale]; //New Color[] to fill texture with
            for (int _index = 0; _index < _textureColor.Length; _index++) //Populate _textureColor
                { _textureColor[_index] = Color.Black; }
            for (int _index = 0; _index < _cellScale * _cellScale; _index++) { //Fill texture with _textureColor
                _aliveCellTexture.SetData<Color>(_textureColor);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            //Spawn cells when spacebar is held
            if (Keyboard.GetState().IsKeyDown(Keys.Space)) {
                for (int _index = 0; _index < 500; _index++)
                {
                    (int, int) _newCoords;
                    do {
                        _newCoords = (_random.Next(0, GridSize), _random.Next(0, GridSize));
                    } while (isAlive(_newCoords) != null);
                    AliveCells.Add(new Cell(_newCoords));
                }
            }

            //Check simulation if enough time has passed
            DeltaTime.Start(); //Make sure timer is started
            if (DeltaTime.Elapsed > TimeSpan.FromMilliseconds(20))
            { DeltaTime.Reset(); //Reset the delta

                //Before anything else: remove duplicates
                {
                    List<Cell> _checkedCells = new List<Cell>();
                    int _targetIndex = AliveCells.Count;
                    for (int _index = 0; _index < _targetIndex; _index++)
                    {
                        bool _valid = true;
                        foreach (Cell _cellChecked in _checkedCells)
                        {
                            if (_cellChecked._position == AliveCells[_index]._position) {
                                AliveCells.Remove(AliveCells[_index]);
                                _targetIndex--; _index--; _valid = false;
                                break;
                            }
                        }
                        if (_valid) _checkedCells.Add(AliveCells[_index]);
                    }
                }

                //Check which cells gets killed
                foreach (Cell _cell in AliveCells)
                {
                    Cell[] _neighbours = GridNeighbours(_cell._position);
                    if (_neighbours.Length < 2 || _neighbours.Length > 3) KillBuffer.Add(_cell);
                }

                //Check which neighbours should be resurrected (if there are any)
                //TODO: Check more than one cell per thread
                if (AliveCells.Count > 0)
                {
                    //List<(int, int)> _blackListedCells = new List<(int, int)>(); //Don't check already known cells
                    _blacklistedCells.Clear(); //Clear the blacklisted cells before beginning
                    List<Thread> _concurrentThreads = new List<Thread>();
                    for (int _index = 0; _index < AliveCells.Count - 1; _index++) //Check each cell to have neighbours resurrected
                    {
                        _concurrentThreads.Add(new Thread(() => resurrectMethod(AliveCells[_index])));
                        _concurrentThreads[_index].Start();
                    }
                    foreach (Thread _thread in _concurrentThreads)
                        _thread.Join(); //Wait for all threads to finalize

                    /*{
                        (int, int)[] _neighbours = new (int, int)[8]; //Save this cells neighbouring positions
                        { //Add each grid to check
                            _neighbours[0] = (_cell._position.Item1 - 1, _cell._position.Item2 - 1);
                            _neighbours[1] = (_cell._position.Item1 + 0, _cell._position.Item2 - 1);
                            _neighbours[2] = (_cell._position.Item1 + 1, _cell._position.Item2 - 1);
                            _neighbours[3] = (_cell._position.Item1 - 1, _cell._position.Item2 + 0);
                            _neighbours[4] = (_cell._position.Item1 + 1, _cell._position.Item2 + 0);
                            _neighbours[5] = (_cell._position.Item1 - 1, _cell._position.Item2 + 1);
                            _neighbours[6] = (_cell._position.Item1 + 0, _cell._position.Item2 + 1);
                            _neighbours[7] = (_cell._position.Item1 + 1, _cell._position.Item2 + 1);
                        }
                        foreach ((int, int) _neighbourCell in _neighbours) { //For each neighbour...:
                            if (isAlive(_neighbourCell) != null) break; //Skip if the cell is already alive
                            foreach ((int, int) _blackListedCell in _blacklistedCells) { //Check if this cell has already been checked
                                if (_neighbourCell == _blackListedCell) goto Post_Resurrect; //If so, skip
                        }                                                                   //Else:
                            if (GridNeighbours(_neighbourCell).Length == 3) { //Does it have 3 neighbours?
                                SpawnBuffer.Add(new Cell(_neighbourCell));
                                if (lastSummonedCell == _neighbourCell) Console.Write("DUPLICATE SPAWN! ");
                                lastSummonedCell = _neighbourCell;
                                Console.WriteLine("Summoned Cell at {0}", _neighbourCell);
                            }
                        }
                        Post_Resurrect: { /* Goto here if this Cell has been checked to skip / }
                    }*/

                }
    
                //If _spawn || _kill buffers != empty then
                //TODO: Make it not start a thread when under threshold
                if (SpawnBuffer.Count != 0)
                {
                    List<Thread> _concurrentThreads = new List<Thread>();
                    int _threads = (int)Math.Ceiling((double)SpawnBuffer.Count / 400);
                    for (int _index = 0; _index < _threads; _index++)
                    {
                        _concurrentThreads.Add(new Thread(() => spawnBufferMethod(_index * 400, _index * 400 + 399)));
                        _concurrentThreads[_index].Start();
                    }
                    foreach (Thread _thread in _concurrentThreads)
                        _thread.Join(); //Wait for all threads to finalize
                    SpawnBuffer.Clear(); //Clear buffer
                }
                /*{
                    foreach (Cell _cell in SpawnBuffer)
                    {
                        AliveCells.Add(_cell);
                    } SpawnBuffer.Clear(); //Clear buffer
                }*/

                if (KillBuffer.Count != 0)
                {
                    foreach (Cell _cell in KillBuffer)
                    {
                        if (!AliveCells.Remove(_cell)) Console.WriteLine("FAILED TO KILL CELL {0}!", _cell._position);
                        Console.WriteLine("Killed cell at {0}", _cell._position);
                    } KillBuffer.Clear(); //Clear buffer
                }
                //End of update block
            }

            base.Update(gameTime);
        }

        void resurrectMethod(Cell cell)
        {
            (int, int)[] _neighbours = new (int, int)[8]; //Save this cells neighbouring positions
            { //Add each grid to check
                _neighbours[0] = (cell._position.Item1 - 1, cell._position.Item2 - 1);
                _neighbours[1] = (cell._position.Item1 + 0, cell._position.Item2 - 1);
                _neighbours[2] = (cell._position.Item1 + 1, cell._position.Item2 - 1);
                _neighbours[3] = (cell._position.Item1 - 1, cell._position.Item2 + 0);
                _neighbours[4] = (cell._position.Item1 + 1, cell._position.Item2 + 0);
                _neighbours[5] = (cell._position.Item1 - 1, cell._position.Item2 + 1);
                _neighbours[6] = (cell._position.Item1 + 0, cell._position.Item2 + 1);
                _neighbours[7] = (cell._position.Item1 + 1, cell._position.Item2 + 1);
            }
            foreach ((int, int) _neighbourCell in _neighbours) { //For each neighbour...:
                if (isAlive(_neighbourCell) != null) break; //Skip if the cell is already alive
                for (int _index = 0; _index < _blacklistedCells.Count; _index++) { //Check if this cell has already been checked
                    if (_neighbourCell == _blacklistedCells[_index]) return; //If so, skip
                }                                                   //Else:
                if (GridNeighbours(_neighbourCell).Length == 3) { //Does it have 3 neighbours?
                    SpawnBuffer.Add(new Cell(_neighbourCell));
                    //Console.WriteLine("Summoned Cell at {0}", _neighbourCell);
                }
                _blacklistedCells.Add(_neighbourCell);
            }
        }

        void spawnBufferMethod(int startRange, int endRange)
        {
            if (endRange > SpawnBuffer.Count) endRange = SpawnBuffer.Count;
            for (int _index = startRange; _index < endRange; _index++)
            {
                AliveCells.Add(SpawnBuffer[_index]);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Wheat);
            _spriteBatch.Begin();
            try {
            foreach (Cell element in AliveCells)
            {
                Vector2 _position = new Vector2(element._position.Item1*_cellScale, element._position.Item2*_cellScale);
                _spriteBatch.Draw(_aliveCellTexture, _position, Color.White);
            } } catch {}
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
