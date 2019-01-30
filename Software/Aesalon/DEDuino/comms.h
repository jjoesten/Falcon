#ifndef comms_h
#define comms_h

constexpr auto BAUDRATE = 115200; // 9600 * 12;
constexpr auto SERIAL_TIMEOUT = 50; // Too low of a value will cause displays to jump if connection is not fast enough;

#if defined(ARDUINO_DUE_NATIVE)
#define COM SerialUSB
#else
#define COM Serial
#endif

/// SERIAL OPERATION FUNCTIONS ///
void initSerial() {
	COM.begin(BAUDRATE);
	COM.setTimeout(SERIAL_TIMEOUT);
}

// Sleep logic
// TODO: Revisit and implement sleep logic for displays
bool gotoSleep = false;
#define TIMED_OUT (millis()-last_comm) > (SLEEP_TIMER)
unsigned long last_comm = millis();

void commsCheck(short report) {
	if (report > 0) {
		last_comm = millis();
		gotoSleep = false;
		return;
	}
	else if (TIMED_OUT) {
		gotoSleep = true;
	}
}

#endif