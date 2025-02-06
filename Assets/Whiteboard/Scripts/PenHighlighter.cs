using UnityEngine;
using UnityEngine.EventSystems;

public class PenHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform _tip;
    private Renderer _penRenderer;
    private Color _originalColor;
    [SerializeField] private Color _highlightColor = Color.yellow; // Set in Inspector

    void Start()
    {
        _penRenderer = _tip.GetComponent<Renderer>();
        _originalColor = _penRenderer.material.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _penRenderer.material.color = _highlightColor; // Change to highlight color
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _penRenderer.material.color = _originalColor; // Restore original color
    }
}
