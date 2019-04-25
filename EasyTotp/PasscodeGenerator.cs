using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EasyTotp
{
	/**
	* An implementation of the HOTP generator specified by RFC 4226. Generates
	* short passcodes that may be used in challenge-response protocols or as
	* timeout passcodes that are only valid for a short period.
	*
	* The default passcode is a 6-digit decimal code. The maximum passcode length is 9 digits.
	*
	* @author sweis@google.com (Steve Weis)
	*
*/
	public class PasscodeGenerator
	{
		private const int MaxPasscodeLength = 9;

		/** Powers of 10 used to shorten the pin to the desired number of digits */
		private readonly int[] digitsPower
			//  0  1  2     3     4      5       6        7         8          9
			= { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000 };

		private readonly Signer signer;

		private readonly int codeLength;

		/// <summary>
		/// Using an interface to allow us to inject different signature implementations.
		/// </summary>
		/// <param name="data">Pre image to sign, represented as sequence of arbitrary bytes</param>
		/// <returns>Signature as sequence of bytes.</returns>
		public delegate byte[] Signer(byte[] data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mac">A {@link Mac} used to generate passcodes</param>
		/// <param name="passCodeLength">The length of the decimal passcode</param>
		public PasscodeGenerator(HMAC mac, int passCodeLength)
			: this(mac.ComputeHash, passCodeLength)
		{
		}

		public PasscodeGenerator(Signer signer, int passCodeLength)
		{
			if (passCodeLength < 0 || passCodeLength > MaxPasscodeLength)
			{
				throw new ArgumentException(
					$"PassCodeLength must be between 1 and {MaxPasscodeLength} digits.");
			}

			this.signer = signer;
			codeLength = passCodeLength;
		}

		/**
	     * @param state 8-byte integer value representing internal OTP state.
	     * @return A decimal response code
	     */
		public string GenerateResponseCode(long state)
		{
#if DEBUG
			//state = 51891420;
#endif
			var value = ByteBuffer.allocate(8).putLong(state).array().Reverse().ToArray();
			return GenerateResponseCode(value);
		}

		/**
	     * @param challenge An arbitrary byte array used as a challenge
	     * @return A decimal response code
	     * @occur
	     */
		private string GenerateResponseCode(byte[] challenge)
		{
			var hash = signer(challenge);

			// Dynamically truncate the hash
			// OffsetBits are the low order bits of the last byte of the hash
			var offset = hash[hash.Length - 1] & 0xF;
			// Grab a positive integer value starting at the given offset.
			var hashToInt = HashToInt(hash, offset);
			var truncatedHash = hashToInt & 0x7FFFFFFF;
			var pinValue = truncatedHash % digitsPower[codeLength];
			return PadOutput(pinValue);
		}

		private string PadOutput(int value)
		{
			return value.ToString().PadRight(codeLength, '0');
		}

		/**
	     * Grabs a positive integer value from the input array starting at
	     * the given offset.
	     * @param bytes the array of bytes
	     * @param start the index into the array to start grabbing bytes
	     * @return the integer constructed from the four bytes in the array
	     */
		private int HashToInt(byte[] bytes, int start)
		{
			//DataInput input = new DataInputStream(
			//	new ByteArrayInputStream(bytes, start, bytes.length - start));
			var slice = bytes.Skip(start).Take(4).ToArray();
			
			// endianess problems - C# reads big-endian and the algo is little... so... do it by hand
			int val = 0;

			val |= slice[0] << 4 * 6;
			val |= slice[1] << 4 * 4;
			val |= slice[2] << 4 * 2;
			val |= slice[3] << 4 * 0;
			
			return val;

			//using (var ms = new MemoryStream(bytes, start, bytes.Length - start))
			//{

			//}
			//using (var input = new BinaryReader(ms))
			//{
			//	var v = input.ReadInt32();
			//	return v;
			//}
		}
	}
}