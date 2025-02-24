using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class NetworkedCanvas : NetworkBehaviour
{
    public Texture2D canvasTexture;
    private Whiteboard whiteboard;

    private void Awake()
    {
        whiteboard = GetComponent<Whiteboard>();
        canvasTexture = whiteboard.texture;
    }

    // Called when a player draws on the canvas
    [ServerRpc(RequireOwnership = false)]
    public void SendDrawCommandServerRpc(Vector2 posStart, Vector2 posEnd, Color[] colors, int brushSize)
    {
        // Update the master texture on the server
        UpdateCanvas(canvasTexture, posStart, posEnd, colors, brushSize);

        // Broadcast the update to all clients
        SendDrawCommandClientRpc(posStart, posEnd, colors, brushSize);
    }

    [ClientRpc]
    public void SendDrawCommandClientRpc(Vector2 posStart, Vector2 posEnd, Color[] colors, int brushSize)
    {
        // On clients, update the local texture with the same drawing command
        if (!IsServer)
        {
            UpdateCanvas(canvasTexture, posStart, posEnd, colors, brushSize);
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
