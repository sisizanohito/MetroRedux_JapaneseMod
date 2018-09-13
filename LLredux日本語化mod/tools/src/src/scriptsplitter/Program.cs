using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace scriptsplitter
{
	/// <summary>
	/// scripts.bin を分割します
	/// </summary>
	public sealed class Program
	{
		/// <summary>
		/// Application Entry Point.
		/// </summary>
		/// <param name="args">パラメータ</param>
		public static void Main(string[] args)
		{
			string fname = args[0];
			string extractdir = args[1];

			Program program = new Program();
			program.Run(fname, extractdir);
		}

		/// <summary>
		/// scripts.bin を各エントリーに分割します
		/// </summary>
		/// <param name="fname">入力ファイル</param>
		/// <param name="dir">展開先ディレクトリ</param>
		private void Run(string fname, string dir)
		{
			// 展開先ディレクトリの作成
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			// scripts.bin の分割
			using (var reader = new BinaryReader(new FileStream(fname, FileMode.Open, FileAccess.Read)))
			{
				for (int i = 0; true; i++)
				{
					UInt32 id = reader.ReadUInt32();
					UInt32 size = reader.ReadUInt32();
					using (var split = new BinaryWriter(new FileStream(
						Path.Combine(dir, string.Format("{0}_{1:X8}.split", i, id)), FileMode.Create, FileAccess.Write)))
					{
						split.Write(reader.ReadBytes((int)size));
					}
					Console.WriteLine("{0} : 0x{1:X8} {2}byte(s)", i, id, size);

					if (reader.BaseStream.Position == reader.BaseStream.Length) break;
				}
			}
		}
	}
}
