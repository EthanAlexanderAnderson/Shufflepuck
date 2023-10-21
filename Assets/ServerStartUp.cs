
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
#if UNITY_SERVER || UNITY_EDITOR || DEDICATED_SERVER || server
using Unity.Services.Multiplay;
#endif
using UnityEngine;

public class ServerStartUp : MonoBehaviour
{
    public static event System.Action ClientInstance;

    private const string InternalServerIP = "0.0.0.0";
    private ushort serverPort = 7777;
#if UNITY_SERVER || UNITY_EDITOR || DEDICATED_SERVER || server
    private IMultiplayService multiplayService;
    const int multiplayServiceTimeout = 20000;

    private string allocationId;
    private MultiplayEventCallbacks serverCallbacks;
    private IServerEvents serverEvents;
#endif
    // Start is called before the first frame update
    async void Start()
    {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-dedicatedServer")
            {
                server = true;
            }
            if (args[i] == "-port" && (i + 1 < args.Length))
            {
                serverPort = (ushort)int.Parse(args[i + 1]);
            }
        }
        if (server)
        {
#if UNITY_SERVER || UNITY_EDITOR || DEDICATED_SERVER || server
            StartServer();
            await StartServerServices();
#endif
        }
        else
        {
            ClientInstance?.Invoke();
        }
    }
#if UNITY_SERVER || UNITY_EDITOR || DEDICATED_SERVER || server
    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIP, serverPort);
        NetworkManager.Singleton.StartServer();
    }

    async Task StartServerServices()
    {
        await UnityServices.InitializeAsync();
        try
        {
            multiplayService = MultiplayService.Instance;
            await multiplayService.StartServerQueryHandlerAsync(2, "n/a", "n/a", "0", "n/a");
        }
        catch (Exception ex)
        {
            Debug.Log($"Something went wrong trying to set up the SQP Service: {ex}");
        }

        try
        {
            var matchmakerPayload = await GetMatchmakerPayload( multiplayServiceTimeout );
            if (matchmakerPayload != null)
            {
                Debug.Log($"Got payload: {matchmakerPayload}");

            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Something went wrong trying to set up the Allocation Services: {ex}");
        }
    }

    private async Task<MatchmakingResults> GetMatchmakerPayload( int timeout )
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();
        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }
        return null;
    }

    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if (multiplayService == null) return null;
        allocationId = null;
        serverCallbacks = new MultiplayEventCallbacks();
        serverCallbacks.Allocate += OnMultiplayAllocation;
        serverEvents = await multiplayService.SubscribeToServerEventsAsync(serverCallbacks);

        allocationId = await AwaitAllocationID();
        var mmPayload = await GetMatchmakerAllocationPayloadAsync();
        return mmPayload;
    }

    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        Debug.Log($"OnAllocation: {allocation.AllocationId}");
        if (string.IsNullOrEmpty(allocation.AllocationId)) return;
        allocationId = allocation.AllocationId;
    }

    private async Task<string> AwaitAllocationID()
    {
        var config = multiplayService.ServerConfig;
        Debug.Log($"Awaiting Allocation. Server Config is:\n" +
                    $"-ServerID: {config.ServerId}\n" +
                    $"-AllocationID: {config.AllocationId}\n" +
                    $"-Port: {config.Port}\n" +
                    $"-QPort: {config.QueryPort}\n" +
                    $"-logs: { config.ServerLogDirectory}");

        while (string.IsNullOrEmpty(allocationId))
        {
            var configId = config.AllocationId;
            if (!string.IsNullOrEmpty(configId) && string.IsNullOrEmpty(allocationId))
            {
                allocationId = configId;
                break;
            }

            await Task.Delay(100);
        }
        return allocationId;
    }

    private async Task<MatchmakingResults> GetMatchmakerAllocationPayloadAsync()
    {
        try
        {
            var payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log($"{nameof(GetMatchmakerAllocationPayloadAsync)}:\n{modelAsJson}");
            return payloadAllocation;
        }
        catch (Exception ex)
        {
            Debug.Log($"Something went wrong trying to get the matchmaker payload in GetMatchmakerAllocationPayloadAsync: {ex}");
        }

        return null;
    }
#endif
}