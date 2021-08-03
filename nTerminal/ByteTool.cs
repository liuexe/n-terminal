using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace ByteTool
{
    public static class Hex
    {
        public static byte[] ConvertHexStringToBytes(string hexString)
        {
            try
            {
                hexString = hexString.Replace(" ", "");
                if (hexString.Length % 2 != 0)
                {
                    throw new ArgumentException("bad length");
                }
                byte[] returnBytes = new byte[hexString.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }
                return returnBytes;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string ConvertBytesToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }
        public static byte[] getRandomBytes(int num)
        {
            byte[] rndBytes = new byte[num];
            Random random = new Random();
            random.NextBytes(rndBytes);
            return rndBytes;
        }
    }
    public static class Struct
    {
        public static byte[] StructToBytes(object structObj)
        {
            int size = Marshal.SizeOf(structObj);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structObj, buffer, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        public static object BytesToStruct(byte[] bytes, Type type)
        {
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length)
            {
                return null;
            }
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, structPtr, size);
            object obj = Marshal.PtrToStructure(structPtr, type);
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

        public static object BytesToStruct(byte[] bytes, Type structType, int offset = 0)
        {
            int size = Marshal.SizeOf(structType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, offset, buffer, size);
                return Marshal.PtrToStructure(buffer, structType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        public static int GetByteLength(Type structType)
        {
            return Marshal.SizeOf(structType);
        }
    }
    public static class Bin
    {
        public static byte[] Import(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}
