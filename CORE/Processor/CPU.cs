using CORE.Processor.Executive;
using CORE.Processor.InstructionData;
using System;
using System.Collections.Generic;

namespace CORE.Processor
{
    public class CPU
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }

        public int F { get; set; }

        public int S { get; set; }
        public int T { get; set; }
        public int X { get; set; }

        public uint I { get; private set; }
        public byte[] Memory { get; set; }

        public Dictionary<byte, uint> InterruptVectors { get; private set; }

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
                case OpCode.Flo:
                    DoFlowControl(instruction);
                    break;
                case OpCode.Sta:
                    DoStackOperation(instruction);
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
                    if (!instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
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

                case OperationTarget.X:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveUIntToMemory((uint)X, (uint)sourceValue);
                    else X = sourceValue;
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
                case ArithmeticOpCode.Mod:
                    T = a % b;
                    break;
                case ArithmeticOpCode.Ror: T = (a >> b) | (a << (-b)); break;
                case ArithmeticOpCode.Rol: T = (a << b) | (a >> (-b)); break;
                case ArithmeticOpCode.Shr: T = (a >> b); break;
                case ArithmeticOpCode.Shl: T = (a << b); break;

                default: throw new Exception($"Unknown arithmetic opcode: {arithmeticOpCode}.");
            }
        }

        private void DoFlowControl(Instruction instruction)
        {
            var flowControlOpCode = (FlowControlOpCode)instruction.Data;
            var sourceOperandValue = GetOperandValue(instruction, false);

            switch (flowControlOpCode)
            {
                case FlowControlOpCode.Jmp:
                    JumpToAddress((uint)sourceOperandValue);
                    break;
                case FlowControlOpCode.Fjmp:
                    if (T == 0) JumpToAddress((uint)sourceOperandValue);
                    break;
                case FlowControlOpCode.Tjmp:
                    if (T != 0) JumpToAddress((uint)sourceOperandValue);
                    break;
                case FlowControlOpCode.Rjmp:
                    JumpRelative(sourceOperandValue);
                    break;
                // todo call, ret, iret, int
            }
        }

        private void DoStackOperation(Instruction instruction)
        {
            var stackOpCode = (StackOpCode)instruction.Data;
            var sourceOperandValue = GetOperandValue(instruction, false);

            switch(stackOpCode)
            {
                case StackOpCode.Push:
                    if (sourceOperandValue > byte.MaxValue) throw new Exception("Operand size mismatch. Wanted byte.");
                    break;

                case StackOpCode.PushW:
                    if (sourceOperandValue > ushort.MaxValue) throw new Exception("Operand size mismatch. Wanted word.");
                    PushShort((ushort)sourceOperandValue);
                    break;

                case StackOpCode.PushD:
                    PushUInt((uint)sourceOperandValue);
                    break;

                case StackOpCode.Pop:
                    break;

                case StackOpCode.PopW:
                    break;

                case StackOpCode.PopD:
                    break;
            }
        }

        private void PushByte(byte value)
        {
            Memory[S--] = value;
        }

        private void PushShort(ushort value)
        {
            Memory[S--] = (byte)(value & 0x00FF);
            Memory[S--] = (byte)((value & 0xFF00) >> 8);
        }

        private void PushUInt(uint value)
        {
            Memory[S--] = (byte)(value & 0x000000FF);
            Memory[S--] = (byte)((value & 0x0000FF00) >> 8);
            Memory[S--] = (byte)((value & 0x00FF0000) >> 16);
            Memory[S--] = (byte)((value & 0xFF000000) >> 24);
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

                case OperationTarget.X:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = (int)GetUIntFromMemory((uint)X);
                    else value = X;
                    break;

                default: throw new Exception($"Unknown operand.");
            }

            return value;
        }

        private void JumpToAddress(uint address)
        {
            I = address;
        }

        private void JumpRelative(int offset)
        {
            if (offset > 0)
                I += (uint)offset;
            else
                I -= (uint)Math.Abs(offset);
        }
    }
}
