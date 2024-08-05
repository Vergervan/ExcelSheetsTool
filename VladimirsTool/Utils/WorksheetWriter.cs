using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace VladimirsTool.Utils
{
    public class WorksheetWriter
    {
        public bool SaveWordFile(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> data)
        {
            Word.Application word = new Microsoft.Office.Interop.Word.Application();
            Word.Document doc = word.Documents.Add();
            object start = 0;
            object end = 0;
            Word.Range tableLocation = doc.Range(ref start, ref end);
            doc.Tables.Add(tableLocation, 2, 1);
            doc.Tables[1].set_Style("Table Grid");

            word.Visible = true;
            word.Activate();

            return true;
        }
        public bool SaveExcelFile(string path, IEnumerable<string> headers, IEnumerable<IEnumerable<string>> data)
        {
            Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Excel.Workbook wb = excel.Workbooks.Add();
            Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

            int counter = 0;
            foreach(var header in headers)
            {
                ws.Range["A1"].Offset[0, counter++].Value = header;
            }

            int length = headers.Count();
            counter = 0;
            foreach(var row in data)
            {
                ws.Range["A2"].Offset[counter++].Resize[1, length].Value = row.ToArray();
            }

            excel.Visible = true;
            ws.Columns.AutoFit();
            ws.Activate();

            return true;
        }
    }
}
