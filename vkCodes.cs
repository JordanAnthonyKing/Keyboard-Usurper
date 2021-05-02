using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyboard_Usurper
{
	// If anyone tells me an enum is provided by CsWin32 I'm gonna flip
	public enum vkCode : uint
	{
		VK_LBUTTON = 0x01,
		VK_RBUTTON = 0x02,
		VK_CANCEL  = 0x03,
		VK_MBUTTON = 0x04,
		VK_XBUTTON1 = 0x05,
		VK_XBUTTON2 = 0x06,
		// UNDEFINED = 0x07,
		VK_BACK = 0x08,
		VK_TAB = 0x09,
		// VK_RESERVED = 0x0A,
		// VK_RESERVED = 0x0B,
		VK_CLEAR = 0x0C,
		VK_RETURN = 0x0D,
		// VK_UNDEFINED = 0x0E,
		// VK_UNDEFINED = 0x0F,
		VK_SHIFT = 0x10,
		VK_CONTROL = 0x11,
		VK_MENU = 0x12,
		VK_PAUSE = 0x13,
		VK_CAPITAL = 0x14,
		VK_KANA = 0x15,
		VK_HANGUEL = 0x15,
		VK_HANGUL = 0x15,
		VK_IME_ON = 0x16,
		VK_JUNJA = 0x17,
		VK_FINAL = 0x18,
		VK_HANJA = 0x19,
		VK_KANJI = 0x19,
		VK_IME_OFF = 0x1A,
		VK_ESCAPE = 0x1B,
		VK_CONVERT = 0x1C,
		VK_NONCONVERT = 0x1D,
		VK_ACCEPT = 0x1E,
		VK_MODECHANGE = 0x1F,
		VK_SPACE = 0x20,
		VK_PRIOR = 0x21,
		VK_NEXT = 0x22,
		VK_END = 0x23,
		VK_HOME = 0x24,
		VK_LEFT = 0x25,
		VK_UP = 0x26,
		VK_RIGHT = 0x27,
		VK_DOWN = 0x28,
		VK_SELECT = 0x29,
		VK_PRINT = 0x2A,
		VK_EXECUTE = 0x2B,
		VK_SNAPSHOT = 0x2C,
		VK_INSERT = 0x2D,
		VK_DELETE = 0x2E,
		VK_HELP = 0x2F,
		VK_0 = 0x30,
		VK_1 = 0x31,
		VK_2 = 0x32,
		VK_3 = 0x33,
		VK_4 = 0x34,
		VK_5 = 0x35,
		VK_6 = 0x36,
		VK_7 = 0x37,
		VK_8 = 0x38,
		VK_9 = 0x39,
 		// VK_UNDEFINED = 0x3A,
		// VK_UNDEFINED = 0x3B,
		// VK_UNDEFINED = 0x3C,
		// VK_UNDEFINED = 0x3D,
		// VK_UNDEFINED = 0x3E,
		// VK_UNDEFINED = 0x3F,
		// VK_UNDEFINED = 0x40,
		VK_A = 0x41,
		VK_B = 0x42,
		VK_C = 0x43,
		VK_D = 0x44,
		VK_E = 0x45,
		VK_F = 0x46,
		VK_G = 0x47,
		VK_H = 0x48,
		VK_I = 0x49,
		VK_J = 0x4A,
		VK_K = 0x4B,
		VK_L = 0x4C,
		VK_M = 0x4D,
		VK_N = 0x4E,
		VK_O = 0x4F,
		VK_P = 0x50,
		VK_Q = 0x51,
		VK_R = 0x52,
		VK_S = 0x53,
		VK_T = 0x54,
		VK_U = 0x55,
		VK_V = 0x56,
		VK_W = 0x57,
		VK_X = 0x58,
		VK_Y = 0x59,
		VK_Z = 0x5A,
		VK_LWIN = 0x5B,
		VK_RWIN = 0x5C,
		VK_APPS = 0x5D,
		// VK_RESERVED = 0x5E,
		VK_SLEEP = 0x5F,
		VK_NUMPAD0 = 0x60,
		VK_NUMPAD1 = 0x61,
		VK_NUMPAD2 = 0x62,
		VK_NUMPAD3 = 0x63,
		VK_NUMPAD4 = 0x64,
		VK_NUMPAD5 = 0x65,
		VK_NUMPAD6 = 0x66,
		VK_NUMPAD7 = 0x67,
		VK_NUMPAD8 = 0x68,
		VK_NUMPAD9 = 0x69,
		VK_MULTIPLY = 0x6A,
		VK_ADD = 0x6B,
		VK_SEPARATOR = 0x6C,
		VK_SUBTRACT = 0x6D,
		VK_DECIMAL = 0x6E,
		VK_DIVIDE= 0x6F,
		VK_F1 = 0x70,
		VK_F2 = 0x71,
		VK_F3 = 0x72,
		VK_F4 = 0x73,
		VK_F5 = 0x74,
		VK_F6 = 0x75,
		VK_F7 = 0x76,
		VK_F8 = 0x77,
		VK_F9 = 0x78,
		VK_F10 = 0x79,
		VK_F11 = 0x7A,
		VK_F12 = 0x7B,
		VK_F13 = 0x7C,
		VK_F14 = 0x7D,
		VK_F15 = 0x7E,
		VK_F16 = 0x7F,
		VK_F17 = 0x80,
		VK_F18 = 0x81,
		VK_F19 = 0x82,
		VK_F20 = 0x83,
		VK_F21 = 0x84,
		VK_F22 = 0x85,
		VK_F23 = 0x86,
		VK_F24 = 0x87,
		// VK_UNASSIGNED = 0x88,
		// VK_UNASSIGNED = 0x89,
		// VK_UNASSIGNED = 0x8A,
		// VK_UNASSIGNED = 0x8B,
		// VK_UNASSIGNED = 0x8C,
		// VK_UNASSIGNED = 0x8D,
		// VK_UNASSIGNED = 0x8E,
		// VK_UNASSIGNED = 0x8F,
		VK_NUMLOCK = 0x90,
		VK_SCROLL = 0x91,
 		// VK_OEM_SPECIFIC = 0x92,
		// VK_OEM_SPECIFIC = 0x93,
		// VK_OEM_SPECIFIC = 0x94,
		// VK_OEM_SPECIFIC = 0x95,
		// VK_OEM_SPECIFIC = 0x96,
		// VK_UNASSIGNED = 0x97,
		// VK_UNASSIGNED = 0x98,
		// VK_UNASSIGNED = 0x99,
		// VK_UNASSIGNED = 0x9A,
		// VK_UNASSIGNED = 0x9B,
		// VK_UNASSIGNED = 0x9C,
		// VK_UNASSIGNED = 0x9D,
		// VK_UNASSIGNED = 0x9E,
		// VK_UNASSIGNED = 0x9F,
		VK_LSHIFT = 0xA0,
		VK_RSHIFT = 0xA1,
		VK_LCONTROL = 0xA2,
		VK_RCONTROL = 0xA3,
		VK_LMENU = 0xA4,
		VK_RMENU = 0xA5,
		VK_BROWSER_BACK = 0xA6,
		VK_BROWSER_FORWARD = 0xA7,
		VK_BROWSER_REFRESH = 0xA8,
		VK_BROWSER_STOP = 0xA9,
		VK_BROWSER_SEARCH = 0xAA,
		VK_BROWSER_FAVOURITES = 0xAB,
		VK_BROWSER_HOME = 0xAC,
		VK_VOLUME_MUTE = 0xAD,
		VK_VOLUME_DOWN = 0xAE,
		VK_VOLUME_UP = 0xAF,
		VK_MEDIA_NEXT_TRACK = 0xB0,
		VK_MEDIA_PREV_TRACK = 0xB1,
		VK_MEDIA_STOP = 0xB2,
		VK_MEDIA_PLAY_PAUSE = 0xB3,
		VK_LAUNCH_MAIL = 0xB4,
		VK_LAUNCH_MEDIA_SELECT = 0xB5,
		VK_LAUNCH_APP_1 = 0xB6,
		VK_LAUNCH_APP_2 = 0xB7,
		// VK_RESERVED = 0xB8,
		// VK_RESERVED = 0xB9,
		VK_OEM_1 = 0xBA,
		VK_OEM_PLUS = 0xBB,
		VK_OEM_COMMA = 0xBC,
		VK_MINUS = 0xBD,
		VK_PERIOD = 0xBE,
		VK_OEM_2 = 0xBF,
		VK_OEM_3 = 0xC0,
		// VK_RESERVED = 0xC1,
		// VK_RESERVED = 0xC2,
		// VK_RESERVED = 0xC3,
		// VK_RESERVED = 0xC4,
		// VK_RESERVED = 0xC5,
		// VK_RESERVED = 0xC6,
		// VK_RESERVED = 0xC7,
		// VK_RESERVED = 0xC8,
		// VK_RESERVED = 0xC9,
		// VK_RESERVED = 0xCA,
		// VK_RESERVED = 0xCB,
		// VK_RESERVED = 0xCC,
		// VK_RESERVED = 0xCD,
		// VK_RESERVED = 0xCE,
		// VK_RESERVED = 0xCF,
		// VK_RESERVED = 0xD0,
		// VK_RESERVED = 0xD1,
		// VK_RESERVED = 0xD2,
		// VK_RESERVED = 0xD3,
		// VK_RESERVED = 0xD4,
		// VK_RESERVED = 0xD5,
		// VK_RESERVED = 0xD6,
		// VK_RESERVED = 0xD7,
		// VK_UNASSIGNED = 0xD8,
		// VK_UNASSIGNED = 0xD9,
		// VK_UNASSIGNED = 0xDA,
		VK_OEM_4 = 0xDB,
		VK_OEM_5 = 0xDC,
		VK_OEM_6 = 0xDD,
		VK_OEM_7 = 0xDE,
		VK_OEM_8 = 0xDF,
		// VK_RESERVED = 0xE0,
		// VK_OEM_SPECIFIC = 0xE1,
		VK_OEM_102 = 0xE2,
		// VK_OEM_SPECIFIC = 0xE3,
		// VK_OEM_SPECIFIC = 0xE4,
		VK_PROCESS_KEY = 0xE5,
		// VK_OEM_SPECIFIC = 0xE6,
		VK_PACKET = 0xE7,
		// VK_UNASSIGNED = 0xE8,
		// VK_OEM_SPECIFIC = 0xE9,
		// VK_OEM_SPECIFIC = 0xEA,
		// VK_OEM_SPECIFIC = 0xEB,
		// VK_OEM_SPECIFIC = 0xEC,
		// VK_OEM_SPECIFIC = 0xED,
		// VK_OEM_SPECIFIC = 0xEE,
		// VK_OEM_SPECIFIC = 0xEF,
		// VK_OEM_SPECIFIC = 0xF0,
		// VK_OEM_SPECIFIC = 0xF1,
		// VK_OEM_SPECIFIC = 0xF2,
		// VK_OEM_SPECIFIC = 0xF3,
		// VK_OEM_SPECIFIC = 0xF4,
		// VK_OEM_SPECIFIC = 0xF5,
		VK_ATTN = 0xF6,
		VK_CRSEL = 0xF7,
		VK_EXSEL = 0xF8,
		VK_EREOF = 0xF9,
		VK_PLAY = 0xFA,
		VK_ZOOM = 0xFB,
		VK_NONAME = 0xFC,
		VK_PA1 = 0xFD,
		VK_OWM_CLEAR = 0xFE,
		// This code is extra as there is no singular Win keycode
		VK_WIN = 0xFF
	}

	public static class StringToCode
	{
		public static vkCode ConvertTo(string simpleName)
		{
			switch (simpleName)
			{
				case "LMB" : return vkCode.VK_LBUTTON;
				case "RMB" : return vkCode.VK_RBUTTON;
				case "CANCEL" : return vkCode.VK_CANCEL;
				case "MMB" : return vkCode.VK_MBUTTON;
				case "XB1" : return vkCode.VK_XBUTTON1;
				case "XB2" : return vkCode.VK_XBUTTON2;
				case "BS" : return vkCode.VK_BACK;
				case "TAB" : return vkCode.VK_TAB;
				case "CLEAR" : return vkCode.VK_CLEAR;
				case "ENTER" : return vkCode.VK_RETURN;
				case "RETURN" : return vkCode.VK_RETURN;
				case "S" : return vkCode.VK_SHIFT;
				case "C" : return vkCode.VK_CONTROL;
				case "A" : return vkCode.VK_MENU;
				case "PAUSE" : return vkCode.VK_PAUSE;
				case "CAPS" : return vkCode.VK_CAPITAL;
				case "KANA" : return vkCode.VK_KANA;
				case "IME_ON" : return vkCode.VK_IME_ON;
				case "JUNJA" : return vkCode.VK_JUNJA;
				case "FINAL" : return vkCode.VK_FINAL;
				case "HANJA" : return vkCode.VK_HANJA;
				case "IME_OFF" : return vkCode.VK_IME_OFF;
				case "ESC" : return vkCode.VK_ESCAPE;
				case "CONVERT" : return vkCode.VK_CONVERT;
				case "NONCONVERT" : return vkCode.VK_NONCONVERT;
				case "ACCEPT" : return vkCode.VK_ACCEPT;
				case "MODECHANGE" : return vkCode.VK_MODECHANGE;
				case "SPC" : return vkCode.VK_SPACE;
				case "PG_UP" : return vkCode.VK_PRIOR;
				case "PG_DN" : return vkCode.VK_NEXT;
				case "END" : return vkCode.VK_END;
				case "HOME" : return vkCode.VK_HOME;
				case "LEFT" : return vkCode.VK_LEFT;
				case "UP" : return vkCode.VK_UP;
				case "RIGHT" : return vkCode.VK_RIGHT;
				case "DOWN" : return vkCode.VK_DOWN;
				case "SELECT" : return vkCode.VK_SELECT;
				case "PRINT" : return vkCode.VK_PRINT;
				case "EXECUTE" : return vkCode.VK_EXECUTE;
				case "PRT_SCR" : return vkCode.VK_SNAPSHOT;
				case "INS" : return vkCode.VK_INSERT;
				case "DEL" : return vkCode.VK_DELETE;
				case "HELP" : return vkCode.VK_HELP;
				case "0" : return vkCode.VK_0;
				case "1" : return vkCode.VK_1;
				case "2" : return vkCode.VK_2;
				case "3" : return vkCode.VK_3;
				case "4" : return vkCode.VK_4;
				case "5" : return vkCode.VK_5;
				case "6" : return vkCode.VK_6;
				case "7" : return vkCode.VK_7;
				case "8" : return vkCode.VK_8;
				case "9" : return vkCode.VK_9;
				case "a" : return vkCode.VK_A;
				case "b" : return vkCode.VK_B;
				case "c" : return vkCode.VK_C;
				case "d" : return vkCode.VK_D;
				case "e" : return vkCode.VK_E;
				case "f" : return vkCode.VK_F;
				case "g" : return vkCode.VK_G;
				case "h" : return vkCode.VK_H;
				case "i" : return vkCode.VK_I;
				case "j" : return vkCode.VK_J;
				case "k" : return vkCode.VK_K;
				case "l" : return vkCode.VK_L;
				case "m" : return vkCode.VK_M;
				case "n" : return vkCode.VK_N;
				case "o" : return vkCode.VK_O;
				case "p" : return vkCode.VK_P;
				case "q" : return vkCode.VK_Q;
				case "r" : return vkCode.VK_R;
				case "s" : return vkCode.VK_S;
				case "t" : return vkCode.VK_T;
				case "u" : return vkCode.VK_U;
				case "v" : return vkCode.VK_V;
				case "w" : return vkCode.VK_W;
				case "x" : return vkCode.VK_X;
				case "y" : return vkCode.VK_Y;
				case "z" : return vkCode.VK_Z;
				case "LW" : return vkCode.VK_LWIN;
				case "RW" : return vkCode.VK_RWIN;
				case "APPS" : return vkCode.VK_APPS;
				case "SLEEP" : return vkCode.VK_SLEEP;
				case "num_0" : return vkCode.VK_NUMPAD0;
				case "num_1" : return vkCode.VK_NUMPAD1;
				case "num_2" : return vkCode.VK_NUMPAD2;
				case "num_3" : return vkCode.VK_NUMPAD3;
				case "num_4" : return vkCode.VK_NUMPAD4;
				case "num_5" : return vkCode.VK_NUMPAD5;
				case "num_6" : return vkCode.VK_NUMPAD6;
				case "num_7" : return vkCode.VK_NUMPAD7;
				case "num_8" : return vkCode.VK_NUMPAD8;
				case "num_9" : return vkCode.VK_NUMPAD9;
				case "MUL" : return vkCode.VK_MULTIPLY;
				case "ADD" : return vkCode.VK_ADD;
				case "SEPARATOR" : return vkCode.VK_SEPARATOR;
				case "SUB" : return vkCode.VK_SUBTRACT;
				case "DEC" : return vkCode.VK_DECIMAL;
				case "DIV" : return vkCode.VK_DIVIDE;
				case "F1" : return vkCode.VK_F1;
				case "F2" : return vkCode.VK_F2;
				case "F3" : return vkCode.VK_F3;
				case "F4" : return vkCode.VK_F4;
				case "F5" : return vkCode.VK_F5;
				case "F6" : return vkCode.VK_F6;
				case "F7" : return vkCode.VK_F7;
				case "F8" : return vkCode.VK_F8;
				case "F9" : return vkCode.VK_F9;
				case "F10" : return vkCode.VK_F10;
				case "F11" : return vkCode.VK_F11;
				case "F12" : return vkCode.VK_F12;
				case "F13" : return vkCode.VK_F13;
				case "F14" : return vkCode.VK_F14;
				case "F15" : return vkCode.VK_F15;
				case "F16" : return vkCode.VK_F16;
				case "F17" : return vkCode.VK_F17;
				case "F18" : return vkCode.VK_F18;
				case "F19" : return vkCode.VK_F19;
				case "F20" : return vkCode.VK_F20;
				case "F21" : return vkCode.VK_F21;
				case "F22" : return vkCode.VK_F22;
				case "F23" : return vkCode.VK_F23;
				case "F24" : return vkCode.VK_F24;
				case "NUMLOCK" : return vkCode.VK_NUMLOCK;
				case "SCRLOCK" : return vkCode.VK_SCROLL;
				case "LS" : return vkCode.VK_LSHIFT;
				case "RS" : return vkCode.VK_RSHIFT;
				case "LC" : return vkCode.VK_LCONTROL;
				case "RC" : return vkCode.VK_RCONTROL;
				case "LA" : return vkCode.VK_LMENU;
				case "LM" : return vkCode.VK_LMENU;
				case "RA" : return vkCode.VK_RMENU;
				case "RM" : return vkCode.VK_RMENU;
				case "BACK" : return vkCode.VK_BROWSER_BACK;
				case "FORW" : return vkCode.VK_BROWSER_FORWARD;
				case "REFRESH" : return vkCode.VK_BROWSER_REFRESH;
				case "STOP" : return vkCode.VK_BROWSER_STOP;
				case "SEARCH" : return vkCode.VK_BROWSER_SEARCH;
				case "FAV" : return vkCode.VK_BROWSER_FAVOURITES;
				case "HOME_PG" : return vkCode.VK_BROWSER_HOME;
				case "MUTE" : return vkCode.VK_VOLUME_MUTE;
				case "VOL_DN" : return vkCode.VK_VOLUME_DOWN;
				case "VOL_UP" : return vkCode.VK_VOLUME_UP;
				case "M_NEXT" : return vkCode.VK_MEDIA_NEXT_TRACK;
				case "M_PREV" : return vkCode.VK_MEDIA_PREV_TRACK;
				case "M_STOP" : return vkCode.VK_MEDIA_STOP;
				case "M_PLAY_PAUSE" : return vkCode.VK_MEDIA_PLAY_PAUSE;
				case "MAIL" : return vkCode.VK_LAUNCH_MAIL;
				case "MEDIA" : return vkCode.VK_LAUNCH_MEDIA_SELECT;
				case "APP1" : return vkCode.VK_LAUNCH_APP_1;
				case "APP2" : return vkCode.VK_LAUNCH_APP_2;
				case ";" : return vkCode.VK_OEM_1;
				case "+" : return vkCode.VK_OEM_PLUS;
				case "," : return vkCode.VK_OEM_COMMA;
				case "-" : return vkCode.VK_MINUS;
				case "." : return vkCode.VK_PERIOD;
				case "/" : return vkCode.VK_OEM_2;
				case "~" : return vkCode.VK_OEM_3;
				case "[" : return vkCode.VK_OEM_4;
				case "\\" : return vkCode.VK_OEM_5;
				case "]" : return vkCode.VK_OEM_6;
				case "'" : return vkCode.VK_OEM_7;
				// TODO: Figure out what this is on a UK keyboard
				case "TODO" : return vkCode.VK_OEM_8;
				case "TODO2" : return vkCode.VK_OEM_102;
				case "PROCESS" : return vkCode.VK_PROCESS_KEY;
				// Not allowed
				// case "" : return vkCode.VK_PACKET;
				case "ATTN" : return vkCode.VK_ATTN;
				case "CRSEL" : return vkCode.VK_CRSEL;
				case "EXSEL" : return vkCode.VK_EXSEL;
				case "EREOF" : return vkCode.VK_EREOF;
				case "PLAY" : return vkCode.VK_PLAY;
				case "ZOOM" : return vkCode.VK_ZOOM;
				// Reserved
				// case "" : return vkCode.VK_NONAME;
				case "PA1" : return vkCode.VK_PA1;
				case "OEM_CLEAR" : return vkCode.VK_OWM_CLEAR;
				case "W" : return vkCode.VK_WIN;
			}

			return vkCode.VK_0;
		}
	}

}
