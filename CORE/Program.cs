using CORE.Processor;
using CORE.Processor.Executive;
using CORE.Processor.InstructionData;
using System;
using System.IO;

namespace CORE
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            var cpu = new CPU();

            var inst = new Instruction
            {
                Source = OperationTarget.Constant,
                Destination = OperationTarget.A,
                OpCode = OpCode.Ari,
                Data = (byte)ArithmeticOpCode.Add,
                OperandSize = OperandSize.Dword
            };
            var inst2 = new Instruction
            {
                Source = OperationTarget.T,
                Destination = OperationTarget.B,
                OpCode = OpCode.Mov,
                OperandSize = OperandSize.Dword
            };
            var inst3 = new Instruction
            {
                Source = OperationTarget.B,
                OpCode = OpCode.Flo,
                Data = (byte)FlowControlOpCode.Jmp
            };

            var cnst = 1500;
            var memory = new byte[128];

            using (var str = new MemoryStream(memory))
            using (var bw = new BinaryWriter(str))
            {
                bw.Write(inst.Encode());
                bw.Write(cnst);
                bw.Write(inst2.Encode());
                bw.Write(inst3.Encode());
            }
            cpu.LoadImage(memory);

            while (true)
            {
                Console.Write($"{cpu.I.ToString("X8")}> ");
                var cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "s":
                        cpu.Step();
                        break;
                    case "d":
                        Console.WriteLine($"A: {cpu.A.ToString("X8")}  B: {cpu.B.ToString("X8")}  C: {cpu.C.ToString("X8")}  D: {cpu.D.ToString("X8")}");
                        Console.WriteLine($"S: {cpu.S.ToString("X8")}  T: {cpu.T.ToString("X8")}  X: {cpu.X.ToString("X8")}  I: {cpu.I.ToString("X8")}");
                        break;
                }
            }
        }
    }
}
