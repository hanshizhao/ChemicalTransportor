using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Core.Events;

public class PlcStateChangedEventArgs : EventArgs
{
    public string ConnectionId { get; set; } = string.Empty;
    public PlcConnectionState OldState { get; set; }
    public PlcConnectionState NewState { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
