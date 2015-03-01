using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathmicChatbot.Models
{
    class UserModel
    {

        public UserModel()
        {

        }
        public string nick { get; set; }
        public DateTime lastvisit { get; set; }
        public int visitcount { get; set; }
    }
}
