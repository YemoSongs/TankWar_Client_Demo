using SyncMsg;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //移动速度
    public float speed = 100f;
    //发射者
    public BaseTank tank;

    //炮弹模型
    private GameObject skin;
    //物理
    Rigidbody rb;
    //初始化
    public void Init()
    {
        //皮肤
        GameObject skinRes = ResManager.LoadPrefab("bulletPrefab");
        skin = Instantiate(skinRes);
        skin.transform.SetParent(transform, false);
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;
        //物理
        rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        //rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        Destroy(gameObject,5);
    }

    private void Update()
    {
        //向前移动
        transform.position += transform.forward * speed * Time.deltaTime;

    }

    //碰撞
    private void OnCollisionEnter(Collision collision)
    {
        //打到的坦克
        GameObject collObj = collision.gameObject;
        BaseTank hitTank = collObj.GetComponent<BaseTank>();

        //不能打自己
        if(hitTank == tank)
        {
            return;
        }

        //打到其他坦克 
        if (hitTank != null)
        {
            SendMsgHit(tank, hitTank);
        }

        //显示爆炸效果
        GameObject explode = ResManager.LoadPrefab("fire");
        Instantiate(explode,transform.position,transform.rotation);
        //摧毁自己
        Destroy(gameObject);
    }

    private void SendMsgHit(BaseTank tank, BaseTank hitTank)
    {
        if (hitTank == null || tank == null)
            return;
        //不是自己发出的炮弹
        if (tank.id != GameMain.id)
            return;
        MsgHit msgHit = new MsgHit();
        msgHit.targetId = hitTank.id;
        msgHit.Id = tank.id;
        msgHit.X = transform.position.x;
        msgHit.Y = transform.position.y;
        msgHit.Z = transform.position.z;
        NetManager.Send(msgHit);

    }
}
