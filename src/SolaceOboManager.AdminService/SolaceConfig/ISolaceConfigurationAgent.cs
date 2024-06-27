using Refit;

namespace SolaceOboManager.AdminService.SolaceConfig;


public interface ISolaceConfigurationAgent
{
    [Post("/SEMP/v2/config/msgVpns/{msgVpnName}/clientUsernames")]
    Task CreateUser(string msgVpnName, [Body] MsgVpnClientUsername user);
}

