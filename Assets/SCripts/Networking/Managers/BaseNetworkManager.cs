using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BaseNetworkManager : NetworkManager
{
    public struct ModeratorConnectionMessage : NetworkMessage { }
    public struct ContestantConnectionMessage : NetworkMessage
    {
        public string Code;
    }
    public struct AudienceConnectionMessage : NetworkMessage
    {
        public string Code;
        public string Name;
    }

    public override void Start()
    {
        base.Start();
        //if (DateTime.Now > new DateTime(2022, 4, 10))
        //{
        //    Application.Quit();
        //}
    }

    //public void Update()
    //{
    //    if(Time.time > 3600)
    //    {
    //        Application.Quit();
    //    }
    //}
}
