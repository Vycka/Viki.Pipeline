using System;
using System.Buffers;

namespace Viki.Pipeline.Core.Packets
{
    public class NullArrayPool 
    {
        public static readonly ArrayPool<byte> Instance;

        static NullArrayPool()
        {
            Instance = new NullArrayPoolInstance();
        }

        private NullArrayPool()
        {
        }

        private class NullArrayPoolInstance : ArrayPool<byte>
        {
            public override byte[] Rent(int minimumLength)
            {
                throw new NotImplementedException();
            }

            public override void Return(byte[] array, bool clearArray = false)
            {
            }
        }
    }
}