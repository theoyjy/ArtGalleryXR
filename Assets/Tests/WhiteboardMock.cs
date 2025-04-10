using UnityEngine;

public class MockWhiteboard : Whiteboard
{
    public Texture2D LastAppliedTexture;
    public bool ClearCalled = false;

    public override void ApplyTexture(Texture2D tex)
    {
        LastAppliedTexture = tex;
    }

    public override void ClearWhiteboard()
    {
        ClearCalled = true;
    }

    public override Texture2D ResizeTexture(Texture2D input, int w, int h)
    {
        return input;
    }
}
