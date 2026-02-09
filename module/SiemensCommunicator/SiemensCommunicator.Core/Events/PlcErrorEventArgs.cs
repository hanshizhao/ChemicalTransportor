using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Core.Events;

public class PlcErrorEventArgs : EventArgs
{
    public string ConnectionId { get; set; } = string.Empty;
    public PlcErrorType ErrorType { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.Now;
    public Exception? Exception { get; set; }
}
