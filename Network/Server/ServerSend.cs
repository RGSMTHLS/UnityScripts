using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    private static void SendTcpData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    private static void SendUDPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }

    private static void SendTcpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTcpDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }

    public static void Welcome(int toClient, string msg)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(msg);
            packet.Write(toClient);
            SendTcpData(toClient, packet);
        }
    }

    public static void SpawnPlayer(int toClient, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTcpData(toClient, packet);
        }
    }

    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUDPDataToAll(player.id, packet);
        }
    }

    public static void PlayerDisconnected(int playerId)
    {
        using(Packet packet = new Packet((int)ServerPackets.playerDisconnects))
        {
            packet.Write(playerId);
            SendTcpDataToAll(packet);
        }
    }

    public static void PlayerHealth(Player player)
    {
        using(Packet packet = new Packet((int)ServerPackets.playerHealth))
        {
            packet.Write(player.id);
            packet.Write(player.health);

            SendTcpDataToAll(packet);
        }
    }

    public static void PlayerRespawned(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRespawn))
        {
            packet.Write(player.id);

            SendTcpDataToAll(packet);
        }
    }

    public static void CreateItemSpawner(int toClient, int spawnerId, Vector3 position, bool hasItem)
    {
        using (Packet packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            packet.Write(spawnerId);
            packet.Write(position);
            packet.Write(hasItem);

            SendTcpData(toClient, packet);
        }
    }

    public static void ItemSpawned(int spawnerId)
    {
        using (Packet packet = new Packet((int)ServerPackets.itemSpawned))
        {
            packet.Write(spawnerId);
            SendTcpDataToAll(packet);
        }
    }

    public static void ItemPickedUp(int spawnerId, int byPlayer)
    {
        using (Packet packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            packet.Write(spawnerId);
            packet.Write(byPlayer);

            SendTcpDataToAll(packet);
        }
    }

    public static void SpawnProjectile(Projectile projectile, int thrownByPlayer)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);
            packet.Write(thrownByPlayer);

            SendTcpDataToAll(packet);
        }
    }

    public static void ProjectilePosition(Projectile projectile)
    {
        using(Packet packet = new Packet((int)ServerPackets.projectilePosition))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void ProjectileExploded(Projectile projectile)
    {
        using(Packet packet = new Packet((int)ServerPackets.projectileExploded))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);

            SendTcpDataToAll(packet);
        }
    }

    public static void SpawnEnemy(Enemy enemy)
    {
        using(Packet packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            SendTcpDataToAll(SpawnEnemy_Data(enemy, packet));
        }
    }

    public static void SpawnEnemy(int toClient, Enemy enemy)
    {
        using(Packet packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            SendTcpData(toClient, SpawnEnemy_Data(enemy, packet));
        }
    }

     private static Packet SpawnEnemy_Data(Enemy enemy, Packet packet)
    {
        packet.Write(enemy.id);
        packet.Write(enemy.transform.position);
        return packet;
    }

    public static void EnemyPosition(Enemy enemy)
    {
        using(Packet packet = new Packet((int)ServerPackets.enemyPosition))
        {
            packet.Write(enemy.id);
            packet.Write(enemy.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void EnemyHealth(Enemy enemy)
    {
        using(Packet packet = new Packet((int)ServerPackets.enemyHealth))
        {
            packet.Write(enemy.id);
            packet.Write(enemy.health);

            SendTcpDataToAll(packet);
        }
    }
}