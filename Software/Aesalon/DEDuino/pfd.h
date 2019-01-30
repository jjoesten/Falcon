// Declare screen object
#ifndef rotatePFD
U8G2_SSD1322_NHD_256X64_F_4W_HW_SPI pfdDisp(U8G2_R2, DED_SEL, DISP_A0);
#else
U8G2_SSD1322_NHD_256X64_F_4W_HW_SPP pfdDisp(U8G2_R0, DED_SEL, DISP_A0);
#endif

// Font Settings
#define pfdFont FalconDED_wide
constexpr auto PFD_CHAR_W = 10;
constexpr auto PFD_CHAR_H = 12;
constexpr auto PFD_H_CONST = 8;
constexpr auto PFD_V_CONST = 2;

// Global PFD line array
char PFD[5][25] = { {0} };

/////////////////
/// FUNCTIONS ///
/////////////////

void initPFD() {
	pfdDisp.begin();

	pfdDisp.setFont(pfdFont);
	pfdDisp.setFontPosTop();
	pfdDisp.firstPage();
	do {
		pfdDisp.drawStr(PFD_H_CONST, 2 * PFD_CHAR_H + PFD_V_CONST, "PFD - READY!");

#ifdef crosshair
		pfdDisp.drawFrame(0, 0, 256, 64);
		pfdDisp.drawLine(128, 0, 128, 64);
		pfdDisp.drawLine(0, 32, 256, 32);
#endif
	} while (pfdDisp.nextPage());
}

void readPFD() {
	COM.print("p");
	for (short i = 0; i < 5; i++) {
		COM.print(i);
		commsCheck(COM.readBytes(PFD[i], 24));
	}
}

void drawPFD() {
	pfdDisp.firstPage();
	do {
		for (unsigned short i = 0; i < 5; i++) {
			pfdDisp.drawStr(PFD_H_CONST, i * PFD_CHAR_H + PFD_V_CONST, PFD[i]);
		}
	} while (pfdDisp.nextPage());
}