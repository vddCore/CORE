using CORE.Processor.Executive;
using CORE.Processor.Executive.Exceptions;
using CORE.Processor.InstructionData;
using System;

namespace CORE.Processor
{
    public class CPU
    {
        public uint A { get; set; }
        public uint B { get; set; }
        public uint C { get; set; }
        public uint D { get; set; }

        public uint S { get; set; }
        public uint T { get; set; }
        public uint X { get; set; }

        public uint I { get; private set; }
        public byte[] Memory { get; set; }

        public bool Halted { get; set; }
        public bool InterruptsEnabled { get; set; }
        public uint[] InterruptVectors { get; }

        private bool IsInterruptExecuting { get; set; }

        public CPU()
        {
            Memory = new byte[1024 * 16384];
            InterruptVectors = new uint[255];
        }

        public void Reset()
        {
            I = 0;
            A = B = C = D = S = T = 0;
        }

        public void Step()
        {
            if (Halted && !IsInterruptExecuting) return;

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

        public void ExecuteInterrupt(byte number)
        {
            CallInterrupt(InterruptVectors[number]);
            ReturnFromInterrupt();
        }

        private uint ReadValueAtInstructionPointer(OperandSize operandSize, bool increment = true)
        {
            var value = ReadValueFromMemory(I, operandSize);

            if (increment)
            {
                switch (operandSize)
                {
                    case OperandSize.Byte: I += 1; break;
                    case OperandSize.Word: I += 2; break;
                    case OperandSize.Dword: I += 4; break;
                }
            }
            return value;
        }

        private uint ReadValueFromMemory(uint address, OperandSize operandSize)
        {
            uint ret = 0;
            switch (operandSize)
            {
                case OperandSize.Byte:
                    ret = Memory[address];
                    break;
                case OperandSize.Word:
                    ret = Memory[address + 0];
                    ret |= (ushort)(Memory[address + 1] << 8);
                    break;
                case OperandSize.Dword:
                    ret = Memory[address];
                    ret |= (uint)(Memory[address + 1] << 8);
                    ret |= (uint)(Memory[address + 2] << 16);
                    ret |= (uint)(Memory[address + 3] << 24);
                    break;

                default: throw new Exception("Invalid operand size.");
            }
            return ret;
        }

        private void SaveValueToMemory(uint address, uint value, OperandSize operandSize)
        {
            switch (operandSize)
            {
                case OperandSize.Byte:
                    if (value > byte.MaxValue) throw new Exception("Operand size mismatch. Wanted at most a byte.");

                    Memory[address] = (byte)value;
                    break;

                case OperandSize.Word:
                    if (value > short.MaxValue) throw new Exception("Operand size mismatch. Wanted at most a word.");

                    Memory[address] = (byte)(value & 0x00FF);
                    Memory[address + 1] = (byte)(value & 0xFF00 >> 8);
                    break;

                case OperandSize.Dword:
                    Memory[address] = (byte)(value & 0x000000FF);
                    Memory[address + 1] = (byte)((value & 0x0000FF00) >> 8);
                    Memory[address + 2] = (byte)((value & 0x00FF0000) >> 16);
                    Memory[address + 3] = (byte)((value & 0xFF000000) >> 24);
                    break;

                default: throw new Exception("Invalid operand size.");
            }
        }

        private Instruction Fetch()
        {
            return Instruction.Decode((ushort)ReadValueAtInstructionPointer(OperandSize.Word));
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
                case OpCode.Tst:
                    DoTest(instruction);
                    break;
                case OpCode.Flo:
                    DoFlowControl(instruction);
                    break;
                case OpCode.Log:
                    DoLogic(instruction);
                    break;
                case OpCode.Sta:
                    DoStackOperation(instruction);
                    break;
                case OpCode.Cpu:
                    DoCpuStateOperation(instruction);
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

                    var addr = ReadValueAtInstructionPointer(instruction.OperandSize);
                    SaveValueToMemory(addr, sourceValue, instruction.OperandSize);
                    break;

                case OperationTarget.A:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveValueToMemory(A, sourceValue, instruction.OperandSize);
                    else A = sourceValue;
                    break;

                case OperationTarget.B:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveValueToMemory(B, sourceValue, instruction.OperandSize);
                    else B = sourceValue;
                    break;

                case OperationTarget.C:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveValueToMemory(C, sourceValue, instruction.OperandSize);
                    else C = sourceValue;
                    break;

                case OperationTarget.D:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveValueToMemory(D, sourceValue, instruction.OperandSize);
                    else D = sourceValue;
                    break;

                case OperationTarget.S:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveValueToMemory(S, sourceValue, instruction.OperandSize);
                    else S = sourceValue;
                    break;

                case OperationTarget.T:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveValueToMemory(T, sourceValue, instruction.OperandSize);
                    else T = sourceValue;
                    break;

                case OperationTarget.X:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.DestinationIsPointer))
                        SaveValueToMemory(X, sourceValue, instruction.OperandSize);
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
                case ArithmeticOpCode.Add: X = a + b; break;
                case ArithmeticOpCode.Sub: X = a - b; break;
                case ArithmeticOpCode.Mul: X = a * b; break;
                case ArithmeticOpCode.Div:
                    if (b == 0)
                    {
                        PushValue(I, OperandSize.Dword);
                        ExecuteInterrupt(0);
                    }
                    X = a / b;
                    break;
                case ArithmeticOpCode.Mod:
                    X = a % b;
                    break;
                case ArithmeticOpCode.Shr: X = (a >> (int)b); break;
                case ArithmeticOpCode.Shl: X = (a << (int)b); break;

