using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace IVLab.Utilities
{
    public static class StreamMethods
    {

        public static void WriteObjectToStream(Stream s, object obj)
        {
            var bytes = ByteMethods.ObjectToByteArray(obj);
            WriteBytestoStream(s, bytes);
        }
        public static async Task WriteObjectToStreamAsync(Stream s, object obj, CancellationToken cancellationToken)
        {
            var bytes =  ByteMethods.ObjectToByteArray(obj);
            await WriteBytestoStreamAsync(s, bytes, cancellationToken);
        }


        public static object ReadObjectFromStream(Stream s)
        {
            byte[] bytes = ReadBytesFromStream(s);
            object obj = ByteMethods.ByteArrayToObject(bytes);
            return obj;
        }

        public static async Task<object> ReadObjectFromStreamAsync(Stream s, CancellationToken cancellationToken)
        {

            byte[] bytes = await ReadBytesFromStreamAsync(s, cancellationToken);
            object obj = ByteMethods.ByteArrayToObject(bytes);
            return obj;
        }

        public static int ReadIntFromStream(Stream s)
        {
            byte[] integerBytes = new byte[4];
            s.Read(integerBytes, 0, integerBytes.Length);


            int integer = System.BitConverter.ToInt32(integerBytes, 0);

            return integer;
        }



        public static async Task<int> ReadIntFromStreamAsync(Stream s, CancellationToken cancellationToken)
        {
            byte[] integerBytes = new byte[4];

            int result = 0;

            while (result == 0 && !cancellationToken.IsCancellationRequested)
            {
                result = await s.ReadAsync(integerBytes, 0, integerBytes.Length, cancellationToken);
                if (result == 0) await Task.Delay(100);

            }

            int integer = System.BitConverter.ToInt32(integerBytes, 0);

            return integer;
        }
        public static void WriteIntToStream(Stream s, int value)
        {
            byte[] integerBytes = BitConverter.GetBytes(value);
            s.Write(integerBytes, 0, integerBytes.Length);

        }
        public static async Task WriteIntToStreamAsync(Stream s, int value, CancellationToken cancellationToken)
        {
            byte[] integerBytes = BitConverter.GetBytes(value);
            await s.WriteAsync(integerBytes, 0, integerBytes.Length, cancellationToken);

        }

        public static void WriteBytestoStream(Stream s, byte[] data)
        {
            StreamMethods.WriteIntToStream(s, data.Length);

            s?.Write(data, 0, data.Length);
        }
        public static async Task WriteBytestoStreamAsync(Stream s, byte[] data, CancellationToken cancellationToken)
        {

            await StreamMethods.WriteIntToStreamAsync(s, data.Length, cancellationToken);

            await s?.WriteAsync(data, 0, data.Length, cancellationToken);
        }
        public static async Task<byte[]> ReadBytesFromStreamAsync(Stream s, CancellationToken cancellationToken)
        {
            int numBytes = await StreamMethods.ReadIntFromStreamAsync(s, cancellationToken);
            byte[] dataBytes = new byte[numBytes];
            int totalBytesReceived = 0;

            while (totalBytesReceived < numBytes && !cancellationToken.IsCancellationRequested)
            {
                int offset = totalBytesReceived;
                int count = numBytes - totalBytesReceived;
                var bytesReadTask = s.ReadAsync(dataBytes, offset, count);
                var bytesReceived = await bytesReadTask;
                totalBytesReceived += bytesReceived;
            }
            return dataBytes;
        }
        public static byte[] ReadBytesFromStream(Stream s)
        {
            int numBytes = StreamMethods.ReadIntFromStream(s);

            byte[] dataBytes = new byte[numBytes];
            int totalBytesReceived = 0;

            while (totalBytesReceived < numBytes)
            {
                int offset = totalBytesReceived;
                int count = numBytes - totalBytesReceived;
                var bytesReceived = s.Read(dataBytes, offset, count);
                totalBytesReceived += bytesReceived;
            }
            return dataBytes;
        }

        public static void WriteStringToStream(Stream s, string text)
        {
            byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
            StreamMethods.WriteBytestoStream(s, textData);
        }

        public static string ReadStringFromStream(Stream s)
        {
            var bytesReceived = ReadBytesFromStream(s);

            string text = System.Text.Encoding.UTF8.GetString(bytesReceived, 0, bytesReceived.Length);
            return text;

        }

        public static async Task WriteStringToStreamAsync(Stream s, string text, CancellationToken cancellationToken)
        {
            byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
            await StreamMethods.WriteBytestoStreamAsync(s, textData, cancellationToken);
        }

        public static async Task<string> ReadStringFromStreamAsync(Stream s, CancellationToken cancellationToken)
        {
            var bytesReceived = await ReadBytesFromStreamAsync(s, cancellationToken);

            string text = System.Text.Encoding.UTF8.GetString(bytesReceived, 0, bytesReceived.Length);
            return text;

        }
    }

}
