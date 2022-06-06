using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Alex.PInvoke
{

    internal class Dxva2
    {
        [DllImport("dxva2.dll")]
        public static extern bool GetMonitorBrightness(IntPtr hMonitor, ref IntPtr pwdMinimumBrightness, ref IntPtr pwdCurrentBrightness, ref IntPtr pwdMaximumBrightness);

        public static bool GetMonitorBrightness(IntPtr hMonitor, out int brightness)
        {
            IntPtr minBrightness = IntPtr.Zero;
            IntPtr maxBrightness = IntPtr.Zero;
            IntPtr curBrightness = IntPtr.Zero;

            if (GetMonitorBrightness(hMonitor, ref minBrightness, ref curBrightness, ref maxBrightness))
            {
                brightness = curBrightness.ToInt32();
                return true;
            }
            else
            {
                brightness = 0;
                return false;
            }

        }

        [DllImport("dxva2.dll")]
        public static extern bool SetMonitorBrightness(IntPtr hMonitor, int dwNewBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        public static int GetPhysicalMonitorCount(IntPtr hMonitor)
        {
            uint count = 0;
            GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref count);

            return (int)count;
        }


        public static void TestMonitors()
        {
            List<(int Handle, string Descr)> monitors = new();

            User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, new(EnumMonitorsDlg), IntPtr.Zero);

            bool EnumMonitorsDlg(IntPtr hMonitor, IntPtr hdcMonitor, ref User32.Rect lprcMonitor, IntPtr dwData)
            {
                monitors.Add((hMonitor.ToInt32(), $"{hMonitor}, {hdcMonitor}, {lprcMonitor},{dwData}"));


                uint monCount = 0;
                // Get the number of physical monitors.
                if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref monCount))
                {
                    var mons = new PHYSICAL_MONITOR[monCount];

                    if (GetPhysicalMonitorsFromHMONITOR(hMonitor, (uint)mons.Length, mons))
                    {
                        foreach (var physMon in mons)
                        {
                            IntPtr brightnessMin = IntPtr.Zero;
                            IntPtr brightnessCur = IntPtr.Zero;
                            IntPtr brightnessMax = IntPtr.Zero;

                            var bri = GetMonitorBrightness(physMon.hPhysicalMonitor, ref brightnessMin, ref brightnessCur, ref brightnessMax);
                        }
                    }
                }

                return true;
            }
        }

        internal static List<MonitorInfo> GetMonitorsInfo()
        {
            List<MonitorInfo> result = new();

            User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, new(EnumMonitorsDlg), IntPtr.Zero);

            bool EnumMonitorsDlg(IntPtr hMonitor, IntPtr hdcMonitor, ref User32.Rect lprcMonitor, IntPtr dwData)
            {
                MonitorInfo mi;
                result.Add(mi = new()
                {
                    Handle = hMonitor,
                    Rect = lprcMonitor,
                });

                uint monCount = 0;
                // Get the number of physical monitors.
                if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref monCount))
                {
                    var mons = new PHYSICAL_MONITOR[monCount];

                    if (GetPhysicalMonitorsFromHMONITOR(hMonitor, (uint)mons.Length, mons))
                    {

                        PhysicalMonitorInfo pmi;
                        foreach (var physMon in mons)
                        {
                            mi.PhysicalMonitors.Add(pmi = new()
                            {
                                Handle = physMon.hPhysicalMonitor,
                                Description = physMon.szPhysicalMonitorDescription,
                            });

                            IntPtr brightnessMin = IntPtr.Zero;
                            IntPtr brightnessCur = IntPtr.Zero;
                            IntPtr brightnessMax = IntPtr.Zero;

                            if (GetMonitorBrightness(physMon.hPhysicalMonitor, ref brightnessMin, ref brightnessCur, ref brightnessMax))
                            {

                                pmi.MinimalBrightness = brightnessMin.ToInt32();
                                pmi.MaximalBrightness = brightnessMax.ToInt32();
                                pmi.InitialBrightness = brightnessCur.ToInt32();
                            }
                        }
                    }
                }

                return true;
            }

            return result;
        }

        public class MonitorInfo
        {
            public IntPtr Handle { get; set; }
            public User32.Rect Rect { get; internal set; }
            public List<PhysicalMonitorInfo> PhysicalMonitors { get; } = new();
        }

        public class PhysicalMonitorInfo
        {
            public IntPtr Handle { get; set; }
            public string? Description { get; set; }

            public int MinimalBrightness { get; internal set; }
            public int MaximalBrightness { get; internal set; }
            public int InitialBrightness { get; internal set; }

            internal void SetBrightness(int v) => SetMonitorBrightness(Handle, v);

            internal int GetBrightness() => GetMonitorBrightness(Handle, out var brightness) ? brightness : -1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct PHYSICAL_MONITOR
    {
        public IntPtr hPhysicalMonitor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }


    internal class User32
    {
        //ChangeDisplaySettingsEX
        [DllImport("user32.dll")]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE__C_Position_Orientation_FixedOutput_124 lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, ENUM_IMODENUM iModeNum, ref DEVMODE__C_Position_Orientation_FixedOutput_124 lpDevMode);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

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
    public struct DEVMODE__D_124
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
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;

        public int dmDisplayFlags__OR__dmNup;

        public int dmDisplayFrequency;
        //public int dmICMMethod;
        //public int dmICMIntent;
        //public int dmMediaType;
        //public int dmDitherType;
        //public int dmReserved1;
        //public int dmReserved2;
        //public int dmPanningWidth;
        //public int dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE__A_FirstStruct
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
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;

        public int dmDisplayFlags__OR__dmNup;

        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE__B_Position
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;

        public POINTL dmPosition;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;

        public int dmDisplayFlags__OR__dmNup;

        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE__B_Position_124
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;

        public POINTL dmPosition;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;

        public int dmDisplayFlags__OR__dmNup;

        public int dmDisplayFrequency;
        //public int dmICMMethod;
        //public int dmICMIntent;
        //public int dmMediaType;
        //public int dmDitherType;
        //public int dmReserved1;
        //public int dmReserved2;
        //public int dmPanningWidth;
        //public int dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE__C_Position_Orientation_FixedOutput
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;

        public POINTL dmPosition;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;

        public int dmDisplayFlags__OR__dmNup;

        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE__C_Position_Orientation_FixedOutput_124
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;

        public POINTL dmPosition;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;

        public int dmDisplayFlags__OR__dmNup;

        public int dmDisplayFrequency;
        //public int dmICMMethod;
        //public int dmICMIntent;
        //public int dmMediaType;
        //public int dmDitherType;
        //public int dmReserved1;
        //public int dmReserved2;
        //public int dmPanningWidth;
        //public int dmPanningHeight;
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
