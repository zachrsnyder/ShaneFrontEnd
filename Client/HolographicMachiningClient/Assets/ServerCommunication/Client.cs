using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class Client : MonoBehaviour
{
    public string server = "127.0.0.1";
    private NetworkStream stream;
    public Int16 port = 1234;
    private TcpClient client;
    private Byte[] recbuffer;

    private const uint headerlen = 2; //2 byte headers (uint16)
    private Byte[] headbuf;
    private Byte[] wribuffer;
    
    //TODO: Implement a shutdown request to be sent over TCP to the server so it can free the port and socket it is using and start listening for new connections
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            client = new TcpClient();
            headbuf = new byte[headerlen];
            recbuffer = new byte[client.ReceiveBufferSize];
            wribuffer = new byte[client.SendBufferSize];
            client.BeginConnect(server, port, RequestCallback, null);
            // StreamWriter sw = new StreamWriter(stream);
            // sw.WriteLine("Hello server!!");
            // Task t1 = stream.FlushAsync();
            // t1.Start();
            // Console.WriteLine("Started task!");
        }
        catch (SocketException e)
        {
            Debug.Log(e);
        }
    }

    private void RequestCallback(IAsyncResult ar)
    {
        Debug.Log("We are connected now!");
        client.EndConnect(ar);
        
        //Begin reading a new header
        client.GetStream().BeginRead(headbuf, 0, headbuf.Length, (IAsyncResult ar) => {ReceiveCallback(ar, true);}, null);
    }

    private void ReceiveCallback(IAsyncResult ar, bool isHeader)
    {
        if (!client.Connected)
        {
            Debug.Log("cant receive, we are disconnected!");
            return;
        }
        if (isHeader)
        {
            client.GetStream().EndRead(ar);
            //Just read in a header
            UInt16 numbytes = BitConverter.ToUInt16(headbuf);
            Array.Resize(ref recbuffer, numbytes); //resize buffer to the size of incoming msg
            //Read in actual message now
            client.GetStream().BeginRead(recbuffer, 0, recbuffer.Length,
                (IAsyncResult ar) => { ReceiveCallback(ar, false); }, null);

        }
        else
        {
            client.GetStream().EndRead(ar);
            //If we are here, it means we have already read the current message into recbuffer, now just print it out
            Debug.Log(Encoding.ASCII.GetString(recbuffer));
            
            //Then we prime server for next incoming header 
            client.GetStream().BeginRead(headbuf, 0, headbuf.Length, (IAsyncResult ar) => {ReceiveCallback(ar, true);}, null);
        }

    }

    //Send a message to the server
    public void SendMsg(string data)
    {
        if (client.Connected)
        {
            Byte[] tempbuf = Encoding.ASCII.GetBytes(data);
            Byte[] sizebytes = BitConverter.GetBytes((UInt16) (tempbuf.Length));
            Debug.Log("writing " + tempbuf.Length + " bytes of data..." + "header is " + sizebytes.Length + " bytes long...");
            Buffer.BlockCopy(sizebytes, 0, wribuffer, 0, sizebytes.Length);
            Buffer.BlockCopy(tempbuf, 0, wribuffer, sizebytes.Length, tempbuf.Length);
            client.GetStream().BeginWrite(wribuffer, 0, (sizebytes.Length + tempbuf.Length), WriteHandler, null);
        }
        else
        {
            Debug.Log("Not connected to server, cant send message!");
            return;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.aKey.wasReleasedThisFrame)
        {
            Debug.Log("A pressed");
            if (client.Connected)
            {
                SendMsg("toggle/buzzer\0");
            }
        }
        else if (Keyboard.current.dKey.wasReleasedThisFrame)
        {
            Debug.Log("D pressed");
            if (client.Connected)
            {
                SendMsg("toggle/redled\0");
            }
        }
    }

    private void WriteHandler(IAsyncResult ar)
    {
        client.GetStream().EndWrite(ar);
        
        Debug.Log("Finished writing!");

    }

    private void OnDestroy()
    {
        client.Dispose();
    }
}
