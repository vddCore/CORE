using CORE.ASM.Enums;
using System;

namespace CORE.ASM
{
    public class Instruction
    {
        public OpCode OpCode { get; set; }
        public OperationTarget Source { get; set; }
        public OperationTarget Destination { get; set; }
        public byte Data { get; set; }

        public short Encode()
        {
            var ret = 0;
            ret |= (byte)OpCode;
            ret |= (byte)Source << 4;
            ret |= (byte)Destination << 7;
            ret |= Data << 10;

            return (short)ret;
        }

        public override string ToString()
        {
            var encoded = (ushort)Encode();
            return $"{OpCode.ToString()}: {Source} -> {Destination}: 0x{encoded.ToString("X2")} [{Convert.ToString(encoded, 2).PadLeft(16, '0')}]";
        }

        public static Instruction Decode(ushort instruction)
        {
            var ret = new Instruction
            {
                OpCode = (OpCode)(instruction & 0x0F),
                Source = (OperationTarget)(instruction & 0x70),
                Destination = (OperationTarget)(instruction & 0x380),
                Data = (byte)(instruction & 0xFC00)
            };

            return ret;
        }
    }
}
