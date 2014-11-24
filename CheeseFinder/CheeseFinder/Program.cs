using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CheeseFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(Console.WindowWidth, Console.LargestWindowHeight);
            CheeseFinder game = new CheeseFinder();
            game.PlayGame();
        }
    }

    /// <summary>
    /// A game of CheeseFinder. Control the Mouse to find the cheese.
    /// </summary>
    class CheeseFinder
    {
        
        //-------------------------------
        const bool _showGrid = false;  //
        const bool _placeWalls = true; //
        const int _gridXNodes = 20;    //For quick setting of options.
        const int _gridYNodes = 20;    //
        const int _wallChance = 25;    //
        const int _catMoveChance = 50;
        //-------------------------------


        private Point[,] _grid = new Point[_gridXNodes, _gridYNodes];
        public Point[,] Grid
        {
            get { return _grid; }
            set { _grid = value; }
        }

        private Point _mouse;
        public Point Mouse 
        {
            get { return _mouse; }
            set { _mouse = value; } 
        }

        private Point _cheese;
        public Point Cheese
        {
            get { return _cheese; }
            set { _cheese = value; }
        }
        private List<Point> _cats = new List<Point>();
        public List<Point> Cats
        {
            get { return _cats; }
            set { _cats = value; }
        }

        private int _score;
        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        private int _highScore = 0;
        public int HighScore
        {
            get { return _score; }
            set { _score = value; }
        }

        Random rng = new Random(); //Defined here so that rng.Next() will not repeat numbers as easily.

        // ---- Constructors


        /// <summary>
        /// Generates the grid for the game, and randomly places the mouse/cheese.
        /// </summary>
        public CheeseFinder()
        {
            this.Initialize();
        }


        // ---- Methods


        public void Initialize()
        {
            int randX = rng.Next(Grid.GetUpperBound(0) + 1); //GridUpperBound is the length of the array.
            int randY = rng.Next(Grid.GetUpperBound(1) + 1); //0 for x, 1 for y.

            while (Cats.Count > 0)
                Cats.RemoveAt(0);

            this.Score = 0;

            //Loop as long as mouse can reach BOTH cat AND cheese.
            do //We re-create the entire grid in case the mouse is trapped in a VERY tiny space.
            {
                //Create a new point at each node of Grid
                for (int y = 0; y < Grid.GetUpperBound(1) + 1; y++)
                    for (int x = 0; x < Grid.GetUpperBound(0) + 1; x++)
                    {
                        Grid[x, y] = new Point(x, y);
                        //Randomly place a wall at this point if const is true. This can possibly block the cheese in -
                        //Todo: attempt to ensure that this won't happen.
                        if (_placeWalls)
                            if (rng.Next(101) < _wallChance)
                                Grid[x, y].Contains = PointContents.Wall;
                    }

                //Position the Mouse at a random point on the field. Note that mouse is placed first.
                PlaceMouse();
                SetMouseDistance(false); //PlaceCheese and PlaceCats rely on MouseDistance values.
                //Position the Cheese at a random point on the field.
                PlaceCheese();
                //Positon the Cat at a random point on the field.
                if(Cats.Count == 1) //This happens if we had to loop at least once.
                    Cats.RemoveAt(0); //Remove old cat before placing new one.
                PlaceCat();

            } while (!SetMouseDistance(true)); // Returns false if mouse can't reach cats and cheese.

        }


        public bool SetMouseDistance(bool startup)
        {
            int counter = 0;
            bool notYetFilled = true; //Set to true when all reachable grid points have been filled with MouseDistance values.

            //Initialize: Set all MouseDistance values to 10000, then the mouse's to 0.
            for (int y = 0; y < Grid.GetUpperBound(1) + 1; y++)
                for (int x = 0; x < Grid.GetUpperBound(0) + 1; x++)
                    Grid[x, y].MouseDistance = 10000;

            Grid[Mouse.X, Mouse.Y].MouseDistance = 0;

            while(notYetFilled) // Loop until all possible points have been filled.
            {
                notYetFilled = false; //Set to true if a point has been filled.

                for (int y = 0; y < Grid.GetUpperBound(1) + 1; y++)
                {
                    for (int x = 0; x < Grid.GetUpperBound(0) + 1; x++)
                    {
                        if (Grid[x, y].MouseDistance == counter) //This is how it knows where to add numbers.
                        {
                            if (startup) //This is used to initialize the levels. CatAvoids ignored.
                            {
                                //if a value here is set at all, it means that the grid has not yet been filled up.

                                //Set value to the left to one higher than counter.
                                if (x > 0)
                                    if (!(Grid[x - 1, y].Contains == PointContents.Wall) && Grid[x - 1, y].MouseDistance == 10000)
                                    {
                                        Grid[x - 1, y].MouseDistance = counter + 1;
                                        notYetFilled = true;
                                    }
                                //Set value to the right.
                                if (x < Grid.GetUpperBound(0))
                                    if (!(Grid[x + 1, y].Contains == PointContents.Wall) && Grid[x + 1, y].MouseDistance == 10000)
                                    {
                                        Grid[x + 1, y].MouseDistance = counter + 1;
                                        notYetFilled = true;
                                    }
                                //Set value above.
                                if (y > 0)
                                    if (!(Grid[x, y - 1].Contains == PointContents.Wall) && Grid[x, y - 1].MouseDistance == 10000)
                                    {
                                        Grid[x, y - 1].MouseDistance = counter + 1;
                                        notYetFilled = true;
                                    }

                                //Set value below.
                                if (y < Grid.GetUpperBound(1))
                                    if (!(Grid[x, y + 1].Contains == PointContents.Wall) && Grid[x, y + 1].MouseDistance == 10000)
                                    {
                                        Grid[x, y + 1].MouseDistance = counter + 1;
                                        notYetFilled = true;
                                    }

                            }
                            else //This will be used to calculate cat movement. CatAvoids followed.
                            {
                                //if a value here is set at all, it means that the grid has not yet been filled up.

                                //Set value to the left to one higher than the counter.
                                if (x > 0)
                                    if (!(Grid[x - 1, y].Contains == PointContents.Wall) 
                                        && Grid[x - 1, y].MouseDistance == 10000
                                        && !(Grid[x - 1, y].CatAvoids))
                                    {
                                        Grid[x - 1, y].MouseDistance = counter + 1;
                                        notYetFilled = true;
                                    }
                                //Set value to the right.
                                if (x < Grid.GetUpperBound(0))
                                    if (!(Grid[x + 1, y].Contains == PointContents.Wall)
                                        && Grid[x + 1, y].MouseDistance == 10000
                                        && !(Grid[x + 1, y].CatAvoids))
                                    {
                                        Grid[x + 1, y].MouseDistance = counter + 1;
                                        notYetFilled = true;
                                    }
                                //Set value above.
                                if (y > 0)
                                    if (!(Grid[x, y - 1].Contains == PointContents.Wall)
                                        && Grid[x, y - 1].MouseDistance == 10000
                                        && !(Grid[x, y - 1].CatAvoids))
                                    {
                                        Grid[x, y - 1].MouseDistance = counter + 1;
                                        notYetFilled = true;
                                    }

                                //Set value below.
                                if (y < Grid.GetUpperBound(1))
                                    if (!(Grid[x, y + 1].Contains == PointContents.Wall) 
                                        && Grid[x, y + 1].MouseDistance == 10000
                                        && !(Grid[x, y + 1].CatAvoids))
                                    {
                                        Grid[x, y + 1].MouseDistance = counter + 1;
                                        notYetFilled = true;
                                    }
                            }
                        }
                    }
                }
                counter++; //Will make loop now look for values 1 higher than before.
            }

            //We are outside the while loop. Check to see if the cheese and cat have values assigned to them.
            //If not the cat, mouse, and cheese cannot reach each other. Return accordingly.

            //A value of 10000 means that the distance valaue has not been set, therefore is
            //unreachable.

            //Startup checked here because the grid is not filled with the Cheese Point instantiated when first called.
            if (startup
                && Grid[Cheese.X, Cheese.Y].MouseDistance != 10000
                && !(Cats.Any(x => Grid[x.X, x.Y].MouseDistance == 10000))) //Cheese and all cats are reachable
                    return true;

            return false;
        }

        /// <summary>
        /// Places the Mouse in the Grid.
        /// </summary>
        public void PlaceMouse()
        {
            int randX;
            int randY;
            bool placed = false;
            
            do
            {
                randX = rng.Next(Grid.GetUpperBound(0) + 1);
                randY = rng.Next(Grid.GetUpperBound(1) + 1);

                if (Grid[randX, randY].CanMoveTo) //Place is there is no wall in the way.
                {
                    Grid[randX, randY].Contains = PointContents.Mouse;
                    this.Mouse = new Point(randX, randY);
                    placed = true;
                }
            } while (!placed); //Loop until mouse is placed.

        }

        /// <summary>
        /// Places the Cheese in the Grid.
        /// </summary>
        public void PlaceCheese()
        {
            int randX;
            int randY;
            bool placed = false;
            
            do
            {
                //Generate random cheese location.
                randX = rng.Next(Grid.GetUpperBound(0) + 1);
                randY = rng.Next(Grid.GetUpperBound(1) + 1);
                
                //Place cheese under the conditions of: 1) Mouse not already at rngNext's generated loctaion. 2) The mouse can get
                //to the cheese.
                if (Grid[randX, randY].CanMoveTo && Grid[randX, randY].MouseDistance != 10000) 
                {                                                                               
                    Grid[randX, randY].Contains = PointContents.Cheese;
                    this.Cheese = new Point(randX, randY);
                    placed = true;
                }
            } while (!placed); //Loop until cheese is placed.

        }

        /// <summary>
        /// Adds a cat in the Field.
        /// </summary>
        public void PlaceCat()
        {
            int randX;
            int randY;
            bool placed = false;

            do
            {
                //Generate random cat location.
                randX = rng.Next(Grid.GetUpperBound(0) + 1);
                randY = rng.Next(Grid.GetUpperBound(1) + 1);

                //Place cat under the conditions of: 1) Other cats,  Mouse and cheese not already at rngNext's generated 
                // loctaion. 2) This cat can get to the mouse.
                if (Grid[randX, randY].CanMoveTo 
                    && !Grid[randX, randY].CatAvoids
                    && Grid[randX, randY].MouseDistance != 10000)
                {                                                                  
                    Grid[randX, randY].Contains = PointContents.Cat;
                    this.Cats.Add(new Point(randX, randY));
                    placed = true;
                }
            } while (!placed); //Loop until cat is placed.

        }
        
        /// <summary>
        /// Writes the game field to the console.
        /// </summary>
        /// <param name="displayGrid">True to display the brackets at every node.</param>
        public void DrawGrid(bool displayGrid)
        {
            for (int y = 0; y < Grid.GetUpperBound(1) + 1; y++)
            {
                if (y == 0)
                {
                    //Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
                    //Draw the border. 
                    //for (int i = 0; i < (Console.WindowHeight / 4); i++ )
                        Console.Write("\n");

                    for (int i = 0; i < (Console.WindowWidth / 4) - ((Grid.GetUpperBound(0) + 1) / 2) + 1; i++)
                        Console.Write(" "); //Keep grid centered - one space for every 4 window width units.

                    for (int i = 0; i < Grid.GetUpperBound(0) + 1; i++)
                        Console.Write("---"); //Top border. Three dashes per node in the list.

                    Console.WriteLine(); //New line before grid is drawn.
                }
                for (int x = 0; x < Grid.GetUpperBound(0) + 1; x++)
                {
                    if (x == 0)
                    {
                        for (int i = 0; i < Console.WindowWidth / 4 - ((Grid.GetUpperBound(0) + 1) / 2); i++)
                            Console.Write(" "); //Keep grid centered - one space for every 4 window width units.
                        
                        Console.Write("|"); //Border before each row.
                    }
                    //The grid itself.
                    Console.Write(Grid[x, y].DrawPoint(_showGrid));
                    Console.ResetColor(); //In case color was changed by Mouse or Cheese.

                    if (x == Grid.GetUpperBound(0))
                        Console.Write("|"); //Border after each row.
                }
                Console.WriteLine(); //New line before bottom border.
            }

            for (int i = 0; i < (Console.WindowWidth / 4) - ((Grid.GetUpperBound(0) + 1) / 2) + 1; i++)
                Console.Write(" "); //Keep grid centered - one space for every 4 window width units.

            for (int i = 0; i < Grid.GetUpperBound(0) + 1; i++)
                Console.Write("---"); //Bottom border. Three dashes per node.
        }

        /// <summary>
        /// Waits for user input and checks to make sure the user hits the arrow keys. Calls ValidMove to ensure that the mouse
        /// can move to the new area (No walls or borders)
        /// </summary>
        /// <returns>The key that was pressed, or "L" if not.</returns>
        public ConsoleKey getUserMove()
        {
            ConsoleKeyInfo input = Console.ReadKey();

            //Only arrows are valid input.
            if (input.Key == ConsoleKey.UpArrow ||
               input.Key == ConsoleKey.DownArrow ||
               input.Key == ConsoleKey.LeftArrow ||
               input.Key == ConsoleKey.RightArrow)
                if (ValidMove(input.Key))
                    return input.Key;
                else
                    return ConsoleKey.L;
            else
            {
                Console.WriteLine("Move your mouse with the arrow keys!");
                return ConsoleKey.L;
            }

        }

        /// <summary>
        /// Ensures that the mouse can move to the new location.
        /// </summary>
        /// <param name="input">The key that the user pressed.</param>
        /// <returns>False if the new location is invalid.</returns>
        public bool ValidMove(ConsoleKey input)
        {
            // Validate each potential key individually.
            switch (input)
            {
                case ConsoleKey.UpArrow:
                    if (this.Mouse.Y > 0)
                        if(Grid[Mouse.X, Mouse.Y - 1].CanMoveTo)
                            return true;

                    break;

                case ConsoleKey.DownArrow:
                    if (this.Mouse.Y < Grid.GetUpperBound(1))
                        if (Grid[Mouse.X, Mouse.Y + 1].CanMoveTo)
                            return true;

                    break;

                case ConsoleKey.LeftArrow:
                    if (this.Mouse.X > 0)
                        if (Grid[Mouse.X - 1, Mouse.Y].CanMoveTo)
                            return true;

                    break;

                case ConsoleKey.RightArrow:
                    if (this.Mouse.X < Grid.GetUpperBound(0))
                        if (Grid[Mouse.X + 1, Mouse.Y].CanMoveTo)
                           return true;

                    break;
            }

            //Will only get here if no arrow was pressed.
            Console.WriteLine("Not a valid move");
            return false;
        }

        /// <summary>
        /// Calls getUserMove to accept and validate input. Physically moves the mouse if valid.
        /// </summary>
        /// <returns></returns>
        public bool MoveMouse()
        {
            ConsoleKey userMove = getUserMove(); //Input is validated here.
            Grid[Mouse.X, Mouse.Y].Contains = PointContents.Space; //Mouse likely no longer to be here after press.

            //Change position if arrow is returned by getUserMove(). "L," the character returned upon incorrect
            //input, will simply do nothing.
            switch (userMove)
            {
                case ConsoleKey.UpArrow:
                    Mouse.Y = Mouse.Y - 1;
                    break;

                case ConsoleKey.DownArrow:
                    Mouse.Y = Mouse.Y + 1; 
                    break;

                case ConsoleKey.LeftArrow:
                    Mouse.X = Mouse.X - 1;
                    break;

                case ConsoleKey.RightArrow:
                    Mouse.X = Mouse.X + 1;
                    break;
            }

            //If the cheese is in the same place as the mouse, the user wins.
            if ((Mouse.X == Cheese.X) && (Mouse.Y == Cheese.Y))
                return true;

            //If not, move the mouse. If L is returned, redraw in old position.
            Grid[Mouse.X, Mouse.Y].Contains = PointContents.Mouse;
            return false;

        }

        public bool MoveCats()
        {
            foreach(Point cat in Cats)
            {
                Point pointToMove = new Point(0, 0); //Will hold the x, y value of where the cat should move.
                bool doMove = false; //If a cat cannot move because of being surrounded, this will stop it from
                //occupying the space that something else is already in.

                //We must call setMouseDistance before moving every cat so that previous cat's movements are
                //compensated for. Reset doMove.
                this.SetMouseDistance(false); //Sets the move priority of each Point. The lower the number, the closer the mouse.
                doMove = false;

                if (rng.Next(100) <= _catMoveChance) //Cats have a 50% chance to move.
                {
                    //The non-nested if statements here prevent the cat from even checking squares outside the
                    //bonds of the grid.

                    //If a position has a lower MouseDistance, set the pointToMove's X and Y to that position and
                    //update pointToMove's MouseDistance.

                    //Checks MouseDistance value to the left.
                    if (cat.X > 0)
                        if (Grid[cat.X - 1, cat.Y].MouseDistance < pointToMove.MouseDistance
                            && !Grid[cat.X - 1, cat.Y].CatAvoids)
                        {
                            doMove = true;
                            pointToMove.X = cat.X - 1;
                            pointToMove.Y = cat.Y;
                            pointToMove.MouseDistance = Grid[cat.X - 1, cat.Y].MouseDistance;
                        }
                    //Checks MouseDistance value to the right.
                    if (cat.X < Grid.GetUpperBound(0))
                        if (Grid[cat.X + 1, cat.Y].MouseDistance < pointToMove.MouseDistance
                            && !Grid[cat.X + 1, cat.Y].CatAvoids)
                        {
                            doMove = true;
                            pointToMove.X = cat.X + 1;
                            pointToMove.Y = cat.Y;
                            pointToMove.MouseDistance = Grid[cat.X + 1, cat.Y].MouseDistance;
                        }
                    //Checks MouseDistance value above.
                    if (cat.Y > 0)
                        if (Grid[cat.X, cat.Y - 1].MouseDistance < pointToMove.MouseDistance
                            && !Grid[cat.X, cat.Y - 1].CatAvoids)
                        {
                            doMove = true;
                            pointToMove.X = cat.X;
                            pointToMove.Y = cat.Y - 1;
                            pointToMove.MouseDistance = Grid[cat.X, cat.Y - 1].MouseDistance;
                        }
                    //Checks MouseDistance value below.
                    if (cat.Y < Grid.GetUpperBound(1))
                        if (Grid[cat.X, cat.Y + 1].MouseDistance < pointToMove.MouseDistance
                            && !Grid[cat.X, cat.Y + 1].CatAvoids)
                        {
                            doMove = true;
                            pointToMove.X = cat.X;
                            pointToMove.Y = cat.Y + 1;
                            pointToMove.MouseDistance = Grid[cat.X, cat.Y + 1].MouseDistance;
                        }

                    if (doMove) //Best move position found. Set the cat to this position.
                    {
                        //First, update the game field to reflect the change.
                        Grid[cat.X, cat.Y].Contains = PointContents.Space;
                        Grid[pointToMove.X, pointToMove.Y].Contains = PointContents.Cat;

                        //Now update the cat in the List that we are reading from.
                        cat.X = pointToMove.X;
                        cat.Y = pointToMove.Y;
                    }
                }
            }

            //If any cat's position matches the mouse's, return true.
            if (Cats.Any(x => (x.X == Mouse.X && x.Y == Mouse.Y)))
                return true;

            return false;
        }


        /// <summary>
        /// Asks user if they want to run the game again.
        /// </summary>
        /// <returns>True if the user wants to play again.</returns>
        private bool runAgain()
        {
            ConsoleKey input;
            
            //Numerical grammer
            string pieceGrammer = "pieces";

            if (this.Score == 1)
                pieceGrammer = "piece";

            //Keep asking until user inputs correctly.
            do
            {
                Console.Clear();
                Console.Write("\n\nYou got caught!\nYou ate {0} {1} of cheese.\n\nPlay again? (Y, N) ", this.Score, pieceGrammer);
                input = Console.ReadKey().Key;

                if (input == ConsoleKey.Y)
                {
                    Initialize(); //Reset the game to how it was when it started.
                    return true;
                }

                if (input == ConsoleKey.N)
                    return false;
            
            //No reason to set a different loop condition.  When user inputs correctly, function will return and exit.
            } while (true);
        }

        /// <summary>
        /// Play a game of CheeeseFinder.
        /// </summary>
        public void PlayGame()
        {
            bool gameRunning = true; //Will beset to True when cheese is found.
            
            
            //The following is a timer for multithreading. This feature is not active for this release.

            //System.Timers.Timer gameTimer = new System.Timers.Timer();
            //gameTimer.Elapsed += new ElapsedEventHandler(OnGameTimer);
            //gameTimer.Interval = 100;
            //gameTimer.Enabled = true;
            //gameTimer.AutoReset = false;


            //Main game loop.
            do
            {
                Console.Clear();
                //Draws the game field.
                this.DrawGrid(_showGrid);
                
                if (MoveMouse()) //Returns true if mouse lands on cheese.
                {
                    this.Score++;
                    this.Grid[Mouse.X, Mouse.Y].Contains = PointContents.Mouse;
                    PlaceCheese();
                    PlaceCat();
                }

                if (MoveCats()) //Returns true if cat lands on mouse.
                    if (!runAgain())//Asks the user if they want to play again.
                        gameRunning = false;

            } while (gameRunning);
        }

        /*
        /// <summary>
        /// This will be used for multithreading during a later release. Not active at this time.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void OnGameTimer(Object source, ElapsedEventArgs e)
        {
            Console.Clear();
            DrawGrid(_showGrid);
            System.Timers.Timer theTimer = (System.Timers.Timer)source;
            theTimer.Enabled = true;
        }
        */
    }

    /// <summary>
    /// The possible contents of a Point in the CheeseFinder game.
    /// </summary>
    enum PointContents { Space, Mouse, Cheese, Wall, Cat }
    

    /// <summary>
    /// A single point in the grid of CheeseFinder.
    /// </summary>
    class Point
    {
        private int _x;
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        private int _y;
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        private PointContents _contains;
        /// <summary>
        /// Changing this property automatically changes the character representation of this grid node, as well
        /// as whether the user can move to this square.
        /// </summary>
        public PointContents Contains
        {
            get { return _contains; }
            set 
            {
                switch (value)
                {
                    case PointContents.Mouse:
                        _containsChar = "M";
                        _canMoveTo = false;
                        _catAvoids = false;
                        break;
                    case PointContents.Cheese:
                        _containsChar = "C";
                        _canMoveTo = true;
                        _catAvoids = true;
                        break;
                    case PointContents.Wall:
                        _containsChar = "X";
                        _canMoveTo = false;
                        _catAvoids = true;
                        break;
                    case PointContents.Cat:
                        _containsChar = "E";
                        _canMoveTo = false;
                        _catAvoids = true;
                        break;
                    default:
                        _containsChar = " ";
                        _canMoveTo = true;
                        _catAvoids = false;
                        break;
                }
                
                _contains = value;
            }
        }

        private string _containsChar;
        public string ContainsChar
        {
            get { return _containsChar; }
        }

        private bool _canMoveTo;
        public bool CanMoveTo
        {
            get { return _canMoveTo; }
        }

        private bool _catAvoids;
        public bool CatAvoids
        {
            get { return _catAvoids; }
        }

        private int _mouseDistance = 10000; //Initialized to a value that will never be reached in this game.
        public int MouseDistance
        {
            get { return _mouseDistance; }
            set { _mouseDistance = value; }
        }


        // ---- Constructors
        

        /// <summary>
        /// Creates a Point object with specified position and contents.
        /// </summary>
        /// <param name="x">Sets the Y position of this point.</param>
        /// <param name="y">Sets the Y position of this point.</param>
        /// <param name="thisHas">What this point should contain.</param>
        public Point(int x, int y, PointContents thisHas)
        {
            this.X = x;
            this.Y = y;
            this.Contains = thisHas;
        }

        /// <summary>
        /// Creates a Point object with specified position.
        /// </summary>
        /// <param name="x">Sets the X position of this point.</param>
        /// <param name="y">Sets the Y position of this point.</param>
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Contains = PointContents.Space;
        }


        // ---- Methods


        /// <summary>
        /// Draws the single point for a grid.
        /// </summary>
        /// <param name="drawGrid">True if brackets are to be displayed</param>
        /// <returns>The string if this point's contentents and the grid if drawGrid = true.</returns>
        public string DrawPoint(bool drawGrid)
        {
            //Change color based on the contents of the grid. The DrawGrid method will restore the white color.
            if (this.Contains == PointContents.Mouse)
                Console.ForegroundColor = ConsoleColor.Green;

            else if (this.Contains == PointContents.Cheese)
                Console.ForegroundColor = ConsoleColor.Yellow;

            else if (this.Contains == PointContents.Cat)
                Console.ForegroundColor = ConsoleColor.Red;

            //If we are to draw the entire grid, return all points with brackets.
            if(drawGrid)
                return "[" + this.ContainsChar + "]";

            //If not, but this is a point that is NOT a space, return the point with brackets.
            if(!(this.Contains == PointContents.Space))
                return "[" + this.ContainsChar + "]";

            //If it IS a space, return an empty cell. Remains "ContainsChar" in case a change is made
            //to the representation of a space.
            return " " + this.ContainsChar + " ";
        }
    }
}
