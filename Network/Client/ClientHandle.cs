﻿using GameServer;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        Debug.Log("Welcome is running...");
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if(GameManager.players.TryGetValue(id, out PlayerManager player))
        {
            player.transform.position = position;
        }
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        if(GameManager.players.TryGetValue(id, out PlayerManager player))
        {
            player.transform.rotation = rotation;
        }
    }

    public static void PlayerDisconnected(Packet packet)
    {
        int id = packet.ReadInt();

        Destroy(GameManager.players[id].gameObject);
        GameManager.players.Remove(id);
    }

    public static void PlayerHealth(Packet packet)
    {
        int id = packet.ReadInt();
        float health = packet.ReadFloat();

        GameManager.players[id].SetHealth(health);
    }

    public static void PlayerRespawned(Packet packet)
    {
        int id = packet.ReadInt();
        GameManager.players[id].Respawn();
    }

    public static void CreateItemSpawner(Packet packet)
    {
        int spawnerId = packet.ReadInt();
        Vector3 spawnerPosition = packet.ReadVector3();
        bool hasItem = packet.ReadBool();

        GameManager.instance.CreateItemSpawner(spawnerId, spawnerPosition, hasItem);
    }

    public static void ItemSpawned(Packet packet)
    {
        int spawnerId = packet.ReadInt();

        GameManager.itemSpawners[spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet packet)
    {
        int spawnerId = packet.ReadInt();
        int byPlayer = packet.ReadInt();

        GameManager.itemSpawners[spawnerId].ItemPickedUp();
        GameManager.players[byPlayer].itemCount++;
    }

    public static void SpawnProjectile(Packet packet)
    {
        int projectileId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        int thrownByPlayer = packet.ReadInt();

        GameManager.instance.SpawnProjectile(projectileId, position);
        GameManager.players[thrownByPlayer].itemCount--;
    }

    public static void ProjectilePosition(Packet packet)
    {
        int projectileId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if(GameManager.projectiles.TryGetValue(projectileId, out ProjectileManager projectile))
        {
            projectile.transform.position = position;
        }
    }

    public static void ProjectileExploded(Packet packet)
    {
        int projectileId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager.projectiles[projectileId].Explode(position);
    }

    public static void SpawnEnemy(Packet packet)
    {
        int enemyId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager.instance.SpawnEnemy(enemyId, position);
    }

    public static void EnemyPosition(Packet packet)
    {
        int enemyId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if(GameManager.enemies.TryGetValue(enemyId, out EnemyManager enemy))
        {
            enemy.transform.position = position;
        }
    }

    public static void EnemyHealth(Packet packet)
    {
        int enemyId = packet.ReadInt();
        float health = packet.ReadFloat();

        GameManager.enemies[enemyId].SetHealth(health);
    }
}