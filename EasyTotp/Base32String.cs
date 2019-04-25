using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/**
* Encodes arbitrary byte arrays as case-insensitive base-32 strings.
* <p>
* The implementation is slightly different than in RFC 4648. During encoding,
* padding is not added, and during decoding the last incomplete chunk is not
* taken into account. The result is that multiple strings decode to the same
* byte array, for example, string of sixteen 7s ("7...7") and seventeen 7s both
* decode to the same byte array.
* TODO(sarvar): Revisit this encoding and whether this ambiguity needs fixing.
*
* @author sweis@google.com (Steve Weis)
* @author Neal Gafter
*/
namespace EasyTotp
{
	public class Base32String
	{
		// singleton

		private static readonly Base32String Instance = new Base32String("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"); // RFC 4648/3548

		private static Base32String getInstance()
		{
			return Instance;
		}

		private readonly int mask;
		private readonly int shift;
		private readonly IDictionary<char, int> charMap;

		private readonly string separator = "-";

		private Base32String(string alphabet)
		{
			// 32 alpha-numeric characters.
			var digits = alphabet.ToCharArray();
			mask = digits.Length - 1;
			shift = numberOfTrailingZeros(digits.Length); //int.numberOfTrailingZeros(DIGITS.Length);
			charMap = new Dictionary<char, int>();
			for (var i = 0; i < digits.Length; i++)
			{
				charMap.Add(digits[i], i);
			}
		}

		// risky
		public static int numberOfTrailingZeros(int i)
		{
			// HD, Figure 5-14
			int y;
			if (i == 0) return 32;
			int n = 31;
			y = i << 16; if (y != 0) { n = n - 16; i = y; }
			y = i << 8; if (y != 0) { n = n - 8; i = y; }
			y = i << 4; if (y != 0) { n = n - 4; i = y; }
			y = i << 2; if (y != 0) { n = n - 2; i = y; }
			return n - (int)((uint)(i << 1) >> 31);
			//return n - ((i << 1) >>> 31);
		}

		public static byte[] decode(string encoded)
		{
			return getInstance().decodeInternal(encoded);
		}

		private byte[] decodeInternal(string encoded)
		{
			// Remove whitespace and separators
			encoded = encoded.Trim().Replace/*All*/(separator, "").Replace/*All*/(" ", "");

			// Remove padding. Note: the padding is used as hint to determine how many
			// bits to decode from the last incomplete chunk (which is commented out
			// below, so this may have been wrong to start with).
			//encoded = encoded.replaceFirst("[=]*$", "");
			encoded = Regex.Replace(encoded, "[=]*$", "");


			// Canonicalize to all upper case
			encoded = encoded.ToUpper(/*Locale.US*/);

			if (encoded.Length == 0)
			{
				return new byte[0];
			}
			var encodedLength = encoded.Length;
			var outLength = encodedLength * shift / 8;
			var result = new byte[outLength];
			var buffer = 0;
			var next = 0;
			var bitsLeft = 0;
			foreach (var c in encoded.ToCharArray())
			{
				if (!charMap.ContainsKey(c))
				{
					throw new /*DecodingException*/ Exception("Illegal character: " + c);
				}
				buffer <<= shift;
				buffer |= charMap[c] & mask;
				bitsLeft += shift;
				if (bitsLeft >= 8)
				{
					result[next++] = (byte)(buffer >> (bitsLeft - 8));
					bitsLeft -= 8;
				}
			}
			// We'll ignore leftover bits for now.
			//
			// if (next != outLength || bitsLeft >= SHIFT) {
			//  throw new DecodingException("Bits left: " + bitsLeft);
			// }
			return result;
		}


		// enforce that this class is a singleton
		public object clone()
		{
			throw new Exception("Cant clone");//CloneNotSupportedException();
		}

		public class DecodingException : Exception
		{
			DecodingException(string message) : base(message)
			{
			}
		}
	}
}
