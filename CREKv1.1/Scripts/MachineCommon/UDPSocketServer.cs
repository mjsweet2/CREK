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

public class UDPSocketServer : MonoBehaviour
{
    public List<string> messages;


    public float timer1 = 0f;
    public float timer2 = 0f;
    public int recieveCount = 0;
    public int sendCount;

    public bool runNetwork;
    public bool isStopingNetwork;



    public string watchOutGoing;
    public string watchIncoming;
    public string replyAddy;
    public string serverAddy;
    public int port;
    Thread thdUDPReceive;
    Thread emptyThreadRecieve;


    IPEndPoint RemoteIpEndPoint;

    UdpClient udpServer;
    



    // Use this for initialization
    void Start()
    {

        messages = new List<string>();
        //RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        //udpClient = new UdpClient(port);
        //ReplyEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6010);

        // This constructor arbitrarily assigns the local port number.
        //udpClient = new UdpClient(6010);


        //udpClient.Connect("192.168.1.211", 2011);

        // Sends a message to the host to which you have connected.
        //byte[] sendBytes = System.Text.Encoding.ASCII.GetBytes("Hello from Windows!");
        //udpClient.Send(sendBytes, sendBytes.Length);


        //IPAddress addy = new IPAddress(0);
        //addy = IPAddress.Parse("192.168.1.211");
        //IPEndPoint destination = new IPEndPoint(addy, 2011);
        //udpClient.Send(sendBytes, sendBytes.Length, destination);

        // Sends a message to a different host using optional hostname and port parameters.
        //UdpClient udpClientB = new UdpClient();
        //udpClientB.Send(sendBytes, sendBytes.Length, "AlternateHostMachineName", 11000);

        //IPEndPoint object will allow us to read datagrams sent from any source.


        // Blocks until a message returns on this socket from a remote host.
        //byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
        //string returnData = System.Text.Encoding.ASCII.GetString(receiveBytes);
        //thdUDPReceive = new Thread( new ThreadStart(recFromClient));
        //thdUDPReceive.Start();

    }



    public void enableNetwork() { runNetwork = true; }
    public void disableNetwork() { runNetwork = false; }

    void recFromClient()
    {

        while (runNetwork)
        {
            // Blocks until a message returns on this socket from a remote host.
            byte[] receiveBytes = udpServer.Receive(ref RemoteIpEndPoint); //recieve any
            watchIncoming = System.Text.Encoding.ASCII.GetString(receiveBytes);
            messages.Add(watchIncoming);
            recieveCount++;

        }
        if (isStopingNetwork)
            udpServer.Close();

    }
    public void sendToClient(string message)
    {
        if (!runNetwork)
            return;

        watchOutGoing = message;
        byte[] replyBytes = System.Text.Encoding.ASCII.GetBytes(watchOutGoing);
        udpServer.Send(replyBytes, replyBytes.Length, RemoteIpEndPoint);
        sendCount++;

    }

    //this start and stop function work
    public void stopNetwork()
    {
        runNetwork = false;
    }
    public void startNetwork()
    {

        if (port == 0)
            return;
   
        //RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(replyAddy), port);
        udpServer = new UdpClient(port);
        runNetwork = true;
        thdUDPReceive = null;
        thdUDPReceive = new Thread(new ThreadStart(recFromClient));
        thdUDPReceive.Start();

    }

    public bool hasMessages()
    {
        return (messages.Count != 0);
    }
    public string getNextMessage()
    {
        string retMessage = messages[0];
        messages.RemoveAt(0);
        return retMessage;
    }

    // Update is called once per frame
    void Update()
    {
        if (runNetwork)
        {
            if (recieveCount == 10)
                timer1 = Time.time;
            if (recieveCount == 70)
                timer2 = Time.time;
        }

    }


}
