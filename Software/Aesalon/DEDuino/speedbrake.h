// Declare Screen Object
#ifdef SB_SSD1306
U8G2_SSD1306_128X64_NONAME_F_4W_HW_SPI sbDisp(U8G2_R0, EXT1_SEL, DISP_A0);
#endif
#ifdef SB_SH1106
U8G2_SSD1306_128X64_NONAME_2_4W_HW_SPI sbDisp(U8G2_R0, EXT1_SEL, DISP_A0);
#endif

// Display Size
constexpr auto SB_SCREEN_W = 128;
constexpr auto SB_SCREEN_H = 64;
constexpr auto NUM_FRAMES = 6;

#define SB_IMG speedbreak_indicator_bits
constexpr auto SB_IMG_W = 192;
constexpr auto SB_IMG_H = 64;

constexpr auto SB_OFFSET_H = SB_IMG_H / 2;

#define SRC_OFFSET (-1 * SB_IMG_H*SpeedBrake[1]) + SB_OFFSET_H
#define DST_OFFSET (-1 * SB_IMG_H*SpeedBrake[0]) + SB_OFFSET_H

// Global SpeedBrake position array
char SpeedBrake[2] = { 3 };

/////////////////
/// FUNCTIONS ///
/////////////////

void initSB() {
	sbDisp.begin();

	sbDisp.firstPage();
	do {
		sbDisp.drawXBMP(-32, 0, SB_IMG_W, SB_IMG_H, SB_IMG); // Start with INOP
#ifdef crosshairs
		sbDisp.drawFrame(0, 0, 128, 64);
		sbDisp.drawLine(64, 0, 64, 64);
		sbDisp.drawLine(0, 32, 128, 32);
#endif
	} while (sbDisp.nextPage());
}

void readSB() {
	SpeedBrake[1] = SpeedBrake[0];
	COM.print('S');
	commsCheck(COM.readBytes(SpeedBrake, 1));
}

void drawSB() {
	if (SpeedBrake[0] == 5) { // Check if SB is zeroed out (i.e. value of 5, which is not something you'll get)
		sbDisp.firstPage();
		do {} while (sbDisp.nextPage());
	}
	else if (SpeedBrake[0] != SpeedBrake[1]) { // Value has changed
		float offsetDelta = ((SRC_OFFSET) - (DST_OFFSET)) / NUM_FRAMES;
		for (short i = 0; i < NUM_FRAMES; i++) {
			if (i == 3 || i == 4)
				break;

			short frameOffset = (SRC_OFFSET) + (i * offsetDelta);
			
			sbDisp.firstPage();
			do {
				sbDisp.drawXBMP(frameOffset, 0, SB_IMG_W, SB_IMG_H, SB_IMG);
			} while (sbDisp.nextPage());
		}

		sbDisp.firstPage();
		do {
			sbDisp.drawXBMP(DST_OFFSET, 0, SB_IMG_W, SB_IMG_H, SB_IMG);
		} while (sbDisp.nextPage());
	}
}