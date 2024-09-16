using RoomMsg;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : BasePanel
{
    //账号文本
    private TextMeshProUGUI idText;
    //战绩文本
    private TextMeshProUGUI scoreText;
    //创建房间按钮
    private Button creatButton;
    //刷新列表按钮
    private Button reflashButton;
    //列表容器
    private Transform content;
    //房间物体
    private GameObject roomObj;


    //初始化
    public override void OnInit()
    {
        skinPath = "RoomListPanel";
        layer = PanelManager.Layer.Panel;
    }
    //显示
    public override void OnShow(params object[] para)
    {
        //寻找组件
        idText = skin.transform.Find("InfoPanel/IdText").GetComponent<TextMeshProUGUI>();
        scoreText = skin.transform.Find("InfoPanel/ScoreText").GetComponent<TextMeshProUGUI>();
        creatButton = skin.transform.Find("CtrlPanel/CreateButton").GetComponent<Button>();
        reflashButton = skin.transform.Find("CtrlPanel/ReflashButton").GetComponent<Button>();
        content = skin.transform.Find("ListPanel/Scroll View/Viewport/Content").GetComponent<Transform>();
        roomObj = skin.transform.Find("Room").gameObject;
        //按钮事件
        creatButton.onClick.AddListener(OnCreateClick);
        reflashButton.onClick.AddListener(OnReflashClick);
        //不激活房间
        roomObj.SetActive(false);
        //显示id
        idText.text = GameMain.id;

        //协议监听
        NetManager.AddMsgListener<MsgGetAchieve>(OnMsgGetAchieve);
        NetManager.AddMsgListener<MsgGetRoomList>(OnMsgGetRoomList);
        NetManager.AddMsgListener<MsgCreateRoom>(OnMsgCreateRoom);
        NetManager.AddMsgListener<MsgEnterRoom>(OnMsgEnterRoom);

        //发送查询
        MsgGetAchieve msgGetAchieve = new MsgGetAchieve();
        NetManager.Send(msgGetAchieve);
        MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
        NetManager.Send(msgGetRoomList);

    }

    //收到进入房间协议
    private void OnMsgEnterRoom(IExtensible msgBase)
    {
        MsgEnterRoom msg = msgBase as MsgEnterRoom;
        //成功进入房间
        if(msg.Result == 0)
        {
            PanelManager.Open<RoomPanel>();
            Close();
        }
        //进入房间失败
        else
        {
            PanelManager.Open<TipPanel>("进入房间失败");
        }
    }


    //收到新建房间协议
    private void OnMsgCreateRoom(IExtensible msgBase)
    {
        MsgCreateRoom msg = msgBase as MsgCreateRoom;
        //成功创建房间
        if(msg.Result == 0)
        {
            PanelManager.Open<TipPanel>("创建成功");
            PanelManager.Open<RoomPanel>();
            Close() ;
        }
        //创建房间失败
        else
        {
            PanelManager.Open<TipPanel>("创建房间失败");
        }
    }

    //收到房间列表协议
    private void OnMsgGetRoomList(IExtensible msgBase)
    {
        MsgGetRoomList msg = msgBase as MsgGetRoomList;
        //清除房间列表
        for(int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject o = content.GetChild(i).gameObject;
            Destroy(o);
        }
        //如果没有房间，不需要进一步处理
        if(msg.Rooms == null)
        {
            return;
        }

        Debug.Log("Room Count:"+msg.Rooms.Count);
        for(int i = 0; i<msg.Rooms.Count; i++)
        {
            GenerateRoom(msg.Rooms[i]);
        }


    }


    //创建一个房间单元
    private void GenerateRoom(RoomInfo roomInfo)
    {
        //创建物体
        GameObject o = Instantiate(roomObj);
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;

        //获取组件
        Transform trans = o.transform;
        TextMeshProUGUI idText = trans.Find("IdText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI countText = trans.Find("CountText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statusText = trans.Find("StatusText").GetComponent<TextMeshProUGUI>();
        Button btn = trans.Find("JoinButton").GetComponent<Button>();

        //填充信息
        idText.text = roomInfo.Id.ToString();
        countText.text = roomInfo.Count.ToString();
        if(roomInfo.Status == 0)
        {
            statusText.text = "准备中";
        }
        else
        {
            statusText.text = "战斗中";
        }

        //按钮事件
        btn.name = idText.text;
        btn.onClick.AddListener(delegate ()
        {
            OnJoinClick(btn.name);
        });
    }

    //点击加入房间按钮
    private void OnJoinClick(string name)
    {
        MsgEnterRoom msg = new MsgEnterRoom();
        msg.Id = int.Parse(name);
        NetManager.Send(msg);
    }

    //收到成绩查询协议
    private void OnMsgGetAchieve(IExtensible msgBase)
    {
        MsgGetAchieve msg = msgBase as MsgGetAchieve;
        scoreText.text = msg.Win + "胜 " + msg.Lost + "负";
    }

    private void OnReflashClick()
    {
        MsgGetRoomList msg = new MsgGetRoomList();
        NetManager.Send(msg);
    }

    //点击新建房间按钮
    private void OnCreateClick()
    {
        MsgCreateRoom msg = new MsgCreateRoom();
        NetManager.Send(msg);
    }

    //关闭
    public override void OnClose()
    {
        //协议监听
        NetManager.RemoveMsgListener<MsgGetAchieve>(OnMsgGetAchieve);
        NetManager.RemoveMsgListener<MsgGetRoomList>(OnMsgGetRoomList);
        NetManager.RemoveMsgListener<MsgCreateRoom>(OnMsgCreateRoom) ;
        NetManager.RemoveMsgListener<MsgEnterRoom>(OnMsgEnterRoom) ;
    }
}
