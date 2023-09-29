using System;
using UnityEngine;

public class PlayerProxy : MonoBehaviour
{
    public Player player;
    private SpriteRenderer sr;
    private void Awake()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        if (player == null)
            return;
        var tf = transform;
        tf.localPosition = new Vector3(player.pos_y, player.pos_x, 0);
        tf.localScale = new Vector3(player.radius * 2, player.radius * 2, 1);
        if (player.IsDead)
        {
            sr.color = Color.grey;
        }
    }
}