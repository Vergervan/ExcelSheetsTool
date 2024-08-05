using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Man = VladimirsTool.Models.Man;

namespace VladimirsTool.Utils
{
    public class WorksheetReader
    {
        private string[] _headerNames;
    
        public IEnumerable<string> Headers => _headerNames;

        public IEnumerable<Man> ParseSheetsByNameAndBirth(Worksheet sheet)
        {
            int rowCount = sheet.UsedRange.Rows.Count, colCount = sheet.UsedRange.Columns.Count;

            int iLN = 0, iFN = 0, iSN = 0, iBD = 0; //LastName, FirstName, Surname, BirthDate indices

            Range firstRow = sheet.UsedRange.Rows[1];
            Array myHeadvalues = (Array)firstRow.Cells.Value;
            _headerNames = myHeadvalues.OfType<object>().Select(o => o.ToString()).ToArray();
            for (int i = 0; i < _headerNames.Length; i++)
            {
                string val = _headerNames[i].Trim().ToUpper();
                if (val == "ФАМИЛИЯ")
                    iLN = i + 1;
                else if (val == "ИМЯ")
                    iFN = i + 1;
                else if (val == "ОТЧЕСТВО")
                    iSN = i + 1;
                else if (val == "ДАТА РОЖДЕНИЯ" || val == "ДР")
                    iBD = i + 1;

                if (iLN != 0 && iFN != 0 && iSN != 0 && iBD != 0)
                    break;
            }
            HashSet<int> basicHeadersIndices = new HashSet<int>() { iLN, iFN, iSN, iBD };
            List<Man> men = new List<Man>();
            if (basicHeadersIndices.Contains(0))
            {
                MessageBox.Show("Некорректный формат таблицы Excel\nНе хватает полей с ФИО и датой рождения");
                return null;
            }


            for (int n = 2; n <= rowCount; n++)
            {
                Man man = new Man();
                Range currentRow = sheet.UsedRange.Rows[n];
                man.LastName = currentRow.Cells[iLN].Value;
                if (string.IsNullOrEmpty(man.LastName)) continue;
                man.FirstName = currentRow.Cells[iFN].Value;
                man.Surname = currentRow.Cells[iSN].Value;
                man.BirthDate = currentRow.Cells[iBD].Value ?? DateTime.MinValue; //Imlicitly converts to DateTime, cause it's a type of cell in the sheet

                int i = 0;
                foreach(var cell in myHeadvalues) 
                {
                    ++i;
                    if (cell == null) continue;
                    man.AddData(cell.ToString().Trim().ToUpper(), currentRow.Cells[i].Value);
                }

                men.Add(man);
            }
            return men;
        }
    }
}
