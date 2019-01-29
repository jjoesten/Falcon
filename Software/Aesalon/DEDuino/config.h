///////////////////////////////////////////////////////////////////////
// Select Arduino Type - Uncomment correct board type                //
///////////////////////////////////////////////////////////////////////

//// Arduino Uno is the default as it is the most common - how ever it is NOT the recommended Arduino to use due to it's low amount of SRAM
//// Arduino Micro is considered the Minimum at this stage of the project
//// Arduino Due is the recommended Arduino in the long run (able to support future expansions)
//// There are two USB connections on the DUE, the NATIVE is the recommended, but in some cases driver issues may strike.

//#define ARDUINO_UNO
//#define ARDUINO_MICRO
//#define ARDUINO_PRO_MICRO
//#define ARDUINO_DUE_NATIVE
#define ARDUINO_DUE_PROG



///////////////////////////////////////////////////////////////////////
// Module Defines                                                    //
// Uncomment The modules you want                                    //
// Without this settings Nothing will work                           //
// NOTE: Lights are going to be overhauled in V1.2                   //
///////////////////////////////////////////////////////////////////////

#define Screens //global enable screens


///////////////////////////////////////////////////////////////////////
// Sub-Module Defines                                                //
// Uncomment The modules you want                                    //
///////////////////////////////////////////////////////////////////////
#ifdef Screens
#define DED_on
#define FuelFlow_on
#define PFD_on 
#define CMDS_on 
#define SpeedBrake_on
#endif


///////////////////////////////////////////////////////////////////////
// Sub-Module config                                                 //
// Uncomment The options you want to enable                          //
///////////////////////////////////////////////////////////////////////


////////// DED/PFL //////////
// uncomment to rotate display 180Degrees
//#define rotateDED
//#define rotatePFD

////////// FFI //////////
//*****************************************************************************************//
//** NOTE:                                                                               **// 
//** Due to performance issues, when using Arduino Uno,                                  **//
//** The RealFFI animation and the Bezel will be Automaticly disabled (check fuelflow.h) **//
//*****************************************************************************************//

//// Select the correct FFI display driver
//// SSD1306 dispaly drivers (such as the ones sold by Adafruit and sparkfun)
#define FFI_SSD1306

//// SH1106 display drivers (commonly found on ebay)
//#define FFI_SH1106

//** NOTE: Ebay bought displays are sometime mislabled and SH1106 displays may be SSD1306 (and vice versa) **//

/// Define bezel to draw FUEL FLOW and PPH on screen.
//// Bezel reduces FPS considerably - if you build a real Bezel, this option should be off
#define Bezel

//// Define RealFFI to draw "Real" FFI - when not defined a BMS style FFI is drawn
//// Real FFI animation requires more RAM available there for is not suitable for an Uno if DED/PFL are enabled
//// This feature will be automaticly disabled when using arduino Uno
#define RealFFI

////////// SpeedBreaks //////////
//// Select the correct SB display driver
//// SSD1306 dispaly drivers (such as the ones sold by Adafruit and sparkfun)
#define SB_SSD1306

//// SH1106 display drivers (commonly found on ebay)
//#define SB_SH1106


////////// DEBUGGING //////////
//// Enable crosshair to help align text on screens ////
//#define crosshair

///*************** DO NOT EDIT BELOW THIS LINE WITHOUT INTENT! ***************//
// if any of the DUE options are enabled, enable the global DUE flag
#if defined(ARDUINO_DUE_NATIVE) || defined(ARDUINO_DUE_PROG)
#define ARDUINO_DUE
#endif
///////////////////////////////////////////////////////////////////////
// Sub-Module Pinout configurations                                  //
// (Advanced settings - Don't change unless you mean it              //
///////////////////////////////////////////////////////////////////////

// Screen pins should be relativly static
#ifdef Screens
#if defined(ARDUINO_UNO) || defined(ARDUINO_DUE)
#define DISP_A0 9
#define DED_SEL 8
#define FF_SEL 7
#define PFD_SEL 6
#define CMDS_SEL 5
#define EXT1_SEL 4
#define EXT2_SEL 3
#endif

#ifdef ARDUINO_MICRO
#define DISP_A0 12
#define DED_SEL 11
#define FF_SEL 10
#define PFD_SEL 9
#define CMDS_SEL 8
#define EXT1_SEL 7
#define EXT2_SEL 6
#endif

#ifdef ARDUINO_PRO_MICRO
#define DISP_A0 10
#define DED_SEL 9
#define FF_SEL 8
#define PFD_SEL 7
#define CMDS_SEL 6
#define EXT1_SEL 5
#define EXT2_SEL 4
#endif

#endif

////////// INTERNAL VARS //////////
// How many seconds should go by before screens turn off after no response recived to the "I'm alive" signal
#define SLEEP_TIMER 120000 // 120 seconds In Miliseconds

// How many milliseconds go before the displays reset after initillizing (for user inspection)
#define POST_BOOT_BIT 4000