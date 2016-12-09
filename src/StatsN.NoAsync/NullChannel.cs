namespace StatsN
{
	public class NullChannel : BaseCommunicationProvider
    {
        public override bool IsConnected
        {
            get
            {
                return true;
            }
        }

        public override bool Connect()
        {
			return true;
        }

        public override void OnDispose(){}

        public override void Send(byte[] payload)
        {
        }
    }
}
