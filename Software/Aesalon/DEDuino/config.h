///////////////////////////////////////////////////////////////////////
// Select Arduino Type - Uncomment correct board type                //
///////////////////////////////////////////////////////////////////////

//#define ARDUINO_DUE_NATIVE
#define ARDUINO_DUE_PROG

///////////////////////////////////////////////////////////////////////
// Module Defines                                                    //
// Uncomment The modules you want                                    //
// Without this settings Nothing will work                           //
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

////////// DED/PFL //////////
// uncomment to rotate display 180Degrees
//#define rotateDED
//#define rotatePFD

////////// FFI //////////

//// Select the correct FFI display driver
//// SSD1306 dispaly drivers (such as the ones sold by Adafruit and sparkfun)
#define FFI_SSD1306
//// SH1106 display drivers (commonly found on ebay)
//#define FFI_SH1106

//// Define RealFFI to draw "Real" FFI - when not defined a BMS style FFI is drawn
//// Real FFI animation requires more RAM available there for is not suitable for an Uno if DED/PFL are enabled
//// This feature will be automaticly disabled when using arduino Uno
#define RealFFI

////////// SpeedBrake //////////
//// Select the correct SB display driver
//// SSD1306 dispaly drivers (such as the ones sold by Adafruit and sparkfun)
#define SB_SSD1306
//// SH1106 display drivers (commonly found on ebay)
//#define SB_SH1106

////////// DEBUGGING //////////
//// Enable crosshair to help align text on screens ////
#define crosshair

////////// INTERNAL VARS //////////
// How many seconds should go by before screens turn off after no response recived to the "I'm alive" signal
#define SLEEP_TIMER 120000 // 120 seconds In Miliseconds

// How many milliseconds go before the displays reset after initillizing (for user inspection)
#define POST_BOOT_BIT 4000



///*************** DO NOT EDIT BELOW THIS LINE WITHOUT INTENT! ***************//
// if any of the DUE options are enabled, enable the global DUE flag
#if defined(ARDUINO_DUE_NATIVE) || defined(ARDUINO_DUE_PROG)
#define ARDUINO_DUE
#endif
///////////////////////////////////////////////////////////////////////
// Sub-Module Pinout configurations                                  //
// (Advanced settings - Don't change unless you mean it              //
///////////////////////////////////////////////////////////////////////

// Screen pins should be relativley static
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
#endif
