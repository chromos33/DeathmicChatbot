using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathmicChatbot.DataFiles
{
    //Only used to transport information
    public class VoteObject
    {
        public string Question;
        public int QuestionID;
        public List<Answer> Answers;

        public VoteObject(){
            Question = "";
            Answers = new List<Answer>();
        }
    }
}
