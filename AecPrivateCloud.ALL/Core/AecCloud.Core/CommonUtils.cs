using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace AecCloud.Core
{

    public static class CommonUtils
    {
        


        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int WNetGetUniversalName(
            string lpLocalPath,
            [MarshalAs(UnmanagedType.U4)] int dwInfoLevel,
            IntPtr lpBuffer,
            [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize);

        const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
        //const int REMOTE_NAME_INFO_LEVEL = 0x00000002;

        const int ERROR_MORE_DATA = 234;
        const int NOERROR = 0;


        /// <summary>
        /// 转换网络映射路径到远程路径;直接捕获了异常
        /// (若不是网络映射路径，则原样返回)
        /// </summary>
        /// <param name="localPath">本地映射路径</param>
        /// <returns></returns>
        public static string GetUniversalName(string localPath)
        {
            
            if (localPath.StartsWith(@"\\")) return localPath;
            // The return value.
            string retVal = null;

            // The pointer in memory to the structure.
            IntPtr buffer = IntPtr.Zero;

            // Wrap in a try/catch block for cleanup.
            try
            {
                // First, call WNetGetUniversalName to get the size.
                int size = 0;

                // Make the call.
                // Pass IntPtr.Size because the API doesn't like null, even though
                // size is zero.  We know that IntPtr.Size will be
                // aligned correctly.
                int apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, (IntPtr)IntPtr.Size, ref size);

                // If the return value is not ERROR_MORE_DATA, then
                // raise an exception.
                if (apiRetVal != ERROR_MORE_DATA)
                {
                    // Throw an exception.
                    throw new Exception(apiRetVal.ToString());
                }
                // Allocate the memory.
                buffer = Marshal.AllocCoTaskMem(size);

                // Now make the call.
                apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);

                // If it didn't succeed, then throw.
                if (apiRetVal != NOERROR)
                {
                    // Throw an exception.
                    throw new Exception(apiRetVal.ToString());//Win32Exception(apiRetVal);
                }

                // Now get the string.  It's all in the same buffer, but
                // the pointer is first, so offset the pointer by IntPtr.Size
                // and pass to PtrToStringAnsi.
                retVal = Marshal.PtrToStringAuto(new IntPtr(buffer.ToInt64() + IntPtr.Size), size);
                retVal = retVal.Substring(0, retVal.IndexOf('\0'));
            }
            catch
            {
                retVal = localPath;
            }
            finally
            {
                // Release the buffer.
                Marshal.FreeCoTaskMem(buffer);
            }

            // First, allocate the memory for the structure.

            // That's all folks.
            return retVal;
        }
    }
}
