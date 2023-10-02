using System;
using System.Text;
using UnityEngine;

public static class PlayerManager
{
    public static Player[] GetTemplate(int id)
    {
        var player1 = new Player(1){pos_x = Player.x_min,pos_y = Player.y_min};
        var player2 = new Player(2){pos_x = Player.x_max,pos_y = Player.y_max};
        Player localPlayer = null;
        if(id != 0)
        {
            localPlayer = new Player(0);
            localPlayer.CopyFrom(id == 1 ? player1 : player2, true);
        }
        Player[] players = {localPlayer, player1, player2 };
        return players;
    }

    public static void CopyFromClient(this Player dst, Player src, bool complete)
    {
        if (src == dst)
            return;
        dst.direction = src.direction;
        dst.speed = src.speed;
        if (complete)
        {
            dst.pos_x = src.pos_x;
            dst.pos_y = src.pos_y;
        }
        // dst.radius = src.radius;
        // dst.hp = src.hp;
    }
    
    public static void CopyFrom(this Player dst, Player src, bool complete)
    {
        if (src == dst)
            return;
        if (complete)
        {
            dst.pos_x = src.pos_x;
            dst.pos_y = src.pos_y;
        }
        dst.direction = src.direction;
        dst.speed = src.speed;
        dst.radius = src.radius;
        dst.hp = src.hp;
    }
    
    public static double DistanceSqr(this Player src, Player dst)
    {
        if (src == dst)
            return 0;
        return Math.Pow(dst.pos_x - src.pos_x, 2) + Math.Pow(dst.pos_y - src.pos_y, 2);
    }
    
    public static bool EqualDir(this Player src, Player dst)
    {
        if (src == dst)
            return true;
        return src.direction == dst.direction && Math.Abs(src.speed - dst.speed) < Player.threshold;
    }
    
    public static bool CollideWith(this Player src, Player dst)
    {
        if (src == dst)
            return false;
        return src.DistanceSqr(dst) < Math.Pow(src.radius + dst.radius, 2);
    }
    
    public static bool TryUpdateTime(this Player src, DateTime time)
    {
        if (src.updateTime < time)
        {
            src.updateTime = time;
            return true;
        }

        return false;
    }
}

public class Player
{
    public static readonly double threshold = 0.1;
    public static readonly float x_max = 5f;
    public static readonly float x_min = -x_max;
    public static readonly float y_max = 5f;
    public static readonly float y_min = -y_max;
    public static readonly float speed_max = 2f;
    
    public int id;
    public float pos_x;
    public float pos_y;
    public float speed;
    public Direction direction;
    public bool shooting;
    public float radius = 0.5f;
    public int hp = 100;

    public bool IsDead => hp <= 0;
    public DateTime updateTime = DateTime.MinValue;
    
    private Player() { }
    
    public Player(int id)
    {
        this.id = id;
    }

    public bool Update()
    {
        if (IsDead)
            return false;
        bool changed = true;
        speed = Mathf.Clamp(speed, 0, speed_max);
        switch (direction)
        {
            case Direction.Up:
                pos_x += speed * MainModule.frameInterval;
                break;
            case Direction.Down:
                pos_x -= speed * MainModule.frameInterval;
                break;
            case Direction.Left:
                pos_y -= speed * MainModule.frameInterval;
                break;
            case Direction.Right:
                pos_y += speed * MainModule.frameInterval;
                break;
            default:
                changed = false;
                break;
        }

        pos_x = Mathf.Clamp(pos_x, x_min + radius, x_max - radius);
        pos_y = Mathf.Clamp(pos_y, y_min + radius, y_max - radius);
        return changed;
    }
    
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("id = ");
        sb.Append(id);
        sb.Append(" x:");
        sb.Append(pos_x.ToString("F2"));
        sb.Append(" y:");
        sb.Append(pos_y.ToString("F2"));
        sb.Append(" ");
        sb.Append(direction);
        sb.Append(" speed:");
        sb.Append(speed.ToString("F2"));
        sb.Append(" hp:");
        sb.Append(hp);
        return sb.ToString();
    }
}
    
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right,
    }
    