using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lngpack
{
	/// <summary>
	/// Char 拡張クラス
	/// </summary>
	internal static class CharExt
	{
		/// <summary>
		/// Shift JIS エンコーディング
		/// </summary>
		private static readonly Encoding ShiftJIS = Encoding.GetEncoding("Shift_JIS");
		/// <summary>
		/// 対象の文字が全角かチェックします
		/// </summary>
		/// <param name="that">that</param>
		/// <returns></returns>
		public static bool IsFullWidth(this char that)
		{
			int cnt = ShiftJIS.GetByteCount(new char[] { that });
			return cnt == 2;
		}
	}
}
