using System;
using System.Text;
using UnityEngine;

public static class Manager
{
    public static void CopyFrom(this Player.Instruction dst, Player.Instruction src)
    {
        // if (src == dst)
        //     return;
        dst.direction = src.direction;
    }
    
    public static Player.Instruction Duplicate(this Player.Instruction src)
    {
        if (src == null)
            return null;
        var inst = new Player.Instruction();
            // {direction = src.direction};
        inst.CopyFrom(src);
        
        return inst;
    }
    
    public static void CopyFrom(this World dst, World src)
    {
        if (src == dst)
            return;
        foreach (var remote_player in src.playerDict.Values)
        {
            dst.playerDict[remote_player.id].CopyFrom(remote_player);
            dst.playerDict[remote_player.id].currentFrame = src.frame;
        }
        dst.frame = src.frame;
    }
    
    public static void CopyFrom(this Player dst, Player src)
    {
        if (src == dst)
            return;
        
        dst.pos_x = src.pos_x;
        dst.pos_y = src.pos_y;
        dst.speed = src.speed;
        dst.radius = src.radius;
        dst.hp = src.hp;

        dst.inst = src.inst?.Duplicate();
    }
    
    public static double DistanceSqr(this Player src, Player dst)
    {
        if (src == dst)
            return 0;
        return Math.Pow(dst.pos_x - src.pos_x, 2) + Math.Pow(dst.pos_y - src.pos_y, 2);
    }
    
    public static bool CollideWith(this Player src, Player dst)
    {
        if (src == dst)
            return false;
        return src.DistanceSqr(dst) < Math.Pow(src.radius + dst.radius, 2);
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
    public float speed = speed_max;
    public float radius = 0.5f;
    public int hp = 100;
    public Instruction inst = new();

    public int currentFrame = 0;
    
    public class Instruction
    {
        public Direction direction = Direction.None;
        public bool shooting;
    }

    public bool IsDead => hp <= 0;
    
    private Player() { }
    
    public Player(int id)
    {
        this.id = id;
    }

    public void SetDir(Direction dir)
    {
        if (dir == Direction.None && inst == null)
        {
            return;
        }

        inst ??= new Instruction();
        inst.direction = dir;
    }
    
    public void Update()
    {
        currentFrame++;
        if (IsDead || inst == null)
            return;
        speed = Mathf.Clamp(speed, 0, speed_max);
        switch (inst.direction)
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
                // changed = false;
                break;
        }

        pos_x = Mathf.Clamp(pos_x, x_min + radius, x_max - radius);
        pos_y = Mathf.Clamp(pos_y, y_min + radius, y_max - radius);
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
        sb.Append(inst.direction);
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
    