using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheeseFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            CheeseFinder game = new CheeseFinder();
            game.PlayGame();
        }
    }

    /// <summary>
    /// A game of CheeseFinder. Control the Mouse to find the cheese.
    /// </summary>
    class CheeseFinder
    {
        //For quick setting of options.
        const bool _showGrid = false;
        const int _gridXNodes = 10;
        const int _gridYNodes = 20;
        const int _wallChance = 15;

        private Point[,] _grid = new Point[_gridXNodes, _gridYNodes];
        public Point[,] Grid
        {
            get { return _grid; }
            set { _grid = value; }
        }

        public Point Mouse { get; set; }
        public Point Cheese { get; set; }


        // ---- Constructors


        /// <summary>
        /// Generates the grid for the game, and randomly places the mouse/cheese.
        /// </summary>
        public CheeseFinder()
        {
            Random rng = new Random();
            int randX = rng.Next(0, Grid.GetUpperBound(0) + 1);
            int randY = rng.Next(0, Grid.GetUpperBound(1) + 1);

            //Create a new point at each node of Grid
            for (int y = 0; y < Grid.GetUpperBound(1) + 1; y++)
                for (int x = 0; x < Grid.GetUpperBound(0) + 1; x++)
                {
                    Grid[x, y] = new Point(x, y);
                    //Randomly place a wall at this point. This can possibly block the cheese in -
                    //Todo: attempt to ensure that this won't happen.
                    if (rng.Next(0, 101) < _wallChance)
                        Grid[x, y].Contains = PointContents.Wall;
                }
            //Position the Mouse at a random point on the field
            do
            {
                randX = rng.Next(0, Grid.GetUpperBound(0) + 1);
                randY = rng.Next(0, Grid.GetUpperBound(1) + 1);

                if (Grid[randX, randY].CanMoveTo) //Place is there is no wall in the way.
                {
                    Grid[randX, randY].Contains = PointContents.Mouse;
                    this.Mouse = new Point(randX, randY);
                }
            } while (!(Grid[randX, randY].Contains == PointContents.Mouse)); //Loop until mouse is placed.

            //Position the Cheese at a random point on the field.
            do
            {
                randX = rng.Next(0, Grid.GetUpperBound(0) + 1);
                randY = rng.Next(0, Grid.GetUpperBound(1) + 1);

                if (Grid[randX, randY].CanMoveTo) // Place if there is no wall or mouse in the way.
                {
                    Grid[randX, randY].Contains = PointContents.Cheese;
                    this.Cheese = new Point(randX, randY);
                }
            } while (!Grid[randX, randY].CanMoveTo); //Loop until cheese is placed.
        }


        // ---- Methods
        
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
                    //Draw the border. 
                    Console.Write(" "); //The upper left corner.
                    for (int i = 0; i < Grid.GetUpperBound(0) + 1; i++)
                        Console.Write("---"); //Top border. Three dashes per node in the list.
                    Console.Write(" "); //The upper right corner.
                    Console.WriteLine(); //New line before grid is drawn.
                }
                for (int x = 0; x < Grid.GetUpperBound(0) + 1; x++)
                {
                    if (x == 0)
                        Console.Write("|"); //Border before each row.

                    //The grid itself.
                    Console.Write(Grid[x, y].DrawPoint(_showGrid));
                    Console.ResetColor(); //In case color was changed by Mouse or Cheese.

                    if (x == Grid.GetUpperBound(0))
                        Console.Write("|"); //Border after each row.
                }
                Console.WriteLine(); //New line before bottom border.
            }

            Console.Write(" "); //Lower-left corner.
            for (int i = 0; i < Grid.GetUpperBound(0) + 1; i++)
                Console.Write("---"); //Bottom border. Three dashes per node.
            Console.Write(" "); //Lower-right corner.
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

        /// <summary>
        /// Play a game of CheeeseFinder.
        /// </summary>
        public void PlayGame()
        {
            bool foundCheese = false; //Will beset to True when cheese is found.

            do
            {
                Console.Clear();
                this.DrawGrid(_showGrid);
                foundCheese = MoveMouse(); //Returns true if mouse touches cheese.

            } while (!foundCheese);

            //Will only exit on victory in this version.
            Console.Clear();
            Console.WriteLine("You win!");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// The possible contents of a Point in the CheeseFinder game.
    /// </summary>
    enum PointContents { Space, Mouse, Cheese, Wall }
    

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
                        _containsChar = 'M';
                        _canMoveTo = false;
                        break;
                    case PointContents.Cheese:
                        _containsChar = 'C';
                        _canMoveTo = true;
                        break;
                    case PointContents.Wall:
                        _containsChar = 'X';
                        _canMoveTo = false;
                        break;
                    default:
                        _containsChar = ' ';
                        _canMoveTo = true;
                        break;
                }
                
                _contains = value;
            }
        }

        private char _containsChar;
        public char ContainsChar
        {
            get { return _containsChar; }
        }

        private bool _canMoveTo;
        public bool CanMoveTo
        {
            get { return _canMoveTo; }
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
