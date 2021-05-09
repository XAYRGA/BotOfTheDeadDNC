using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace xayrga.binutils
{

    public enum ProcessMemoryAccess
    {
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        PROCESS_VM_OPERATION = 0x0008,
    }

    public class ProcessMemoryInterface
    {
        int _accessMode = 0x30; // PROCESS_VM_READ |  PROCESS_VM_WRITE 

        Process _process;
        int _processHandle;

        public Process Process { get => _process; }

        public ProcessMemoryInterface(Process proc, int permissions = 0x38)
        {
            _process = proc;
            _accessMode = permissions;
            try { _processHandle = (int)OpenProcess(_accessMode, false, proc.Id); }
            catch (Exception E) { throw new EntryPointNotFoundException($"ProcessMemoryInterface::ctor() Cannot create process handle!\n{E.ToString()}"); }
            if (_processHandle == 0)
                throw new EntryPointNotFoundException($"ProcessMemoryInterface::ctor() Cannot create process handle, _processHandle==NULL");
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress,
          byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);


        public uint getU32(int address)
        {
            byte[] byOut = new byte[4];
            int read = 0;
            ReadProcessMemory(_processHandle, address, byOut, 4, ref read);
            if (read < 4)
                throw new AccessViolationException("ProcessMemoryInterface::getU32() truncated response from memory read call.");
            return BitConverter.ToUInt32(byOut, 0);
        }

        public ushort getU16(int address)
        {
            byte[] byOut = new byte[2];
            int read = 0;
            ReadProcessMemory(_processHandle, address, byOut, 2, ref read);
            if (read < 2)
                throw new AccessViolationException("ProcessMemoryInterface::getU16() truncated response from memory read call.");
            return BitConverter.ToUInt16(byOut, 0);
        }


        public int getI32(int address)
        {
            byte[] byOut = new byte[4];
            int read = 0;
            ReadProcessMemory(_processHandle, address, byOut, 4, ref read);
            if (read < 4)
                throw new AccessViolationException("ProcessMemoryInterface::getI32() truncated response from memory read call.");
            return BitConverter.ToInt32(byOut, 0);
        }

        public float getF32(int address)
        {
            byte[] byOut = new byte[4];
            int read = 0;
            ReadProcessMemory(_processHandle, address, byOut, 4, ref read);
            if (read < 4)
                throw new AccessViolationException("ProcessMemoryInterface::getI32() truncated response from memory read call.");
            return BitConverter.ToSingle(byOut, 0);
        }

        public short getI16(int address)
        {
            byte[] byOut = new byte[2];
            int read = 0;
            ReadProcessMemory(_processHandle, address, byOut, 2, ref read);
            if (read < 2)
                throw new AccessViolationException("ProcessMemoryInterface::getI16() truncated response from memory read call.");
            return BitConverter.ToInt16(byOut, 0);
        }

        public byte getU8(int address)
        {
            byte[] byOut = new byte[1];
            int read = 0;
            ReadProcessMemory(_processHandle, address, byOut, 1, ref read);
            if (read < 1)
                throw new AccessViolationException("ProcessMemoryInterface::getU8() truncated response from memory read call.");
            return byOut[0];
        }






        public uint getPtrI32(int address, byte depth)
        {
            var value = getU32(address);
            if (depth > 0)
            {
                depth--;
                return getPtrI32((int)value, depth);
            }
            return value;
        }

        /*
        public T getData<T>(int address)
        {
            var size = Marshal.SizeOf<T>();
            byte[] byOut = new byte[size];
            int read = 0;
            ReadProcessMemory(_processHandle, address, byOut, size, ref read);
            if (read < size)
                throw new AccessViolationException("ProcessMemoryInterface::getData() truncated response from memory read call.");          
        }
        */

        public string getString(int address, byte terminator = 0x00)
        {
            var baseAddress = address;
            var length = 0;
            while (getU8(address + length) != terminator)
                length++;
            byte[] data = new byte[length];
            int dummy = 0;
            ReadProcessMemory(_processHandle, address, data, length, ref dummy);
            return Encoding.ASCII.GetString(data, 0, length);
        }


        public string getStringL(int address, int len)
        {
            var baseAddress = address;
            byte[] data = new byte[len];
            int dummy = 0;
            ReadProcessMemory(_processHandle, address, data, len, ref dummy);
            return Encoding.ASCII.GetString(data, 0, len);
        }
        public byte setU8(int address, byte data)
        {
            byte[] byOut = new byte[1];
            byOut[0] = data;
            int read = 0; 
            WriteProcessMemory(_processHandle, address, byOut, 1, ref read);
            if (read < 1)
                throw new AccessViolationException("ProcessMemoryInterface::setU8() truncated response from memory read call.");
            return byOut[0];
        }


    }
}