using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using makefont.BmFont;

namespace makefont
{
	/// <summary>
	/// Font データ生成
	/// </summary>
	public sealed class Program
	{
		/// <summary>
		/// スペース
		/// </summary>
		private const char CHAR_SPACE = ' ';
		/// <summary>
		/// サーカムフレックス
		/// </summary>
		private const char CHAR_CIRCUM = '^';

		/// <summary>
		/// NULL Font Character
		/// </summary>
		private static FontChar NullFontChar = new FontChar();

		/// <summary>
		/// Application Entry Point.
		/// </summary>
		/// <param name="args">パラメータ</param>
		public static void Main(string[] args)
		{
            var chtable = args[0];
             var fntfile = args[1];

            var program = new Program();
			program.Run(chtable, fntfile);
		}

		/// <summary>
		/// Character テーブルと BMFont のデータから Font エントリを生成します
		/// </summary>
		/// <param name="chtablefile">キャラクタテーブル</param>
		/// <param name="fntfile">BMFont ファイル</param>
		private void Run(string chtablefile, string fntfile)
		{
			// Character Table 読み込み
			var chtable = this.LoadCharacterTable(chtablefile);
			// Font 情報読み込み
			var fonttable = FontLoader.Load(fntfile);
			// Font 検索 Function
			Func<char, FontChar> SearchFont = (seachch) =>
			{
				FontChar ret = NullFontChar;
				foreach (var fontch in fonttable.Chars)
				{
					if (seachch == (char)fontch.ID)
					{
						ret = fontch;
						break;
					}
				}
				return ret;
			};

			int idx = 0;
			foreach (var ch in chtable)
			{
				FontChar fch = null;
				/*
				 * Font の検索
				 */
				// NULL 文字
				if (ch == 0x0000)
				{
					fch = NullFontChar;
                }// SPACE は改行区切りとして使う
                else if(ch == CHAR_SPACE)
                {
                    fch = NullFontChar;
                }
                // ^ を空白の変わりに使う
                else if (ch == CHAR_CIRCUM)
                {
                    fch = SearchFont(CHAR_SPACE);
                }

                else
				{
					// 通常の文字
					fch = SearchFont(ch);
				}

				// Font Character を出力
				if (fch == NullFontChar || fch == null)
				{
					this.WriteData(idx, 0, 0, 0, 0, 0, 0, 0);
					if (fch == null)
					{
						Console.Error.WriteLine("Unknown Font Character - {0}:{1}", idx, ch);
					}
				}
				else if(fch != NullFontChar)
				{
					this.WriteData(idx, fch);
				}

				idx++;
			}
		}

		/// <summary>
		/// フォントエントリを出力します
		/// </summary>
		/// <param name="idx">インデックス</param>
		/// <param name="ch">BMFont キャラクタ</param>
		private void WriteData(int idx, FontChar ch)
		{
			this.WriteData(idx, (Int16)ch.X, (Int16)ch.Y, (Int16)ch.Width, (Int16)ch.Height, (Int16)ch.XOffset, (Int16)ch.YOffset, (Int16)ch.XAdvance);
		}

		/// <summary>
		/// フォントエントリを出力します
		/// </summary>
		/// <param name="idx">インデックス</param>
		/// <param name="x">X座標</param>
		/// <param name="y">Y座標</param>
		/// <param name="width">幅</param>
		/// <param name="height">高さ</param>
		/// <param name="xoffset">X Offset</param>
		/// <param name="yoffset">Y Offset</param>
		/// <param name="xadvance">X Advance</param>
		private void WriteData(int idx, Int16 x, Int16 y, Int16 width, Int16 height, Int16 xoffset, Int16 yoffset, Int16 xadvance)
		{
			Console.WriteLine("\t\t\t<data idx=\"{0}\" x=\"{1}\" y=\"{2}\" width=\"{3}\" height=\"{4}\"", idx, x, y, width, height);
			Console.WriteLine("\t\t\t      xoffset=\"{0}\" yoffset=\"{1}\" xadvance=\"{2}\" />", xoffset, yoffset, xadvance);
		}

