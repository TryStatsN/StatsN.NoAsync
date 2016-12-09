namespace StatsN.Exceptions
{
	public class StatsdLogMessage
    {
        public string Message { get; set; }
        public EventType Weight { get; set; }
        public StatsdLogMessage(string message, EventType weight)
        {
            this.Message = message ?? string.Empty;
            Weight = weight;
        }
    }
}
