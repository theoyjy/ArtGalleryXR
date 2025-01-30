using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class WhiteboardMarker : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize = 5;
    [SerializeField] private Camera _mainCamera;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    private Quaternion _lastTouchRot;

    private bool _isHolding = false;
    private Vector3 _offset;
    private Plane _drawPlane;

    // For hovering
    private Color _originalColor;
    private bool _isHovering = false;
    private Renderer _penRenderer;

    void Start()
    {
        _renderer = _tip.GetComponent<Renderer>();
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        _tipHeight = _tip.localScale.y;

        _mainCamera = Camera.main;
        _drawPlane = new Plane(Vector3.forward, Vector3.zero); // Adjust if needed

        // Get pen renderer for highlighting
        _penRenderer = GetComponent<Renderer>();
        _originalColor = _penRenderer.material.color;
    }

    void Update()
    {
        CheckForHover();

        if (Input.GetMouseButtonDown(1)) // Left-click to grab/drop
        {
            if (_isHolding)
            {
                DropPen();
            }
            else
            {
                TryGrabPen();
            }
        }

        if (_isHolding)
        {
            MovePenWithMouse();
            Draw();
        }
    }

    private void TryGrabPen()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                _isHolding = true;
                _offset = transform.position - hit.point;
            }
        }
    }

    private void DropPen()
    {
        _isHolding = false;
        _whiteboard = null;
        _touchedLastFrame = false;
    }

    private void MovePenWithMouse()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (_drawPlane.Raycast(ray, out float distance))
        {
            transform.position = ray.GetPoint(distance) + _offset;
        }
    }

    private void CheckForHover()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!_isHovering)
                {
                    _isHovering = true;
                    _penRenderer.material.color = Color.yellow; // Highlight pen
                }
                return;
            }
        }

        if (_isHovering)
        {
            _isHovering = false;
            _penRenderer.material.color = _originalColor; // Reset pen color
        }
    }


    private void Draw()
    {
        if (Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight))
        {
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                }

                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2));
                var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));

                // Out of bounds check
                if (x < 0 || x > _whiteboard.textureSize.x ||
                    y < 0 || y > _whiteboard.textureSize.y)
                {
                    return;
                }

                if (_touchedLastFrame)
                {
                    _whiteboard.texture.SetPixels(x, y, _penSize, _penSize, _colors);

                    for (float f = 0.01f; f < 1.00f; f += 0.05f)
                    {
                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                        _whiteboard.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, _colors);
                    }

                    transform.rotation = _lastTouchRot;

                    _whiteboard.texture.Apply();
                }

                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        _whiteboard = null;
        _touchedLastFrame = false;
    }
}
