using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class MainModule: MonoBehaviour
{
    public static readonly float frameInterval = 0.05f;
    private static MainModule _instance;
    public MainModule Instance => _instance;
    
    public static InputManager InputManager;
    public static PlayerProxy PlayerProxy;
    public static ClientLocal[] Clients = { new(1), new(2) };
    public static ServerLogic Server = new();
    private static float curFixedTime;
    private static float lastUpdateTime;

    public void Awake()
    {
        _instance = this;
        InputManager = gameObject.GetOrAddComponent<InputManager>();
        PlayerProxy = gameObject.GetOrAddComponent<PlayerProxy>();
    }
    
    public void Update()
    {
        NetworkManager.ProcessPacket();
        
        if (curFixedTime - lastUpdateTime >= frameInterval)
        {
            foreach (var client in Clients)
            {
                client.Update();
            }
            Server.Update();
            lastUpdateTime += frameInterval;
        }

        curFixedTime += Time.deltaTime;
    }

    public void FixedUpdate()
    {
        // curFixedTime += Time.fixedDeltaTime;
    }
}

