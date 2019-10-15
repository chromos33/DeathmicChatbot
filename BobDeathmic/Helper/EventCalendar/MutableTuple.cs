using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Helper
{
    public class MutableTuple<T1, T2>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }
        public MutableTuple(T1 First, T2 Second)
        {
            this.First = First;
            this.Second = Second;
        }
    }
}
