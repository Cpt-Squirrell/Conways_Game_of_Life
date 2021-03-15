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
        private int GridSize; //The size of the grid/ zoom level
        public static int ChunkSize; //The size of one chunk
        public int CellScale; //How large the Cell's are
        private Texture2D CellTexure; //The texture Cell's render with
        public static List<Cell> AliveCells; //Buffer of all alive Cell's
        public static List<Cell> SpawnBuffer; //Cell's awaiting to be created
        public static List<Cell> KillBuffer; //Cell's to be terminated
        public static List<(int, int)> CheckedDeadCells; //Preventing checking same cell multiple times
        //private static (int, int) lastSummonedCell = (0, 0); //For debugging
        List<(int, int)> _blacklistedCells = new List<(int, int)>(); //List of already checked cells; For use in resurrecting

        private static Vector2 CameraPosition = new Vector2(0, 0);
        private int _oldMouseWheelValue = Mouse.GetState().ScrollWheelValue;

        private Stopwatch DeltaTime = new Stopwatch();
        private Random _random = new Random();
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //Return an alive Cell at the grid or else null
        //Cell isAlive( (int, int) position ) { return new Thread( () => isAliveThreaded( (1,1))); }
        
        Cell isAlive( (int, int) position) {
            Chunk[] chunks = new Chunk[9];
            { //Add each grid to check
                chunks[0] = (Chunk.GetChunk((position.Item1 - 1, position.Item2 - 1)));
                chunks[1] = (Chunk.GetChunk((chunks[0].Position.Item1 + 0, chunks[0].Position.Item2 - 1)));
                chunks[2] = (Chunk.GetChunk((chunks[0].Position.Item1 + 1, chunks[0].Position.Item2 - 1)));
                chunks[3] = (Chunk.GetChunk((chunks[0].Position.Item1 - 1, chunks[0].Position.Item2 + 0)));
                chunks[4] = (Chunk.GetChunk((chunks[0].Position.Item1 + 1, chunks[0].Position.Item2 + 0)));
                chunks[5] = (Chunk.GetChunk((chunks[0].Position.Item1 - 1, chunks[0].Position.Item2 + 1)));
                chunks[6] = (Chunk.GetChunk((chunks[0].Position.Item1 + 0, chunks[0].Position.Item2 + 1)));
                chunks[7] = (Chunk.GetChunk((chunks[0].Position.Item1 + 1, chunks[0].Position.Item2 + 1)));
            }
            foreach (Chunk chunk in chunks)
            {
                List<Cell> cells = chunk.GetCells();
                foreach (Cell cell in cells)
                    { if (cell._position == position) return cell; }
            }
            return null;
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
            CellScale = 5;
            GridSize = 100;
            ChunkSize = 20;
            AliveCells = new List<Cell>();
            SpawnBuffer = new List<Cell>();
            KillBuffer = new List<Cell>();
            CheckedDeadCells = new List<(int, int)> ();

            //Initialize and start cameraThread
            Thread CameraThread = new Thread(cameraThread);
            CameraThread.Start();

            //Create the 'aircraft carrier'
            /*AliveCells.Add(new Cell( (5, 5) ) );
            AliveCells.Add(new Cell( (6, 5) ) );
            AliveCells.Add(new Cell( (5, 6) ) );
            AliveCells.Add(new Cell( (8, 7) ) );
            AliveCells.Add(new Cell( (8, 8) ) );
            AliveCells.Add(new Cell( (7, 8) ) );
            AliveCells.Add(new Cell( (6, 6) ) );*/

            //Create a replicator
            /*AliveCells.Add(new Cell( (3, 3) ) );
            AliveCells.Add(new Cell( (4, 3) ) );
            AliveCells.Add(new Cell( (5, 3) ) );
            AliveCells.Add(new Cell( (3, 4) ) );
            AliveCells.Add(new Cell( (5, 4) ) );
            AliveCells.Add(new Cell( (3, 5) ) );
            AliveCells.Add(new Cell( (4, 5) ) );
            AliveCells.Add(new Cell( (5, 5) ) );*/

            //Create a replicator
            AliveCells.Add(new Cell( (100, 100) ) );
            AliveCells.Add(new Cell( (103, 100) ) );
            AliveCells.Add(new Cell( (100, 101) ) );
            AliveCells.Add(new Cell( (101, 101) ) );
            AliveCells.Add(new Cell( (102, 101) ) );
            AliveCells.Add(new Cell( (101, 102) ) );

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 920;
            _graphics.ApplyChanges();
            IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            refreshTextures();
        }

        protected override void Update(GameTime gameTime)
        {
            //Spawn cells when spacebar is held
            if (Keyboard.GetState().IsKeyDown(Keys.Space)) {
                for (int _index = 0; _index < 600; _index++)
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
                if (AliveCells.Count > 0)
                {
                    _blacklistedCells.Clear(); //Clear the blacklisted cells before beginning
                    List<Thread> _concurrentThreads = new List<Thread>();
                    int _threads = (int)Math.Ceiling((double)AliveCells.Count / 100);
                    for (int _index = 0; _index < _threads; _index++) //Check each cell to have neighbours resurrected
                    {
                        int _endRange;
                        int _startRange = _index * 100;
                        if (_index * 100 + 99 > AliveCells.Count) _endRange = AliveCells.Count;
                        else _endRange = _index * 100 + 99;
                        Cell[] _cellArray = new Cell[_endRange - _startRange];
                        for (int _field = 0; _field < _cellArray.Length; _field++)
                            _cellArray[_field] = AliveCells[_index * 100 + _field];
                        _concurrentThreads.Add(new Thread(() => resurrectMethod(_cellArray)));
                        _concurrentThreads[_index].Start();
                    }
                    foreach (Thread _thread in _concurrentThreads)
                        _thread.Join(); //Wait for all threads to finalize

                }
    
                //If _spawn || _kill buffers != empty then
                //TODO: Make it not start a thread when under threshold
                if (SpawnBuffer.Count != 0)
                /*{
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
                }*/
                {
                    foreach (Cell _cell in SpawnBuffer)
                    {
                        AliveCells.Add(_cell);
                    } SpawnBuffer.Clear(); //Clear buffer
                }

                if (KillBuffer.Count != 0)
                {
                    foreach (Cell _cell in KillBuffer)
                    {
                        if (!AliveCells.Remove(_cell)) Console.WriteLine("FAILED TO KILL CELL {0}!", _cell._position);
                        //Console.WriteLine("Killed cell at {0}", _cell._position);
                    } KillBuffer.Clear(); //Clear buffer
                }
                //End of update block
            }

            base.Update(gameTime);
        }

        void resurrectMethod(Cell[] cellArray)
        {
            foreach (Cell cell in cellArray)
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
                    if (isAlive(_neighbourCell) != null) continue; //Skip if the cell is already alive
                    bool _isChecked = false;
                    for (int _index = 0; _index < _blacklistedCells.Count; _index++) { //Check if this cell has already been checked
                        if (_neighbourCell == _blacklistedCells[_index]) _isChecked = true; break; //If so, skip
                    } if (_isChecked) continue;                                                   //Else:
                    if (GridNeighbours(_neighbourCell).Length == 3) { //Does it have 3 neighbours?
                        SpawnBuffer.Add(new Cell(_neighbourCell));
                        //Console.WriteLine("Summoned Cell at {0}", _neighbourCell);
                    }
                    _blacklistedCells.Add(_neighbourCell);
                }
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

        void refreshTextures() //Make new textures using the scales
        {
            CellTexure = new Texture2D(GraphicsDevice, CellScale, CellScale); //Declare texture
            Color[] _textureColor = new Color[CellScale * CellScale]; //New Color[] to fill texture with
            for (int _index = 0; _index < _textureColor.Length; _index++) //Populate _textureColor
                { _textureColor[_index] = Color.Black; }
            for (int _index = 0; _index < CellScale * CellScale; _index++) { //Fill texture with _textureColor
                CellTexure.SetData<Color>(_textureColor);
            }
        }

        void cameraThread()
        {
            Stopwatch _stopWatch = new Stopwatch();
            _stopWatch.Start();
            while (true)
            {
                CameraPosition.X += 1 * _stopWatch.ElapsedMilliseconds;
                if (Keyboard.GetState().IsKeyDown(Keys.D)) CameraPosition.X -= 1 * _stopWatch.ElapsedMilliseconds;
                if (Keyboard.GetState().IsKeyDown(Keys.W)) CameraPosition.Y += 1 * _stopWatch.ElapsedMilliseconds;
                if (Keyboard.GetState().IsKeyDown(Keys.S)) CameraPosition.Y -= 1 * _stopWatch.ElapsedMilliseconds;
                if (Mouse.GetState().ScrollWheelValue > _oldMouseWheelValue && CellScale < 10) //If the wheel has increased
                {
                    CellScale++;
                    //Math.Clamp(CellScale, 1, 10); //Make sure it does not enter extremes
                    refreshTextures();
                }
                else if (Mouse.GetState().ScrollWheelValue < _oldMouseWheelValue && CellScale > 1) //If the wheel has decreased
                {
                    CellScale--;
                    //Math.Clamp(CellScale, 1, 10); //Make sure it does not enter extremes
                    refreshTextures();
                }
                _oldMouseWheelValue = Mouse.GetState().ScrollWheelValue; //Set new _old value for mouse wheel

                //Console.WriteLine(Mouse.GetState().ScrollWheelValue);
                //Console.WriteLine(CellScale);
                _stopWatch.Reset();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Wheat);
            _spriteBatch.Begin();
            try {
            foreach (Cell element in AliveCells)
            {
                Vector2 _position = new Vector2(element._position.Item1*CellScale, element._position.Item2*CellScale);
                _spriteBatch.Draw(CellTexure, _position + CameraPosition, Color.White);
            } } catch {}
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
