using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MessageFilter
{
    class MyFileIO
    {
        public static Dictionary<string, string> PopulateTextWordsFromCsv(string path)//Provide path to textwords file, returns dictionary of contents contents
        {
            Dictionary<string, string> FilterWordsDictionary = File.ReadLines(path).Select(line => line.Split(',')).ToDictionary(line => line[0], line => line[1]); //=> lambda operator = lambda expression body

            return FilterWordsDictionary;
        }
    }
}
