// Declare screen object
#ifndef rotateDED
U8G2_SSD1322_NHD_256X64_F_4W_HW_SPI cmdsDisp(U8G2_R2, DED_SEL, DISP_A0);
#else
U8G2_SSD1322_NHD_256X64_F_4W_HW_SPP cmdsDisp(U8G2_R0, DED_SEL, DISP_A0);
#endif

// Font Settings
#define cmdsFont FalconDED_wide
constexpr auto CMDS_CHAR_W = 9;
constexpr auto CMDS_CHAR_H = 35;
constexpr auto CMDS_H_CONST = 0;
constexpr auto CMDS_V_CONST = 2;

// Global CMDS line array
char CMDS[2][25] = { {0} };

/////////////////
/// FUNCTIONS ///
/////////////////

void initCMDS() {
	cmdsDisp.begin();

	cmdsDisp.setFont(cmdsFont);
	cmdsDisp.setFontPosTop();
	cmdsDisp.firstPage();
	do {
		cmdsDisp.drawStr(0, 0, "CMDS - READY!");
	} while (cmdsDisp.nextPage());
}

void readCMDS() {
	COM.print("M");
	for (short i = 0; i < 2; i++) {
		COM.print(i);
		commsCheck(COM.readBytes(CMDS[i], 24));
	}
}

void drawCMDS() {
	cmdsDisp.firstPage();
	do {
		for (unsigned int i = 0; i < 2; i++) {
			cmdsDisp.drawStr(CMDS_H_CONST, i * CMDS_CHAR_H + CMDS_V_CONST, CMDS[i]);
		}
	} while (cmdsDisp.nextPage());
}

