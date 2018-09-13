using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace symunpack
{
	/// <summary>
	/// 0xFA8EEA67 symbol umpacker.
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
			string xmlfname = args[1];
			Program program = new Program();
			program.Run(fname, xmlfname);
		}

		/// <summary>
		/// Font symbol データを展開
		/// </summary>
		/// <param name="fname">入力ファイル名</param>
		/// <param name="xmlfname">出力XMLファイル名</param>
		private void Run(string fname, string xmlfname)
		{
			using (var writer = new StreamWriter(xmlfname, false, Encoding.UTF8))
			{
				writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				writer.WriteLine("<symbol>");
				using(var reader = new BinaryReader(new FileStream(fname, FileMode.Open, FileAccess.Read)))
				{
					var magic = reader.ReadByte();		// = 0x04
					Console.WriteLine("MAGIC : 0x{0:X2}", magic);

					// エントリの展開
					while(reader.BaseStream.Position != reader.BaseStream.Length)
					{
						var type = reader.ReadUInt32();
						var size = reader.ReadUInt32();
						Console.WriteLine("Entry {0} / {1}byte[s]", type, size);
						writer.WriteLine("\t<entry type=\"{0}\">", type);
						var data = reader.ReadBytes((int)size);
						switch (type)
						{
						case 1:
							// Type 1 展開
							this.UnpackEntry1(writer, data);
							break;
						case 2:
							// Type 2 展開
							this.UnpackEntry2(writer, data);
							break;
						default:
							throw new NotImplementedException();
						}
						writer.WriteLine("\t</entry>");
					}
				}
				writer.WriteLine("</symbol>");
			}
		}

		/// <summary>
		/// 1 Entry 目(Symbol データ?)を展開します
		/// </summary>
		/// <param name="writer">出力先</param>
		/// <param name="data">展開元データ</param>
		private void UnpackEntry1(TextWriter writer, byte[] data)
		{
			using (var reader = new BinaryReader(new MemoryStream(data)))
			{
				/*
				 * エントリーヘッダ
				 */
				var entry_id = reader.ReadUInt32();		// = 0xC96EAEB6
				var entry_size = reader.ReadUInt32();
				{
					var magic = reader.ReadByte();			// = 0x04
					var count = reader.ReadUInt32();
					Console.WriteLine("Entry ID = 0x{0:X8} Size = {1}byte[s] Magic = 0x{2:X2} Count = {3}", entry_id, entry_size, magic, count);

					for (int i = 0; i < count; i++)
					{
						/*
						 * Item ヘッダ情報
						 */
						var itemunknown = reader.ReadUInt32();	// = 0x00000000
						var itemsize = reader.ReadUInt32();
						var itemmagic = reader.ReadByte();		// = 0x04
						var h1 = reader.ReadUInt32();
						var h2 = reader.ReadUInt32();
						var h3 = reader.ReadUInt32();
						//var h4 = reader.ReadUInt32();
						var pitchx = reader.ReadSingle();	// dot pitch(u or v)
						var pitchy = reader.ReadSingle();	// dot pitch(u or v)
						var h7 = reader.ReadSingle();
						if (itemunknown != 0x00000000 || itemmagic != 0x04)
						{
							Console.WriteLine("Item Unknown - 0x{0:X8} 0x{1:X2}", itemunknown, itemmagic);
						}
                        //writer.WriteLine("\t\t<item idx=\"{0}\" h1=\"0x{1:X8}\" h2=\"0x{2:X8}\" h3=\"0x{3:X8}\" h4=\"0x{4:X8}\"", i, h1, h2, h3, h4);
                        writer.WriteLine("\t\t<item idx=\"{0}\" h1=\"0x{1:X8}\" h2=\"0x{2:X8}\" h3=\"0x{3:X8}\"", i, h1, h2, h3);
                        writer.WriteLine("\t\t      pitch_x=\"{0}\" pitch_y=\"{1}\"", pitchx, pitchy);
						writer.WriteLine("\t\t      h7=\"{0}\">", h7);

						/*
						 * データ展開
						 */
						var dataid = reader.ReadUInt32();		// = 0x8F0F9A56
						var datasize = reader.ReadUInt32();
						var datamagic = reader.ReadByte();		// = 0x04
						var datacount = reader.ReadUInt32();
						Console.WriteLine("Data Header ID = 0x{0:X8} Magic = 0x{1:X2} Count = {2}", dataid, datamagic, datacount);
						for (var dataidx = 0; dataidx < datacount; dataidx++)
						{
							var unknown1 = reader.ReadUInt32();		// = 0x00000000
							var unknown2 = reader.ReadUInt32();		// = 0x0000000F
							var unknown3 = reader.ReadByte();		// = 0x04
							if (unknown1 != 0x00000000 || unknown2 != 0x0000000F || unknown3 != 0x04)
							{
								Console.WriteLine("Data Unknown - 0x{0:X8} 0x{1:X8} 0x{2:X2}", unknown1, unknown2, unknown3);
							}
							var x = reader.ReadInt16();
							var y = reader.ReadInt16();
							var width = reader.ReadInt16();
							var height = reader.ReadInt16();
							var xoffset = reader.ReadInt16();
							var yoffset = reader.ReadInt16();
							var xadvance = reader.ReadInt16();
							writer.WriteLine("\t\t\t<data idx=\"{0}\" x=\"{1}\" y=\"{2}\" width=\"{3}\" height=\"{4}\"", dataidx, x, y, width, height);
							writer.WriteLine("\t\t\t      xoffset=\"{0}\" yoffset=\"{1}\" xadvance=\"{2}\" />", xoffset, yoffset, xadvance);
						}

						writer.WriteLine("\t\t</item>");
					}
				}
			}
		}

		/// <summary>
		/// 2 Entry 目を展開します
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="data"></param>
		private void UnpackEntry2(TextWriter writer, byte[] data)
		{
			using (var reader = new BinaryReader(new MemoryStream(data)))
			{
				UInt32 count = reader.ReadUInt32();
				Console.WriteLine("Count = {0}", count);
				int totallength = 4;	// uint32 分
				for (UInt32 i = 0; i < count; i++)
				{
					int length;
					writer.WriteLine("\t\t<value text=\"{0}\" />",
						Encoding.ASCII.GetString(this.ReadToNull(reader, out length)));
					totallength += length;
				}
				Console.WriteLine("{0}/{1} byte[s] to read.", totallength, data.Length);
			}
		}

		/// <summary>
		/// NULL が出てくるまでストリーム読み込み
		/// </summary>
		/// <param name="reader">入力ストリーム</param>
		/// <param name="length">読み込んだ長さ</param>
		/// <returns>バイト列</returns>
		private byte[] ReadToNull(BinaryReader reader, out int length)
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
	}
}
