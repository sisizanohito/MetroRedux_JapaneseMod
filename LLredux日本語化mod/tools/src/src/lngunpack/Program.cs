using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace lngunpack
{
	/// <summary>
	/// Metro 2033 .lng ファイル Unpacker.
	/// </summary>
	public sealed class Program
	{
		/// <summary>
		/// Application Entry Point
		/// </summary>
		/// <param name="args">パラメータ</param>
		public static void Main(string[] args)
		{
			string ifname = args[0];
			string ofname = args[1];

			var program = new Program();
			program.Run(ifname, ofname);
		}

		/// <summary>
		/// .lng ファイルを展開します
		/// </summary>
		/// <param name="ifname">入力ファイル名</param>
		/// <param name="ofname">出力ファイル名</param>
		private void Run(string ifname, string ofname)
		{
			using(var writer = new StreamWriter(ofname, false, Encoding.UTF8))
			{
				using(var reader = new BinaryReader(new FileStream(ifname, FileMode.Open, FileAccess.Read)))
				{
					// 先頭 8 byte は Magic
					var magic = reader.ReadUInt64();					// = 0x0000000400000000
					Console.WriteLine("MAGIC : 0x{0:X16}", magic);
					// Unknown1 Entry のインデックスか？用途か？
					var unknown1 = reader.ReadUInt64();					// = 0x0000000100000000
					Console.WriteLine("Unknown1 : 0x{0:X16}", unknown1);

					/*
					 * キャラクタテーブル読み込み
					 * # キャラクタテーブルは UTF-16 で入っている
					 */
					var chtblsize = reader.ReadUInt32();
					Console.WriteLine("Character Table Size : {0}byte(s)", chtblsize);
                    Console.WriteLine("Character Table Dump");
					var chtable = new char[chtblsize / 2];
					for (int i = 0; i < chtblsize / 2; i++)
					{
						var ch = (char)reader.ReadUInt16();
						chtable[i] = ch;

						Console.WriteLine("{0}=0x{1:X4} {2}", i, (int)ch, ch);
					}

					// Unknown2 Entry のインデックスか？用途か？
					var unknown2 = reader.ReadUInt32();					// = 0x00000002
					Console.WriteLine("Unknown2 : 0x{0:X8}", unknown2);

					/*
					 * テキストテーブル読み込み
					 */
					var strtblsize = reader.ReadUInt32();
					Console.WriteLine("String Table Size : {0}byte(s)", strtblsize);

					UInt32 totalsize = 0;
					while (totalsize < strtblsize)
					{
						uint length;
						// キー読み込み
						string key = Encoding.ASCII.GetString(this.ReadToNull(reader, out length));
						totalsize += length;
						// 値読み込み
						string value = this.ConvertString(this.ReadToNull(reader, out length), chtable);
						totalsize += length;

						writer.WriteLine("{0}={1}", key, value);
					}
				}
			}
		}

		/// <summary>
		/// NULL が出てくるまでストリーム読み込み
		/// </summary>
		/// <param name="reader">入力ストリーム</param>
		/// <param name="length">読み込んだ長さ</param>
		/// <returns>バイト列</returns>
		private byte[] ReadToNull(BinaryReader reader, out uint length)
		{
			length = 0;
			var ret = new List<byte>();
			while (true)
			{
				byte b = reader.ReadByte();
				length++;
				if (b == 0x00) break;
				ret.Add(b);
			}
			return ret.ToArray();
		}

		/// <summary>
		/// 読み込んだ値をキャラクタテーブルを使用して文字列に変換します
		/// @キャラクタ -> 文字変換
		/// Index & 0xE0 != 0xE0 の場合 : ch = table[Index]
		/// Index & 0xE0 == 0xE0 の場合 : ch = 223 + (255 * Index下位4bit) + Index[i + i]
		/// # 第一バイトの下位 4bit がページ、第二バイトがオフセット
		/// </summary>
		/// <param name="buffer">String データ</param>
		/// <param name="table">キャラクタテーブル</param>
		/// <returns>変換後文字列</returns>
		private string ConvertString(byte[] buffer, char[] table)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < buffer.Length; i++)
			{
				if ((buffer[i] & 0xE0) == 0xE0)
				{
					int page = buffer[i] & 0x0F;
					int offset = buffer[i + 1];
					int index = 223 + (255 * page) + offset;
					if (index < table.Length)
					{
						sb.Append(table[index]);
					}
					else
					{
						// 不明な文字
						sb.Append("##ERROR##");
						Console.WriteLine("{0:X2} {1:X2} ", buffer[i], buffer[i + 1]);
					}
					i += 1;
				}
				else
				{
					sb.Append(table[buffer[i]]);
				}
			}
			return sb.ToString();
		}
	}
}
