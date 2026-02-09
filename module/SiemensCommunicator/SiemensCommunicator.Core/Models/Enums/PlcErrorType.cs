namespace SiemensCommunicator.Core.Models.Enums;

public enum PlcErrorType
{
    ConnectionFailed = 0,
    ConnectionLost = 1,
    ReadFailed = 2,
    WriteFailed = 3,
    InvalidConfig = 4,
    Timeout = 5
}
