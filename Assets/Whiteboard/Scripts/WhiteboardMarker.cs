//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Unity.Collections;
//using UnityEngine;
//using UnityEngine.EventSystems; // Required for IPointerEnterHandler

//public class WhiteboardMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    [SerializeField] private Transform _tip;
//    [SerializeField] private int _penSize = 5;

//    private Renderer _renderer;
//    private Color[] _colors;
//    private float _tipHeight;
//    private bool _isHovering = false;
//    private bool _isHolding = false;
//    private Vector3 _offset;
//    private Camera _playerCamera;
//    private Plane _drawPlane;

//    void Start()
//    {
//        _renderer = _tip.GetComponent<Renderer>();
//        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
//        _tipHeight = _tip.localScale.y;
//        _drawPlane = new Plane(Vector3.forward, Vector3.zero); // Adjust as needed\       

//        StartCoroutine(getCamera());
//    }

//    private IEnumerator getCamera()
//    {
//        yield return new WaitForSeconds(1.0f); // Blocks execution here

//        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
//        Debug.Log("Number of players: " + players.Length);

//        _playerCamera = players[players.Length - 1].GetComponentInChildren<Camera>();
//        if (_playerCamera == null)
//        {
//            Debug.LogError("Player camera was null. Fell back to main camera.");
//            _playerCamera = Camera.main; // Ensure we're using the correct camera
//        }
//    }

//    void Update()
//    {
//        if (_isHovering && Input.GetMouseButtonDown(0)) // Left-click when hovering
//        {
//            if (_isHolding)
//            {
//                DropPen();
//            }
//            else
//            {
//                GrabPen();
//            }
//        }

//        if (_isHolding)
//        {
//            MovePenWithMouse();
//        }
//    }

//    private void GrabPen()
//    {
//        _isHolding = true;
//        _offset = transform.position - GetMouseWorldPosition();
//    }

//    private void DropPen()
//    {
//        _isHolding = false;
//    }

//    private void MovePenWithMouse()
//    {
//        //Vector3 newPosition = GetMouseWorldPosition() + _offset;
//        //transform.position = newPosition;
//        if (_playerCamera == null) return;

//        Vector3 mouseWorldPosition = _playerCamera.ScreenToWorldPoint(
//            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.5f) // Adjust depth dynamically
//        );

//        transform.position = mouseWorldPosition;
//    }

//    private Vector3 GetMouseWorldPosition()
//    {
//        Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);
//        if (_drawPlane.Raycast(ray, out float distance))
//        {
//            return ray.GetPoint(distance);
//        }
//        return transform.position;
//    }

//    // Detect when the cursor hovers over the pen
//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        _isHovering = true;
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        _isHovering = false;
//    }

//    private void Draw()
//    {
//        if (Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight))
//        {
//            if (_touch.transform.CompareTag("Whiteboard"))
//            {
//                if (_whiteboard == null)
//                {
//                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
//                }

//                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

//                var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2));
//                var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));

//                // Out of bounds check
//                if (x < 0 || x > _whiteboard.textureSize.x ||
//                    y < 0 || y > _whiteboard.textureSize.y)
//                {
//                    return;
//                }

//                if (_touchedLastFrame)
//                {
//                    _whiteboard.texture.SetPixels(x, y, _penSize, _penSize, _colors);

//                    for (float f = 0.01f; f < 1.00f; f += 0.05f)
//                    {
//                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
//                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
//                        _whiteboard.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, _colors);
//                    }

//                    transform.rotation = _lastTouchRot;

//                    _whiteboard.texture.Apply();
//                }

//                _lastTouchPos = new Vector2(x, y);
//                _lastTouchRot = transform.rotation;
//                _touchedLastFrame = true;
//                return;
//            }
//        }

//        _whiteboard = null;
//        _touchedLastFrame = false;
//    }
//}


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class WhiteboardMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize = 5;
    [SerializeField] private Transform _whiteboardTransform;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    //private Quaternion _lastTouchRot;

    private Camera _playerCamera;
    private bool _isHolding = false;
    private bool _isHovering = false;
    private Vector3 _offset;

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
        transform.rotation = Quaternion.Euler(0, 90, 90);
#if UNITY_ANDROID
        Draw();
        //Debug.Log("Android");
#else
        if(_isHolding && Input.GetMouseButtonDown(0))
        {
            DropPen();
        }
        else if (_isHovering && !_isHolding && Input.GetMouseButtonDown(0)) // Left-click when hovering
        {
            GrabPen();   
        }

        if (_isHolding)
        {
            MovePenWithMouse();
            Draw();
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
    }

    private void MovePenWithMouse()
    {
        if (_playerCamera == null) return;

        Vector3 mouseWorldPosition = _playerCamera.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.5f) // Adjust depth dynamically
        );

        transform.position = mouseWorldPosition;
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
        if (transform.position.z > 15.0f)
            transform.position = new Vector3(transform.position.x, transform.position.y, 16.1f);

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

                    //transform.rotation = _lastTouchRot;

                    _whiteboard.texture.Apply();
                }

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
