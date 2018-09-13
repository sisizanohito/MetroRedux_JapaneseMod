using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace text_xlsx
{
    class Program
    {
        static void Main(string[] args)
        {
            var text = args[0];
            var xlsx = args[1];
            var date = new List<string[]>();

            using (var reder = new StreamReader(text, Encoding.UTF8))
            {
                string line = "";
                while (( line = reder.ReadLine()) != null)
                {
                    var value = line.Split('=');
                    date.Add(value);
                }

            }

            // ブックを読み込む
            using (var workbook = new XLWorkbook())
            {
                // 1シート目を取得
                var worksheet = workbook.Worksheets.Add("翻訳");
                worksheet.Cell("A1").Value = "ID";
                worksheet.Cell("B1").Value = "原文";
                worksheet.Cell("C1").Value = "翻訳";
                for (int i = 0;i<date.Count;i++)
                {
                    var value = date[i];
                    var kye = value[0];
                    var caption = "";
                    if(value.Length > 1)
                    {
                        caption = value[1];
                        for (int j = 2; j < value.Length; j++)
                        {
                            caption = caption + "=" + value[j];
                        }
                    }
                    
                    worksheet.Cell($"A{i + 2}").Value = kye;
                    worksheet.Cell($"B{i + 2}").Value = caption;
                    worksheet.Cell($"C{i + 2}").Value = caption;
                }
                workbook.SaveAs(xlsx);

                Console.WriteLine("出力終了");

            }

           
        }
    }
}
