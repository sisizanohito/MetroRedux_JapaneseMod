using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.IO;

namespace sympack
{
	/// <summary>
	/// 0xFA8EEA67 symbol packer
	/// </summary>
	public sealed class Program
	{
		/// <summary>
		/// Application Entry Point.
		/// </summary>
		/// <param name="args">パラメータ</param>
		public static void Main(string[] args)
		{
			string xmlfname = args[0];
			string fname = args[1];

			Program program = new Program();
			program.Run(xmlfname, fname);
		}

		/// <summary>
		/// XML ファイルの内容を Packing します
		/// </summary>
		/// <param name="xmlfname">入力XMLファイル名</param>
		/// <param name="fname">出力ファイル名</param>
		private void Run(string xmlfname, string fname)
		{
			var doc = new XmlDocument();
			doc.Load(xmlfname);

			using(var writer = new BinaryWriter(new FileStream(fname, FileMode.Create, FileAccess.Write)))
			{
				writer.Write((byte)0x04);		// MAGIC

				// Entry 1 書き込み
				{
					writer.Write((UInt32)0x00000001);
					var entry = this.CreateEntry1((doc.SelectSingleNode("//symbol/entry[@type=1]") as XmlElement));
					writer.Write((UInt32)entry.Length);
					writer.Write(entry);
				}
				// Entry 2 書き込み
				{
					writer.Write((UInt32)0x00000002);
					var entry = this.CreateEntry2((doc.SelectSingleNode("//symbol/entry[@type=2]") as XmlElement));
					writer.Write((UInt32)entry.Length);
					writer.Write(entry.GetBuffer(), 0, (int)entry.Length);
				}
			}
		}

