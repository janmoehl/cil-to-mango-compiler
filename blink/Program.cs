class MainClass {
	// hard compiled functions in flashed main.c
	static public void SYSsetLED(bool i) {}
	static public void SYSwait(int i) {}

	// executed bytecode:
	static public void Main() {
		bool ledOn = true;
		while (true) {
			SYSsetLED(ledOn);
			SYSwait(400000);
			ledOn = !ledOn;
		}
	}
}
