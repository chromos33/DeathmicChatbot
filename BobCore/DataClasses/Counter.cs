using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobCore.DataClasses
{
    public class Counter
    {
        public int count;
        public string name;
        public Counter(string _name)
        {
            name = _name;
            count = 0;
        }
        public Counter()
        {

        }
        public int add(int i = 1)
        {
            count += i;
            return count;
        }
        public int set(int i)
        {
            count = i;
            return count;
        }
        public int get()
        {
            return count;
        }

    }
}
