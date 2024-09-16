using SysMsg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager 
{

    //定义套接字
    static Socket socket;
    //接收缓冲区
    static ByteArray readBuff;
    //写入队列
    static Queue<ByteArray> writeQueue;

    //消息列表
    //static List<MsgBase> msgList = new List<MsgBase>();
    static List<ProtoBuf.IExtensible> msgList = new List<ProtoBuf.IExtensible>();
    //消息列表长度
    static int msgCount = 0;
    //每一次Update处理的消息量
    readonly static int MAX_MESSAGE_FIRE = 10;

    //是否启用心跳
    public static bool isUsePing = false;
    //心跳间隔时间
    public static int pingInterval = 30;
    //上一次发送Ping的时间
    static float lastPingTime = 0;
    //上一次收到Pong的时间
    static float lastPongTime = 0;


    #region 消息监听

    //消息委托类型
    public delegate void MsgListener(ProtoBuf.IExtensible msgBase);

    //消息监听列表
    private static Dictionary<string,MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    //添加消息监听
    public static void AddMsgListener<T>( MsgListener listener) where T :ProtoBuf.IExtensible
    {
        string msgName = typeof(T).Name;

        //添加
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        //新增
        else
        {
            msgListeners.Add(msgName, listener);
        }
    }

    //删除消息监听
    public static void RemoveMsgListener<T>(MsgListener listener) where T :ProtoBuf.IExtensible
    {

        string msgName = typeof(T).Name;

        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
            //删除
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }

    //分发消息
    private static void FireMsg(ProtoBuf.IExtensible msgBase)
    {
        string msgName = msgBase.GetType().Name;
        Debug.Log(msgName);
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }





    #endregion

    #region 事件相关

    //事件委托类型
    public delegate void EventListener(string err);

    //事件监听列表
    private static Dictionary<E_NetEvent, EventListener> eventListeners = new Dictionary<E_NetEvent, EventListener>();


    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="e_NetEvent"></param>
    /// <param name="listener"></param>
    public static void AddEventListener(E_NetEvent e_NetEvent , EventListener listener)
    {
        if (eventListeners.ContainsKey(e_NetEvent))
        {
            eventListeners[e_NetEvent] += listener;
        }
        else
        {
            eventListeners.Add(e_NetEvent, listener);
        }
    }   

    /// <summary>
    /// 删除事件监听
    /// </summary>
    /// <param name="e_NetEvent"></param>
    /// <param name="listener"></param>
    public static void RemoveEventListener(E_NetEvent e_NetEvent, EventListener listener)
    {
        if(eventListeners.ContainsKey(e_NetEvent))
        {
            eventListeners[e_NetEvent] -= listener;
            if (eventListeners[e_NetEvent] == null)
            {
                eventListeners.Remove(e_NetEvent);
            }
        }
    }

    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="e_NetEvent"></param>
    /// <param name="err"></param>
    private static void FireEvent(E_NetEvent e_NetEvent, string err)
    {
        if (eventListeners.ContainsKey(e_NetEvent))
        {
            eventListeners[e_NetEvent](err);
        }
    }

    #endregion

    #region Connect

    static bool isConneting = false;

    //连接服务端
    public static void Connect(string ip , int port)
    {
        //状态判断
        if(socket != null && socket.Connected)
        {
            Debug.Log("Connect fail , already connected!");
            return;
        }
        if (isConneting)
        {
            Debug.Log("Connect fail ,isConneting");
            return;
        }
        //初始化成员
        InitState();
        //参数设置
        socket.NoDelay = false;
        //Connect
        isConneting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);

    }

    //Connect回调
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ ");
            FireEvent(E_NetEvent.ConnectSucc, "");
            isConneting = false;

            //开始接收
            socket.BeginReceive(readBuff.bytes,readBuff.writeIdx,readBuff.remain,0,ReceiveCallback,socket);
        }
        catch (SocketException e)
        {
            Debug.Log("Socket Connect fail "+e.ToString());
            FireEvent(E_NetEvent.ConnectFail, e.ToString());
            isConneting = false;
        }
    }

    

    //初始化状态  重置缓冲区等成员变量
    private static void InitState()
    {
        //Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //接收缓冲区
        readBuff = new ByteArray();
        //写入队列
        writeQueue = new Queue<ByteArray> ();
        //是否正在连接
        isConneting = false;
        //是否正在关闭
        isClosing = false;
        
        //消息列表
        msgList = new List<ProtoBuf.IExtensible> ();
        //消息列表长度
        msgCount = 0;

        //上一次发送Ping的时间
        lastPingTime = Time.time;
        //上一次收到Pong的时间
        lastPongTime = Time.time;

        //监听Pong协议
        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener<MsgPong>(OnMsgPong);
        }
    }



    #endregion


    #region Receive
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ( Socket ) ar.AsyncState;
            int count = socket.EndReceive(ar);
            if(count == 0)
            {
                Close();
                return;
            }
            readBuff.writeIdx += count;
            //处理二进制消息
            OnReceiveData();
            //继续接收数据
            if(readBuff.remain < 8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.length * 2);
            }
            socket.BeginReceive(readBuff.bytes,readBuff.writeIdx,readBuff.remain,0,ReceiveCallback,socket);
        }
        catch(SocketException e)
        {
            Debug.Log("Socket Receive fail "+ e.ToString());
        }
    }

    //数据处理
    private static void OnReceiveData()
    {
        //消息长度
        if (readBuff.length <= 2)
            return;
        // 获取消息体长度（大端字节序）
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        // 大端字节序：高字节在前，低字节在后
        Int16 bodyLength = (Int16)((bytes[readIdx] << 8) | bytes[readIdx + 1]);

        //如果缓冲区消息长度小于 解析出的消息总长度 说明消息不完整
        if (readBuff.length < bodyLength)
            return;
        //缓冲区 读取索引+2 表示消息总长度已经读过了
        readBuff.readIdx += 2;

        //解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes,readBuff.readIdx,out  nameCount);
        if(protoName == "")
        {
            Debug.Log("OnReceiveData MsgBase.DecodeName fail");
            return;
        }
        readBuff.readIdx += nameCount;
        Debug.Log("OnReceiveMsg :" + protoName);

        //解析协议体
        int bodyCount = bodyLength - nameCount;
        //MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes,readBuff.readIdx, bodyCount);
        ProtoBuf.IExtensible msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        //添加到消息队列
        lock (msgList)
        {
            msgList.Add(msgBase);
        }
        msgCount++;
        //继续读取消息
        if(readBuff.length > 2)
        {
            OnReceiveData();
        }
    }

    #endregion


    #region Send
    public static void Send(ProtoBuf.IExtensible msg)
    {
        //状态判断
        if(socket == null || !socket.Connected) return;

        if(isConneting || isClosing) return;

        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2+len];

        //组装长度
        sendBytes[0] = (byte)(len / 256);
        sendBytes[1] = (byte)(len % 256);

        //组装名字
        Array.Copy(nameBytes,0,sendBytes,2,nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        //Debug.Log(BitConverter.ToString(sendBytes));

        //写入队列
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;  //writeQueue的长度

        lock(writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }

        //Send
        if(count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }

    }

    private static void SendCallback(IAsyncResult ar)
    {
        //获取state EndSend的处理
        Socket socket = ar.AsyncState as Socket;
        //状态判断
        if(socket == null || !socket.Connected) return;

        int count = socket.EndSend(ar);

        //获取写入队列的第一个数据
        ByteArray ba;
        lock(writeQueue)
        {
            ba = writeQueue.First();
        }
        //完整发送
        ba.readIdx += count;

        if(ba.length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.FirstOrDefault();
            }
        }
        //继续发送
        if(ba != null)
        {
            socket.BeginSend(ba.bytes,ba.readIdx,ba.length,0,SendCallback,socket);
        }
        //正在关闭
        else if (isClosing)
        {
            socket.Close();
        }

    }


    #endregion

    #region 心跳

    //发送Ping协议
    private static void PingUpdate()
    {
        //是否启用
        if(!isUsePing)
        {
            return;
        }

        //发送Ping
        if(Time.time - lastPingTime > pingInterval)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            lastPingTime = Time.time;
        }

        //检测Pong时间
        if(Time.time - lastPongTime > pingInterval*4)
        {
            Close();
        }


    }



    private static void OnMsgPong(ProtoBuf.IExtensible msgBase)
    {
        lastPongTime = Time.time;
    }

    #endregion


    #region Update
    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

    //更新消息
    private static void MsgUpdate()
    {
        //初步判断，提高效率
        if (msgCount == 0)
            return;

        //重复处理消息
        for (int i = 0;i< MAX_MESSAGE_FIRE;i++)
        {
            //获取第一条消息
            ProtoBuf.IExtensible msgBase = null;
            lock(msgList)
            {
                if(msgList.Count > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            //分发消息
            if(msgBase != null)
            {
                FireMsg(msgBase);
            }
            //没有消息了
            else
            {
                break;
            }
        }

    }



    #endregion

    #region Close
    static bool isClosing = false;

    public static void Close()
    {
        if (socket == null || !socket.Connected)
            return;
        if (isConneting)
            return;

        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(E_NetEvent.Close, "");
        }
    }

    internal static void AddMsgListener(string v, object onMsgLogin)
    {
        throw new NotImplementedException();
    }

    #endregion


}
