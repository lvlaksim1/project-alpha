using System.Runtime.InteropServices;
using System.Text;

namespace Baraban.Services;

internal static class DpapiStore
{
    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob
    {
        public int cbData;
        public IntPtr pbData;
    }

    [DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool CryptProtectData(
        ref DataBlob pDataIn,
        string? szDataDescr,
        IntPtr pOptionalEntropy,
        IntPtr pvReserved,
        IntPtr pPromptStruct,
        int dwFlags,
        ref DataBlob pDataOut);

    [DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool CryptUnprotectData(
        ref DataBlob pDataIn,
        IntPtr ppszDataDescr,
        IntPtr pOptionalEntropy,
        IntPtr pvReserved,
        IntPtr pPromptStruct,
        int dwFlags,
        ref DataBlob pDataOut);

    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern IntPtr LocalFree(IntPtr hMem);

    public static byte[] Protect(string value) => Transform(Encoding.UTF8.GetBytes(value), true);

    public static string Unprotect(byte[] value) => Encoding.UTF8.GetString(Transform(value, false));

    private static byte[] Transform(byte[] input, bool protect)
    {
        var inputPtr = Marshal.AllocHGlobal(input.Length);
        try
        {
            Marshal.Copy(input, 0, inputPtr, input.Length);
            var inputBlob = new DataBlob { cbData = input.Length, pbData = inputPtr };
            var outputBlob = new DataBlob();
            var ok = protect
                ? CryptProtectData(ref inputBlob, "Baraban session", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref outputBlob)
                : CryptUnprotectData(ref inputBlob, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref outputBlob);

            if (!ok)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                var output = new byte[outputBlob.cbData];
                Marshal.Copy(outputBlob.pbData, output, 0, outputBlob.cbData);
                return output;
            }
            finally
            {
                if (outputBlob.pbData != IntPtr.Zero)
                    LocalFree(outputBlob.pbData);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(inputPtr);
        }
    }
}
