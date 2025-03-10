using Unity.Netcode;
using System.IO.Compression;
using UnityEngine;
using System.IO;

public class TextureSyncManager : NetworkBehaviour
{
    // 一个测试用的贴图
    private Texture2D textureToSend;
    public Whiteboard whiteboard;

    private void Start()
    {
        // 创建一个测试用的贴图
        textureToSend = whiteboard.texture;
    }

    private void Awake()
    {
        if(!textureToSend)
        {
            textureToSend = whiteboard.texture;
        }
        
    }


    // -------------- 客户端调用 --------------
    // 当客户端想把贴图发送给所有其他客户端时：
    public void SendTextureToServer()
    {

        //if (!IsClient)
        //{
        //    Debug.LogError("Only clients can send textures to the server!: Client" + IsClient + " Server: " + IsServer + " IsHost: " + IsHost);
        //    return;
        //}

        if ((IsServer || IsHost) && !GetComponent<NetworkObject>().IsSpawned)
        {
            GetComponent<NetworkObject>().Spawn();
            Debug.Log("TextureSyncManager spawned. IsSpawned: " + GetComponent<NetworkObject>().IsSpawned);
        }

        if (!textureToSend)
        {
            Debug.LogError("No texture assigned to send!1");
            try
            {
                textureToSend = whiteboard.texture;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error getting texture from whiteboard: " + e.Message);
                return;
            }
        }

       
        // 把贴图转为字节
        byte[] texData = textureToSend.EncodeToPNG();
        // Compresse the texture data
        texData = Compress(texData);


        // 调用 ServerRpc，把字节数据传给服务器
        SendTextureToServerRpc(texData);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SendTextureToServer();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("TextureSyncManager spawned. IsSpawned: " + GetComponent<NetworkObject>().IsSpawned);
        // It is now safe to call ServerRpc functions
        SendTextureToServer();
    }


    [ServerRpc(RequireOwnership = false)]
    private void SendTextureToServerRpc(byte[] textureBytes, ServerRpcParams serverRpcParams = default)
    {
        // 服务器收到客户端发送的贴图后，可以做一些处理
        // 比如保存起来、校验等，这里简单演示一下：
        Debug.Log($"[Server] Received texture from client, size: {textureBytes.Length} bytes.");

        // 然后把贴图再广播给所有客户端
        BroadcastTextureToClientRpc(textureBytes);
    }

    // -------------- 服务器广播给客户端 --------------
    [ClientRpc]
    private void BroadcastTextureToClientRpc(byte[] textureBytes, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[Client] Received broadcasted texture, size: {textureBytes.Length} bytes.");

        // 在客户端这边还原 Texture2D
        Texture2D receivedTexture = new Texture2D(2, 2); // 初始大小随便给
        textureBytes = Decompress(textureBytes);
        bool isLoaded = receivedTexture.LoadImage(textureBytes);
        if (!isLoaded)
        {
            Debug.LogWarning("[Client] Failed to load texture from received bytes!");
            return;
        }

        // 这里可以把这张贴图赋值给某个 UI 或 3D 物体的材质
        ApplyTextureToUIOrObject(receivedTexture);
    }

    private void ApplyTextureToUIOrObject(Texture2D tex)
    {
        // 示例：把贴图赋值到一个 RawImage
        // rawImage.texture = tex;

        // 或者赋值到一个材质
        // myRenderer.material.mainTexture = tex;

        Debug.Log("[Client] Successfully applied the received texture!");
    }

    private byte[] Compress(byte[] data)
    {
        using (MemoryStream output = new MemoryStream())
        {
            using (GZipStream gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                gzip.Write(data, 0, data.Length);
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
