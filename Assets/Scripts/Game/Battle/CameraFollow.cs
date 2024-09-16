using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    //距离矢量
    public Vector3 distance = new Vector3 (0.0f, 8f, -18f);
    //相机
    public Camera cam;
    //偏移值
    public Vector3 offset = new Vector3(0,5,0);
    //相机移动速度
    public float speed = 4f;

    void Start()
    {
        //默认为主摄像机
        cam = Camera.main;
        //相机初始化
        Vector3 pos = transform.position;
        Vector3 forword = transform.forward;
        Vector3 initPos = pos - 30 * forword + Vector3.up * 10;
        cam.transform.position = initPos;
    }

    
    void LateUpdate()
    {
        //坦克位置
        Vector3 pos = transform.position;
        //坦克方向
        Vector3 forward = transform.forward;
        //相机目标位置
        Vector3 targetPos = pos;
        targetPos = pos + forward * distance.z;
        targetPos.y += distance.y;

        //相机位置
        Vector3 camPos = cam.transform.position;
        camPos = Vector3.MoveTowards(camPos, targetPos, Time.deltaTime * speed);
        cam.transform.position = camPos;
        //对准坦克
        cam.transform.LookAt(pos + offset);
    }
}
