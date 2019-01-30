// Declare Screen Object
#ifdef FFI_SSD1306
U8G2_SSD1306_128X64_NONAME_F_4W_HW_SPI ffDisp(U8G2_R0, FF_SEL, DISP_A0);
#endif
#ifdef FFI_SH1106
U8G2_SH1106_128X64_NONAME_F_4W_HW_SPI ffDisp(U8G2_R0, FF_SEL, DISP_A0);
#endif

constexpr auto FFI_SCREEN_W = 128;
constexpr auto FFI_SCREEN_H = 64;

// Font Settings
#define ffFont fuelflow_u8g2
constexpr auto FF_CHAR_W = 20;
constexpr auto FF_CHAR_H = 30;
constexpr auto FF_H_CONST = 0;
constexpr auto FF_V_CONST = 0;

// Global Fuel Flow line array
char FuelFlow[5];

// Screen Center
const unsigned short FFI_SCREEN_W_MID = FFI_SCREEN_W / 2;
const unsigned short FFI_SCREEN_H_MID = FFI_SCREEN_H / 2;

// Digit Positions
const unsigned short FF_POS_X_1 = int(FFI_SCREEN_W_MID - ((FF_CHAR_W * 7) / 2)) + ((FF_CHAR_W + 1) * 1) + FF_H_CONST;
const unsigned short FF_POS_X_2 = int(FFI_SCREEN_W_MID - ((FF_CHAR_W * 7) / 2)) + ((FF_CHAR_W + 1) * 2) + FF_H_CONST;
const unsigned short FF_POS_X_3 = int(FFI_SCREEN_W_MID - ((FF_CHAR_W * 7) / 2)) + ((FF_CHAR_W + 1) * 3) + FF_H_CONST;
const unsigned short FF_POS_X_4 = int(FFI_SCREEN_W_MID - ((FF_CHAR_W * 7) / 2)) + ((FF_CHAR_W + 1) * 4) + FF_H_CONST;
const unsigned short FF_POS_X_5 = int(FFI_SCREEN_W_MID - ((FF_CHAR_W * 7) / 2)) + ((FF_CHAR_W + 1) * 5) + FF_H_CONST;

// Digit Y position
const unsigned short FF_POS_Y = FFI_SCREEN_H_MID + FF_V_CONST;

/////////////////
/// FUNCTIONS ///
/////////////////

void initFF() {
	ffDisp.begin();

	ffDisp.setFont(ffFont);
	ffDisp.setFontPosCenter();

	ffDisp.firstPage();
	do {
		ffDisp.drawStr(FF_POS_X_1, FF_POS_Y, "99999");

#ifdef crosshair
		ffDisp.drawFrame(0, 0, 128, 64);
		ffDisp.drawLine(64, 0, 64, 64);
		ffDisp.drawLine(0, 32, 128, 32);
#endif
	} while (ffDisp.nextPage());
}

void readFF() {
	COM.print('F');
	commsCheck(COM.readBytes(FuelFlow, 5));
}

