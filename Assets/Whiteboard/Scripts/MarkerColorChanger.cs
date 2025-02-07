using UnityEngine;

public class MarkerColorChanger : MonoBehaviour
{
    public Color newColor = Color.red; // Default color
    public GameObject tip;
    private Material markerMaterial;
   
    void Start()
    {
        // Find the material named "Marker" in the Resources folder or assigned materials
        markerMaterial = tip.GetComponent<Renderer>().material;

        if (markerMaterial == null)
        {
            Debug.LogError("Material 'Marker' not found! Make sure it's in a Resources folder.");
            return;
        }
    }

    void Update()
    {
        markerMaterial.color = newColor;
    }

    public void ChangeColor(Color color)
    {
        if (markerMaterial != null)
        {
            markerMaterial.color = newColor;
        }
    }
}