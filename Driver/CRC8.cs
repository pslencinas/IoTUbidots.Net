using System;
using Microsoft.SPOT;

namespace CRC
{
    //https://ghsi.de/CRC/index.php?Polynom=100000111&Message=002E6000%0D%0A

    class CRC8
    {
        static byte[] table = new byte[256];

        // x8 + x2 + x1 + x0
        const ushort poly = 0x107;

        public byte ComputeChecksum(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {
                foreach (byte b in bytes)
                {
                    crc = table[crc ^ b];
                }
            }
            return crc;
        }

        public CRC8()
        {
            for (int i = 0; i < 256; ++i)
            {
                int temp = i;
                for (int j = 0; j < 8; ++j)
                {
                    if ((temp & 0x80) != 0)
                    {
                        temp = (temp << 1) ^ poly;
                    }
                    else
                    {
                        temp <<= 1;
                    }
                }
                table[i] = (byte)temp;
            }
        }
    }
}
