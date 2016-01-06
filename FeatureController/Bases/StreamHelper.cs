using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureController.Bases
{
    public static class StreamHelper
    {
        public static List<string> ReadLines(this StreamReader reader, int count)
        {
            List<string> lines = new List<string>();
            string tmp = reader.ReadLine();
            while (tmp != null)
            {
                lines.Add(tmp);
                if (lines.Count >= count)
                {
                    break;
                }
                tmp = reader.ReadLine();
            }
            return lines;
        }

        public static void WriteLines(this StreamWriter writer, List<string> lines)
        {
            foreach (var line in lines)
            {
                writer.WriteLine(line);
            }
        }
    }


}
