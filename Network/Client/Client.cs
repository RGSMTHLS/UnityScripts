﻿using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

//Attach to an empty gameobject as ClientManager
public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        tcp = new TCP();
        udp = new UDP();
        InitializeClientData();
        isConnected = true;
        Debug.Log("Connecting to " + ip + "...");
        tcp.Connect();
        Debug.Log("Connected");
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            Debug.Log("Socket begin connect to ip: " + instance.ip + ", port: " + instance.port + ", socket: " + socket);
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }
        private void ConnectCallback(IAsyncResult result)
        {
            Debug.Log("ConnectCallback running");
            Debug.Log("result is: " + result);
            socket.EndConnect(result);

            if (!socket.Connected)
            {
                Debug.Log("This runs if socket is not connected, we return");
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            Debug.Log("stream.BeginRead should start here");
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet packet)
        {
            try
            {
                if(socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }catch(Exception ex) {
                Debug.Log($"Error sending data to server via TCP: {ex.Message}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if(byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data));

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }catch(Exception ex)
            {
                Disconnect();
                Debug.LogError(ex.Message);
            }
        }

        private bool HandleData(byte[] data)
        {
            Debug.Log("Data is being handled");
            int packetLength = 0;
            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if(packetLength <= 0)
                {
                    return true;
                }
            }

            while(packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using(Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Debug.Log("Packet id: " + packetId);
                        packetHandlers[packetId](packet);
                    }
                });

                packetLength = 0;
                if(receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if(packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if(packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }
    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            Debug.Log(instance.ip);
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int locallPort)
        {
            socket = new UdpClient(locallPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet packet = new Packet())
            {
                SendData(packet);
            }
        }
        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(instance.myId);
                if(socket != null)
                {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }catch(Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if(data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(data);
            }catch(Exception ex)
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] data)
        {
            using (Packet packet = new Packet(data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    int packetId = packet.ReadInt();

                    packetHandlers[packetId](packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();
            endPoint = null;
            socket = null;
        }
    }
    private void InitializeClientData()
    {
        Debug.Log("Initializing client data...");
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome },
            {(int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            {(int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            {(int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            {(int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
            {(int)ServerPackets.playerHealth, ClientHandle.PlayerHealth },
            {(int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawned },
            {(int)ServerPackets.createItemSpawner, ClientHandle.CreateItemSpawner },
            {(int)ServerPackets.itemSpawned, ClientHandle.ItemSpawned },
            {(int)ServerPackets.itemPickedUp, ClientHandle.ItemPickedUp },
            {(int)ServerPackets.spawnProjectile, ClientHandle.SpawnProjectile },
            {(int)ServerPackets.projectilePosition, ClientHandle.ProjectilePosition },
            {(int)ServerPackets.projectileExploded, ClientHandle.ProjectileExploded },
            {(int)ServerPackets.spawnEnemy, ClientHandle.SpawnEnemy },
            {(int)ServerPackets.enemyPosition, ClientHandle.EnemyPosition},
            {(int)ServerPackets.enemyHealth, ClientHandle.EnemyHealth },
        };
        Debug.Log("Initialized packets");
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();
            Debug.Log("Disconnected from server.");
        }
    }
}
