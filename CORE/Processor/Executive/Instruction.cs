using System;

namespace CORE.Processor.Executive
{
    public class Instruction
    {
        public OpCode OpCode { get; set; }
        public OperationTarget Source { get; set; }
        public OperationTarget Destination { get; set; }
        public OperandFlags OperandFlags { get; set; }
        public byte Data { get; set; }

        public short Encode()
        {
            var ret = 0;
            ret |= (byte)OpCode;
            ret |= (byte)Source << 4;
            ret |= (byte)Destination << 7;
            ret |= (byte)OperandFlags << 10;
            ret |= Data << 12;

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
                Source = (OperationTarget)((instruction & 0x70) >> 4),
                Destination = (OperationTarget)((instruction & 0x380) >> 7),
                OperandFlags = (OperandFlags)((instruction & 0xC00) >> 10),
                Data = (byte)((instruction & 0xF000) >> 12)
            };

            return ret;
        }
    }
}
