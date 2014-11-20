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

    class CheeseFinder
    {
        private Point[,] _grid = new Point[10,20];
        public Point[,] Grid
        {
            get { return _grid; }
            set { _grid = value; }
        }

        public Point Mouse { get; set; }
        public Point Cheese { get; set; }


        // ---- Constructors


        public CheeseFinder()
        {
            Random rng = new Random();
            int randX = rng.Next(0, Grid.GetUpperBound(0) + 1);
            int randY = rng.Next(0, Grid.GetUpperBound(1) + 1);

            for (int y = 0; y < Grid.GetUpperBound(1) + 1; y++)
                for (int x = 0; x < Grid.GetUpperBound(0) + 1; x++)
                    Grid[x, y] = new Point(x, y);

            Grid[randX, randY].Contains = PointContents.Mouse;
            this.Mouse = new Point(randX, randY);

            do
            {
                randX = rng.Next(0, Grid.GetUpperBound(0) + 1);
                randY = rng.Next(0, Grid.GetUpperBound(1) + 1);

                if (Grid[randX, randY].CanMoveTo)
                {
                    Grid[randX, randY].Contains = PointContents.Cheese;
                    this.Cheese = new Point(randX, randY);
                }
            } while (!Grid[randX, randY].CanMoveTo);
        }


        /// ---- Methods
        

        public void DrawGrid(bool displayGrid)
        {
            for (int i = 0; i < Grid.GetUpperBound(0) + 1; i++)
            {
                //if(i == 0)
                //{

                //}
                for (int j = 0; j < Grid.GetUpperBound(1) + 1; j++)
                    Console.Write(Grid[i, j].DrawPoint(displayGrid));

                Console.WriteLine();
            }
        }

        public ConsoleKey getUserMove()
        {
            ConsoleKeyInfo input = Console.ReadKey();

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

        public bool ValidMove(ConsoleKey input)
        {
            switch (input)
            {
                case ConsoleKey.UpArrow:
                    if (this.Mouse.Y > 0)
                        return true;

                    break;

                case ConsoleKey.DownArrow:
                    if (this.Mouse.Y < Grid.GetUpperBound(0) + 1)
                        return true;

                    break;

                case ConsoleKey.LeftArrow:
                    if (this.Mouse.X > 0)
                        return true;

                    break;

                case ConsoleKey.RightArrow:
                    if (this.Mouse.X < Grid.GetUpperBound(0) + 1)
                        return true;

                    break;
            }

            Console.WriteLine("Not a valid move");
            return false;
        }

        public bool MoveMouse(ConsoleKey input)
        {
            ConsoleKey userMove = getUserMove();
            Grid[Mouse.X, Mouse.Y].Contains = PointContents.Space;

            switch (userMove)
            {
                case ConsoleKey.UpArrow:
                    Console.WriteLine("Up");
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

            Console.WriteLine("Test");
            if ((Mouse.X == Cheese.X) && (Mouse.Y == Cheese.Y))
                return true;

            Console.WriteLine("You move");
            Grid[Mouse.X, Mouse.Y].Contains = PointContents.Mouse;
            return false;

        }


        public void PlayGame()
        {
            ConsoleKeyInfo input;

            do
            {
                Console.Clear();
                this.DrawGrid(true);;
                MoveMouse(input);
            } while (true);
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
            if(drawGrid)
                return "[" + ContainsChar + "]";

            return " " + ContainsChar + " ";
        }
    }
}
