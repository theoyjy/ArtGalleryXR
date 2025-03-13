using Unity.Netcode;
using System.IO.Compression;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public class TextureSyncManager : NetworkBehaviour
{
    public Whiteboard whiteboard;

    private static byte[] latestTextureData; // Store the latest texture on the server


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("TextureSyncManager spawned. IsSpawned: " + GetComponent<NetworkObject>().IsSpawned);

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

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { requesterClientId }
                }
            };

            SendLatestTextureToClientRpc(latestTextureData, clientRpcParams);
        }
        else
        {
            Debug.Log("[Server] No stored texture available.");
        }
    }

    // ---------- Server Sends the Latest Texture to Requesting Client ----------
    [ClientRpc]
    private void SendLatestTextureToClientRpc(byte[] textureBytes, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[Client] Received initial texture, size: {textureBytes.Length} bytes.");

        //textureBytes = Decompress(textureBytes);

        Texture2D receivedTexture = new Texture2D(2, 2);
        if (!receivedTexture.LoadImage(textureBytes))
        {
            Debug.LogWarning("[Client] Failed to load texture from received bytes!");
            return;
        }

        ApplyTextureToUIOrObject(receivedTexture);
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
            byte[] texData = whiteboard.texture.EncodeToJPG(75);
            //File.WriteAllBytes("C:/Users/sky/BeforeSend.jpg", texData);
            //File.WriteAllBytes("C:/Users/sky/BeforeSend.jpg", whiteboard.texture.EncodeToJPG());

            //byte[] texDataCompressed = Compress(texData);
            //File.WriteAllBytes("C:/Users/sky/BeforeSendCompressed.gz", texDataCompressed);
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
        //ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log($"[Server] Received texture from client .., size: {textureBytes.Length} bytes.");

        // Store the latest texture on the server
        latestTextureData = textureBytes;

        // Broadcast to all clients except the sender
        //ClientRpcParams clientRpcParams = new ClientRpcParams
        //{
        //    Send = new ClientRpcSendParams
        //    {
        //        TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
        //            .Where(clientId => clientId != senderClientId)
        //            .ToArray()
        //    }
        //};


        BroadcastTextureToClientRpc(textureBytes);//, clientRpcParams);
    }

    // ---------- Server Broadcasts Texture to Other Clients ----------
    [ClientRpc]
    private void BroadcastTextureToClientRpc(byte[] textureBytes, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[Client] Received broadcasted texture, size: {textureBytes.Length} bytes.");

        textureBytes = Decompress(textureBytes);


        Texture2D receivedTexture = new Texture2D(2, 2);

        if (!receivedTexture.LoadImage(textureBytes))
        {
            Debug.LogWarning("[Client] Failed to load texture from received bytes!");
            return;
        }

        File.WriteAllBytes("C:/Users/sky/Recieved.jpg", receivedTexture.EncodeToJPG());
        ApplyTextureToUIOrObject(receivedTexture);
    }

    private void ApplyTextureToUIOrObject(Texture2D tex)
    {
        whiteboard.ApplyTexture(tex);
        Debug.Log("[Client] Successfully applied the received texture!");
    }

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
