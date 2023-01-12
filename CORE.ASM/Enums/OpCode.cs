namespace CORE.ASM.Enums
{
    public enum OpCode
    {
        Mov = 0x00, // Move
        Ari = 0x01, // Arithmetic [ADD MUL SUB DIV]
        Tst = 0x02, // Test [>= <= == < > !=]
        Jmp = 0x03, // Jump [JMP TJMP FJMP]
        Prt = 0x04, // Port operation [IN OUT]
        Log = 0x05, // Logical operation [AND OR XOR NOT]
        Sta = 0x06, // Stack operation
    }
}
