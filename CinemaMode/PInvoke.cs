using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Alex.PInvoke
{
    internal class User32
    {
        //ChangeDisplaySettingsEX
        [DllImport("user32.dll")]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, ENUM_IMODENUM iModeNum, out DEVMODE lpDevMode);
    }

    public enum ENUM_IMODENUM : int
    {
        NUM_CURRENT_SETTINGS = -1,
        ENUM_REGISTRY_SETTINGS = -2,
    }

    public enum DISP_CHANGE : int
    {
        Successful = 0,
        Restart = 1,
        Failed = -1,
        BadMode = -2,
        NotUpdated = -3,
        BadFlags = -4,
        BadParam = -5,
        BadDualView = -6
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public short dmOrientation;
        public short dmPaperSize;
        public short dmPaperLength;
        public short dmPaperWidth;
        public short dmScale;
        public short dmCopies;
        public short dmDefaultSource;
        public short dmPrintQuality;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmUnusedPadding;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
    }

    //[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    //public struct DEVMODE
    //{
    //    public const int CCHDEVICENAME = 32;
    //    public const int CCHFORMNAME = 32;

    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
    //    [System.Runtime.InteropServices.FieldOffset(0)]
    //    public string dmDeviceName;
    //    [System.Runtime.InteropServices.FieldOffset(32)]
    //    public Int16 dmSpecVersion;
    //    [System.Runtime.InteropServices.FieldOffset(34)]
    //    public Int16 dmDriverVersion;
    //    [System.Runtime.InteropServices.FieldOffset(36)]
    //    public Int16 dmSize;
    //    [System.Runtime.InteropServices.FieldOffset(38)]
    //    public Int16 dmDriverExtra;
    //    [System.Runtime.InteropServices.FieldOffset(40)]
    //    public /*DM*/int dmFields;

    //    [System.Runtime.InteropServices.FieldOffset(44)]
    //    Int16 dmOrientation;
    //    [System.Runtime.InteropServices.FieldOffset(46)]
    //    Int16 dmPaperSize;
    //    [System.Runtime.InteropServices.FieldOffset(48)]
    //    Int16 dmPaperLength;
    //    [System.Runtime.InteropServices.FieldOffset(50)]
    //    Int16 dmPaperWidth;
    //    [System.Runtime.InteropServices.FieldOffset(52)]
    //    Int16 dmScale;
    //    [System.Runtime.InteropServices.FieldOffset(54)]
    //    Int16 dmCopies;
    //    [System.Runtime.InteropServices.FieldOffset(56)]
    //    Int16 dmDefaultSource;
    //    [System.Runtime.InteropServices.FieldOffset(58)]
    //    Int16 dmPrintQuality;

    //    [System.Runtime.InteropServices.FieldOffset(44)]
    //    public POINTL dmPosition;
    //    [System.Runtime.InteropServices.FieldOffset(52)]
    //    public Int32 dmDisplayOrientation;
    //    [System.Runtime.InteropServices.FieldOffset(56)]
    //    public Int32 dmDisplayFixedOutput;

    //    [System.Runtime.InteropServices.FieldOffset(60)]
    //    public short dmColor; // See note below!
    //    [System.Runtime.InteropServices.FieldOffset(62)]
    //    public short dmDuplex; // See note below!
    //    [System.Runtime.InteropServices.FieldOffset(64)]
    //    public short dmYResolution;
    //    [System.Runtime.InteropServices.FieldOffset(66)]
    //    public short dmTTOption;
    //    [System.Runtime.InteropServices.FieldOffset(68)]
    //    public short dmCollate; // See note below!
    //    [System.Runtime.InteropServices.FieldOffset(70)]
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
    //    public string dmFormName;
    //    [System.Runtime.InteropServices.FieldOffset(102)]
    //    public Int16 dmLogPixels;
    //    [System.Runtime.InteropServices.FieldOffset(104)]
    //    public Int32 dmBitsPerPel;
    //    [System.Runtime.InteropServices.FieldOffset(108)]
    //    public Int32 dmPelsWidth;
    //    [System.Runtime.InteropServices.FieldOffset(112)]
    //    public Int32 dmPelsHeight;
    //    [System.Runtime.InteropServices.FieldOffset(116)]
    //    public Int32 dmDisplayFlags;
    //    [System.Runtime.InteropServices.FieldOffset(116)]
    //    public Int32 dmNup;
    //    [System.Runtime.InteropServices.FieldOffset(120)]
    //    public Int32 dmDisplayFrequency;
    //}

    public struct POINTL
    {
        public Int32 x;
        public Int32 y;
    }

    /// <summary>
    /// Selects duplex or double-sided printing for printers capable of duplex printing.
    /// </summary>
    public enum DMDUP : short
    {
        /// <summary>
        /// Unknown setting.
        /// </summary>
        DMDUP_UNKNOWN = 0,

        /// <summary>
        /// Normal (nonduplex) printing.
        /// </summary>
        DMDUP_SIMPLEX = 1,

        /// <summary>
        /// Long-edge binding, that is, the long edge of the page is vertical.
        /// </summary>
        DMDUP_VERTICAL = 2,

        /// <summary>
        /// Short-edge binding, that is, the long edge of the page is horizontal.
        /// </summary>
        DMDUP_HORIZONTAL = 3,
    }

    /// <summary>
    /// Switches between color and monochrome on color printers.
    /// </summary>
    public enum DMCOLOR : short
    {
        DMCOLOR_UNKNOWN = 0,

        DMCOLOR_MONOCHROME = 1,

        DMCOLOR_COLOR = 2
    }

    [Flags()]
    public enum ChangeDisplaySettingsFlags : uint
    {
        CDS_NONE = 0,
        CDS_UPDATEREGISTRY = 0x00000001,
        CDS_TEST = 0x00000002,
        CDS_FULLSCREEN = 0x00000004,
        CDS_GLOBAL = 0x00000008,
        CDS_SET_PRIMARY = 0x00000010,
        CDS_VIDEOPARAMETERS = 0x00000020,
        CDS_ENABLE_UNSAFE_MODES = 0x00000100,
        CDS_DISABLE_UNSAFE_MODES = 0x00000200,
        CDS_RESET = 0x40000000,
        CDS_RESET_EX = 0x20000000,
        CDS_NORESET = 0x10000000
    }

}
