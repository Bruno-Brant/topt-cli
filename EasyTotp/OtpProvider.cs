
/*
 * Copyright 2010 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using System;

/**
* Class containing implementation of HOTP/TOTP.
* Generates OTP codes for one or more accounts.
*
* @author Steve Weis (sweis@google.com)
* @author Cem Paya (cemp@google.com)
*/
namespace EasyTotp
{
	public class OtpProvider : IOtpSource
	{

		private readonly int pinLength = 6; // HOTP or TOTP

		public string getNextCode(Account account)
		{
			return getCurrentCode(account);
		}

		private string getCurrentCode(Account account)
		{
			if (account == null)
			{
				throw new OtpSourceException("No account name");
			}

			var type = account.Type;
			var secret = account.Secret;

			long otpState = 0;

			if (type == OtpType.Totp)
			{
				// For time-based OTP, the state is derived from clock.
				otpState =
					mTotpCounter.getValueAtTime(Utilities.millisToSeconds(mTotpClock.currentTimeMillis()));
			}
			else if (type == OtpType.Hotp)
			{
				// For counter-based OTP, the state is obtained by incrementing stored counter.
				account.Counter++;
				var counter = account.Counter;
				otpState = Convert.ToInt64(counter);
			}

			return ComputePin(secret, otpState);
		}

		public OtpProvider(TotpClock totpClock) : this(DefaultInterval, totpClock)
		{
		}

		private OtpProvider(int interval, TotpClock totpClock)
		{
			mTotpCounter = new TotpCounter(interval);
			mTotpClock = totpClock;
		}

		/**
     * Computes the one-time PIN given the secret key.
     *
     * @param secret    the secret key
     * @param otp_state current token state (counter or time-interval)
     * @return the PIN
     */
		private string ComputePin(string secret, long otpState)
		{
			if (secret == null || secret.Length == 0)
			{
				throw new OtpSourceException("Null or empty secret");
			}


			var signer = AccountDb.getSigningOracle(secret);
			var pcg = new PasscodeGenerator(signer,
				pinLength);

			return pcg.GenerateResponseCode(otpState);

		}

		/// <summary>
		/// Default passcode timeout period (in seconds)
		/// </summary>
		private static readonly int DefaultInterval = 30;

		/// </summary>
		/// Counter for time-based OTPs (TOTP).
		/// <summary>
		private readonly TotpCounter mTotpCounter;

		///
		/// Clock input for time-based OTPs (TOTP).
		///
		private readonly TotpClock mTotpClock;
	}
}
