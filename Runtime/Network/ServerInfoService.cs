using System;
using System.Collections.Generic;
using System.Linq;
using Minimoo.Common.Enum;
using UnityEngine;

namespace Minimoo.Network
{
    /// <summary>
    /// 서버 정보 서비스
    /// </summary>
    public class ServerInfoService : Singleton<ServerInfoService>
    {
        [field: SerializeField]
        public SerializedDictionary<ServerEnvironmentType, SerializedDictionary<ServerType, ServerInfo>> Infos { get; set; } = new();
        /*
        {
            {
                ServerEnvironmentType.Local,
                new ()
                {
                    new ServerInfo(ServerType.Lobby, "localhost", 28000 , false, true),
                    new ServerInfo(ServerType.CatGirl, "localhost", 28100 , false, true),
                    new ServerInfo(ServerType.Matchgem, "localhost", 28200 , false, true),
                    new ServerInfo(ServerType.Cralwer, "localhost", 28300 , false, true),
                }
            },
            {
                ServerEnvironmentType.Dev,
                 new ()
                 {
                    new ServerInfo(ServerType.Lobby, "oliverpc.tplinkdns.com", 28000 , false, true),
                    new ServerInfo(ServerType.CatGirl, "oliverpc.tplinkdns.com", 28100 , false, true),
                    new ServerInfo(ServerType.Matchgem, "oliverpc.tplinkdns.com", 28200 , false, true),
                    new ServerInfo(ServerType.Cralwer, "oliverpc.tplinkdns.com", 28300 , false, true)
                 }
            },
            {
                ServerEnvironmentType.Production,
                 new ()
                 {
                    new ServerInfo(ServerType.Lobby, "minimoo.minimumstudio.click", 28000 , true, true),
                    new ServerInfo(ServerType.CatGirl, "minimoo.minimumstudio.click", 28100, false, true),
                    new ServerInfo(ServerType.Matchgem, "minimoo.minimumstudio.click", 28200, false, true),
                    new ServerInfo(ServerType.Cralwer, "minimoo.minimumstudio.click", 28300, false, true)
                 }
            }
        };
        */

        public ServerInfo Get(ServerType serverType)
        {
            var environment = NetworkService.Instance.Environment;
            return Get(environment, serverType);
        }

        public ServerInfo Get(ServerEnvironmentType environment, ServerType serverType)
        {
            if (Infos.TryGetValue(environment, out var serverInfoMap))
            {
                if (serverInfoMap.TryGetValue(serverType, out var info))
                {
                    return info;
                }
            }

            return null;
        }

    }
}
