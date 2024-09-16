using BattleMsg;
using SyncMsg;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager 
{

    //战场中的坦克
    public static Dictionary<string,BaseTank> tanks = new Dictionary<string, BaseTank>();

    //初始化
    public static void Init()
    {
        //添加监听
        NetManager.AddMsgListener<MsgEnterBattle>(OnMsgEnterBattle);
        NetManager.AddMsgListener<MsgBattleResult>(OnMsgBattleResult);
        NetManager.AddMsgListener<MsgLeaveBattle>(OnMsgLeaveBattle);

        NetManager.AddMsgListener<MsgSyncTank>(OnMsgSyncTank);
        NetManager.AddMsgListener<MsgFire>(OnMsgFire);
        NetManager.AddMsgListener<MsgHit>(OnMsgHit);

    }

    // 收到同步协议
    private static void OnMsgSyncTank(IExtensible msgBase)
    {
        MsgSyncTank msg = msgBase as MsgSyncTank;
        //不能同步自己
        if (msg.Id == GameMain.id)
            return;
        //查找坦克
        SyncTank tank = (SyncTank)GetTank(msg.Id);
        if (tank == null)
            return;
        //移动同步
        tank.SyncPos(msg);

    }

    //收到开火协议
    private static void OnMsgFire(IExtensible msgBase)
    {
        MsgFire msg = msgBase as MsgFire;
        //不能同步自己
        if(msg.Id == GameMain.id)
            return;
        //查找坦克
        SyncTank tank = (SyncTank)GetTank(msg.Id);
        if(tank == null) return;
        //开火
        tank.SyncFire(msg);
    }

    //收到击中协议
    private static void OnMsgHit(IExtensible msgBase)
    {
        MsgHit msg = msgBase as MsgHit;
        //查找坦克
        BaseTank tank = GetTank(msg.targetId);
        if (tank == null) return;
        //被击中
        tank.Attacked(msg.Damage);

        Debug.Log($"击中协议  id:{msg.Id},hp:{msg.Hp},Damage:{msg.Damage}");
    }


    //添加坦克
    public static void AddTank(string id, BaseTank tank)
    {
        tanks.Add(id, tank);
    }

    //删除坦克
    public static void RemoveTank(string id)
    {
        tanks.Remove(id);
    }


    //获取坦克
    public static BaseTank GetTank(string id)
    {
        if(tanks.ContainsKey(id))
            return tanks[id];
        return null;
    }

    //获取玩家控制的坦克
    public static BaseTank GetCtrlTank()
    {
        return GetTank(GameMain.id);
    }


    //重置战场
    public static void Reset()
    {
        //场景
        foreach(BaseTank tank in tanks.Values)
        {
            MonoBehaviour.Destroy(tank.gameObject);
        }

        //列表
        tanks.Clear();
    }

    



    //玩家离开协议回调
    private static void OnMsgLeaveBattle(IExtensible msgBase)
    {
        MsgLeaveBattle msg = msgBase as MsgLeaveBattle;
        //查找坦克
        BaseTank tank = GetTank(msg.Id);
        if (tank == null) return;
        //删除坦克
        RemoveTank(msg.Id);
        MonoBehaviour.Destroy(tank.gameObject);
    }

    //战斗结果协议回调
    private static void OnMsgBattleResult(IExtensible msgBase)
    {
        MsgBattleResult msg = msgBase as MsgBattleResult;
        //判断显示胜利还是失败
        bool isWin = false;
        BaseTank tank = GetCtrlTank();
        if(tank != null&&tank.camp == msg.winCamp)
        {
            isWin = true;
        }
        tank.isOver = true;
        //显示界面
        PanelManager.Open<ResultPanel>(isWin);
    }

    //进入战场协议回调
    private static void OnMsgEnterBattle(IExtensible msgBase)
    {
        MsgEnterBattle msg = msgBase as MsgEnterBattle;
        MsgEnterBattle(msg);
    }

    //开始战斗
    private static void MsgEnterBattle(MsgEnterBattle msg)
    {
        //重置
        BattleManager.Reset();
        //关闭界面
        PanelManager.Close("RoomPanel");
        PanelManager.Close("ResultPanel");
        //产生坦克
        for (int i = 0;i<msg.Tanks.Count;i++)
        {
            GenerateTank(msg.Tanks[i]);
        }
    }

    //产生坦克
    public static void GenerateTank(TankInfo tankInfo)
    {
        //GameObject
        string objName = "Tank_" + tankInfo.Id;
        GameObject tankObj = new GameObject(objName);

        //AddComponent
        BaseTank tank = null;
        if(tankInfo.Id == GameMain.id)
        {
            tank = tankObj.AddComponent<CtrlTank>();
        }
        else
        {
            tank = tankObj.AddComponent<SyncTank>();
        }

        //camera
        if(tankInfo.Id == GameMain.id)
        {
            CameraFollow cf = tankObj.AddComponent<CameraFollow>();
        }

        //属性
        tank.camp = tankInfo.Camp;
        tank.id = tankInfo.Id;
        tank.hp = tankInfo.Hp;
        //pos rotation
        Vector3 pos = new Vector3(tankInfo.X,tankInfo.Y,tankInfo.Z); 
        Vector3 rot = new Vector3(tankInfo.Ex,tankInfo.Ey,tankInfo.Ez);
        tank.transform.position = pos;
        tank.transform.eulerAngles = rot;
        //init
        if(tankInfo.Camp == 1)
        {
            tank.Init("tankPrefab");
        }
        else
        {
            tank.Init("tankPrefab2");
        }
        tank.isOver = false;
        //列表
        AddTank(tankInfo.Id, tank);

    }
}
