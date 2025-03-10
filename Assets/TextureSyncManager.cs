using Unity.Netcode;
using System.IO.Compression;
using UnityEngine;
using System.IO;

public class TextureSyncManager : NetworkBehaviour
{
    // һ�������õ���ͼ
    private Texture2D textureToSend;
    public Whiteboard whiteboard;

    private void Start()
    {
        // ����һ�������õ���ͼ
        textureToSend = whiteboard.texture;
    }

    private void Awake()
    {
        if(!textureToSend)
        {
            textureToSend = whiteboard.texture;
        }
        
    }


    // -------------- �ͻ��˵��� --------------
    // ���ͻ��������ͼ���͸����������ͻ���ʱ��
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

       
        // ����ͼתΪ�ֽ�
        byte[] texData = textureToSend.EncodeToPNG();
        // Compresse the texture data
        texData = Compress(texData);


        // ���� ServerRpc�����ֽ����ݴ���������
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
        // �������յ��ͻ��˷��͵���ͼ�󣬿�����һЩ����
        // ���籣��������У��ȣ��������ʾһ�£�
        Debug.Log($"[Server] Received texture from client, size: {textureBytes.Length} bytes.");

        // Ȼ�����ͼ�ٹ㲥�����пͻ���
        BroadcastTextureToClientRpc(textureBytes);
    }

    // -------------- �������㲥���ͻ��� --------------
    [ClientRpc]
    private void BroadcastTextureToClientRpc(byte[] textureBytes, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[Client] Received broadcasted texture, size: {textureBytes.Length} bytes.");

        // �ڿͻ�����߻�ԭ Texture2D
        Texture2D receivedTexture = new Texture2D(2, 2); // ��ʼ��С����
        textureBytes = Decompress(textureBytes);
        bool isLoaded = receivedTexture.LoadImage(textureBytes);
        if (!isLoaded)
        {
            Debug.LogWarning("[Client] Failed to load texture from received bytes!");
            return;
        }

        // ������԰�������ͼ��ֵ��ĳ�� UI �� 3D ����Ĳ���
        ApplyTextureToUIOrObject(receivedTexture);
    }

    private void ApplyTextureToUIOrObject(Texture2D tex)
    {
        // ʾ��������ͼ��ֵ��һ�� RawImage
        // rawImage.texture = tex;

        // ���߸�ֵ��һ������
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
