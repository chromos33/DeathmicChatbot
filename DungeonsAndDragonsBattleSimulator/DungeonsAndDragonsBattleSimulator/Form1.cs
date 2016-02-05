using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DungeonsAndDragonsBattleSimulator.Classes;
using DungeonsAndDragonsBattleSimulator.Classes.SubClasses;

namespace DungeonsAndDragonsBattleSimulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Random randomizer = new Random();


            BattleField Battlefield = new BattleField(randomizer);
            Character ally = new Character();
            ally.Name = "ally";
            Character enemy = new Character();
            enemy.Name = "enemy";
            Battlefield.addCharacter(ally);
            Battlefield.addCharacter(enemy);
            string msg = "";
            foreach(Character chara in Battlefield.getCharacters())
            {
                msg += chara.Name + " x: " + chara.getPoint().X + " y:" + chara.getPoint().Y+System.Environment.NewLine;
            }
            ally.MoveTo(enemy.getPoint());
            MessageBox.Show(msg);
        }
    }
}
