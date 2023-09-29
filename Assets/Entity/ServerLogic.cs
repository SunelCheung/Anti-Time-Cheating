using System;
using UnityEngine;

public class ServerLogic
{
    private static readonly int delay_threshold = 5000;
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
        players[packet_in.src].CopyFromClient(packet_in.instruction);
        change_trigger[packet_in.src] = true;
    }
    
    public void Update()
    {
        foreach (var player in players)
        {
            if(player == null)
                continue;
            player.Update();
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
                packet.instruction.CopyFromServer(player, true);
                NetworkManager.Send(packet);
            }
        }

        if (players[1].CollideWith(players[2]))
        {
            players[1].hp = 0;
        }
    }
}