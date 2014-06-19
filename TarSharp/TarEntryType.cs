namespace TarSharp
{
    public enum TarEntryType
    {
        Normal,
        HardLink,
        SymbolicLink,
        CharacterDeviceNode,
        BlockDeviceNode,
        Directory,
        FifoNode,
        ContiguousFile
    }
}
