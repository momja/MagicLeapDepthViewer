using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IVLab.Utilities.RemoteRendering
{
    public enum MessageType
    {
        Error,
        ConnectionEstablished,
        ConnectionClosed,
        ObjectData,
        TextureData,
        StringData,
    }

    public class MessageHeader 
    {
        public MessageType MessageType;
        public int MessageLength;
    }
}
