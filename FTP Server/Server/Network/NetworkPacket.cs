namespace Message_Board.Network
{
    public struct NetworkPacket
    {
        public int index;
        public int Max;
        public byte[] Content;


        public NetworkPacket(int index, int max, byte[] content)
        {
            this.index = index;
            Max = max;
            Content = content;
        }
    }
}