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
                Data = (byte)ArithmeticOpCode.Add
            };
            var inst2 = new Instruction
            {
                Source = OperationTarget.T,
                Destination = OperationTarget.B,
                OpCode = OpCode.Mov
            };

            var cnst = 1500;
            var memory = new byte[128];

            using (var str = new MemoryStream(memory))
            using (var bw = new BinaryWriter(str))
            {
                bw.Write(inst.Encode());
                bw.Write(cnst);
                bw.Write(inst2.Encode());
            }

            cpu.LoadImage(memory);
            cpu.Step();
            cpu.Step();
            Console.WriteLine(cpu.B);

            Console.ReadLine();
        }
    }
}
