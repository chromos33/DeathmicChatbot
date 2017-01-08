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
        public List<UserPick> User;
        public UserPickedList()
        {

        }
    }
    class UserPick
    {
        public string UserName;
        public UserPick()
        {

        }
    }
}
