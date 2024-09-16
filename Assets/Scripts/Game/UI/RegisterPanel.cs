using LoginMsg;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : BasePanel
{
    //账号输入框 
    private TMP_InputField idInput;
    //密码输入框 
    private TMP_InputField pwInput;
    //重复输入框 
    private TMP_InputField repInput;
    //注册按钮 
    private Button regBtn;
    //关闭按钮 
    private Button closeBtn;

    //初始化 
    public override void OnInit()
    {
        skinPath = "RegisterPanel";
        layer = PanelManager.Layer.Panel;
    }

    //显示 
    public override void OnShow(params object[] args)
    {
        //寻找组件 
        idInput = skin.transform.Find("IdInput").GetComponent<TMP_InputField>();
        pwInput = skin.transform.Find("PwInput").GetComponent<TMP_InputField>();
        repInput = skin.transform.Find("RepInput").GetComponent<TMP_InputField>();
        regBtn = skin.transform.Find("RegisterBtn").GetComponent<Button>();
        closeBtn = skin.transform.Find("CloseBtn").GetComponent<Button>();
        //监听 
        regBtn.onClick.AddListener(OnRegClick);
        closeBtn.onClick.AddListener(OnCloseClick);
        //网络协议监听 
        NetManager.AddMsgListener<MsgRegister>( OnMsgRegister);
    }

    //收到注册协议
    private void OnMsgRegister(IExtensible msgBase)
    {
        MsgRegister msg = (MsgRegister)msgBase;
        if(msg.Result == 0)
        {
            Debug.Log("注册成功");
            //提示
            PanelManager.Open<TipPanel>("注册成功");
            //关闭界面
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("注册失败");
        }
    }

    //当按下关闭按钮
    private void OnCloseClick()
    {
        Close();
    }

    //当按下注册按钮
    private void OnRegClick()
    {
        //用户名密码为空
        if(idInput.text == "" || pwInput.text == "")
        {
            PanelManager.Open<TipPanel>("用户名和密码不能为空");
            return;
        }

        //两次密码不同
        if(repInput.text != pwInput.text)
        {
            PanelManager.Open<TipPanel>("两次输入的密码不同");
            return;
        }

        //发送
        MsgRegister msgReg = new MsgRegister();
        msgReg.Id = idInput.text;
        msgReg.Pw = pwInput.text;
        NetManager.Send(msgReg);
        Debug.Log("send loginmsg");
    }


 

    //关闭 
    public override void OnClose()
    {
        //网络协议监听 
        NetManager.RemoveMsgListener<MsgLogin>(OnMsgRegister);
    }
    


}
