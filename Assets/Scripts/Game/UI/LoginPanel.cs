using LoginMsg;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LoginPanel : BasePanel
{
    private TMP_InputField idInput;
    private TMP_InputField pwInput;
    private Button loginBtn;
    private Button regBtn;


    //初始化 
    public override void OnInit()
    {
        skinPath = "LoginPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] para)
    {
        //寻找组件
        idInput = skin.transform.Find("idInput").GetComponent<TMP_InputField>();
        pwInput = skin.transform.Find("pwInput").GetComponent<TMP_InputField>();
        loginBtn = skin.transform.Find("LoginBtn").GetComponent<Button>();
        regBtn = skin.transform.Find("RegisterBtn").GetComponent<Button>();

        //监听
        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);

        //网络协议监听
        NetManager.AddMsgListener<MsgLogin>(OnMsgLogin);
        //网络事件监听
        NetManager.AddEventListener(E_NetEvent.ConnectSucc, OnConnectSuss);
        NetManager.AddEventListener(E_NetEvent.ConnectFail, OnConnectFail);
        //连接服务器
        NetManager.Connect("47.120.52.179", 8001);
    }


    public override void OnClose()
    {
        //网络协议监听
        NetManager.RemoveMsgListener<MsgLogin>(OnMsgLogin);


        //网络事件监听
        NetManager.RemoveEventListener(E_NetEvent.ConnectSucc, OnConnectSuss);
        NetManager.RemoveEventListener(E_NetEvent.ConnectFail , OnConnectFail);

    }


    private void OnConnectFail(string err)
    {
        //PanelManager.Open<TipPanel>("OnConnectFail"+err);
        Debug.Log("OnConnectFail");
    }

    private void OnConnectSuss(string err)
    {
        //PanelManager.Open<TipPanel>("OnConnectSucc"+err);
        Debug.Log("OnConnectSucc");
    }

    //收到登录协议
    private void OnMsgLogin(IExtensible msgBase)
    {
        MsgLogin msg = msgBase as MsgLogin;
        if(msg.Result == 0)
        {
            Debug.Log("登录成功");
            //进入游戏

            //添加坦克
            //GameObject tankObj = new GameObject("myTank");
            //CtrlTank ctrlTank = tankObj.AddComponent<CtrlTank>();
            //ctrlTank.Init("tankPrefab");
            ////设置相机
            //tankObj.AddComponent<CameraFollow>();

            //设置id
            GameMain.id = msg.Id;
            //打开房间列表界面
            PanelManager.Open<RoomListPanel>();
            //关闭界面
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("登录失败");
        }

    }

    //当按下注册按钮时
    private void OnRegClick()
    {
        PanelManager.Open<RegisterPanel>();
    }

    //当按下登录按钮
    private void OnLoginClick()
    {
        //用户名密码为空
        if(idInput.text == "" || pwInput.text == "")
        {
            PanelManager.Open<TipPanel>("用户名和密码不能为空");
            return;
        }
        //发送
        MsgLogin msgLogin = new MsgLogin();
        msgLogin.Id = idInput.text;
        msgLogin.Pw = pwInput.text;

        Debug.Log("send loginmsg");
        NetManager.Send(msgLogin);
    }


}
