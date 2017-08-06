using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetcodeIO.NET.Utils
{
	/// <summary>
	/// Utility for generating crypto keys
	/// </summary>
	public static class KeyUtils
	{
		private static Random rand = new Random();

		/// <summary>
		/// Generate a random key
		/// </summary>
		public static void GenerateKey(byte[] keyBuffer)
		{
			rand.NextBytes(keyBuffer);
		}
	}
}
