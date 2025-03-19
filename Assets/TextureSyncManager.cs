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
        int totalSize = textureBytes.Length;
        using (var writer = new FastBufferWriter(totalSize + sizeof(int), Allocator.Temp))
        {
            writer.WriteValueSafe(totalSize);
            writer.WriteBytesSafe(textureBytes, totalSize);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                "TextureMessage",
                clientId,
                writer,
                NetworkDelivery.ReliableFragmentedSequenced
            );
            Debug.Log($"[Server] Sent texture to client {clientId}, size: {totalSize} bytes.");
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
            int rate = 75;
            byte[] texData;
            do
            {
                texData = whiteboard.texture.EncodeToJPG(rate);
                rate -= 5;
            } while (texData.Length > 65000); // after series experience, 65000 is the maximum size of a packet
            
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
            .Where(clientId => clientId != senderClientId)
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
        int length;
        reader.ReadValueSafe(out length);

        byte[] textureBytes = new byte[length];

        reader.ReadBytesSafe(ref textureBytes, length);

        // Decompress if necessary
        //textureBytes = Decompress(textureBytes); // Uncomment if you compressed the data

        // Load the texture
        Texture2D receivedTexture = new Texture2D(2, 2);
        if (!receivedTexture.LoadImage(textureBytes))
        {
            Debug.LogWarning("[Client] Failed to load texture from received bytes!");
            return;
        }
        //File.WriteAllBytes("C:\\Users\\sky\\received.jpg", receivedTexture.EncodeToJPG());

        // Apply the texture to the whiteboard
        ApplyTextureToUIOrObject(receivedTexture);
    }

    private void ApplyTextureToUIOrObject(Texture2D tex)
    {
        whiteboard.ApplyTexture(tex);
        Debug.Log("[Client] Successfully applied the received texture!");
    }

    // Stroke sync::::::::::::::::::::::::

    // Called when a player draws on the canvas

    [ServerRpc(RequireOwnership = false)]
    public void SendDrawCommandServerRpc(Vector2 posStart, Vector2 posEnd, Color[] colors, int brushSize, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"[Server] Received draw command from client {serverRpcParams.Receive.SenderClientId}.");

        // Update the master texture on the server
        UpdateCanvas(whiteboard.texture, posStart, posEnd, colors, brushSize);

        latestTextureData = whiteboard.texture.EncodeToJPG(100); // default is 75

        // Broadcast the update to all clients except the sender
        SendDrawCommandClientRpc(posStart, posEnd, colors, brushSize, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != serverRpcParams.Receive.SenderClientId).ToArray() }
        });
    }

    [ClientRpc]
    public void SendDrawCommandClientRpc(Vector2 posStart, Vector2 posEnd, Color[] colors, int brushSize, ClientRpcParams clientRpcParams = default)
    {
        if (!IsServer)
        {
            Debug.Log($"[Client] Received draw command from server.");
            UpdateCanvas(whiteboard.texture, posStart, posEnd, colors, brushSize);
        }
    }


    // Example function to update the canvas (implement your drawing logic here)
    private void UpdateCanvas(Texture2D texture, Vector2 posStart, Vector2 posEnd, Color[] colors, int brushSize)
    {
        // Convert brushSize and positions to integer values
        int startX = Mathf.RoundToInt(posStart.x);
        int startY = Mathf.RoundToInt(posStart.y);

        // Draw at the starting position
        texture.SetPixels(startX, startY, brushSize, brushSize, colors);

        // Interpolate between start and end positions to draw a smooth line
        for (float f = 0.01f; f < 1.0f; f += 0.05f)
        {
            int lerpX = Mathf.RoundToInt(Mathf.Lerp(posStart.x, posEnd.x, f));
            int lerpY = Mathf.RoundToInt(Mathf.Lerp(posStart.y, posEnd.y, f));
            texture.SetPixels(lerpX, lerpY, brushSize, brushSize, colors);
        }

        texture.Apply();
    }
}