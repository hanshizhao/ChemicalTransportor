using SiemensCommunicator.Core.Interfaces;
using SiemensCommunicator.Core.Models.Enums;

namespace SiemensCommunicator.Application;

/// <summary>
/// SiemensCommunicator 模块 API 服务
/// </summary>
public class SystemAppService : IDynamicApiController
{
    private readonly ISystemService _systemService;
    private readonly IPlcConnectionService _plcConnectionService;

    public SystemAppService(
        ISystemService systemService,
        IPlcConnectionService plcConnectionService)
    {
        _systemService = systemService;
        _plcConnectionService = plcConnectionService;
    }

    /// <summary>
    /// 获取模块描述
    /// </summary>
    /// <returns>模块描述信息</returns>
    public string GetDescription()
    {
        return "SiemensCommunicator - 西门子 PLC 通讯模块";
    }

    /// <summary>
    /// 获取所有 PLC 连接状态（健康检查接口）
    /// </summary>
    /// <returns>连接ID与状态的字典</returns>
    public async Task<Dictionary<long, PlcConnectionState>> GetAllConnectionStates()
    {
        return await _plcConnectionService.GetAllStatesAsync();
    }
}
