using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DungeonsAndDragonsBattleSimulator.Classes.SubClasses;
using System.Windows.Forms;

namespace DungeonsAndDragonsBattleSimulator.Classes
{
    class Character
    {
        Point position;
        BattleField battlefield;
        public string Name;
        public Character()
        {

        }
        public void setBattlefield(BattleField _battlefield)
        {
            battlefield = _battlefield;
        }
        public void setPosition(Point _position)
        {
            position = _position;
        }
        public bool isOccupied(Point _position)
        {
            bool isoccupied = false;
            if(position.X == _position.X && position.Y == _position.Y)
            {
                isoccupied = true;
            }
            return isoccupied;
        }
        public Point getPoint()
        {
            return position;
        }
        public Point MoveTo(Point _goal)
        {
            Console.WriteLine("Start X: " + position.X + " Y: " + position.Y);
            
            Point goalpoint = new Point(-10000,-10000);
            while(!_goal.isAdjacent(goalpoint))
            {
                goalpoint = Move(_goal, goalpoint);
                MessageBox.Show("X: "+goalpoint.X+" Y: "+goalpoint.Y);
                MessageBox.Show(_goal.isAdjacent(goalpoint).ToString());
            }
            Console.WriteLine("X: " + _goal.X + " Y: " + _goal.Y);
            return goalpoint;
        }
        private Point Move(Point _goal,Point oldpoint = null)
        {
            List<Point> MovementPoints = new List<Point>();
            for(int i =-1;i<=1;i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    MovementPoints.Add(new Point(position.X - i, position.Y - j));
                }
            }
            List<Point> possibleMovementPoints = new List<Point>();
            foreach (Point movepoint in MovementPoints)
            {
                if(battlefield.isMovable(movepoint))
                {
                    if(oldpoint != null)
                    {
                        if(oldpoint.X == movepoint.X && oldpoint.Y == movepoint.Y)
                        {

                        }
                        else
                        {
                            possibleMovementPoints.Add(movepoint);
                        }
                    }
                    else
                    {
                        possibleMovementPoints.Add(movepoint);
                    }
                   
                }
            }
            Point nextmove = null;
            foreach(Point movepoint in possibleMovementPoints)
            {
                if(nextmove != null)
                {
                    if (movepoint.Distance(_goal) < nextmove.Distance(_goal))
                    {
                        
                        nextmove = movepoint;
                    }
                }
                else
                {
                    nextmove = movepoint;
                }
            }
            return nextmove;
        }

    }
}