		/// <summary>
		/// Entry 1 を構築します
		/// </summary>
		/// <param name="element">XmlElement</param>
		/// <returns>Stream</returns>
		private byte[] CreateEntry1(XmlElement element)
		{
			/*
			 * Entry Data
			 */
			return BinaryEditHelper.ToBinary((entrydatawriter) =>
			{
				/*
				 * Item
				 */
				var itemdata = BinaryEditHelper.ToBinary((itemwriter) =>
				{
					/*
					 * Item Body
					 */
					int itemcount = 0;
					var itembody = BinaryEditHelper.ToBinary((itembodywriter) =>
					{
						foreach (var itemnode in element.SelectNodes("./item"))
						{
							/*
							 * chunk body
							 */
							var chunkbody = BinaryEditHelper.ToBinary((chunkbodywriter) =>
							{
								var itemelement = itemnode as XmlElement;
								var h1 = UInt32.Parse(this.GetHexNumber(itemelement, "h1"), NumberStyles.HexNumber);
								var h2 = UInt32.Parse(this.GetHexNumber(itemelement, "h2"), NumberStyles.HexNumber);
                                var h3 = UInt32.Parse(this.GetHexNumber(itemelement, "h3"), NumberStyles.HexNumber);
                                var pitch_x = float.Parse(itemelement.GetAttribute("pitch_x"));
								var pitch_y = float.Parse(itemelement.GetAttribute("pitch_y"));
								var h7 = float.Parse(itemelement.GetAttribute("h7"));

								chunkbodywriter.Write((byte)0x04);				// Magic
								chunkbodywriter.Write(h1);						// H1
								chunkbodywriter.Write(h2);						// H2
                                chunkbodywriter.Write(h3);                      // H3
                                chunkbodywriter.Write(pitch_x);					// Pitch X
								chunkbodywriter.Write(pitch_y);					// Pitch Y
								chunkbodywriter.Write(h7);						// H7

								// Symbol
								chunkbodywriter.Write((UInt32)0x8F0F9A56);		// Unknown
								/*
								 * Symbol Body
								 */
								var symbolbody = BinaryEditHelper.ToBinary((symbolbodywriter) =>
								{
									var datanodes = itemelement.SelectNodes("./data");

									// Symbol Body 書き込み
									symbolbodywriter.Write((byte)0x04);					// Magic
									symbolbodywriter.Write((UInt32)datanodes.Count);	// Count

									foreach (var datanode in datanodes)
									{
										var dataelement = datanode as XmlElement;
										var x = Int16.Parse(dataelement.GetAttribute("x"));
										var y = Int16.Parse(dataelement.GetAttribute("y"));
										var width = Int16.Parse(dataelement.GetAttribute("width"));
										var height = Int16.Parse(dataelement.GetAttribute("height"));
										var xoffset = Int16.Parse(dataelement.GetAttribute("xoffset"));
										var yoffset = Int16.Parse(dataelement.GetAttribute("yoffset"));
										var xadvance = Int16.Parse(dataelement.GetAttribute("xadvance"));

										symbolbodywriter.Write((UInt32)0x00000000);		// Unknown
										symbolbodywriter.Write((UInt32)0x0000000F);		// Size
										symbolbodywriter.Write((byte)0x04);				// Magic
										symbolbodywriter.Write(x);						// X
										symbolbodywriter.Write(y);						// Y
										symbolbodywriter.Write(width);					// Width
										symbolbodywriter.Write(height);					// Height
										symbolbodywriter.Write(xoffset);				// X Offset
										symbolbodywriter.Write(yoffset);				// Y Offset
										symbolbodywriter.Write(xadvance);				// X Advance
									}
								});

								// Symbol body 書き込み
								chunkbodywriter.Write((UInt32)symbolbody.Length);	// Size
								chunkbodywriter.Write(symbolbody);
							});

							// Item Body 書き込み
							itembodywriter.Write((UInt32)0x00000000);		// Unknown
							itembodywriter.Write((UInt32)chunkbody.Length);	// Size
							itembodywriter.Write(chunkbody);				// Chunk Body
							itemcount++;
						}
					});

					// Item 書き込み
					itemwriter.Write((byte)0x04);					// Magic
					itemwriter.Write((UInt32)itemcount);			// Item Body Count
					itemwriter.Write(itembody);						// Item Body[n]
				});

				// Entry Data 書き込み
				entrydatawriter.Write((UInt32)0xC96EAEB6);			// Unknown
				entrydatawriter.Write((UInt32)itemdata.Length);		// Size
				entrydatawriter.Write(itemdata);					// Entry Data
			});
		}

		/// <summary>
		/// Entry 2 を構築します
		/// </summary>
		/// <param name="element">XmlElement</param>
		/// <returns>Stream</returns>
		private MemoryStream CreateEntry2(XmlElement element)
		{
			var memstream = new MemoryStream();
			var writer = new BinaryWriter(memstream);

			var texts = element.SelectNodes("./value");
			writer.Write((UInt32)texts.Count);

			foreach (var text in texts)
			{
				string value = (text as XmlElement).GetAttribute("text");
				writer.Write(Encoding.ASCII.GetBytes(value));
				writer.Write((byte)0x00);
			}

			return memstream;
		}

		/// <summary>
		/// 変換可能な HEX 文字列を取得します
		/// </summary>
		/// <param name="element">XmlElement</param>
		/// <param name="name">Attribute Name</param>
		/// <returns>変換可能な Hex 文字列</returns>
		private string GetHexNumber(XmlElement element, string name)
		{
			var value = element.GetAttribute(name);
			if (value.StartsWith("0x"))
			{
				value = value.Substring(2);
			}

			return value;
		}

		private UInt16 ParseHexToUInt16(XmlElement element, string name)
		{
			var value = element.GetAttribute(name);
			try
			{
				if (value.StartsWith("0x"))
				{
					value = value.Substring(2);
				}

				return UInt16.Parse(value, NumberStyles.HexNumber);
			}
			catch (OverflowException ex)
			{
				Console.WriteLine(value);
				throw ex;
			}
		}
	}
}
