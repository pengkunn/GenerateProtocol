using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GenerateProtocol.Core;
using GenerateProtocol.Template;


namespace GenerateProtocol.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            IDictionary<string, string> exportFiles = GenerateClass.GenerateAllMessage();
            foreach (KeyValuePair<string, string> exportFile in exportFiles)
            {
                if (File.Exists("../../../Export/" + exportFile.Key + ".h"))
                    File.Delete("../../../Export/" + exportFile.Key + ".h");

                File.AppendAllText("../../../Export/" + exportFile.Key + ".h", exportFile.Value);
            }
            Console.ReadKey();
        }
    }
}
