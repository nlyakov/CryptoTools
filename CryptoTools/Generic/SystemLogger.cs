using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CryptoTools.Generic
{
    public class SystemLogger : IDisposable
    {
        private const uint BlockSize = 128 * 1024; // 500KB

        private GCHandle bhandle;
        private IntPtr fhandle;
        private readonly IntPtr ptrBuffer;

        private readonly byte[] tmpBuffer;
        private int BufferIdx;
        public int BytesWritten;


        public SystemLogger()
        {
            tmpBuffer = new byte[BlockSize];
            BufferIdx = 0;
            BytesWritten = 0;
            bhandle = GCHandle.Alloc(tmpBuffer, GCHandleType.Pinned);
            ptrBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(tmpBuffer, 0);
            fhandle = IntPtr.Zero;
        }

        public void Dispose()
        {
            Close();
            UnpinBuffer();
            GC.SuppressFinalize(this);
        }

        ~SystemLogger()
        {
            Close();
            UnpinBuffer();
        }

        public void UnpinBuffer()
        {
            if (bhandle.IsAllocated)
                bhandle.Free();
        }

        public static string NewFileVolume(string FolderName, string FileName)
        {
            string fpath = Settings.LogPath + FolderName + "\\";
            Directory.CreateDirectory(fpath);

            fpath += FileName;
            int i = 1;
            while (File.Exists(string.Format(fpath, i.ToString().PadLeft(4, '0'))))
                i++;
            return string.Format(fpath, i.ToString().PadLeft(4, '0'));
        }

        public static string LogFileHeader()
        {
            return
                new string('-', 70) + Environment.NewLine +
                DateTime.Now.ToString("yyyy-MM-dd---HH-mm-ss") + ": Log started" + Environment.NewLine +
                new string('-', 70) + Environment.NewLine;
        }

        public void BufferAdd(byte[] data, int datalength)
        {
            if (BufferIdx + datalength > tmpBuffer.Length)
                Write(BufferIdx);
            Buffer.BlockCopy(data, 0, tmpBuffer, BufferIdx, datalength);

            BufferIdx += datalength;
        }

        public void BufferAdd(string data)
        {
            if (BufferIdx + data.Length > tmpBuffer.Length)
                Write(BufferIdx);
            Encoding.ASCII.GetBytes(data, 0, data.Length, tmpBuffer, BufferIdx);
            BufferIdx += data.Length;
        }

        public void OpenForWriting(string filename)
        {
            Close();
            BufferIdx = 0;
            BytesWritten = 0;
            fhandle = Kernel32.CreateFile(filename, Kernel32.GENERIC_WRITE, Kernel32.FILE_SHARE_READ, 0, Kernel32.CREATE_ALWAYS, 0, 0);
            if (fhandle == IntPtr.Zero)
            {
                Win32Exception WE = new Win32Exception();
                ApplicationException AE = new ApplicationException("WinFileIO:OpenForWriting - Could not open file - " + WE.Message);
                throw AE;
            }
        }

        void Write(int BytesToWrite)
        {
            int NumberOfBytesWritten = 0;
            if (!Kernel32.WriteFile(fhandle, ptrBuffer, BytesToWrite, ref NumberOfBytesWritten, 0))
            {
                Win32Exception WE = new Win32Exception();
                ApplicationException AE = new ApplicationException("WinFileIO:Write - Error occurred writing a file. - " +
                    WE.Message);
                throw AE;
            }
            BufferIdx = 0;
            BytesWritten += NumberOfBytesWritten;
        }

        public bool Close()
        {
            if (BufferIdx > 0)
                Write(BufferIdx);

            bool Success = true;
            if (fhandle != IntPtr.Zero)
            {
                Success = Kernel32.CloseHandle(fhandle);
                fhandle = IntPtr.Zero;
            }
            return Success;
        }
    }
}