using SyncMsg;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlTank : BaseTank
{

    //上一次发送同步信息的时间
    private float lastSendSyncTime = 0;
    //同步帧率
    public static float syncInterval = 1f;

    void Update()
    {
        //移动控制
        MoveUpdate();
        //炮塔控制
        TurretUpdate();
        //开炮
        FireUpdate();
        //发送同步信息
        SyncUpdate();

    }

    //发送同步信息
    private void SyncUpdate()
    {
        //已经死亡 
        if (IsDie())
        {
            return;
        }
        if (isOver)
            return;

        //时间间隔判断
        if (Time.time - lastSendSyncTime < syncInterval)
            return;
        lastSendSyncTime = Time.time;
        //发送同步协议
        MsgSyncTank msg = new MsgSyncTank();
        msg.X = transform.position.x;
        msg.Y = transform.position.y;
        msg.Z = transform.position.z;
        msg.Ex = transform.eulerAngles.x;
        msg.Ey = transform.eulerAngles.y;
        msg.Ez = transform.eulerAngles.z;
        msg.turretY = turret.localEulerAngles.y;
        NetManager.Send(msg);
    }

    //移动控制
    public void MoveUpdate()
    {
        //已经死亡 
        if (IsDie())
        {
            return;
        }
        if (isOver)
            return;
        //旋转
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0,x*steer*Time.deltaTime,0);

        //前进后退
        float y = Input.GetAxis("Vertical");
        Vector3 s = y*transform.forward* speed* Time.deltaTime;
        transform.transform.position += s;

    }
    
    //炮塔控制
    public void TurretUpdate()
    {
        //已经死亡 
        if (IsDie())
        {
            return;
        }
        if (isOver)
            return;
        //获取轴向
        float axis = 0;
        if(Input.GetKey(KeyCode.Q))
        {
            axis = -1;
        }
        else if(Input.GetKey(KeyCode.E))
        {
            axis = 1;
        }
        //旋转角度
        Vector3 le = turret.localEulerAngles;
        le.y += axis * Time.deltaTime * turretSpeed;
        turret.localEulerAngles = le;
    }


    //开炮
    public void FireUpdate()
    {
        //已经死亡 
        if (IsDie())
        {
            return;
        }
        if (isOver)
            return;
        //按键判断
        if (!Input.GetKey(KeyCode.Space))
        {
            return;
        }
        //cd 是否判断
        if(Time.time - lastFireTime <fireCd)
        {
            return;
        }
        //发射
        Bullet bullet = Fire();
        //发送同步协议
        MsgFire msg = new MsgFire();
        msg.X = bullet.transform.position.x;
        msg.Y = bullet.transform.position.y;
        msg.Z = bullet.transform.position.z;
        msg.Ex = bullet.transform.eulerAngles.x;
        msg.Ey = bullet.transform.eulerAngles.y;
        msg.Ez = bullet.transform.eulerAngles.z;
        NetManager.Send(msg);
    }


}
