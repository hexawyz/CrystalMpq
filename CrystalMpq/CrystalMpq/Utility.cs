using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace CrystalMpq
{
	internal static class Utility
	{
		[ThreadStatic]
		private static byte[] sharedBuffer;

		/// <summary>Gets a buffer of at least <paramref name="minLength"/> bytes./summary>
		/// <remarks>
		/// While actively using the buffer, you must <c>make sure</c> to not call any other method using the same shared buffer.
		/// Also, no references to the buffer should be leaked after the method requesting the buffer has returned.
		/// <c>Not following these rules carefully will likely lead to a crash.</c>
		/// </remarks>
		/// <param name="minLength">Minimum required length.</param>
		/// <returns>A buffer of at least <paramref name="minLength"/> bytes.</returns>
		public static byte[] GetSharedBuffer(int minLength) { return sharedBuffer = (sharedBuffer == null || sharedBuffer.Length < minLength) ? new byte[minLength] : sharedBuffer; }

		[ThreadStatic]
		private static MD5 sharedMD5;

		/// <summary>Gets a shared <see cref="SharedMD5"/> implementation.</summary>
		/// <remarks>The shared <see cref="MD5"/> object should be used with care, with the same rules as the shared buffer.</remarks>
		/// <value>A <see cref="SharedMD5"/> object that can be used to compute a hash.</value>
		public static MD5 SharedMD5
		{
			get
			{
				if (sharedMD5 == null) sharedMD5 = MD5.Create();

				sharedMD5.Initialize();

				return sharedMD5;
			}
		}

		public static bool CompareData(byte[] a, byte[] b)
		{
			if (a == null) throw new ArgumentNullException("a");
			if (b == null) throw new ArgumentNullException("b");

			if (a.Length != b.Length) throw new ArgumentException();

			for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;

			return true;
		}

		public static unsafe bool CompareData(byte[] a, byte* b)
		{
			if (a == null) throw new ArgumentNullException("a");
			if (b == null) throw new ArgumentNullException("b");

			for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;

			return true;
		}

		public static unsafe bool CompareData(byte* a, byte* b, uint length)
		{
			if (a == null) throw new ArgumentNullException("a");
			if (b == null) throw new ArgumentNullException("b");

			for (int i = 0; i < length; i++) if (a[i] != b[i]) return false;

			return true;
		}

		public static unsafe string FourCCToString(byte[] fourCC)
		{
			if (fourCC == null) throw new ArgumentNullException();
			if (fourCC.Length != 4) throw new ArgumentException();

			fixed (byte* fourCCPointer = fourCC)
				return FourCCToString(fourCCPointer);
		}

		public static unsafe string FourCCToString(byte* fourCC)
		{
			if (fourCC == null) throw new ArgumentNullException();

			var buffer = stackalloc char[4];

			buffer[0] = (char)fourCC[0];
			buffer[1] = (char)fourCC[1];
			buffer[2] = (char)fourCC[2];
			buffer[3] = (char)fourCC[3];

			return new string(buffer);
		}

		public static unsafe string FourCCToString(uint fourCC)
		{
			if (fourCC == null) throw new ArgumentNullException();

			var buffer = stackalloc char[4];

			buffer[0] = (char)(fourCC & 0xFF);
			buffer[1] = (char)((fourCC >> 8) & 0xFF);
			buffer[2] = (char)((fourCC >> 16) & 0xFF);
			buffer[3] = (char)(fourCC >> 24);

			return new string(buffer);
		}
	}
}
