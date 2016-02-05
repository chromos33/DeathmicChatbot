using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonsAndDragonsBattleSimulator.Classes.SubClasses
{
    class Point
    {
        private int x;
        private int y;
        private bool filled;
        public Point(int x,int y)
        {
            X = x;
            Y = y;
        }
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        public bool Filled
        {
            get { return filled; }
        }
        public void togglefilled()
        {
            filled = !filled;
        }
        public double Distance(Point _goal)
        {
            return Math.Sqrt(Math.Pow(((double)_goal.X - (double)X), 2) + Math.Pow(((double)_goal.Y - (double)Y), 2));
        }
        public bool isAdjacent(Point _goal)
        {

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if(X-i == _goal.X && Y-j == _goal.Y)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