		/// <summary>
		/// ファイルから Character テーブルを読み込みます
		/// </summary>
		/// <param name="chtablefile">ファイル</param>
		/// <returns>キャラクタテーブル</returns>
		private List<char> LoadCharacterTable(string chtablefile)
		{
			var chtable = new List<char>();
            char[] table = {
                (char)0xF8FF,
                (char)0x0000,
                (char)0x0020,
                (char)0x0065,
                (char)0x0074,
                (char)0x006F,
                (char)0x0061,
                (char)0x006E,
                (char)0x0069,
                (char)0x0072,
                (char)0x0073,
                (char)0x0068,
                (char)0x006C,
                (char)0x0075,
                (char)0x002E,
                (char)0x0064,
                (char)0x006D,
                (char)0x0079,
                (char)0x0063,
                (char)0x0067,
                (char)0x0077,
                (char)0x0066,
                (char)0x0021,
                (char)0x002C,
                (char)0x0070,
                (char)0x006B,
                (char)0x0062,
                (char)0x0076,
                (char)0x0027,
                (char)0x0049,
                (char)0x0041,
                (char)0x0054,
                (char)0x0053,
                (char)0x0045,
                (char)0x0048,
                (char)0x0057,
                (char)0x003F,
                (char)0x004F,
                (char)0x0044,
                (char)0x0052,
                (char)0x004D,
                (char)0x0043,
                (char)0x004C,
                (char)0x004E,
                (char)0x0059,
                (char)0x0050,
                (char)0x002D,
                (char)0x0047,
                (char)0x0042,
                (char)0x0046,
                (char)0x004B,
                (char)0x0055,
                (char)0x006A,
                (char)0x0056,
                (char)0x0078,
                (char)0x003C,
                (char)0x003E,
                (char)0x2026,
                (char)0x2013,
                (char)0x0030,
                (char)0x007A,
                (char)0x0022,
                (char)0x0029,
                (char)0x0028,
                (char)0x0031,
                (char)0x004A,
                (char)0x0071,
                (char)0x005F,
                (char)0x0032,
                (char)0x003A,
                (char)0x0033,
                (char)0x0035,
                (char)0x003D,
                (char)0x0036,
                (char)0x0034,
                (char)0x2019,
                (char)0x0051,
                (char)0x0037,
                (char)0x0038,
                (char)0x0058,
                (char)0x0039,
                (char)0x005A,
                (char)0x002A,
                (char)0x002B,
                (char)0x002F,
                (char)0x0026,
                (char)0x201D,
                (char)0x201C,
                (char)0x003B,
                (char)0x00FC,
                (char)0x005D,
                (char)0x005B,
                (char)0x005C,
                (char)0x00A0,
                (char)0x0023,
                (char)0x00A9,
                (char)0x2014,
                (char)0x0024,
                (char)0x007C,
                (char)0x00ED,
                (char)0x00F6,
                (char)0x00AE,
                (char)0x0040,
                (char)0x2018,
                (char)0x0025,
                (char)0x00E9,
                (char)0x2122,
                (char)0x00E1,
                (char)0x201E,
                (char)0x0142,
                (char)0x00F3,
                (char)0x0441,
                (char)0x0421,
                (char)0x0445,
                (char)0x0060,
                (char)0x0410,
                (char)0x3126,
                (char)0x00C0,
                (char)0x00C1,
                (char)0x00C2,
                (char)0x00C3,
                (char)0x00C4,
                (char)0x00C5,
                (char)0x00C6,
                (char)0x00C7,
                (char)0x00C8,
                (char)0x00C9,
                (char)0x00CA,
                (char)0x00CB,
                (char)0x00CC,
                (char)0x00CD,
                (char)0x00CE,
                (char)0x00CF,
                (char)0x00D0,
                (char)0x00D1,
                (char)0x00D2,
                (char)0x00D3,
                (char)0x00D4,
                (char)0x00D5,
                (char)0x00D6,
                (char)0x00D7,
                (char)0x00D8,
                (char)0x00D9,
                (char)0x00DA,
                (char)0x00DB,
                (char)0x00DC,
                (char)0x00DD,
                (char)0x00DE,
                (char)0x00DF,
                (char)0x00E0,
                (char)0x00E2,
                (char)0x00E3,
                (char)0x00E4,
                (char)0x00E5,
                (char)0x00E6,
                (char)0x00E7,
                (char)0x00E8,
                (char)0x00EA,
                (char)0x00EB,
                (char)0x00EC,
                (char)0x00EE,
                (char)0x00EF,
                (char)0x00F0,
                (char)0x00F1,
                (char)0x00F2,
                (char)0x00F4,
                (char)0x00F5,
                (char)0x00F7,
                (char)0x00F8,
                (char)0x00F9,
                (char)0x00FA,
                (char)0x00FB,
                (char)0x00FD,
                (char)0x00FE,
                (char)0x0001,
                (char)0x0002,
                (char)0x0003,
                (char)0x0004,
                (char)0x0005,
                (char)0x0006,
                (char)0x0007,
                (char)0x0008,
                (char)0x0009,
                (char)0x000A,
                (char)0x000B,
                (char)0x000C,
                (char)0x000D,
                (char)0x000E,
                (char)0x000F,
                (char)0x0010,
                (char)0x0011,
                (char)0x0012,
                (char)0x0013,
                (char)0x0014,
                (char)0x0015,
                (char)0x0016,
                (char)0x0017,
                (char)0x0018,
                (char)0x0019,
                (char)0x001A,
                (char)0x001B,
                (char)0x001C,
                (char)0x001D,
                (char)0x001E,
                (char)0x001F,
                (char)0x005E,
                (char)0x007B,
                (char)0x007D,
                (char)0x007E
            };
            chtable.AddRange(table);


            using (var reader = new StreamReader(chtablefile))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == "")
                    {
                       continue;
                    }
                    var hoge = line.ToCharArray();
                    if (!chtable.Contains(hoge[0]))
                    {
                        chtable.Add(hoge[0]);
                    }
                }
            }

            return chtable;
		}
	}
}