void drawFF() {
	if (FuelFlow[3] == 0) { // Check if FF is zeroed out (i.e. middle digit is null (not the character "0"))
		ffDisp.firstPage();
		do {} while (ffDisp.nextPage());
	}
	else {
		// break FF into segments for scroll animation
		char FFh = FuelFlow[2];
		// Animation, find the previous next two digits
		char FFhPrev;
		if (FuelFlow[2] == 48) // If zero, next is 9
			FFhPrev = 57;
		else
			FFhPrev = FuelFlow[2] - 1;

		char FFhNext;
		if (FuelFlow[2] == 57) // If 9, next is 0
			FFhNext = 48;
		else
			FFhNext = FuelFlow[2] + 1;

		char FFhTwoOver;
		if (FFhNext == 57)
			FFhTwoOver = 48;
		else
			FFhTwoOver = FFhNext + 1;

		// Two upper digits
		char FFtt = FuelFlow[0];
		char FFt = FuelFlow[1];

#ifdef RealFFI
		byte RollOverFlags = 0;
#define FFtRollOver (RollOverFlags & 0x01) // FFt is about to roll over
#define FFttRollOver (RollOverFlags & 0x02) // FFtt is about to roll over

		// FFt
		char FFtNext;
		char FFtPrev;

		if (FFt == 48)
			FFtPrev = 57;
		else
			FFtPrev = FFt - 1;

		if (FFt == 57)
			FFtNext = 48;
		else
			FFtNext = FFt + 1;

		// FFtt
		char FFttNext;
		char FFttPrev;

		if (FFtt == 56)
			FFttNext = 47;
		else
			FFttNext = FFtt + 1;

		if (FFtt == 48) {
			FFttPrev = 47;
			FFtt = 47;
		}
		else if (FFtt == 49)
			FFttPrev = 47;
		else
			FFttPrev = FFtt - 1;

		if (FFh == 57) { // If FFh is "9" we are at roll over (up or down, doesn't matter)
			RollOverFlags |= 0x01; // If FFh is "9", going up
			if (FFt == 57) { // If FFt is 9 and rolling over the FFt is also rolling
				RollOverFlags |= 0x02;
			}
		}		
#endif

		// Use tens digit to calculate the vertical offset for animation (tens and singles are always zero on the gauge)
		short offset = short((FuelFlow[3] - 48) * FF_CHAR_H / 10);

		/// BEGIN PICTURE LOOP ///
		ffDisp.firstPage();
		do {
#ifdef RealFFI
			// Draw FFtt
			if (FFttRollOver) { // Rolling over, animate
				ffDisp.setCursor(FF_POS_X_1, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * -1)) + offset + FF_V_CONST);
				ffDisp.print(FFttNext);
				ffDisp.setCursor(FF_POS_X_1, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 0)) + offset + FF_V_CONST);
				ffDisp.print(FFtt);
				ffDisp.setCursor(FF_POS_X_1, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 1)) + offset + FF_V_CONST);
				ffDisp.print(FFttPrev);
			}
			else { // no rollover, just print
				ffDisp.setCursor(FF_POS_X_1, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * -1)) + FF_V_CONST);
				ffDisp.print(FFttNext);
				ffDisp.setCursor(FF_POS_X_1, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 0)) + FF_V_CONST);
				ffDisp.print(FFtt);
				ffDisp.setCursor(FF_POS_X_1, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 1)) + FF_V_CONST);
				ffDisp.print(FFttPrev);
			}

			// Draw FFt
			if (FFtRollOver) { // Rolling over, animate
				ffDisp.setCursor(FF_POS_X_2, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * -1)) + offset + FF_V_CONST);
				ffDisp.print(FFtNext);
				ffDisp.setCursor(FF_POS_X_2, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 0)) + offset + FF_V_CONST);
				ffDisp.print(FFt);
				ffDisp.setCursor(FF_POS_X_2, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 1)) + offset + FF_V_CONST);
				ffDisp.print(FFtPrev);
			}
			else { // No rollover, just print
				ffDisp.setCursor(FF_POS_X_2, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * -1)) + FF_V_CONST);
				ffDisp.print(FFtNext);
				ffDisp.setCursor(FF_POS_X_2, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 0)) + FF_V_CONST);
				ffDisp.print(FFt);
				ffDisp.setCursor(FF_POS_X_2, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 1)) + FF_V_CONST);
				ffDisp.print(FFtPrev);
			}
#else
			// Draw normal FFtt and FFt
			ffDisp.setCursor(FF_POS_X_1, FFI_SCREEN_H_MID + FF_V_CONST);
			ffDisp.print(FFtt); // First two digits
			ffDisp.setCursor(FF_POS_X_2, FFI_SCREEN_H_MID + FF_V_CONST);
			ffDisp.print(FFt); // First two digits
#endif

			// Print FFh animation
			ffDisp.setCursor(FF_POS_X_3, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * -2)) + offset + FF_V_CONST);
			ffDisp.print(FFhTwoOver);
			ffDisp.setCursor(FF_POS_X_3, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * -1)) + offset + FF_V_CONST);
			ffDisp.print(FFhNext);
			ffDisp.setCursor(FF_POS_X_3, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 0)) + offset + FF_V_CONST);
			ffDisp.print(FFh);
			ffDisp.setCursor(FF_POS_X_3, FFI_SCREEN_H_MID + short(((FF_CHAR_H + 1) * 1)) + offset + FF_V_CONST);
			ffDisp.print(FFhPrev);

			// Print the statics
			ffDisp.drawStr(FF_POS_X_4, FFI_SCREEN_H_MID + FF_V_CONST, "0");
			ffDisp.drawStr(FF_POS_X_5, FFI_SCREEN_H_MID + FF_V_CONST, "0");
		} while (ffDisp.nextPage());
	} 
}