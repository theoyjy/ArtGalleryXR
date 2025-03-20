using Unity.Netcode;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System;
using Unity.Collections;
using System.Collections.Generic;
using System.Collections;


[System.Serializable]
public struct StrokeCommand : INetworkSerializable
{
    public Vector2 posStart;
    public Vector2 posEnd;
    public Color[] colors;
    public int brushSize;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref posStart);
        serializer.SerializeValue(ref posEnd);
        serializer.SerializeValue(ref brushSize);

        int colorCount = (colors == null) ? 0 : colors.Length;
        serializer.SerializeValue(ref colorCount);

        if (serializer.IsReader)
            colors = new Color[colorCount];

        for (int i = 0; i < colorCount; i++)
        {
            float r = colors[i].r, g = colors[i].g, b = colors[i].b, a = colors[i].a;
            serializer.SerializeValue(ref r);
            serializer.SerializeValue(ref g);
            serializer.SerializeValue(ref b);
            serializer.SerializeValue(ref a);
            if (serializer.IsReader)
                colors[i] = new Color(r, g, b, a);
        }
    }
}


public class TextureSyncManager : NetworkBehaviour
{
    public Whiteboard whiteboard;

    private static byte[] latestTextureData; // Store the latest texture on the server

    private List<StrokeCommand> strokeBuffer = new List<StrokeCommand>();
    // Update intervals for full texture and strokes.
    private float strokeUpdateInterval = 0.1f; // 100 ms


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("TextureSyncManager spawned. IsSpawned: " + GetComponent<NetworkObject>().IsSpawned);

        if(IsServer || IsHost)
        {
            StartCoroutine(ProcessStrokeBufferCoroutine());
        }

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

    // Coroutine that processes buffered stroke commands.
    private IEnumerator ProcessStrokeBufferCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(strokeUpdateInterval);
            if (strokeBuffer.Count > 0)
            {
                // Process all buffered strokes.
                foreach (var stroke in strokeBuffer)
                {
                    ApplyStroke(whiteboard.texture, stroke);
                }
                whiteboard.texture.Apply();

                // Clear the buffer after processing.
                strokeBuffer.Clear();
                latestTextureData = whiteboard.texture.EncodeToJPG(100);
                Debug.Log("[Server] Processed and broadcasted buffered strokes.");
            }
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
        
        StrokeCommand stroke = new StrokeCommand
        {
            posStart = posStart,
            posEnd = posEnd,
            colors = colors,
            brushSize = brushSize
        };
        strokeBuffer.Add(stroke);

        // Broadcast the update to all clients except the sender
        SendDrawCommandClientRpc(stroke, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { 
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                //.Where(id => id != serverRpcParams.Receive.SenderClientId)
                .ToArray() }
        });
    }

    [ClientRpc]
    public void SendDrawCommandClientRpc(StrokeCommand stroke, ClientRpcParams clientRpcParams = default)
    {
        if (!IsServer)
        {
            Debug.Log($"[Client] Received draw command from server.");
            ApplyStroke(whiteboard.texture, stroke);
        }
    }

    private void ApplyStroke(Texture2D texture, StrokeCommand stroke)
    {
        int startX = Mathf.RoundToInt(stroke.posStart.x);
        int startY = Mathf.RoundToInt(stroke.posStart.y);
        texture.SetPixels(startX, startY, stroke.brushSize, stroke.brushSize, stroke.colors);

        // Interpolate between start and end for a smooth stroke.
        for (float f = 0.01f; f < 1.0f; f += 0.05f)
        {
            int lerpX = Mathf.RoundToInt(Mathf.Lerp(stroke.posStart.x, stroke.posEnd.x, f));
            int lerpY = Mathf.RoundToInt(Mathf.Lerp(stroke.posStart.y, stroke.posEnd.y, f));
            texture.SetPixels(lerpX, lerpY, stroke.brushSize, stroke.brushSize, stroke.colors);
        }
        texture.Apply();
    }
}
