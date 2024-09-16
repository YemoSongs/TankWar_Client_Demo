using System;
using UnityEngine;

using BattleMsg;
using ProtoBuf;
using System.Buffers;
using Unity.VisualScripting;

public class MsgBase
{


    #region ProtoBuf

    public static byte[] Encode(ProtoBuf.IExtensible msgBase)
    {
        using (var memory = new System.IO.MemoryStream())
        {
            ProtoBuf.Serializer.Serialize<ProtoBuf.IExtensible>(memory, msgBase);
            return memory.ToArray();
        }
    }

    public static ProtoBuf.IExtensible Decode(string protoName, byte[] bytes, int offset, int count)
    {
        Type t = Type.GetType(protoName);

        if (t == null)
        {
            Debug.LogError($"Failed to resolve type '{protoName}'.");
            return null;
        }

        //Debug.Log($"Decoding {protoName}, Type: {t.Name}");

        try
        {
            using (var memory = new System.IO.MemoryStream(bytes, offset, count))
            {
                return (ProtoBuf.IExtensible)ProtoBuf.Serializer.NonGeneric.Deserialize(t, memory);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deserializing {protoName}: {ex.Message}");
            return null;
        }
    }

    //编码协议名（2字节长度 + 字符串）
    public static byte[] EncodeName(ProtoBuf.IExtensible msg)
    {

        Debug.Log("endcodeName:" + msg.GetType().FullName);
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msg.GetType().FullName);
        Int16 len = (Int16)nameBytes.Length;

        byte[] bytes = new byte[2 + len];

        // 使用大端字节序
        bytes[0] = (byte)((len >> 8) & 0xFF); // 高位字节
        bytes[1] = (byte)(len & 0xFF);        // 低位字节

        Array.Copy(nameBytes, 0, bytes, 2, len);

        return bytes;
    }

    //解码协议名
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;

        if (offset + 2 > bytes.Length)
        {
            return "";
        }

        Int16 len = (Int16)((bytes[offset] << 8) | bytes[offset + 1]); // 大端字节序

        if (offset + 2 + len > bytes.Length)
        {
            return "";
        }

        count = 2 + len;
        string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);

        return name;
    }





    #endregion


    #region Json

    //协议名 
    //public string protoName = "";


    ////编码
    //public static byte[] Encode(MsgBase msgBase)
    //{
    //    string s = JsonUtility.ToJson(msgBase);
    //    return System.Text.Encoding.UTF8.GetBytes(s);
    //}

    ////解码
    //public static MsgBase Decode(string protoName, byte[] bytes,int offset,int count)
    //{
    //    string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
    //    MsgBase msgBase = (MsgBase)JsonUtility.FromJson(s,Type.GetType(protoName));
    //    return msgBase;
    //}

    ////编码协议名（2字节长度 + 字符串）
    //public static byte[] EncodeName(MsgBase msgBase)
    //{
    //    byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);
    //    Int16 len = (Int16)nameBytes.Length;

    //    byte[] bytes = new byte[2+len];

    //    bytes[0] = (byte)(len%256);
    //    bytes[1] = (byte)(len/256);

    //    Array.Copy(nameBytes, 0, bytes, 2, len);

    //    return bytes;
    //}


    ////解码协议名
    //public static string DecodeName(byte[] bytes, int offset,out int count)
    //{
    //    count = 0;

    //    if(offset + 2 > bytes.Length)
    //    {
    //        return "";
    //    }

    //    Int16 len = (Int16)((bytes[offset+1] << 8)| bytes[offset]);

    //    if(offset + 2 + len > bytes.Length)
    //    {
    //        return "";
    //    }

    //    count = 2+ len;
    //    string name = System.Text.Encoding.UTF8.GetString(bytes,offset + 2,len);

    //    return name;
    //}


    #endregion


}