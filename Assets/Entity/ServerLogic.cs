using System;
using System.Text;
using UnityEngine;

public class ServerLogic
{
    private static readonly int delay_threshold = 5000;
    private static readonly int jitter_threshold = 150;
    public Player[] players;
    public bool[] change_trigger;
    
    
    public ServerLogic()
    {
        players = PlayerManager.GetTemplate(0);
        change_trigger = new bool[players.Length];
        NetworkManager.RegisterCb(0, ProcessPacket);
    }
    
    private void ProcessPacket(NetworkPacket packet_in)
    {
        if (packet_in.src == 0 || packet_in.src != packet_in.instruction.id)
        {
            Debug.LogError($"invalid packet: feign to others");
            return;
        }

        var timeDelta = (DateTime.Now - packet_in.time).Milliseconds;
        if (timeDelta < 0 || timeDelta > delay_threshold)
        {
            Debug.Log($"invalid packet: {timeDelta} ms");
            return;
        }
        var player = players[packet_in.src];
        if (player.TryUpdateTime(packet_in.time))
        {
            // player.CopyFromClient(packet_in.instruction, 
            //     packet_in.instruction.DistanceSqr(player) < Mathf.Pow(jitter_threshold / 1000f * player.speed,2));

            player.CopyFromClient(packet_in.instruction, true);
            change_trigger[packet_in.src] = true;
        }
    }
    
    public void Update()
    {
        if (players[1].CollideWith(players[2]))
        {
            players[1].hp = 0;
            change_trigger[1] = true;
        }
        
        foreach (var player in players)
        {
            if(player == null)
                continue;
            if (player.Update())
            {
                change_trigger[player.id] = true;
            }
            if (!change_trigger[player.id])
            {
                continue;
            }

            foreach (var client in MainModule.Clients)
            {
                var packet = new NetworkPacket
                {
                    src = 0,
                    dst = client.id,
                    instruction = new Player(player.id),
                };
                packet.instruction.CopyFrom(player, true);
                NetworkManager.Send(packet);
            }
            change_trigger[player.id] = false;
        }
    }
    
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("server");
        // sb.Append("\t ping:");
        // sb.Append(ping);
        sb.Append("\n");
        foreach (var player in players)
        {
            sb.Append(player);
            sb.Append("\n");
        }
        return sb.ToString();
    }
}