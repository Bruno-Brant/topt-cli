

/**
 * A class for handling a variety of utility things.  This was mostly made
 * because I needed to centralize dialog related constants. I foresee this class
 * being used for other code sharing across Activities in the future, however.
 *
 * @author alexei@czeskis.com (Alexei Czeskis)
 */
class Utilities {
    private  const long SecondInMillis = 1000;

    // Constructor -- Does nothing yet
    private Utilities() {
    }

	public static long millisToSeconds(long timeMillis) {
        return timeMillis / SecondInMillis;
    }
}
