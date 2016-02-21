using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RussellGroup.Pims.DataAccess
{
    public static class MimeHelper
    {
        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        static extern int FindMimeFromData(
            IntPtr pBC,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)] byte[] pBuffer,
            int cbSize,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
            int dwMimeFlags,
            out IntPtr ppwzMimeOut,
            int dwReserved);

        public static string GetMimeType(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("The file was not found.", fileName);
            }

            var maximumContentLength = (int)new FileInfo(fileName).Length;
            maximumContentLength = maximumContentLength > 4096 ? 4096 : maximumContentLength;

            IntPtr mimeTypeOut;
            var data = File.ReadAllBytes(fileName);

            var result = FindMimeFromData(IntPtr.Zero, fileName, data, maximumContentLength, null, 0, out mimeTypeOut, 0);

            if (result != 0)
            {
                throw Marshal.GetExceptionForHR(result);
            }

            var mimeType = Marshal.PtrToStringUni(mimeTypeOut);
            Marshal.FreeCoTaskMem(mimeTypeOut);

            return mimeType;
        }

        public static string GetMimeType(byte[] data)
        {
            var maximum = data.Length;
            maximum = maximum > 4096 ? 4096 : maximum;

            IntPtr mimeTypeOut;

            var result = FindMimeFromData(IntPtr.Zero, null, data, maximum, null, 0, out mimeTypeOut, 0);

            if (result != 0)
            {
                throw Marshal.GetExceptionForHR(result);
            }

            var mimeType = Marshal.PtrToStringUni(mimeTypeOut);
            Marshal.FreeCoTaskMem(mimeTypeOut);

            return mimeType;
        }
    }
}