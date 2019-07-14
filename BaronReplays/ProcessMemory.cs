using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace BaronReplays
{
    class ProcessMemory
    {
        [DllImport("kernel32.dll")]
        private static extern UIntPtr OpenProcess(UInt32 dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(UIntPtr hObject);
        [DllImport("kernel32.dll ")]
        static extern bool ReadProcessMemory(UIntPtr hProcess, UInt32 lpBaseAddress, byte[] lpBuffer, UInt32 nSize, out int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        private static extern int VirtualQueryEx(UIntPtr hProcess, UInt32 lpAddress, ref MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [Flags]
        public enum MemoryState : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000,
        }

        [Flags]
        public enum MemoryType : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000,
        }

        [Flags]
        public enum AllocationProtect : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public UInt32 BaseAddress;
            public UInt32 AllocationBase;
            public AllocationProtect AllocationProtect;
            public UInt32 RegionSize;
            public MemoryState State;
            public uint Protect;
            public MemoryType Type;
        }

        private UIntPtr processHandle;
        private List<MEMORY_BASIC_INFORMATION> memorySnapshot;
        private Process process;


        public bool openProcess(Process p)
        {
            const UInt32 PROCESS_ALL_ACCESS = 0x1F0FFF;
            process = p;
            this.processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (this.processHandle == UIntPtr.Zero)
                return false;
            return true;
        }

        public bool openProcess(string pName)
        {
            const UInt32 PROCESS_ALL_ACCESS = 0x1F0FFF;
            Process[] p = Process.GetProcessesByName(pName);
            if (p.Length == 0)
                return false;
            process = p[0];
            this.processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (this.processHandle == UIntPtr.Zero)
                return false;
            return true;
        }

        public bool closeProcess()
        {
            bool isSuccess = CloseHandle(this.processHandle);
            this.processHandle = UIntPtr.Zero;
            return isSuccess;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool QueryFullProcessImageName([In]UIntPtr hProcess, [In]int dwFlags, [Out]StringBuilder lpExeName, ref int lpdwSize);
        public String GetProcessPath()
        {
            int capacity = 1024;
            StringBuilder fileName = new StringBuilder(capacity);
            QueryFullProcessImageName(this.processHandle, 0, fileName, ref capacity);
            String s = null;
            try
            {
                s = fileName.ToString(0, capacity);
            }
            catch(Exception e)
            {
                return null;
            }
            return fileName.ToString(0, capacity);
        }

        public byte[] readMemory(UInt32 address, UInt32 size)
        {
            byte[] bytes = new byte[size];
            int count;
            ReadProcessMemory(this.processHandle, address, bytes, size, out count);
            if (count == 0)
                return null;
            //if(!ReadProcessMemory(this.processHandle, address, bytes, size, out count))
            //    return null;

            //byte[] real = new byte[count];
            //for (int i = 0; i < count; i++)
            //    real[i] = bytes[i];
            return bytes;
        }

        public void recordMemorysInfo(Boolean includingProtectedMemory)
        {
            this.memorySnapshot = new List<MEMORY_BASIC_INFORMATION>();
            UInt32 addrNow = 1;
            while (addrNow != 0)
            {
                MEMORY_BASIC_INFORMATION info = new MEMORY_BASIC_INFORMATION();
                int infoLen = VirtualQueryEx(this.processHandle, addrNow, ref info, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                UInt32 addrLast = addrNow;
                addrNow = info.BaseAddress + info.RegionSize;
                if (addrNow < addrLast)
                    break;
                if (!includingProtectedMemory)
                {
                    if (info.AllocationProtect != AllocationProtect.PAGE_READWRITE)
                        continue;
                }
                if (info.Type != MemoryType.MEM_PRIVATE)
                    continue;
                if (info.State == MemoryState.MEM_COMMIT)
                    this.memorySnapshot.Add(info);
            }
        }


        public int findDataInContent(byte[] target, byte[] content)
        {
            int end = content.Length - target.Length;
            for (int i = 0; i < end; i++)
            {
                int j = 0;
                for (; j < target.Length; j++)
                {
                    if (target[j] != content[i + j])
                        break;
                    if (j == target.Length - 1)
                    {
                        return i;
                    }
                }
                i += j;
            }
            return -1;
        }

        public int findDataBefore(byte[] target, uint address, uint rangeLimit)
        {
            byte[] memContent = this.readMemory(address - rangeLimit, rangeLimit);
            memContent = memContent.Reverse().ToArray();
            target = target.Reverse().ToArray();
            int pos = findDataInContent(target, memContent);
            if (pos > 0)
                pos += target.Length;
            return pos;
        }

        public int findDataAfter(byte[] target, uint address, uint rangeLimit)
        {
            byte[] memContent = this.readMemory(address, rangeLimit);
            int pos = findDataInContent(target, memContent);
            return pos;
        }


        public UInt32[] findInteger(int n)
        {
            List<UInt32> locations = new List<UInt32>();
            foreach (MEMORY_BASIC_INFORMATION info in this.memorySnapshot)
            {
                byte[] mem = this.readMemory(info.BaseAddress, info.RegionSize);
                if (mem == null)
                    continue;
                int end = mem.Length - sizeof(int);
                for (int i = 0; i < end; i++)
                {
                    int num = BitConverter.ToInt32(mem, i);
                    if (num == n)
                        locations.Add(info.BaseAddress + (uint)i);
                }
            }
            return locations.ToArray();
        }

        public UInt32[] findString(string str, Encoding encoding, int max = -1)
        {
            byte[] strAscII = encoding.GetBytes(str);
            List<UInt32> locations = new List<UInt32>();
            foreach (MEMORY_BASIC_INFORMATION info in this.memorySnapshot)
            {
                byte[] mem = this.readMemory(info.BaseAddress, info.RegionSize);
                if (mem == null)
                    continue;
                int pos = findDataInContent(strAscII, mem);
                if (pos > 0)
                {
                    locations.Add(info.BaseAddress + (uint)pos);
                    if (--max == 0)
                        break;
                }
            }
            return locations.ToArray();
        }


        [DllImport("Kernel32")]
        public static extern IntPtr GetProcAddress(int handle, String funcname);

        [DllImport("kernel32.dll")]
        public static extern Int32 GetModuleHandle(String lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(UIntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UInt32 lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(UIntPtr hProcess, IntPtr lpAddress, uint dwSize, MemoryState flAllocationType, AllocationProtect flProtect);


        [DllImport("kernel32")]
        public static extern IntPtr CreateRemoteThread(UIntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId
        );


        public bool injectDll(String path)
        {
            Process p = Process.GetProcessById(process.Id);
            string fullPath = Environment.CurrentDirectory + @"\" + path;
            Logger.Instance.WriteLog("Inject: " + path + " to " + p.ProcessName);

            String moduleName = BaronReplays.Helper.FileHelper.GetFileName(path);

            ProcessModuleCollection processModules = null;
            try
            {
                processModules = p.Modules;

                foreach (ProcessModule pm in processModules)
                {
                    if (pm.ModuleName.CompareTo(moduleName) == 0)
                    {
                        Logger.Instance.WriteLog("Inject failed because module already exist");
                        return false;
                    }
                }
            }
            catch (Exception)
            {

            }

            byte[] dllPathBytes = Encoding.Unicode.GetBytes(fullPath);
            IntPtr remoteMemoryAddress = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)dllPathBytes.Length, MemoryState.MEM_COMMIT, AllocationProtect.PAGE_READWRITE);
            UInt32 bytesWrote;
            WriteProcessMemory(processHandle, remoteMemoryAddress, dllPathBytes, (uint)dllPathBytes.Length, out bytesWrote);
            if (bytesWrote == 0)
                return false;
            IntPtr loadLibraryWAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");
            uint remoteThreadId;
            IntPtr handle = CreateRemoteThread(processHandle, (IntPtr)null, 0, loadLibraryWAddr, remoteMemoryAddress, 0, out remoteThreadId);
            if (handle == IntPtr.Zero)
                return false;
            return true;
        }


    }
}
