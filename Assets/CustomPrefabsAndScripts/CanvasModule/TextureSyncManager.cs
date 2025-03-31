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

    bool IsClearing = false;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("TextureSyncManager spawned. IsSpawned: " + GetComponent<NetworkObject>().IsSpawned);

        try
        {
            if (IsServer || IsHost)
            {
                StartCoroutine(ProcessStrokeBufferCoroutine());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[OnNetworkSpawn] Error starting stroke buffer coroutine: {e.Message}\n{e.StackTrace}");
        }

        // Register custom message handlers
        try
        {
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
        }
        catch (Exception e)
        {
            Debug.LogError($"[OnNetworkSpawn] Error registering custom message handler: {e.Message}\n{e.StackTrace}");
        }

        try
        {
            if (IsClient && !IsHost)
            {
                Debug.Log("[Client] Requesting latest texture from server...");
                RequestLatestTextureFromServerRpc();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[OnNetworkSpawn] Error requesting latest texture from server: {e.Message}\n{e.StackTrace}");
        }
    }

    void Update()
    {
        try
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                SendTextureToServer();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Update] Error checking key down / sending texture to server: {e.Message}\n{e.StackTrace}");
        }
    }

    // Coroutine that processes buffered stroke commands.
    private IEnumerator ProcessStrokeBufferCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(strokeUpdateInterval);

            try
            {
                if (strokeBuffer.Count > 0)
                {
                    // Process all buffered strokes.
                    foreach (var stroke in strokeBuffer)
                    {
                        ApplyStroke(whiteboard.texture, stroke);
                    }

                    if(IsClearing)
                    {
                        whiteboard.ClearWhiteboard();
                        IsClearing = false;
                        continue;
                    }

                    whiteboard.texture.Apply();

                    // Clear the buffer after processing.
                    strokeBuffer.Clear();
                    latestTextureData = whiteboard.texture.EncodeToJPG(100);
                    Debug.Log("[Server] Processed and broadcasted buffered strokes.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProcessStrokeBufferCoroutine] Error processing stroke buffer: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    // ---------- Client Requests the Latest Texture ----------
    [ServerRpc(RequireOwnership = false)]
    private void RequestLatestTextureFromServerRpc(ServerRpcParams serverRpcParams = default)
    {
        try
        {
            ulong requesterClientId = serverRpcParams.Receive.SenderClientId;

            if (latestTextureData != null && latestTextureData.Length > 0)
            {
                Debug.Log($"[Server] Sending stored texture to new client: {requesterClientId}");
                SendTextureToClientCustomMessage(requesterClientId, latestTextureData);
            }
            else
            {
                Debug.Log("[Server] No stored texture available.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[RequestLatestTextureFromServerRpc] Error sending texture to client: {e.Message}\n{e.StackTrace}");
        }
    }

    // ---------- Server Sends the Latest Texture to Requesting Client ----------
    private void SendTextureToClientCustomMessage(ulong clientId, byte[] textureBytes)
    {
        try
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
        catch (Exception e)
        {
            Debug.LogError($"[SendTextureToClientCustomMessage] Error sending texture to client {clientId}: {e.Message}\n{e.StackTrace}");
        }
    }

    // ---------- Client Sends Texture to Server ----------
    public void SendTextureToServer()
    {
        try
        {
            if ((IsServer || IsHost) && !GetComponent<NetworkObject>().IsSpawned)
            {
                GetComponent<NetworkObject>().Spawn();
            }

            int rate = 75;
            byte[] texData;
            do
            {
                texData = whiteboard.texture.EncodeToJPG(rate);
                rate -= 5;
            } while (texData.Length > 65000 && rate > 0);


            SendTextureToServerRpc(texData);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SendTextureToServer] Error in sending texture to server: {e.Message}\n{e.StackTrace}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendTextureToServerRpc(byte[] textureBytes, ServerRpcParams serverRpcParams = default)
    {
        try
        {
            Debug.Log($"[Server] Received texture from client, size: {textureBytes.Length} bytes.");


            strokeBuffer.Clear();

            // Store the latest texture on the server
            latestTextureData = textureBytes;
            whiteboard.texture.LoadImage(textureBytes);
            

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
        catch (Exception e)
        {
            Debug.LogError($"[SendTextureToServerRpc] Error receiving/broadcasting texture from client: {e.Message}\n{e.StackTrace}");
        }
    }

    // ---------- Handle Received Texture Data ----------
    private void OnTextureMessageReceived(FastBufferReader reader)
    {
        try
        {
            Debug.Log("[Client] Received texture data via custom message.");

            // Allocate a byte array to store the received data
            int length;
            reader.ReadValueSafe(out length);

            byte[] textureBytes = new byte[length];
            reader.ReadBytesSafe(ref textureBytes, length);

            // Load the texture
            Texture2D receivedTexture = new Texture2D(2, 2);
            if (!receivedTexture.LoadImage(textureBytes))
            {
                Debug.LogWarning("[Client] Failed to load texture from received bytes!");
                return;
            }

            // Apply the texture to the whiteboard
            ApplyTextureToUIOrObject(receivedTexture);
        }
        catch (Exception e)
        {
            Debug.LogError($"[OnTextureMessageReceived] Error reading/applying texture data: {e.Message}\n{e.StackTrace}");
        }
    }

    private void ApplyTextureToUIOrObject(Texture2D tex)
    {
        try
        {
            whiteboard.ApplyTexture(tex);
            Debug.Log("[Client] Successfully applied the received texture!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ApplyTextureToUIOrObject] Error applying texture to whiteboard: {e.Message}\n{e.StackTrace}");
        }
    }

    // ---------- Stroke Sync ----------
    [ServerRpc(RequireOwnership = false)]
    public void SendDrawCommandServerRpc(Vector2 posStart, Vector2 posEnd, Color[] colors, int brushSize, ServerRpcParams serverRpcParams = default)
    {
        try
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
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                        .Where(id => id != serverRpcParams.Receive.SenderClientId)
                        .ToArray()
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"[SendDrawCommandServerRpc] Error receiving draw command from client: {e.Message}\n{e.StackTrace}");
        }
    }

    [ClientRpc]
    public void SendDrawCommandClientRpc(StrokeCommand stroke, ClientRpcParams clientRpcParams = default)
    {
        try
        {
            if (!IsServer)
            {
                Debug.Log("[Client] Received draw command from server.");
                ApplyStroke(whiteboard.texture, stroke);
                whiteboard.texture.Apply();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SendDrawCommandClientRpc] Error applying draw command on client: {e.Message}\n{e.StackTrace}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendClearWhiteboardOprServerRpc(ServerRpcParams serverRpcParams = default)
    {
        try
        {
            Debug.Log($"[Server] Received clear command from client {serverRpcParams.Receive.SenderClientId}.");
            strokeBuffer.Clear();
            whiteboard.ClearWhiteboard();
            IsClearing = true;

            // Broadcast the update to all clients except the sender
            SendClearClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                        .Where(id => id != serverRpcParams.Receive.SenderClientId)
                        .ToArray()
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"[SendClearWhiteboardOprServerRpc] Error receiving clear command from client: {e.Message}\n{e.StackTrace}");
        }
    }

    [ClientRpc]
    public void SendClearClientRpc(ClientRpcParams clientRpcParams = default)
    {
        try
        {
            if (!IsServer)
            {
                Debug.Log("[Client] Received clear command from server.");
                whiteboard.ClearWhiteboard();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SendClearClientRpc] Error applying clear command on client: {e.Message}\n{e.StackTrace}");
        }
    }

    private void ApplyStroke(Texture2D texture, StrokeCommand stroke)
    {
        try
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
        }
        catch (Exception e)
        {
            Debug.LogError($"[ApplyStroke] Error applying stroke: {e.Message}\n{e.StackTrace}");
        }
    }
}
