using CORE.Processor.Executive;
using CORE.Processor.InstructionData;
using System;

namespace CORE.Processor
{
    public class CPU
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }
        public int S { get; set; }
        public int T { get; set; }

        public uint I { get; private set; }
        public byte[] Memory { get; set; }

        public CPU()
        {
            Memory = new byte[1024 * 16384];
        }

        public void Reset()
        {
            I = 0;
            A = B = C = D = S = T = 0;
        }

        public void Step()
        {
            var insn = Fetch();
            Console.WriteLine(insn.ToString());

            Execute(insn);
        }

        public void LoadImage(byte[] image)
        {
            I = 0;
            foreach (byte b in image)
                Memory[I++] = b;

            Reset();
        }

        private ushort GetUShortFromMemory()
        {
            ushort ret = (ushort)(Memory[I++]);
            ret |= (ushort)(Memory[I++] << 8);

            return ret;
        }

        private uint GetUIntFromMemory()
        {
            uint ret = (uint)(Memory[I++]);
            ret |= (uint)(Memory[I++] << 8);
            ret |= (uint)(Memory[I++] << 16);
            ret |= (uint)(Memory[I++] << 24);

            return ret;
        }

        private uint GetUIntFromMemory(uint pointer)
        {
            uint ret = (uint)(Memory[pointer]);
            ret |= (uint)(Memory[pointer + 1] << 8);
            ret |= (uint)(Memory[pointer + 2] << 16);
            ret |= (uint)(Memory[pointer + 3] << 24);

            return ret;
        }

        private void SaveUIntToMemory(uint addr, uint value)
        {
            Memory[addr + 0] = (byte)(value & 0x000000FF);
            Memory[addr + 1] = (byte)((value & 0x0000FF00) >> 8);
            Memory[addr + 2] = (byte)((value & 0x00FF0000) >> 16);
            Memory[addr + 3] = (byte)((value & 0xFF000000) >> 24);
        }

        private Instruction Fetch()
        {
            return Instruction.Decode(GetUShortFromMemory());
        }

        private void Execute(Instruction instruction)
        {
            switch (instruction.OpCode)
            {
                case OpCode.Mov:
                    DoMove(instruction);
                    break;
                case OpCode.Ari:
                    DoArithmetic(instruction);
                    break;
                default:
                    throw new Exception("Invalid opcode.");
            }
        }

        private void DoMove(Instruction instruction)
        {
            var sourceValue = GetOperandValue(instruction, false);

            switch (instruction.Destination)
            {
                case OperationTarget.Constant:
                    if(!instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        throw new Exception("Invalid destination operand: Constant.");

                    var addr = GetUIntFromMemory();
                    SaveUIntToMemory(addr, (uint)sourceValue);
                    break;

                case OperationTarget.A:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveUIntToMemory((uint)A, (uint)sourceValue);
                    else A = sourceValue;
                    break;

                case OperationTarget.B:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveUIntToMemory((uint)B, (uint)sourceValue);
                    else B = sourceValue;
                    break;

                case OperationTarget.C:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveUIntToMemory((uint)C, (uint)sourceValue);
                    else C = sourceValue;
                    break;

                case OperationTarget.D:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveUIntToMemory((uint)D, (uint)sourceValue);
                    else D = sourceValue;
                    break;

                case OperationTarget.S:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveUIntToMemory((uint)S, (uint)sourceValue);
                    else S = sourceValue;
                    break;

                case OperationTarget.T:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveUIntToMemory((uint)T, (uint)sourceValue);
                    else T = sourceValue;
                    break;

                default: throw new Exception($"Unknown destination operand {instruction.Source}.");
            }
        }

        private void DoArithmetic(Instruction instruction)
        {
            var arithmeticOpCode = (ArithmeticOpCode)instruction.Data;

            var a = GetOperandValue(instruction, false);
            var b = GetOperandValue(instruction, true);

            switch (arithmeticOpCode)
            {
                case ArithmeticOpCode.Add: T = a + b; break;
                case ArithmeticOpCode.Sub: T = a - b; break;
                case ArithmeticOpCode.Mul: T = a * b; break;
                case ArithmeticOpCode.Div:
                    if (b == 0) throw new Exception("Division by zero.");
                    T = a / b;
                    break;
            }
        }

        private int GetOperandValue(Instruction instruction, bool destinationOperand)
        {
            var value = 0;

            var flagToCheck = destinationOperand ? OperandFlags.DestinationIsPointer :
                                                   OperandFlags.SourceIsPointer;

            switch (destinationOperand ? instruction.Destination : instruction.Source)
            {
                case OperationTarget.Constant:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = Memory[GetUIntFromMemory()];
                    else value = (int)GetUIntFromMemory();
                    break;

                case OperationTarget.A:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = (int)GetUIntFromMemory((uint)A);
                    else value = A;
                    break;

                case OperationTarget.B:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = (int)GetUIntFromMemory((uint)B);
                    else value = B;
                    break;

                case OperationTarget.C:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = (int)GetUIntFromMemory((uint)C);
                    else value = C;
                    break;

                case OperationTarget.D:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = (int)GetUIntFromMemory((uint)D);
                    else value = D;
                    break;

                case OperationTarget.S:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = (int)GetUIntFromMemory((uint)S);
                    else value = S;
                    break;

                case OperationTarget.T:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = (int)GetUIntFromMemory((uint)T);
                    else value = T;
                    break;

                default: throw new Exception($"Unknown operand.");
            }

            return value;
        }
    }
}
