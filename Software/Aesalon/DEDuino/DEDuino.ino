/*
 Name:		DEDuino.ino
 Created:	3/28/2018 1:28:12 PM
 Author:	Joshua Joesten
 Notes: Uncomment the DEFINE for the Arduino board in use
		Board supported by this version:
		Arduino Uno
		Arduino Micro
		Arduino Due
		
		All Common config options are found under "config.h"

		Uncomment the DEFINE for the features you wish to use
		Features included in this version:
		Displays: DED, FFI, CMDS, PFD
*/

#include <SPI.h>
#include <Arduino.h>
#include "config.h"

/*
	All Configuration options are set via the "config.h" file
	DO NOT edit below this segment
*/ 
///****************************************************************///
#define MICRO_DELAY delayMicroseconds(250);

//////////////////////////////////////////////////////////////////////
// ARDUINO UNO														//
// SCK - Pin 13														//
// MISO - Pin 12 (Not used in this project)							//
// MOSI - Pin 11													//
// SS - Pin 10 (Set to output and pulled high on startup			//
//////////////////////////////////////////////////////////////////////
#ifdef ARDUINO_UNO
#define SCK 13
#define MISO 12
#define MOSI 11
#define SS 10
#endif

//////////////////////////////////////////////////////////////////////
// ARDUINO MICRO													//
// SCK, MISO, MOSI - all on dedicated pins no define needed			//
// use the Arduino Micro pinout for reference						//
// https://www.arduino.cc/en/uploads/Main/ArduinoMicro_Pinout3.png	//
//////////////////////////////////////////////////////////////////////
#ifdef ARDUINO_MICRO
#endif

//////////////////////////////////////////////////////////////////////
// ARDUINO DUE														//
// SCK, MOSI, MISO - all located on the ISCP Header					//
// Only MOSI and SCK needs to be connected							//
//																	//
//		1 - MISO | 0 0 | 2 - VCC									//
//		3 - SCK  | 0 0 | 4 - MOSI									//
//		5 - Reset| 0 0 | 6 - GND									//
//																	//
// NOTE:															//
//		* Due has TWO (2) ICSP headers,								//
//		  Use the one closer to the ARM chip						//
//		* Connect the Due using the "Native USB" port				//
//////////////////////////////////////////////////////////////////////
#ifdef ARDUINO_DUE
#endif

////////////////
/// INCLUDES ///
////////////////

// Serial Comm
#include "comms.h"

// Displays
#include "U8g2lib.h"

#ifdef SpeedBrake_on
#include "speedbrake_img.h"
#include "speedbrake.h"
#endif

#if defined DED_on || defined PFD_on
#include "falconded_u8g2.h"
#endif

#ifdef DED_on
#include "ded.h"
#endif

#ifdef PFD_on
#include "pfd.h"
#endif

#ifdef CMDS_on
#include "cmds.h"
#endif

#ifdef FuelFlow_on
#include "fuelflow_u8g2.h"
#include "fuelflow.h"
#endif

#include "internal.h"

///////////////
/// Globals ///
///////////////

short Run = 0;

/////////////////
/// Functions ///
/////////////////

/// MAIN PROGRAM ///
void setup() {
	delay(2000); // Allow screens to boot on power before init.

	initSerial();

	// Initialize displays
#ifdef FuelFlow_on
	initFF();
#endif

#ifdef SpeedBrake_on
	initSB();
#endif

#ifdef DED_on
	initDED();
#endif

#ifdef PFD_on
	initPFD();
#endif

#ifdef CMDS_on
	initCMDS();
#endif

	delay(POST_BOOT_BIT); // Allows the user to verify screens
}

void loop() {
	if (gotoSleep)
		goDark();

	// Fuel Flow
#ifdef FuelFlow_on
	readFF();
	drawFF();
#endif

	// DED
#ifdef DED_on
	readDED();
	drawDED();
#endif

	// Fuel Flow (again) - for refresh rate
#ifdef FuelFlow_on
	readFF();
	drawFF();
#endif

	// Non refresh critical functions, run these alternating every loop
	switch (Run)
	{
	case 0:
#ifdef PFD_on
		readPFD();
		drawPFD();
#endif
		Run = 1;
		break;
	case 1:
#ifdef CMDS_on
		readCMDS();
		drawCMDS();
#endif
		Run = 2;
		break;
	case 2:
#ifdef SpeedBrake_on
		readSB();
		drawSB();
#endif
		Run = 0;
		break;
	}
}


