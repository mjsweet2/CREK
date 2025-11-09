/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;

public class TCPSocketClient : MonoBehaviour {


    bool isConnected = false;

    public bool isStopingNetwork;

    TcpClient tcpClient;
    NetworkStream stream;

    public string serverAddy;
    public int port;

    public string recieveString;


    // Buffer to store the response bytes.
    public byte [] sendBuffer; //I don't allocate this, the encoding function does
    public byte[] recieveBuffer;


    Thread thdUDPReceive;



    // Use this for initialization
    void Start () {

        recieveBuffer = new byte[1024];

        

    }
	
	// Update is called once per frame
	void Update () {

    
        
	}

    void recFromHost()
    {

        while (isConnected)
        {

            //replyString is set after the message is read
            //byte[] replyBytes = System.Text.Encoding.ASCII.GetBytes(recieveString);
            //udpServer.Send(replyBytes, replyBytes.Length, ReplyEndPoint);


            // Blocks until a message returns on this socket from a remote host.
            try
            {
                // Read the first batch of the TcpServer response bytes.
                int bytesRead = stream.Read(recieveBuffer, 0, recieveBuffer.Length);
                recieveString = System.Text.Encoding.ASCII.GetString(recieveBuffer, 0, bytesRead);

                
            }
            catch (SocketException e)
            {
                Debug.Log(e.ToString());
            }


        }
        if (isStopingNetwork)
            tcpClient.Close();


    }

    public void Send(string m)
    {
        if (!isConnected)
            return;
        try
        {
            // Translate the passed message into ASCII and store it as a Byte array.
            sendBuffer = System.Text.Encoding.ASCII.GetBytes(m);
            
            // Send the message to the connected TcpServer. 
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            

        }
        catch (SocketException e)
        {
            Debug.Log(e.ToString());
        }


    }

    public bool hasMessage() { return (recieveString != ""); }

   

    public void Recieve0()
    {
        try
        {
            // Read the first batch of the TcpServer response bytes.
            int bytesRead = stream.Read(recieveBuffer, 0, recieveBuffer.Length);
            recieveString = System.Text.Encoding.ASCII.GetString(recieveBuffer, 0, bytesRead);

            

        }
        catch (SocketException e)
        {
            Debug.Log(e.ToString());
        }

    }
    public string getMessage()
    {

        string ret = recieveString;
        recieveString = "";
        return ret;

    }
    
    public void Connect()
    {
        try
        {
            if (!isConnected)
            {
                // Create a TcpClient.
                tcpClient = new TcpClient(serverAddy, port);
                stream = tcpClient.GetStream();
                isConnected = true;

                thdUDPReceive = null;
                thdUDPReceive = new Thread(new ThreadStart(recFromHost));
                thdUDPReceive.Start();

            }
            else
            {
                //Send("shutdownserver"); //this shuts the server down
                Close();
            }



        }
        catch (SocketException e)
        {
            Debug.Log(e.ToString());
        }

        
    }

    public void Close()
    {
        isConnected = false;
        stream.Close();
        tcpClient.Close();
        
    }
}
