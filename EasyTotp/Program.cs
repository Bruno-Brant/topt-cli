using System;

namespace EasyTotp
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// TODO: simplify (remove the account object as it no longer interests us???)
			var account = new Account
			{
				Secret = args[0],
				Counter = 0,
				Type = OtpType.Totp
			};

			var otpProvider = new OtpProvider(
				new TotpClock(
					new NetworkTimeProvider(
						new System.Net.Http.HttpClient()
					),
					int.Parse(args[1])
				));

			var nextCode = otpProvider.getNextCode(account);
			Console.Write(nextCode);
		}
	}
}