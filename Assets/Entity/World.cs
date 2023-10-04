using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Vector2 = UnityEngine.Vector2;

public class World
{
    public int frame;
    public Dictionary<int, Player> playerDict = new();
    public Player this[int index] => playerDict[index];

    public World()
    {
        var player1 = new Player(1){pos = new Vector2(Player.x_min / 2, Player.y_min / 2)};
        var player2 = new Player(2){pos = new Vector2(Player.x_max / 2, Player.y_max / 2), speed = 1.5f};
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