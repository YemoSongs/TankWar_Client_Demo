using LoginMsg;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameMain : MonoBehaviour
{
    public static string id = "";

    private void Start()
    {
        //网络监听
        NetManager.AddEventListener(E_NetEvent.Close, OnConnectClose);
        NetManager.AddMsgListener<MsgKick>(OnMsgKick);
        //初始化
        PanelManager.Init();
        BattleManager.Init();

        //打开登录面板
        PanelManager.Open<LoginPanel>();

    }

    //被踢下线
    private void OnMsgKick(IExtensible msgBase)
    {

        // 定义委托
        UnityEngine.Events.UnityAction okAction = () =>
        {
            PanelManager.Open<LoginPanel>();
        };

        PanelManager.Open<TipPanel>("被踢下线", okAction);

    }



    //关闭连接
    private void OnConnectClose(string err)
    {
        Debug.Log("断开连接");
    }


    private void Update()
    {
        NetManager.Update();
    }


    private void OnDestroy()
    {
        NetManager.Close();
    }
}
