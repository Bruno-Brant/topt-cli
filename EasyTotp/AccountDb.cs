using System;
using System.Security.Cryptography;

/**
 * A database of email addresses and secret values
 *
 * @author sweis@google.com (Steve Weis)
 */
namespace EasyTotp
{
	class AccountDb
	{
		public static PasscodeGenerator.Signer getSigningOracle(string secret)
		{
			try
			{
				var keyBytes = decodeKey(secret);
				HMAC mac = new HMACSHA1(keyBytes);
				//mac.init(new SecretKeySpec(keyBytes, ""));

				// Create a signer object out of the standard Java MAC implementation.
				return b => mac.ComputeHash(b);

			}
			//catch (Base32string.DecodingException | NoSuchAlgorithmException | InvalidKeyException error) {
			catch (Exception e)
			{
				//System.out.println(error.getMessage());
				System.Console.WriteLine(e.Message);
				throw;
			}
		}

		private static byte[] decodeKey(string secret)
		{
			return Base32String.decode(secret);
		}
	}
}
