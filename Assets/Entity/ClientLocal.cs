using System.Text;

public class ClientLocal
{
    public int id;
    public bool stop_trigger;
    public bool local_operation;
    public Player[] players;
    public Player localPlayer => players[0];
    public static readonly double threshold = 0.1;
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
        players[instruction.id].CopyFromServer(instruction, true);
        if (instruction.id == id)
        {
            localPlayer.CopyFromServer(instruction, instruction.DistanceSqr(localPlayer) > threshold);
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
            localPlayer.CopyTo(packet.instruction);
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
        sb.Append("\n");
        foreach (var player in players)
        {
            sb.Append(player);
            sb.Append("\n");
        }
        return sb.ToString();
    }
}