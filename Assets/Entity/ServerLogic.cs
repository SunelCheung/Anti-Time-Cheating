using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ServerLogic
{
    private static readonly int max_acc_delay_ms = 5000;
    private static readonly int min_window_ms = 100;
    private static readonly int min_window_size = (int)(min_window_ms *1.0f / MainModule.frameInterval / 1000);
    private static readonly int max_window_size = (int)(max_acc_delay_ms*1.0f / MainModule.frameInterval / 1000);
    public World world = new();

    public int realtimeFrame;
    public int last_sent_package_id;

    public Dictionary<int, Player.Instruction>[] unExecInst = new[]
        { null, new Dictionary<int, Player.Instruction>(), new Dictionary<int, Player.Instruction>()};
    public int[] pings = { 0, 0 ,0};
    
    public ServerLogic()
    {
        NetworkManager.RegisterCb(0, ProcessPacket);
    }
    
    private void ProcessPacket(NetworkPacket packet)
    {
        if (packet.src == 0)
        {
            Debug.LogError($"invalid packet: feign to others");
            return;
        }

        var timeDelta = (DateTime.Now - packet.time).Milliseconds;
        if (timeDelta < 0 || timeDelta > max_acc_delay_ms)
        {
            Debug.Log($"invalid packet: {timeDelta} ms");
            return;
        }

        pings[packet.src] = timeDelta;
        var tuple = packet.content as Tuple<int, Player.Instruction>;
        unExecInst[packet.src][tuple.Item1] = tuple.Item2;
        var ack_packet = new NetworkPacket
        {
            id = ++last_sent_package_id,
            src = 0,
            dst = packet.src,
            type = NetworkPacket.Type.Ack,
            content = tuple.Item1,
        };
        NetworkManager.Send(ack_packet);

        // var player = players[packet.src];
        // if (player.TryUpdateTime(packet.time))
        // {
            // player.CopyFromClient(packet_in.instruction, 
            //     packet_in.instruction.DistanceSqr(player) < Mathf.Pow(jitter_threshold / 1000f * player.speed,2));

        //     player.CopyFromClient(packet.instruction, true);
        //     change_trigger[packet.src] = true;
        // }
    }
    
    public void Update()
    {
        bool update = false;
        realtimeFrame++;
        for (int i = world.frame; i < realtimeFrame - min_window_size; i++)
        {
            bool instArrive = true;
            if (realtimeFrame - i < max_window_size)
            {
                foreach (var player in world.playerDict.Values)
                {
                    instArrive &= unExecInst[player.id].ContainsKey(i);
                }
            }

            if (instArrive)
            {
                foreach (var player in world.playerDict.Values)
                {
                    player.inst = unExecInst[player.id][i];
                    unExecInst[player.id].Remove(i);
                }
                world.Update();
                update = true;
            }
        }

        if (update)
        {
            foreach (var pair in world.playerDict)
            {
                var packet = new NetworkPacket
                {
                    id = ++last_sent_package_id,
                    src = 0,
                    dst = pair.Key,
                    type = NetworkPacket.Type.State,
                    content = world, // Note: this object can't be modified later!!
                };
                NetworkManager.Send(packet);
            }
        }
        
        if (world[1].CollideWith(world[2]))
        {
            world[1].hp = 0;
        }
        //
        // foreach (var player in players)
        // {
        //     if(player == null)
        //         continue;
        //     if (player.Update())
        //     {
        //         change_trigger[player.id] = true;
        //     }
        //     if (!change_trigger[player.id])
        //     {
        //         continue;
        //     }
        //
        //     foreach (var client in MainModule.Clients)
        //     {
        //         var packet = new NetworkPacket
        //         {
        //             src = 0,
        //             dst = client.id,
        //             instruction = new Player(player.id),
        //         };
        //         packet.instruction.CopyFrom(player, true);
        //         NetworkManager.Send(packet);
        //     }
        //     change_trigger[player.id] = false;
        // }
    }
    
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("server realtime_frame:");
        sb.Append(realtimeFrame);
        sb.Append("\t ping:");
        sb.Append(pings[1]);
        sb.Append(" ");
        sb.Append(pings[2]);
        sb.Append("\n");
        sb.Append(world);
        return sb.ToString();
    }
}