

/**
 * Types of secret keys.
 */
public enum OtpType {  // must be the same as in res/values/strings.xml:type
    Totp,  // time based
    Hotp  // counter based
}
