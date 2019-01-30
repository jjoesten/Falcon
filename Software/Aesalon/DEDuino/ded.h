// Declare screen object
#ifndef rotateDED
U8G2_SSD1322_NHD_256X64_F_4W_HW_SPI dedDisp(U8G2_R2, DED_SEL, DISP_A0);
#else
U8G2_SSD1322_NHD_256X64_F_4W_HW_SPP dedDisp(U8G2_R0, DED_SEL, DISP_A0);
#endif

// Font Settings
#define dedFont FalconDED_wide
constexpr auto DED_CHAR_W = 10;
constexpr auto DED_CHAR_H = 12;
constexpr auto DED_H_CONST = 12;
constexpr auto DED_V_CONST = 1;

// Global DED Line array
char DED[5][25] = { {0} };

/////////////////
/// FUNCTIONS ///
/////////////////

void initDED() {
	dedDisp.begin();

	dedDisp.setFont(dedFont);
	dedDisp.setFontPosTop();
	dedDisp.firstPage(); // Begin picture loop
	do {
		dedDisp.drawStr(DED_H_CONST, 2 * DED_CHAR_H + DED_V_CONST, "DED - READY!");

#ifdef crosshair
		dedDisp.drawFrame(0, 0, 256, 64);
		dedDisp.drawLine(128, 0, 128, 64);
		dedDisp.drawLine(0, 32, 256, 32);
#endif

	} while (dedDisp.nextPage()); // End picture loop
}

void readDED() {
	COM.print("d");
	for (short i = 0; i < 5; i++) {
		COM.print(i);
		commsCheck(COM.readBytes(DED[i], 24));
	}
}

void drawDED() {
	dedDisp.firstPage();
	do {
		for (unsigned short i = 0; i < 5; i++) {
			dedDisp.drawStr(DED_H_CONST, i * DED_CHAR_H + DED_V_CONST, DED[i]);
		}
	} while (dedDisp.nextPage());
}