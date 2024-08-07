using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VladimirsTool.Models;

namespace VladimirsTool.Utils
{
    public class CSVReader
    {
        private string[] _headerNames;
        public IEnumerable<string> Headers => _headerNames;

        public IEnumerable<Man> Parse(string path)
        {
            List<Man> men = new List<Man>();
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                _headerNames = csvParser.ReadFields();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    if (fields.Length == 0) continue;
                    Man man = new Man();
                    for (int i = 0; i < _headerNames.Length; i++)
                    {
                        if (string.IsNullOrEmpty(_headerNames[i])) continue;
                        man.AddData(_headerNames[i], new CellValue(fields[i]));
                    }

                    string manString = man.ToString();
                    if (string.IsNullOrEmpty(manString) || string.IsNullOrWhiteSpace(manString)) continue;
                    man.CalculateHashCode();
                    men.Add(man);
                }
            }
            return men; 
        }
    }
}
