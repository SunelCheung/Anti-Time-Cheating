using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class MainModule: MonoBehaviour
{
    private static MainModule _instance;
    public MainModule Instance => _instance;
    
    public static InputManager InputManager;
    public static PlayerProxy PlayerProxy;
    public static ClientLocal[] Clients = { new ClientLocal(1), new ClientLocal(2) };
    public static ServerLogic Server = new ServerLogic();
    public static readonly float frameInterval = 0.05f; // 固定间隔为 0.05 秒
    private static float lastFixedTime = 0f;
    private static int _frame = 0;
    public int Frame => _frame;
    // private static float runningTime = 0f;

    public void Awake()
    {
        _instance = this;
        InputManager = gameObject.GetOrAddComponent<InputManager>();
        PlayerProxy = gameObject.GetOrAddComponent<PlayerProxy>();
    }

    public void Update()
    {
        NetworkManager.ProcessPacket();
        // while(true){
        //     if(lastFixedTime < (_frame + 1) * frameInterval)
        //     {
        //         break;
        //     }
        //     if (Time.fixedTime - lastFixedTime < frameInterval)
        //     {
        //         continue;
        //     }
        //     lastFixedTime = Time.fixedTime;
        //     runningTime += lastFixedTime;
        //     _frame += 1;
            if (Time.fixedTime - lastFixedTime >= frameInterval)
            {
                // 执行固定间隔操作
                lastFixedTime = Time.fixedTime;
                foreach (var client in Clients)
                {
                    client.Update();
                }
                Server.Update();
                // Debug.Log(Clients[1]);
            }
        // }
    }
}

