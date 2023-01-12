using System;

namespace CORE.Processor.Executive
{
    public class Instruction
    {
        public OpCode OpCode { get; set; }
        public OperationTarget Source { get; set; }
        public OperationTarget Destination { get; set; }
        public OperandFlags OperandFlags { get; set; }
        public OperandSize OperandSize { get; set; }

        public byte Data { get; set; }

        public short Encode()
        {
            var ret = 0;
            ret |= (byte)OpCode;
            ret |= (byte)Source << 3;
            ret |= (byte)Destination << 6;
            ret |= (byte)OperandFlags << 9;
            ret |= (byte)OperandSize << 11;
            ret |= Data << 13;

            return (short)ret;
        }

        public override string ToString()
        {
            var encoded = (ushort)Encode();
            return $"{OpCode.ToString()}: {Source} -> {Destination} [{OperandSize.ToString().ToUpper()}]: 0x{encoded.ToString("X2")} [{Convert.ToString(encoded, 2).PadLeft(16, '0')}]";
        }

        public static Instruction Decode(ushort instruction)
        {
            var ret = new Instruction
            {
                OpCode = (OpCode)(instruction & 0x7),
                Source = (OperationTarget)((instruction & 0x38) >> 3),
                Destination = (OperationTarget)((instruction & 0x1C0) >> 6),
                OperandFlags = (OperandFlags)((instruction & 0x600) >> 9),
                OperandSize = (OperandSize)((instruction & 0x1800) >> 11),
                Data = (byte)((instruction & 0xE000) >> 13)
            };

            return ret;
        }
    }
}
