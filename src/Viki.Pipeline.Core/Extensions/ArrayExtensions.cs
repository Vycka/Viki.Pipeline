using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Viki.Pipeline.Core.Extensions
{
    public static class ArrayExtensions
    {
        // Commented out cuz not sure about compatibility on .NET Core ... NET 6 ...

        //private delegate void MemorySetter(IntPtr array, byte value, int count);

        //private static readonly MemorySetter _memset;

        //static ArrayExtensions()
        //{
        //    _memset = CreateMemset();
        //}

        //private static MemorySetter CreateMemset()
        //{
        //    var m = new DynamicMethod(
        //        "memset",
        //        MethodAttributes.Public | MethodAttributes.Static,
        //        CallingConventions.Standard,
        //        typeof(void),
        //        new[] { typeof(IntPtr), typeof(byte), typeof(int) },
        //        typeof(ArrayExtensions),
        //        false);
        //    var il = m.GetILGenerator();
        //    il.Emit(OpCodes.Ldarg_0); // address
        //    il.Emit(OpCodes.Ldarg_1); // initialization value
        //    il.Emit(OpCodes.Ldarg_2); // number of bytes
        //    il.Emit(OpCodes.Initblk);
        //    il.Emit(OpCodes.Ret);
        //    return (MemorySetter)m.CreateDelegate(typeof(MemorySetter));
        //}

        ///// <summary>
        ///// c# version of memset for performance freaks :)
        ///// http://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c
        ///// </summary>
        //public static void Memset(this byte[] array, byte value, int count, int offset = 0)
        //{
        //    GCHandle h = default(GCHandle);
        //    try
        //    {
        //        h = GCHandle.Alloc(array, GCHandleType.Pinned);
        //        IntPtr addr = h.AddrOfPinnedObject() + offset;
        //        _memset(addr, value, count);
        //    }
        //    finally
        //    {
        //        if (h.IsAllocated)
        //            h.Free();
        //    }
        //}
    }
}