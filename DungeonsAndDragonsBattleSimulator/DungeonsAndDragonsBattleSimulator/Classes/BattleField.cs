using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonsAndDragonsBattleSimulator.Classes.SubClasses;

namespace DungeonsAndDragonsBattleSimulator.Classes
{
    class BattleField
    {
        List<Character> Characters;
        List<Point> MapConfig;
        Random randomenerator;
        public BattleField(Random randomizer,int configurement = 0)
        {
            // 0 = simple room 10x10, 1 = 2 wide Pathway,2 = 1 wide Pathway,3 = crossintersection
            randomenerator = randomizer;
            Characters = new List<Character>();
            MapConfig = new List<Point>();
            switch (configurement)
            {
                default:
                    for(int i = 0; i<=10;i++)
                    {
                        for (int j = 0; j <= 10; j++)
                        {
                            MapConfig.Add(new Point(i,j));
                        }
                    }
                    break;
                case 1:
                    for (int i = 0; i <= 2; i++)
                    {
                        for (int j = 0; j <= 20; j++)
                        {
                            MapConfig.Add(new Point(i, j));
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i <= 1; i++)
                    {
                        for (int j = 0; j <= 20; j++)
                        {
                            MapConfig.Add(new Point(i, j));
                        }
                    }
                    break;
                case 3:
                    for (int i = 0; i <= 10; i++)
                    {
                        for (int j = 0; j <= 10; j++)
                        {
                            if(i==5 || i == 5)
                            {
                                MapConfig.Add(new Point(i, j));
                            }
                            if (j == 5 || j == 5)
                            {
                                MapConfig.Add(new Point(i, j));
                            }
                        }
                    }
                    break;

            }
        }
        public void addCharacter(Character character)
        {
            if(Characters.Count >0)
            {
                bool positionoccupied = false;
                Point position = null;
                while (true)
                {
                    position = MapConfig.ElementAt(randomenerator.Next(MapConfig.Count));
                    foreach (Character Character in Characters)
                    {
                        if (Character.isOccupied(position))
                        {
                            positionoccupied = true;
                        }
                    }
                    if(!positionoccupied)
                    {
                        break;
                    }
                }
                character.setPosition(position);
                character.setBattlefield(this);
                Characters.Add(character);               
            }
            else
            {
                character.setPosition(MapConfig.ElementAt(randomenerator.Next(MapConfig.Count)));
                character.setBattlefield(this);
                Characters.Add(character);
            }
        }
        public List<Character> getCharacters()
        {
            return Characters;
        }
        public bool isMovable(Point _point)
        {
            IEnumerable<Point> mapconfigqueryresult = MapConfig.Where(x => x.X == _point.X && x.Y == _point.Y);
            bool result = false;
            if(mapconfigqueryresult.Count() > 0)
            {
                result = true;
                IEnumerable<Character> characterqueryresult = Characters.Where(x => x.getPoint().X == _point.X && x.getPoint().Y == _point.Y);
                if(characterqueryresult.Count() > 0)
                {
                    result = false;
                }

            }
            return result;
        }


    }
}
