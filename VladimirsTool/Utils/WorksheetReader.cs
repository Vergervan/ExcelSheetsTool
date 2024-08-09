using System;
using System.Collections.Generic;
using System.Linq;
using VladimirsTool.Models;
using ClosedXML.Excel;
using Man = VladimirsTool.Models.Man;

namespace VladimirsTool.Utils
{
    public class WorksheetReader
    {
        private string[] _headerNames;
    
        public IEnumerable<string> Headers => _headerNames;

        public IEnumerable<Man> Parse(IXLWorksheet sheet)
        {
            var usedRange = sheet.RangeUsed();
            int rowCount = usedRange.RowCount(), colCount = usedRange.ColumnCount();
   
            var firstRow = usedRange.Row(1);
            var cells = firstRow.Cells();
            _headerNames = new string[colCount];

            int counter = 0;
            foreach (var cell in cells)
            {
                _headerNames[counter++] = cell.Value.IsBlank ? null : cell.Value.ToString().Trim().ToUpper();
            }

            //LINQ removes null cells. It causes bugs and wrong cell counting 
            //var _headerNames2 = myHeadvalues.OfType<object>().Select(p => p?.ToString()).ToArray();
            
            List<Man> men = new List<Man>();

            for (int n = 2; n <= rowCount; n++)
            {
                Man man = new Man();
                var currentRow = usedRange.Row(n);
                for(int i = 0; i < colCount; i++)
                {
                    if (_headerNames[i] == null) continue;
                    var xlCellValue = currentRow.Cell(i + 1);
                    var formattedString = xlCellValue.GetFormattedString();
                    CellValue data = xlCellValue.DataType == XLDataType.DateTime ? new CellValue(xlCellValue.GetDateTime(), formattedString) : new CellValue(xlCellValue.Value.ToString());
                    man.AddData(_headerNames[i], data);
                }
                string manString = man.ToString();
                if (string.IsNullOrEmpty(manString) || string.IsNullOrWhiteSpace(manString)) continue;
                man.CalculateHashCode();
                men.Add(man);
            }
            return men;
        }
    }
}
