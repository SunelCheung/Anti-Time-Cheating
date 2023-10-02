using System.Collections.Generic;
using System.Text;

public class World
{
    public int frame;
    public Dictionary<int, Player> playerDict = new();
    public Player this[int index] => playerDict[index];

    public World()
    {
        var player1 = new Player(1){pos_x = Player.x_min,pos_y = Player.y_min};
        var player2 = new Player(2){pos_x = Player.x_max,pos_y = Player.y_max};
        playerDict = new Dictionary<int, Player> { { 1, player1 }, { 2, player2 } };
    }

    public void Update()
    {
        frame++;
        foreach (var player in playerDict.Values)
        {
            player.Update();
        }
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("frame:");
        sb.Append(frame);
        sb.Append("\n");
        foreach (var player in playerDict.Values)
        {
            sb.Append(player);
            sb.Append("\n");
        }
        return sb.ToString();
    }
}