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
        }
    }

    class CheeseFinder
    {

    }

    enum PointContents { Space, Mouse, Cheese, Wall }
    
    class Point
    {
        private PointContents _contains;
        public PointContents Contains
        {
            get { return _contains; }
            set 
            {
                switch (PointContents)
                {
                    case PointContents.Mouse:
                        _containsChar = 'M';
                        break;
                    case PointContents.Cheese:
                        _containsChar = 'C';
                        break;
                    case PointContents.Wall:
                        _containsChar = 'X';
                        break;
                    default:
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
    }
}
