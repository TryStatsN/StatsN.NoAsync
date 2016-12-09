using System;
using System.Net;
using System.Net.Sockets;

namespace StatsN
{
	public class Tcp : BaseCommunicationProvider
    {
#pragma warning disable CC0052 // Make field readonly
#pragma warning disable CC0033 // Dispose Fields Properly
        private TcpClient Client;
#pragma warning restore CC0033 // Dispose Fields Properly
#pragma warning restore CC0052 // Make field readonly
        private NetworkStream Stream;
        private IPEndPoint ipEndpoint;
#pragma warning disable CC0052 // Make field readonly
        private object padLock = new object();
#pragma warning restore CC0052 // Make field readonly
        public override bool IsConnected
        {
            get
            {
                return Client?.Connected ?? false;
            }
        }

        public override bool Connect()
        {
            if(ipEndpoint == null)
            {
                ipEndpoint = GetIpAddress();
            }
            if (ipEndpoint == null) return false;
            lock (padLock)
            {
                if (Client?.Connected ?? false) return true; //this could change since things could q up
                try
                {
                    DisposeClient();
                    Client = new TcpClient();
                    Client.Connect(this.ipEndpoint.Address, Options.Port);
                    if (Client.Connected) Stream = Client.GetStream();
                    return Client.Connected && Stream != null && Stream.CanWrite;
                }
                catch (Exception e)
                {
                    this.Options.LogException(e);
                    return false;
                }
            }
            
        }

        public override void OnDispose()
        {
            DisposeClient();
        }
        public void DisposeClient()
        {
#if NETFULL
            Client?.Close();
#else
            Client?.Dispose();
#endif
            Client = null;
        }

        public override void Send(byte[] payload)
        {
            if(!Client.Connected || !Stream.CanWrite)
            {
                Options.LogEvent("Tcp Stream not ready to write bytes dropping payload on the floor", Exceptions.EventType.Warning);
                return;
            }
            try
            {
                Stream.Write(payload, 0, payload.Length);
            }
            catch(Exception e)
            {
                this.Options.LogException(e);
            }
            
        }
    }
}
