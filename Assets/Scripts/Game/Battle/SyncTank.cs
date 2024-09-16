using SyncMsg;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTank : BaseTank
{

    //预测信息， 哪个时间到达哪个位置
    private Vector3 lastPos;        //代表最近一次收到的位置同步协议（MsgSyncTank）的位置和旋转信息
    private Vector3 lastRot;
    private Vector3 forecastPos;    //预测的信息
    private Vector3 forecastRot;
    private float forecastTime;     //最近一次收到的位置同步协议的时间



    public override void Init(string skinPath)
    {
        base.Init(skinPath);
        //不受物理运动影响
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        //初始化预测信息
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
        forecastPos = transform.position;
        forecastRot = transform.eulerAngles;
        forecastTime = Time.time;

    }



    private void Update()
    {
        //更新位置
        ForecastUpdate();
    }

    //更新位置
    private void ForecastUpdate()
    {
        //时间
        float t = (Time.time - forecastTime)/CtrlTank.syncInterval;
        t = Mathf.Clamp01(t);
        //位置
        Vector3 pos = transform.position;
        pos = Vector3.Lerp(pos, forecastPos, t);
        transform.position = pos;
        //旋转
        Quaternion quat = transform.rotation;
        Quaternion forecastQuat = Quaternion.Euler(forecastRot);
        quat = Quaternion.Lerp(quat, forecastQuat, t);
        transform.rotation = quat;
    }

    
    //移动同步
    public void SyncPos(MsgSyncTank msg)
    {
        //预测位置
        Vector3 pos = new Vector3(msg.X,msg.Y,msg.Z);
        Vector3 rot = new Vector3(msg.Ex,msg.Ey,msg.Ez);
        forecastPos = pos + 2 * (pos - lastPos);
        forecastRot = rot + 2 * (rot - lastRot);
        //更新
        lastPos = pos;
        lastRot = rot;
        forecastTime = Time.time;
        //炮塔
        Vector3 le = turret.localEulerAngles;
        le.y = msg.turretY;
        turret.localEulerAngles = le;
    }


    //开火
    public void SyncFire(MsgFire msg)
    {
        Bullet bullet = Fire();
        //更新坐标
        Vector3 pos = new Vector3(msg.X, msg.Y,msg.Z);
        Vector3 rot = new Vector3(msg.Ex, msg.Ey,msg.Ez);

        bullet.transform.position = pos;
        bullet.transform.eulerAngles = rot;
    }
}
