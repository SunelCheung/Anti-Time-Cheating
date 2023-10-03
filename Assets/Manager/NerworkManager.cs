using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkPacket
{
    public int id;
    public Type type;
    public DateTime time = DateTime.Now;
    public int src;
    public int dst;
    public object content;
    
    public enum Type
    {
        Command,
        State,
        Ack,
    }
}

public static class NetworkManager
{
    public static int delayMin = 1;
    public static int delayMax = 50;
    private static System.Random random = new();
    private static Dictionary<int, Action<NetworkPacket>> callback = new(){{0, null}, {1, null}, {2, null}};

    private static ConcurrentQueue<NetworkPacket> packetQueue = new();

    public static void Send(NetworkPacket packet)
    {
        ThreadPool.QueueUserWorkItem(async state =>
        {
            await SendPacket(packet);
        });
    }
    
    public static async Task SendPacket(NetworkPacket packet)
    {
        if (packet.dst != 0 && packet.src != 0)
        {
            Debug.LogError("invalid operation!");
            return;
        }
        await Task.Delay(random.Next(delayMin,delayMax));
        packetQueue.Enqueue(packet);
    }

    public static void RegisterCb(int id, Action<NetworkPacket> cb)
    {
        callback[id] += cb;
    }

    public static void ProcessPacket()
    {
        while (packetQueue.TryDequeue(out var packet))
        {
            callback[packet.dst](packet);
        }
    }
}

