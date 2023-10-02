using System;
using System.Text;

public class ClientLocal
{
    public int id;
    public bool stop_trigger;
    public bool local_operation;
    public int ping;
    public Player[] players;
    public Player localPlayer => players[0];
    public ClientLocal(int id)
    {
        this.id = id;
        players = PlayerManager.GetTemplate(id);
        NetworkManager.RegisterCb(id, ProcessPacket);
    }
    
    private void ProcessPacket(NetworkPacket packet)
    {
        if (packet.src != 0)
        {
            return;
        }
        
        var instruction = packet.instruction;
        if (players[instruction.id].TryUpdateTime(packet.time))
        {
            players[instruction.id].CopyFrom(instruction, true);
            ping = (DateTime.Now - packet.time).Milliseconds;
        }
        if (instruction.id == id)
        {
            if (!instruction.EqualDir(localPlayer))
            {
                packet = new NetworkPacket
                {
                    src = id,
                    dst = 0,
                    instruction = new Player(id),
                };
                packet.instruction.CopyFrom(localPlayer, true);
                NetworkManager.Send(packet);
            }

            if (localPlayer.TryUpdateTime(packet.time))
            {
                localPlayer.CopyFrom(instruction, false);
                // localPlayer.CopyFrom(instruction, instruction.DistanceSqr(localPlayer) > Player.threshold);
            }
        }
    }

    public void Update()
    {
        localPlayer.speed = localPlayer.direction == Direction.None ? 0 : Player.speed_max;
        if (local_operation || stop_trigger)
        {
            var packet = new NetworkPacket
            {
                src = id,
                dst = 0,
                instruction = new Player(id),
            };
            packet.instruction.CopyFrom(localPlayer, true);
            NetworkManager.Send(packet);
            stop_trigger = false;
        }
        foreach (var player in players)
        {
            player.Update();
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("client");
        sb.Append(id);
        sb.Append("\t ping:");
        sb.Append(ping);
        sb.Append("\n");
        foreach (var player in players)
        {
            sb.Append(player);
            sb.Append("\n");
        }
        return sb.ToString();
    }
}