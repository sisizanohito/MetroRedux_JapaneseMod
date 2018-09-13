using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace scriptconcat
{
	/// <summary>
	/// scripts.bin 作成
	/// </summary>
	public sealed class Program
	{
		/// <summary>
		/// Application Entry Point.
		/// </summary>
		/// <param name="args">パラメータ</param>
		public static void Main(string[] args)
		{
			string dir = args[0];
			string scriptsfile = args[1];
			Program program = new Program();

			program.Run(dir, scriptsfile);
		}

		/// <summary>
		/// ディレクトリの内容を結合して scripts.bin を生成します
		/// </summary>
		/// <param name="dir">結合元ディレクトリ</param>
		/// <param name="scriptsfile">統合先ファイル</param>
		private void Run(string dir, string scriptsfile)
		{
			var pattern = new Regex("(?<seq>[0-9]+)_(?<id>[A-Za-z0-9]+)\\.split");

			// 連結ファイル取得
			var files = Directory.EnumerateFiles(dir).OrderBy((file) =>
			{
				var fname = Path.GetFileName(file);
				var match = pattern.Match(fname);
				var seq = int.Parse(match.Groups["seq"].Value);
				return seq;
			});

			using(var writer = new BinaryWriter(new FileStream(scriptsfile, FileMode.Create, FileAccess.Write)))
			{
				foreach (var file in files)
				{
					var fname = Path.GetFileName(file);
					var match = pattern.Match(fname);
					var id = UInt32.Parse(match.Groups["id"].Value, System.Globalization.NumberStyles.HexNumber);

					// ファイル連結
					Console.WriteLine("Concat {0} ...", fname);
					var data = File.ReadAllBytes(file);
					writer.Write(id);
					writer.Write((UInt32)data.Length);
					writer.Write(data);
				}
			}
		}
	}
}
