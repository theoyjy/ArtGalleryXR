using Unity.Netcode;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System;
using Unity.Collections;

public class TextureSyncManager : NetworkBehaviour
{
    public Whiteboard whiteboard;

    private static byte[] latestTextureData; // Store the latest texture on the server

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("TextureSyncManager spawned. IsSpawned: " + GetComponent<NetworkObject>().IsSpawned);

        // Register custom message handlers
        if (IsClient)
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(
                "TextureMessage",
                (ulong senderId, FastBufferReader reader) =>
                {
                    OnTextureMessageReceived(reader);
                }
            );
        }

        if (IsClient && !IsHost)
        {
            Debug.Log("[Client] Requesting latest texture from server...");
            RequestLatestTextureFromServerRpc();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SendTextureToServer();
        }
    }

    // ---------- Client Requests the Latest Texture ----------
    [ServerRpc(RequireOwnership = false)]
    private void RequestLatestTextureFromServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong requesterClientId = serverRpcParams.Receive.SenderClientId;

        if (latestTextureData != null && latestTextureData.Length > 0)
        {
            Debug.Log($"[Server] Sending stored texture to new client: {requesterClientId}");

            // Send the texture using custom messaging
            SendTextureToClientCustomMessage(requesterClientId, latestTextureData);
        }
        else
        {
            Debug.Log("[Server] No stored texture available.");
        }
    }

    // ---------- Server Sends the Latest Texture to Requesting Client ----------
    private void SendTextureToClientCustomMessage(ulong clientId, byte[] textureBytes)
    {
        using (var writer = new FastBufferWriter(textureBytes.Length, Allocator.Temp))
        {
            writer.WriteBytesSafe(textureBytes, textureBytes.Length);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                "TextureMessage",
                clientId,
                writer,
                NetworkDelivery.ReliableFragmentedSequenced
            );
            Debug.Log($"[Server] Sent texture to client {clientId}, size: {textureBytes.Length} bytes.");
        }
    }

    // ---------- Client Sends Texture to Server ----------
    public void SendTextureToServer()
    {
        if ((IsServer || IsHost) && !GetComponent<NetworkObject>().IsSpawned)
        {
            GetComponent<NetworkObject>().Spawn();
        }

        try
        {
            byte[] texData = whiteboard.texture.EncodeToJPG(25);
            //byte[] texDataCompressed = Compress(texData); // Optional: Compress the data
            SendTextureToServerRpc(texData);
        }
        catch (Exception e)
        {
            Debug.LogError("Error in SendTextureToServer: " + e.Message);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendTextureToServerRpc(byte[] textureBytes, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"[Server] Received texture from client, size: {textureBytes.Length} bytes.");

        // Store the latest texture on the server
        latestTextureData = textureBytes;

        // Broadcast to all clients except the sender
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        var targetClients = NetworkManager.Singleton.ConnectedClientsIds
            //.Where(clientId => clientId != senderClientId)
            .ToArray();

        foreach (var clientId in targetClients)
        {
            SendTextureToClientCustomMessage(clientId, textureBytes);
        }
    }

    // ---------- Handle Received Texture Data ----------
    private void OnTextureMessageReceived(FastBufferReader reader)
    {
        Debug.Log("[Client] Received texture data via custom message.");

        // Allocate a byte array to store the received data
        byte[] textureBytes = new byte[reader.Length];
        reader.ReadBytesSafe(ref textureBytes, reader.Length);

        // Decompress if necessary
        //textureBytes = Decompress(textureBytes); // Uncomment if you compressed the data

        // Load the texture
        Texture2D receivedTexture = new Texture2D(2, 2);
        if (!receivedTexture.LoadImage(textureBytes))
        {
            Debug.LogWarning("[Client] Failed to load texture from received bytes!");
            return;
        }
        File.WriteAllBytes("C:\\Users\\sky\\received.jpg", receivedTexture.EncodeToJPG());

        // Apply the texture to the whiteboard
        ApplyTextureToUIOrObject(receivedTexture);
    }

    private void ApplyTextureToUIOrObject(Texture2D tex)
    {
        whiteboard.ApplyTexture(tex);
        Debug.Log("[Client] Successfully applied the received texture!");
    }

    // ---------- Compression/Decompression (Optional) ----------
    private byte[] Compress(byte[] data)
    {
        using (MemoryStream output = new MemoryStream())
        {
            using (GZipStream gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal, true))
            {
                gzip.Write(data, 0, data.Length);
                gzip.Flush();
            }
            return output.ToArray();
        }
    }

    private byte[] Decompress(byte[] compressedData)
    {
        using (MemoryStream input = new MemoryStream(compressedData))
        using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
        using (MemoryStream output = new MemoryStream())
        {
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }
}