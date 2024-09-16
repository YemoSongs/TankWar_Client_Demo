using RoomMsg;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : BasePanel
{
    //开战按钮
    private Button startButton;
    //退出按钮
    private Button closeButton;
    //列表容器
    private Transform content;
    //玩家信息物体
    private GameObject playerObj;



    //初始化
    public override void OnInit()
    {
        skinPath = "RoomPanel";
        layer = PanelManager.Layer.Panel;
    }

    //显示
    public override void OnShow(params object[] para)
    {
        //寻找组件
        startButton = skin.transform.Find("CtrlPanel/StartButton").GetComponent<Button>();
        closeButton = skin.transform.Find("CtrlPanel/CloseButton").GetComponent<Button>();
        content = skin.transform.Find("ListPanel/Scroll View/Viewport/Content");
        playerObj = skin.transform.Find("Player").gameObject;
        //不激活玩家信息
        playerObj.SetActive(false);
        //按钮事件
        startButton.onClick.AddListener(OnStartClick);
        closeButton.onClick.AddListener(OnCloseClick);


        //协议监听
        NetManager.AddMsgListener<MsgGetRoomInfo>(OnMsgGetRoomInfo);
        NetManager.AddMsgListener<MsgLeaveRoom>(OnMsgLeaveRoom);
        NetManager.AddMsgListener<MsgStartBattle>(OnMsgStartBattle);


        Invoke("SendGetRoomInfo", 1);
    }


    private void SendGetRoomInfo()
    {
        //发送查询
        MsgGetRoomInfo msg = new MsgGetRoomInfo();
        NetManager.Send(msg);
    }


    //收到开战协议
    private void OnMsgStartBattle(IExtensible msgBase)
    {
        MsgStartBattle msg = msgBase as MsgStartBattle;
        //开战
        if(msg.Result == 0)
        {
            Close();
        }
        //开战失败
        else
        {
            PanelManager.Open<TipPanel>("开战失败！ 两队至少都需要一名玩家，只有队长可以开始战斗！");
        }

    }

    //收到退出房间协议
    private void OnMsgLeaveRoom(IExtensible msgBase)
    {
        MsgLeaveRoom msg = msgBase as MsgLeaveRoom;
        //成功退出房间
        if(msg.Result == 0)
        {
            PanelManager.Open<TipPanel>("退出房间");
            PanelManager.Open<RoomListPanel>();
            Close();
        }
        //退出房间失败
        else
        {
            PanelManager.Open<TipPanel>("退出房间失败");
        }


    }


    //收到玩家列表协议
    private void OnMsgGetRoomInfo(IExtensible msgBase)
    {
        Debug.Log("Recieve : OnMsgGetRoomInfo");
        MsgGetRoomInfo msg = msgBase as MsgGetRoomInfo;
        //清除玩家列表
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject o = content.GetChild(i).gameObject;
            Destroy(o);
        }
        //重新生成列表
        if(msg.Players == null)
        {
            Debug.Log("收到的房间内的玩家信息为空");
            return;
        }
        Debug.Log("GetRoomInfo:"+msg.Players.Count);

        for (int i = 0; i < msg.Players.Count; i++)
        {
            GeneratePlayerInfo(msg.Players[i]);
        }
    }

    //创建一个玩家信息单元
    private void GeneratePlayerInfo(PlayerInfo playerInfo)
    {
        //创建物体
        GameObject o = Instantiate(playerObj);
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;

        //获取组件
        Transform trans = o.transform;
        TextMeshProUGUI idText = trans.Find("IdText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI campText = trans.Find("CampText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI scoreText = trans.Find("ScoreText").GetComponent<TextMeshProUGUI>();

        //填充信息
        idText.text = playerInfo.Id;
        if(playerInfo.Camp == 1)
        {
            campText.text = "红";
        }
        else
        {
            campText.text = "蓝";
        }

        if(playerInfo.isOwner == 1)
        {
            campText.text = campText.text + "! ";
        }

        scoreText.text = playerInfo.Win + "胜 " + playerInfo.Lost + "负";

    }

    //点击退出按钮
    private void OnCloseClick()
    {
        MsgLeaveRoom msg = new MsgLeaveRoom();
        NetManager.Send(msg);
    }

    //点击开战按钮
    private void OnStartClick()
    {
        MsgStartBattle msg = new MsgStartBattle();
        NetManager.Send(msg);
    }

    //关闭
    public override void OnClose()
    {
        //协议监听
        NetManager.RemoveMsgListener<MsgGetRoomInfo>(OnMsgGetRoomInfo);
        NetManager.RemoveMsgListener<MsgLeaveRoom>(OnMsgLeaveRoom) ;
        NetManager.RemoveMsgListener<MsgStartBattle>(OnMsgStartBattle);

    }


}
