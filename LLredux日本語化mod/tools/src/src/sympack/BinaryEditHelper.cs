using System;
using System.IO;

namespace sympack
{
	/// <summary>
	/// Binary 編集支援クラス
	/// </summary>
	public sealed class BinaryEditHelper
	{
		public static byte[] ToBinary(Action<BinaryWriter> action)
		{
			byte[] data;
			var stream = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				action(writer);
				data = new byte[stream.Length];
				Array.Copy(stream.GetBuffer(), data, stream.Length);
			}

			return data;
		}
	}
}
