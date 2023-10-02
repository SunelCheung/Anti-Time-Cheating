using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ClientLocal
{
    public int id;
    public bool stop_trigger;
    public bool local_operation = true;
    public DateTime last_sent_pkg_time;
    public int ping;
    public World world = new World();
    public Dictionary<int, Player.Instruction> unack_inst = new Dictionary<int, Player.Instruction>();
    public Player localPlayer = new(0);
    public int last_sent_pkg_id;
    public int currentFrame;
    
    public ClientLocal(int id)
    {
        this.id = id;

        NetworkManager.RegisterCb(id, ProcessPacket);
    }
    
    private void ProcessPacket(NetworkPacket packet)
    {
        if (packet.src != 0)
        {
            return;
        }

        switch (packet.type)
        {
            case NetworkPacket.Type.Command:
                Debug.LogError($"invalid packet:{packet.type}");
                break;
            case NetworkPacket.Type.State:
                var state = packet.content as World;
                if (state?.frame > world.frame)
                {
                    world.CopyFrom(state);
                }
                break;
            case NetworkPacket.Type.Ack:
                // unack_inst[] = null;
                unack_inst.Remove((int)packet.content);
                ping = ((DateTime.Now - last_sent_pkg_time) / 2).Milliseconds;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Update()
    {
        currentFrame++;
        
        var packet = new NetworkPacket
        {
            type = NetworkPacket.Type.Command,
            id = ++last_sent_pkg_id,
            src = id,
            dst = 0,
            content = new Tuple<int, Player.Instruction> (currentFrame, localPlayer.inst.Duplicate()),
        };
        unack_inst[currentFrame] = localPlayer.inst.Duplicate();
        last_sent_pkg_time = packet.time;
        NetworkManager.Send(packet);
        
        if (local_operation || stop_trigger)
        {
            stop_trigger = false;
            localPlayer.CopyFrom(world.playerDict[id]);
            for (int i = localPlayer.currentFrame; i <= currentFrame; i++)
            {
                if (unack_inst.TryGetValue(i, out localPlayer.inst))
                {
                    localPlayer.Update();
                }
            }
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("client");
        sb.Append(id);
        sb.Append("\t ping:");
        sb.Append(ping);
        sb.Append("\t local player:");
        sb.Append(localPlayer);
        sb.Append("\n");
        sb.Append(world);
        return sb.ToString();
    }
}