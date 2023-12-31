using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ClientLocal
{
    public int id;
    
    public static readonly int drop_threshold = 120;
    public Dictionary<int, DateTime> pkg_sent_time = new();
    public int ping;
    public World world = new();
    public Dictionary<int, Player.Instruction> unack_inst = new();
    public Player localPlayer = new(0);
    public int last_sent_pkg_id;
    public int currentFrame;
    private int last_ack_frame;

    private bool suppress_correct => MainModule.Instance.SuppressCorrect && id == 2;

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
                world.CopyFrom(packet.content as World);
                if (MainModule.Instance.Lockstep)
                {
                    last_ack_frame = world.frame;
                }
                break;
            case NetworkPacket.Type.Ack:
                int frame = (int)packet.content;
                ping = (int)((DateTime.Now - pkg_sent_time[frame]) / 2).TotalMilliseconds;
                pkg_sent_time.Remove(frame);
                if (!MainModule.Instance.Lockstep)
                    unack_inst.Remove(frame);
                break;
            default:
                throw new InvalidDataException($"invalid packet:{packet.type}");
        }
    }
    
    public void Update()
    {
        if (!MainModule.Instance.Lockstep)
        {
            if (currentFrame - drop_threshold > last_ack_frame)
            {
                last_ack_frame = currentFrame - drop_threshold;
                unack_inst.Remove(last_ack_frame);
            }
            else
            {
                for (int i = last_ack_frame; i < currentFrame; i++)
                {
                    if (unack_inst.ContainsKey(i+1))
                    {
                        break;
                    }
            
                    last_ack_frame++;
                }
            }
        }

        currentFrame++;
        var nextOp = localPlayer.inst.Duplicate();
        var packet = new NetworkPacket
        {
            type = NetworkPacket.Type.Command,
            id = ++last_sent_pkg_id,
            src = id,
            dst = 0,
            content = new Tuple<int, Player.Instruction> (currentFrame, nextOp),
        };
        if(nextOp != null)
            unack_inst[currentFrame] = nextOp;
        pkg_sent_time[currentFrame] = packet.time;
        NetworkManager.Send(packet, suppress_correct ? 4000 : 0);
        
        localPlayer.CopyFrom(world.playerDict[id]);
        
        for (int i = last_ack_frame; i < currentFrame; i++)
        {
            localPlayer.inst = unack_inst.TryGetValue(i + 1, out var instruction) ? instruction.Duplicate() : null;
            localPlayer.Update();
        }

        if (suppress_correct)
        {
            if (localPlayer.CollideWith(world[1]))
            {
                NetworkManager.Release(2, 0);
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