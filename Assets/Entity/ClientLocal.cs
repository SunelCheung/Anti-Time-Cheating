using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ClientLocal
{
    public int id;
    public bool stop_trigger;
    public bool local_operation = true;
    public DateTime last_sent_pkg_time;
    public int ping;
    public World world = new();
    public Dictionary<int, Player.Instruction> unack_inst = new();
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
            case NetworkPacket.Type.State:
                Refresh(packet.content as World);
                break;
            case NetworkPacket.Type.Ack:
                ping = (int)((DateTime.Now - last_sent_pkg_time) / 2).TotalMilliseconds;
                break;
            default:
                throw new InvalidDataException($"invalid packet:{packet.type}");
        }
    }

    public void Refresh(World remote_world)
    {
        if (remote_world.frame > world.frame)
        {
            for (int i = world.frame; i < remote_world.frame; i++)
            {
                unack_inst.Remove(i);
            }
            world.CopyFrom(remote_world);
        }
    }
    
    public void Update()
    {
        currentFrame++;
        var nextOp = localPlayer.inst.Duplicate();
        var packet = new NetworkPacket
        {
            type = NetworkPacket.Type.Command,
            id = ++last_sent_pkg_id,
            src = id,
            dst = 0,
            content = new Tuple<int, Player.Instruction> (currentFrame+1, nextOp),
        };
        if(nextOp != null)
            unack_inst[currentFrame + 1] = nextOp;
        last_sent_pkg_time = packet.time;
        NetworkManager.Send(packet);
        
        if (local_operation || stop_trigger)
        {
            stop_trigger = false;
            localPlayer.CopyFrom(world.playerDict[id]);
            
            for (int i = localPlayer.frame; i <= currentFrame; i++)
            {
                if (unack_inst.TryGetValue(i + 1, out var instruction))
                {
                    localPlayer.inst = instruction.Duplicate();
                }
                localPlayer.Update();
                // if(id==2)
                //     Debug.LogError($"{currentFrame}  {localPlayer.frame}  {localPlayer}");
            }
        }
        else if (unack_inst.Count == 0)
        {
            localPlayer.CopyFrom(world.playerDict[id]);
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