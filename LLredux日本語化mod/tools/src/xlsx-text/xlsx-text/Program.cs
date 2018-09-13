using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace xlsx_text
{
    class Program
    {
        static void Main(string[] args)
        {
            var xlsx = args[0];
            var text = args[1];
            var date = new List<string[]>();

            // ブックを読み込む
            using (var workbook = new XLWorkbook(xlsx))
            {
                // シートを順次取得

                var worksheets = workbook.Worksheets;

                for (int i = 2; i <= worksheets.Count; i++)
                {
                    var worksheet = worksheets.Worksheet(i);
                    // A列を末尾まで走査
                    var lastRow = worksheet.LastRowUsed().RowNumber();
                    for (int j = 2; j <= lastRow; j++)
                    {
                        var id = worksheet.Cell($"A{j}").Value?.ToString();
                        var caption = worksheet.Cell($"C{j}").Value?.ToString();
                        //string[] value = new string[] { id, InsertSpace(caption) };
                        string[] value = new string[] { id, caption };
                        date.Add(value);
                    }
                }
            }

            using (var writer = new StreamWriter(text, false, Encoding.UTF8))
            {
                foreach (string[] pair in date)
                {
                    writer.WriteLine("{0}={1}", pair[0], pair[1]);
                    Console.WriteLine(string.Format("Key : {0} / Value : {1}", pair[0], pair[1]));
                }
            }
        }

        static string InsertSpace(string text)
        {
            string pattern = @"(\<)(?<tagName>[a-zA-Z0-9=_]+)(\>)";
            string tagName = Regex.Match(text, pattern).Groups["tagName"].Value;
            int index = text.IndexOf("<" + tagName + ">");
            if(index == -1)
            {
                return AddSpace(text);
            }
            string caption = AddSpace(text.Substring(0, index));

            string next = text.Substring(index+tagName.Length+2);
            string result = InsertSpace(next);

            return caption + "<" + tagName + ">" + result;
        }
        static string AddSpace(string text)
        {
            string str = "";
            for(int i =0;i<text.Length;i++)
            {
                str = str + text[i] + " ";
            }
            return str;
        }
    }
}
