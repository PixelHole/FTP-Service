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
        public static string ExecutionSuccessFlag { get; } = "<ES>";
        
        // project specific flags
        public static string InvalidLoginInfoFlag { get; } = "<530>";
        public static string UsernameAcceptedFlag { get; } = "<331>";
        public static string LoginSuccessFlag { get; } = "<230>";
        
        public static string FileOperationSuccessFlag { get; } = "<250>";
        public static string FileOperationFailureFlag { get; } = "<550>";
        
        public static string DirectoryOperationSuccessFlag { get; } = "<257>";
        
        public static string ListTransferFlag { get; } = "<125>";
        public static string FileTransferFlag { get; } = "<150>";
        
        public static string TransferSuccessFlag { get; } = "<226>";

        public static string FailedHandshakeFlag { get; } = "<FH>";
    }
}