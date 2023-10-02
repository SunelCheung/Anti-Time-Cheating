using System;
using System.Collections.Generic;
using UnityEngine;

    public class InputManager : MonoBehaviour
    {
        private Player[] LocalPlayer = new Player [2];
        private Dictionary<int, bool> IsDown = new(){{0,true},{1,true}}; 
        public void Start()
        {
            LocalPlayer[0] = MainModule.Clients[0].localPlayer;
            LocalPlayer[1] = MainModule.Clients[1].localPlayer;
        }

        public void Update()
        {
            IsDown[0] = true;
            IsDown[1] = true;
            
            if (Input.GetKey(KeyCode.W))
            {
                LocalPlayer[0].SetDir(Direction.Up);
            }
            else if(Input.GetKey(KeyCode.S))
            {
                LocalPlayer[0].SetDir(Direction.Down);
            }
            else if(Input.GetKey(KeyCode.A))
            {
                LocalPlayer[0].SetDir(Direction.Left);
            }
            else if(Input.GetKey(KeyCode.D))
            {
                LocalPlayer[0].SetDir(Direction.Right);
            }
            else
            {
                LocalPlayer[0].SetDir(Direction.None);
                IsDown[0] = false;
            }
            
            if(Input.GetKey(KeyCode.UpArrow))
            {
                LocalPlayer[1].SetDir(Direction.Up);
            }
            else if(Input.GetKey(KeyCode.DownArrow))
            {
                LocalPlayer[1].SetDir(Direction.Down);
            }
            else if(Input.GetKey(KeyCode.LeftArrow))
            {
                LocalPlayer[1].SetDir(Direction.Left);
            }
            else if(Input.GetKey(KeyCode.RightArrow))
            {
                LocalPlayer[1].SetDir(Direction.Right);
            }
            else
            {
                LocalPlayer[1].SetDir(Direction.None);
                IsDown[1] = false;
            }

            for (int i = 0; i < MainModule.Clients.Length; i++)
            {
                if (!IsDown[i] && MainModule.Clients[i].local_operation != IsDown[i])
                {
                    MainModule.Clients[i].stop_trigger = true;
                }
                MainModule.Clients[i].local_operation = IsDown[i];
            }
            
        }
    }