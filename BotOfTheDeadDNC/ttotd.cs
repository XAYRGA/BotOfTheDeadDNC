using System;
using System.Collections.Generic;
using System.Text;
using xayrga.binutils;

namespace BotOfTheDeadDNC
{
    public class TTOTDWordObject
    {
        private int address;
        private ProcessMemoryInterface pmI;

        public bool enabled; 
        public float proximity;
        public byte wordLength;
        public byte typedLength;
        public string word;

        public static TTOTDWordObject load(int addr, ProcessMemoryInterface iface)
        {
            var wO = new TTOTDWordObject();
            wO.address = addr;
            wO.enabled = iface.getU16(addr + 0x4D) != 0x0001;
            wO.proximity = iface.getF32(addr + 0x48);
            wO.wordLength = iface.getU8(addr + 0x98);
            wO.typedLength = iface.getU8(addr + 0x99);
            wO.word = iface.getStringL(addr + 0x58, wO.wordLength);
            wO.pmI = iface;
            return wO;
        }

        public void update()
        {
            enabled = pmI.getU16(address + 0x4D) != 0x0001;
            proximity = pmI.getF32(address + 0x48);
            wordLength = pmI.getU8(address + 0x98);
            typedLength = pmI.getU8(address + 0x99);
            word = pmI.getStringL(address + 0x58, wordLength);
        }

        public void finish()
        {
            pmI.setU8(address + 0x99, wordLength);
        }

        public int MemoryAddress
        {
            get
            {
                return address;
            }
        }
    }
}
