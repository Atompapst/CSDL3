using System;
using System.Text;
namespace CSDL.Input {
    /// <summary>A managed snapshot of a HID enumeration entry.</summary>
    public sealed record HidDeviceInfo(
        string Path,
        ushort VendorID,
        ushort ProductID,
        string? SerialNumber,
        ushort ReleaseNumber,
        string? Manufacturer,
        string? Product,
        ushort UsagePage,
        ushort Usage,
        int InterfaceNumber,
        HidBusType BusType
    ) {
        internal static HidDeviceInfo FromNative(hid_device_info info) {
            return new HidDeviceInfo(
                ReadUtf8(info.Path), info.VendorID, info.ProductID, ReadWide(info.SerialNumber), info.ReleaseNumber,
                ReadWide(info.ManufacturerString), ReadWide(info.ProductString), info.UsagePage, info.Usage,
                info.InterfaceNumber, info.BusType);
        }

        private static string ReadUtf8(nint value) {
            return value == 0 ? string.Empty : System.Runtime.InteropServices.Marshal.PtrToStringUTF8(value) ?? string.Empty;
        }

        private static unsafe string? ReadWide(nint value) {
            if (value == 0) {
                return null;
            }

            if (OperatingSystem.IsWindows()) {
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(value);
            }

            uint* text = (uint*)value;
            int length = 0;
            while (text[length] != 0) {
                length++;
            }
            return Encoding.UTF32.GetString(new ReadOnlySpan<byte>(text, length * sizeof(uint)));
        }
    }
}