                default:
                    PushValue((uint)InvalidOpCodeReason.ArithmeticOpCode, OperandSize.Dword);
                    PushValue(instruction.Data, OperandSize.Dword);
                    PushValue(I, OperandSize.Dword);

                    ExecuteInterrupt(1);
                    break;
            }
        }

        private void DoTest(Instruction instruction)
        {
            var testOpCode = (TestOpCode)instruction.Data;

            var a = GetOperandValue(instruction, false);
            var b = GetOperandValue(instruction, true);

            var result = 0u;

            switch (testOpCode)
            {
                case TestOpCode.Equal:
                    if (a == b) result = 1;
                    break;
                case TestOpCode.GreaterThan:
                    if (a > b) result = 1;
                    break;
                case TestOpCode.LesserThan:
                    if (a < b) result = 1;
                    break;
                case TestOpCode.GreaterOrEqual:
                    if (a >= b) result = 1;
                    break;
                case TestOpCode.LesserOrEqual:
                    if (a <= b) result = 1;
                    break;
                default:
                    PushValue((uint)InvalidOpCodeReason.TestOpCode, OperandSize.Dword);
                    PushValue(instruction.Data, OperandSize.Dword);
                    PushValue(I, OperandSize.Dword);

                    ExecuteInterrupt(1);
                    break;
            }

            T = result;
        }

        private void DoFlowControl(Instruction instruction)
        {
            var flowControlOpCode = (FlowControlOpCode)instruction.Data;
            var sourceOperandValue = GetOperandValue(instruction, false);

            switch (flowControlOpCode)
            {
                case FlowControlOpCode.Jmp:
                    JumpToAddress(sourceOperandValue);
                    break;
                case FlowControlOpCode.Fjmp:
                    if (T == 0) JumpToAddress(sourceOperandValue);
                    break;
                case FlowControlOpCode.Tjmp:
                    if (T != 0) JumpToAddress(sourceOperandValue);
                    break;
                case FlowControlOpCode.Rjmp:
                    JumpRelative((int)sourceOperandValue);
                    break;
                case FlowControlOpCode.Call:
                    CallAddress(sourceOperandValue);
                    break;
                case FlowControlOpCode.Int:
                    if (InterruptsEnabled)
                        ExecuteInterrupt((byte)sourceOperandValue);
                    break;
                case FlowControlOpCode.Ret:
                    ReturnFromCall();
                    break;
                case FlowControlOpCode.Iret:
                    ReturnFromInterrupt();
                    break;
                default:
                    PushValue((uint)InvalidOpCodeReason.FlowControlOpCode, OperandSize.Dword);
                    PushValue(instruction.Data, OperandSize.Dword);
                    PushValue(I, OperandSize.Dword);

                    ExecuteInterrupt(1);
                    break;
            }
        }

        private void DoLogic(Instruction instruction)
        {
            var logicOpCode = (LogicOpCode)instruction.Data;

            var a = GetOperandValue(instruction, false);
            var b = GetOperandValue(instruction, true);

            switch (logicOpCode)
            {
                case LogicOpCode.And:
                    X = a & b;
                    break;
                case LogicOpCode.Or:
                    X = a | b;
                    break;
                case LogicOpCode.Not:
                    X = ~a;
                    break;
                case LogicOpCode.Xor:
                    X = a ^ b;
                    break;
                default:
                    PushValue((uint)InvalidOpCodeReason.LogicOpCode, OperandSize.Dword);
                    PushValue(instruction.Data, OperandSize.Dword);
                    PushValue(I, OperandSize.Dword);

                    ExecuteInterrupt(1);
                    break;
            }
        }

        private void DoStackOperation(Instruction instruction)
        {
            var stackOpCode = (StackOpCode)instruction.Data;

            switch (stackOpCode)
            {
                case StackOpCode.Push:
                    var sourceOperandValue = GetOperandValue(instruction, false);

                    if (sourceOperandValue > byte.MaxValue) throw new Exception("Operand size mismatch. Wanted byte.");
                    PushValue(sourceOperandValue, instruction.OperandSize);
                    break;

                case StackOpCode.Pop:
                    if (instruction.OperandFlags.HasFlag(OperandFlags.SourceIsPointer))
                        throw new Exception("Cannot pop from stack to pointer.");

                    var value = PopValue(instruction.OperandSize);
                    switch (instruction.Destination)
                    {
                        case OperationTarget.A: A = value; break;
                        case OperationTarget.B: B = value; break;
                        case OperationTarget.C: C = value; break;
                        case OperationTarget.D: D = value; break;
                        case OperationTarget.T: T = value; break;
                        case OperationTarget.X: X = value; break;

                        case OperationTarget.Constant:
                        case OperationTarget.S:
                            throw new Exception("Cannot pop from stack into stack pointer or constant, or a memory pointer.");

                        default:
                            throw new Exception($"Unknown destination operand {instruction.Destination}.");
                    }
                    break;
                default:
                    PushValue((uint)InvalidOpCodeReason.StackOpCode, OperandSize.Dword);
                    PushValue(instruction.Data, OperandSize.Dword);
                    PushValue(I, OperandSize.Dword);

                    ExecuteInterrupt(1);
                    break;
            }
        }

        private void DoCpuStateOperation(Instruction instruction)
        {
            var cpuStateOpCode = (CpuStateOpCode)instruction.Data;

            switch (cpuStateOpCode)
            {
                case CpuStateOpCode.Rst:
                    Reset();
                    break;
                case CpuStateOpCode.Cli:
                    InterruptsEnabled = false;
                    break;
                case CpuStateOpCode.Sti:
                    InterruptsEnabled = true;
                    break;
                case CpuStateOpCode.SetVe:
                    var sourceOperandValue = GetOperandValue(instruction, false);
                    var destinationOperandValue = GetOperandValue(instruction, true);

                    if (sourceOperandValue > 255)
                        throw new Exception("Interrupt vector index out of bounds.");

                    InterruptVectors[sourceOperandValue] = destinationOperandValue;
                    break;
                default:
                    PushValue((uint)InvalidOpCodeReason.CpuStateOpCode, OperandSize.Dword);
                    PushValue(instruction.Data, OperandSize.Dword);
                    PushValue(I, OperandSize.Dword);

                    ExecuteInterrupt(1);
                    break;
            }
        }

        private void PushValue(uint value, OperandSize operandSize)
        {
            switch (operandSize)
            {
                case OperandSize.Byte:
                    if (value > byte.MaxValue) throw new Exception("Operand size mismatch. Wanted byte.");

                    Memory[S--] = (byte)value;
                    break;

                case OperandSize.Word:
                    if (value > ushort.MaxValue) throw new Exception("Operand size mismatch. Wanted word.");

                    Memory[S--] = (byte)(value & 0x00FF);
                    Memory[S--] = (byte)((value & 0xFF00) >> 8);
                    break;

                case OperandSize.Dword:
                    Memory[S--] = (byte)(value & 0x000000FF);
                    Memory[S--] = (byte)((value & 0x0000FF00) >> 8);
                    Memory[S--] = (byte)((value & 0x00FF0000) >> 16);
                    Memory[S--] = (byte)((value & 0xFF000000) >> 24);
                    break;

                default: throw new Exception("Invalid operand size.");
            }
        }

        private uint PopValue(OperandSize operandSize)
        {
            var value = 0u;
            switch (operandSize)
            {
                case OperandSize.Byte:
                    value = Memory[S++];
                    break;

                case OperandSize.Word:
                    value = Memory[S--];
                    value |= (ushort)(Memory[S--] << 8);
                    break;

                case OperandSize.Dword:
                    value = Memory[S--];
                    value |= (ushort)(Memory[S--] << 8);
                    value |= (ushort)(Memory[S--] << 16);
                    value |= (ushort)(Memory[S--] << 24);
                    break;
            }
            return value;
        }

        private uint GetOperandValue(Instruction instruction, bool destinationOperand)
        {
            var value = 0u;

            var flagToCheck = destinationOperand ? OperandFlags.DestinationIsPointer
                                                 : OperandFlags.SourceIsPointer;

            switch (destinationOperand ? instruction.Destination : instruction.Source)
            {
                case OperationTarget.Constant:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = Memory[ReadValueAtInstructionPointer(instruction.OperandSize)];
                    else value = ReadValueAtInstructionPointer(instruction.OperandSize);
                    break;

                case OperationTarget.A:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = ReadValueFromMemory(A, instruction.OperandSize);
                    else value = A;
                    break;

                case OperationTarget.B:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = ReadValueFromMemory(B, instruction.OperandSize);
                    else value = B;
                    break;

                case OperationTarget.C:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = ReadValueFromMemory(C, instruction.OperandSize);
                    else value = C;
                    break;

                case OperationTarget.D:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = ReadValueFromMemory(D, instruction.OperandSize);
                    else value = D;
                    break;

                case OperationTarget.S:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = ReadValueFromMemory(S, instruction.OperandSize);
                    else value = S;
                    break;

                case OperationTarget.T:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = ReadValueFromMemory(T, instruction.OperandSize);
                    else value = T;
                    break;

                case OperationTarget.X:
                    if (instruction.OperandFlags.HasFlag(flagToCheck))
                        value = ReadValueFromMemory(X, instruction.OperandSize);
                    else value = X;
                    break;
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

        private void CallAddress(uint address)
        {
            PushValue(I, OperandSize.Dword);
            JumpToAddress(address);
        }

        private void ReturnFromCall()
        {
            I = PopValue(OperandSize.Dword);
        }

        private void CallInterrupt(uint address)
        {
            IsInterruptExecuting = true;

            PushValue(I, OperandSize.Dword);
            PushValue(A, OperandSize.Dword);
            PushValue(B, OperandSize.Dword);
            PushValue(C, OperandSize.Dword);
            PushValue(D, OperandSize.Dword);
            PushValue(S, OperandSize.Dword);
            PushValue(T, OperandSize.Dword);
            PushValue(X, OperandSize.Dword);

            JumpToAddress(address);
        }

        private void ReturnFromInterrupt()
        {
            X = PopValue(OperandSize.Dword);
            T = PopValue(OperandSize.Dword);
            S = PopValue(OperandSize.Dword);
            D = PopValue(OperandSize.Dword);
            C = PopValue(OperandSize.Dword);
            B = PopValue(OperandSize.Dword);
            A = PopValue(OperandSize.Dword);

            // Pop I
            JumpToAddress(PopValue(OperandSize.Dword));
            IsInterruptExecuting = false;
        }
    }
}
