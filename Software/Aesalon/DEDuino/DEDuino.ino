/*
 Name:		DEDuino.ino
 Created:	1/29/2019 3:08:39 PM
 Author:	jjoesten

 Notes:		Boards supported by this version:
			Arduino DUE

			All common config options are found in "config.h"

			Uncomment the DEFINE for the features you wish to enable
			Featues included in this version:
			Displays: DED, FFI, CMDS, PFD, Speedbrake
*/

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

/*
	ALL CONFIGURATIONS OPTIONS ARE SET IN "config.h"
	DO NOT EDIT BELOW THIS SEGMENT
*/

#include <SPI.h>
#include <Wire.h>
#include <Arduino.h>
#include "config.h"

#define MICRO_DELAY delayMicroseconds(250);

////////////////
/// INCLUDES ///
////////////////

// Serial Communication
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
/// GLOBALS ///
///////////////

short Run = 0;

/// MAIN PROGRAM ///

void setup() {
	delay(2000); // Allow screens to boot before init.

	initSerial();

	// Initialize displays
#ifdef FuelFlow_on
	initFF();
#endif

#ifdef Speedbrake_on
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

	delay(POST_BOOT_BIT); // Allows time for the user to verify screens
}

// the loop function runs over and over again until power down or reset
void loop() {
	if (gotoSleep)
		goDark();
	
	// Refresh critical functions, run these every frame (FF twice for refresh rate)

#ifdef FuelFlow_on
	readFF();
	drawFF();
#endif

#ifdef DED_on
	readDED();
	drawDED();
#endif

#ifdef FuelFlow_on
	readFF();
	drawFF();
#endif

	// Non refresh critical functions, run these on alternating loops
	// PFD loop 0
	// CMDS loop 1
	// SB loop 2
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
