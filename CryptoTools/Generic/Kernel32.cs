using System;
using System.Runtime.InteropServices;

namespace CryptoTools.Generic
{
    public static class Kernel32
    {
        public const uint GENERIC_WRITE = 0x40000000;
        public const int FILE_SHARE_READ = 1;
        public const uint CREATE_ALWAYS = 2;

        // Define the Windows system functions that are called by this class via COM Interop:
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr CreateFile
        (
             string FileName,          // file name
             uint DesiredAccess,       // access mode
             uint ShareMode,           // share mode
             uint SecurityAttributes,  // Security Attributes
             uint CreationDisposition, // how to create
             uint FlagsAndAttributes,  // file attributes
             int hTemplateFile         // handle to template file
        );

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool WriteFile
        (
            IntPtr handle,					   // handle to file
            IntPtr pBuffer,             // data buffer
            int NumberOfBytesToWrite,	 // Number of bytes to write.
            ref int pNumberOfBytesWritten,// Number of bytes that were written..
            int Overlapped					   // Overlapped buffer which is used for async I/O.  Not used here.
        );

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle
        (
             IntPtr hObject     // handle to object
        );

    }
}
