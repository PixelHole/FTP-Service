namespace Message_Board.Network
{
    public static class NetworkFlags
    {
        public static string ClientDisconnectedFlag { get; } = "<CD>";
        public static string ConnectionFailed { get; } = "<CF>";
        public static string ConnectionSuccess { get; } = "<CS>";
        public static string EndOfFileFlag { get; } = "<EOF>";
        public static string SeparatorFlag { get; } = "<SP>";
        public static string ReadyFlag { get; } = "<RD>";
        public static string InvalidCommandFlag { get; } = "<IC>";
    }
}