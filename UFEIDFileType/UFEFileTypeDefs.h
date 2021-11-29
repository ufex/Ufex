
namespace UniversalFileExplorer
{
#define FT_UNKNOWN			"FT_UNKNOWN"

// Image Files  Raster/Vector/3D
#define FT_RAST_WINBMP		"GRPH_WINBMP"
#define FT_RAST_JPEG		"GRPH_JPEG"
#define FT_RAST_GIF			"GRPH_GIF"

#define FT_RAST_PNG			"GRPH_PNG"
#define FT_RAST_TIFF		"GRPH_TIFF"

#define FT_RAST_ICO			"GRPH_WINICON"
#define FT_RAST_CUR			"GRPH_WINCUR"

#define FT_VECT_WMF			"GRPH_WMF"
#define FT_VECT_EMF			"GRPH_EMF"
#define FT_VECT_CGM			"GRPH_CGM"

// Music Files
#define FT_SOUN_MIDI        "AUDI_MIDI"

#define FT_SOUN_OGG			"AUDI_OGG"

#define FT_SOUN_MP3			"AUDI_MP3"
#define FT_SOUN_MPEG4		"AUDI_MP4"




#define FT_SOUN_RIFF		"AUDI_RIFF"
#define FT_SOUN_WAVE		"AUDI_WAVE"
#define FT_SOUN_SF2			"AUDI_SF2"
#define FT_SOUN_DLS			"AUDI_DLS"


// Sheet Music

#define FT_SOUN_PTB			"TEXT_PTAB"
#define FT_SOUN_GTRPRO		"TEXT_GTRPRO"

// Text Based Documents
#define FT_TEXT_ASCII       "TEXT_ASCII"
#define FT_TEXT_UNICODE     "TEXT_UNICODE"

#define FT_TEXT_XML			"TEXT_XML"


#define FT_TEXT_CPP			"TEXT_CPP"

#define FT_TEXT_INI			"TEXT_INI"

// General Documents
#define FT_DOCS_MSCMPD	    "DOCS_MSCMPD"
#define FT_DOCS_ACRO        "DOCS_ACRO"
#define FT_DOCS_MSWORD		"DOCS_MSWORD"

#define FT_TABL_MSXLS       "DOCS_MSXLS"

#define FT_DOCS_WINHLP		"DOCS_WINHELP"
#define FT_DOCS_CHM			"DOCS_CHM"

#define FT_DOCS_MSPPT		"DOCS_MSPPT"


// Archive File Types		0x04__
#define FT_ARCH_ZIP			"ARCH_ZIP"
#define FT_ARCH_RAR			"ARCH_RAR"
#define FT_ARCH_CAB			"ARCH_CAB"


// Database File Formats    0x05__

#define FT_DATA_ACCESS		"DATA_ACCESS"

// Program File Formats		0x06__
#define FT_EXEC_MZ			"EXEC_MZ"
#define FT_EXEC_NE			"EXEC_NE"
#define FT_EXEC_LE			"EXEC_LE"
#define FT_EXEC_PE			"EXEC_PE"

#define FT_EXEC_ELF			"EXEC_ELF"

#define FT_EXEC_SHOCKWAVE	"EXEC_SHOCKWAVE"

#define FT_EXEC_JAVACLASS	"EXEC_JAVACLASS"


//#define FT_EXEC_NES			0x0632
//#define FT_EXEC_GB			0x0633
//#define FT_EXEC_SNES		0x0634
//#define FT_EXEC_SEGAGEN		0x0635

#define FT_EXEC_MSI			"EXEC_MSI"

// Application Specific file formats

#define FT_APPS_UNREALPACK	"APPS_UNREALPACK"

#define FT_PACK_COFF        "PACK_COFF"

#define FT_LINK_WINSHORT	"LINK_WINSHORT"

#define FT_APPS_MESCRIPT	"APPS_MESCRIPT"



/***Old Stuff*************
#define FT_UNKNOWN			0xFFFF

// Image Files  Raster/Vector/3D
#define FT_RAST_WINBMP		0x0001
#define FT_RAST_JPEG		0x0002
#define FT_RAST_GIF			0x0003

#define FT_RAST_PNG			0x0005
#define FT_RAST_TIFF		0x0006

#define FT_RAST_ICO			0x0010
#define FT_RAST_CUR			0x0011

#define FT_VECT_WMF			0x0021
#define FT_VECT_WMFPLACE	0x0022
#define FT_VECT_EMF			0x0023
#define FT_VECT_CGM			0x0024

// Music Files
#define FT_SOUN_MIDI        0x0101

#define FT_SOUN_OGG			0x0113

#define FT_SOUN_MP3			0x0122
#define FT_SOUN_MPEG4		0x0123




#define FT_SOUN_RIFF		0x0130
#define FT_SOUN_WAVE		0x0131
#define FT_SOUN_SF2			0x0132
#define FT_SOUN_DLS			0x0133


// Sheet Music

#define FT_SOUN_PTB			0x0155
#define FT_SOUN_GTRPRO		0x0156

// Text Based Documents
#define FT_TEXT_ASCII       0x0201
#define FT_TEXT_UNICODE     0x0202

#define FT_TEXT_XML			0x0220


#define FT_TEXT_CPP			0x0230

#define FT_TEXT_INI			0x0241

// General Documents
#define FT_DOCS_MSCMPD	    0x0301
#define FT_DOCS_ACRO        0x0302
#define FT_DOCS_MSWORD		0x0304

#define FT_TABL_MSXLS       0x0311

#define FT_DOCS_WINHLP		0x0321
#define FT_DOCS_CHM			0x0322

#define FT_DOCS_MSPPT		0x0340


// Archive File Types		0x04__
#define FT_ARCH_ZIP			0x0401
#define FT_ARCH_RAR			0x0402
#define FT_ARCH_CAB			0x0421


// Database File Formats    0x05__

#define FT_DATA_ACCESS		0x0501

// Program File Formats		0x06__
#define FT_EXEC_MZ			0x0601
#define FT_EXEC_NE			0x0602
#define FT_EXEC_LE			0x0603
#define FT_EXEC_PE			0x0604

#define FT_EXEC_ELF			0x0607

#define FT_EXEC_SHOCKWAVE	0x0610

#define FT_EXEC_JAVACLASS	0x0623


#define FT_EXEC_NES			0x0632
#define FT_EXEC_GB			0x0633
#define FT_EXEC_SNES		0x0634
#define FT_EXEC_SEGAGEN		0x0635

#define FT_EXEC_MSI			0x0652

// Application Specific file formats

#define FT_APPS_UNREALPACK	0x0902

#define FT_PACK_COFF        0x0950

#define FT_LINK_WINSHORT	0x0960

#define FT_APPS_MESCRIPT	0x0973
***********************/


};