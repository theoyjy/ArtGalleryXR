using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;

public class WhiteboardMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize = 5;
    [SerializeField] private Transform _whiteboardTransform;
    public NetworkedCanvas networkedCanvas;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    //private Quaternion _lastTouchRot;

    private Camera _playerCamera;
    public bool _isHolding = false;
    private bool _isHovering = false;
    private bool _isSnapped = false;
    private Vector3 _offset;
    private Card card;

    void Start()
    {
        _renderer = _tip.GetComponent<Renderer>();
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        _tipHeight = _tip.localScale.y;
        //_drawPlane = new Plane(Vector3.forward, Vector3.zero); // Adjust if needed

        StartCoroutine(getCamera());
    }

    private IEnumerator getCamera()
    {
        yield return new WaitForSeconds(1.0f); // Blocks execution here

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Number of players: " + players.Length);

        while (players.Length == 0)
        {
            yield return new WaitForSeconds(0.5f); // Blocks execution here
            players = GameObject.FindGameObjectsWithTag("Player");
        }

        _playerCamera = players[players.Length - 1].GetComponentInChildren<Camera>();
        if (_playerCamera == null)
        {
            Debug.Log("Player camera was null. Fell back to main camera.");
            _playerCamera = Camera.main; // Ensure we're using the correct camera
        }
    }

    void Update()
    {
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        transform.localRotation = Quaternion.Euler(90, 180, 0);// 90);
        card = transform.parent.GetComponentInChildren<Card>();
        Quaternion quat = card.GetWorldQuatRot();
        transform.rotation *= quat;

#if UNITY_ANDROID
        Draw();
        //Debug.Log("Android");
#else
        if (_isHolding && Input.GetMouseButtonDown(0))
        {
            DropPen();
        }
        else if (_isHovering && !_isHolding && Input.GetMouseButtonDown(0)) // Left-click when hovering
        {
            GrabPen();   
        }
        
        if (Input.GetMouseButtonDown(1) && _isHolding)
        {
            _isSnapped = !_isSnapped;
            if(!_isSnapped)
                _touchedLastFrame = false;

        }
        if (_isHolding && _isSnapped)
        {
            MovePenWithMouse();
            Draw();
        }
        else if(_isHolding && !_isSnapped)
        {
            MovePenWithMouse();
        }     
    #endif
    }

    private void GrabPen()
    {
        _isHolding = true;
        _offset = transform.position - GetMouseWorldPosition();
        
    }

    private void DropPen()
    {
        _isHolding = false;
        _whiteboard = null;
        _touchedLastFrame = false;
        _isSnapped = false;
    }

    private void MovePenWithMouse()
    {
        //transform.position = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, _whiteboardTransform.position.z - 0.8f);
        if (_playerCamera == null || _whiteboardTransform == null) return;

        // Get the normal of the whiteboard
        card = transform.parent.GetComponentInChildren<Card>();
        Vector3 whiteboardNormal = card.GetNormal(); // Assuming forward is the normal direction

        // Create a plane representing the whiteboard's surface
        Plane whiteboardPlane = new Plane(whiteboardNormal, _whiteboardTransform.position);
        
        // Convert mouse position to world space ray
        Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);

        // Find intersection of the ray with the whiteboard's plane
        if (whiteboardPlane.Raycast(ray, out float distance))
        {
            // Get the world position where the mouse ray hits the whiteboard plane
            Vector3 hitPoint = ray.GetPoint(distance);

            // Set the pen's position to the hit point (so it "snaps" onto the board)
            transform.position = hitPoint - 0.8f * whiteboardNormal;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return transform.position;
    }

    private void Draw()
    {

#if UNITY_ANDROID
        if (_playerCamera == null || _whiteboardTransform == null) return;

        // Get the normal of the whiteboard
        card = transform.parent.GetComponentInChildren<Card>();
        Vector3 whiteboardNormal = card.GetNormal(); // Assuming forward is the normal direction

        // Create a plane representing the whiteboard's surface
        Plane whiteboardPlane = new Plane(whiteboardNormal, _whiteboardTransform.position);
        
        // Convert VR controller position to world space ray
        Ray ray = new Ray(transform.position, -transform.forward);

        // Find intersection of the ray with the whiteboard's plane
        if (whiteboardPlane.Raycast(ray, out float distance))
        {
            // Get the world position where the mouse ray hits the whiteboard plane
            Vector3 hitPoint = ray.GetPoint(distance);

            // Set the pen's position to the hit point (so it "snaps" onto the board)
            transform.position = hitPoint - 0.8f * whiteboardNormal;
        }
#endif
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
                if (x < 0 || x + (_penSize) > _whiteboard.textureSize.x ||
                    y < 0 || y + (_penSize) > _whiteboard.textureSize.y)
                {
                    return; // Stop drawing if part of the pen would go out of bounds
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

                    //transform.rotation = _lastTouchRot;

                    _whiteboard.texture.Apply();
                }

                // Send to network
                if (networkedCanvas != null)
                {
                    Vector2 currentPos = new Vector2(x, y);
                    networkedCanvas.SendDrawCommandServerRpc(_lastTouchPos, currentPos, _colors, _penSize);
                }
                else
                    Debug.Log("NetworkedCanvas is NULL");

                // Set for next iterations
                _lastTouchPos = new Vector2(x, y);
                //_lastTouchRot = transform.rotation;
                _touchedLastFrame = true;

                return;
            }
        }
        _whiteboard = null;
        _touchedLastFrame = false;
    }

    // Detect when the cursor hovers over the pen
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
    }
}
