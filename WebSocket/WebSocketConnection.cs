using UnityEngine;
using WebSocketSharp;
using System;

public class WebSocketConnection
{
    private WebSocket websocket;
    public event EventHandler<string> OnMessage;
    public event EventHandler<byte[]> OnBinaryMessage;

    public WebSocketConnection(string url)
    {
        websocket = new WebSocket(url);

        websocket.OnOpen += (sender, e) =>
        {
            //Debug.Log("Connected to the server.");
        };

        websocket.OnMessage += (sender, e) =>
        {
            if (e.IsText)
            {
                // 处理文本消息
                string message = e.Data;
                OnMessage?.Invoke(this, message);
            }
            else if (e.IsBinary)
            {
                // 处理二进制消息
                byte[] binaryData = e.RawData;
                OnBinaryMessage?.Invoke(this, binaryData);
            }
        };

        websocket.OnClose += (sender, e) =>
        {
            //Debug.Log("Disconnected from the server.");
        };

        websocket.OnError += (sender, e) =>
        {
            //Debug.Log("Error occurred: " + e.Message);
        };

        Connect();
    }

    public void Connect()
    {
        websocket.Connect();
    }

    public void Send(string message)
    {
        if (websocket.ReadyState == WebSocketState.Open)
        {
            websocket.Send(message);
        }
    }

    public void Close()
    {
        websocket.Close();
    }

    public bool CheckConnect()
    {
        return websocket != null && websocket.ReadyState == WebSocketState.Open;
    }
}
