using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeathmicChatbot.DataFiles
{
    class UserPickedList
    {
        public string Reason;
        public List<UserPick> User = new List<UserPick>();
        public UserPickedList()
        {

        }
        public UserPickedList(string _reason,UserPick newuser)
        {

        }
    }
    class UserPick
    {
        public string UserName;
        public UserPick()
        {

        }
        public UserPick(string name)
        {
            UserName = name;
        }
    }
}
