using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Import
{
    class Program
    {
        static void Main(string[] args)
        {
            var usertextfiles = Directory.EnumerateFiles("join_logging/*.txt");
            foreach(string currentfile in usertextfiles)
            {
                Console.WriteLine(currentfile);
            }
        }
    }
}
