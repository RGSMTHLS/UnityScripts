using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    public delegate void PacketHandler(int fromClient, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    public static TcpListener tcpListener;
    public static UdpClient udpListener;
    public static void Start(int maxPlayers, int port)
    {
        Debug.Log($"Starting server on port {port} with a maximum of {maxPlayers} players");
        MaxPlayers = maxPlayers;
        Port = port;


        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on port: {Port}.");

    }
    private static void UDPReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(result, ref clientEndpoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt();

                if (clientId == 0)
                {
                    return;
                }

                if (clients[clientId].udp.endPoint == null)
                {
                    clients[clientId].udp.Connect(clientEndpoint);
                    return;
                }

                if (clients[clientId].udp.endPoint.ToString() == clientEndpoint.ToString())
                {
                    clients[clientId].udp.HandleData(packet);
                }
            }

        }
        catch (Exception ex)
        {
            Debug.Log("UDP error " + ex.Message + ". This is being called when server shuts down");
        }
    }

    public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint != null)
            {
                udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("SendUDPData error: " + ex.Message);
        }
    }

    private static void TCPConnectCallback(IAsyncResult result)
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");

        try
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    Debug.Log($"Trying to connect client {i}...");
                    clients[i].tcp.Connect(client);
                    return;
                }
            }
            Debug.Log("TCPConnectCallback loop finished");
        }
        catch (Exception ex)
        {
            Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: ${ex.Message}");
            return;
        }

        Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server is full!");
    }

    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                { (int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
                {(int)ClientPackets.playerThrowItem, ServerHandle.PlayerThrowItem }
            };
        Debug.Log("Initialized Packets.");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
