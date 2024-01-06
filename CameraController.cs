using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private Transform _focus;
    [SerializeField, Range(0f, 20f)] private float _distance = 5f;
    [SerializeField, Min(0f)] private float _focusRadius = 1f;
    [SerializeField, Range(0, 360)] private float _rotationSpeed = 10f;
    [SerializeField, Range(-89, 89)] private float _minVerticalAngle = -30, _maxVericalAngle = 60;


    private const int MaxCameraDistance = 5, MinCameraDistance = 0;
    private float _cameraPanSmoothing = 0.3f;
    private float _currentVelocity = 0;
    private Vector3 _focusPoint, _previousFocusPoint;
    private Vector2 _orbitAngles = new Vector2(25f, 0);
    private float _currentRotationAngle;
    private float _currentRotationVelocity;


    private float _maxRotation = 180f;

    private float _minRotation = 0f;

    // private float _lastManualRotation;
    //private bool _manualRotation;
    private Camera _cam;
    private Transform _player;
    private List<ICameraListener> _listeners = new List<ICameraListener>();


    [field: SerializeField] public bool CameraIsPanning { get; private set; }

    public void AddListener(ICameraListener listener) {
        _listeners.Add(listener);
    }

    Vector3 CameraHalfExtends {
        get {
            Vector3 halfExtends;
            halfExtends.y = _cam.nearClipPlane * Mathf.Tan(0.5f * Mathf.Rad2Deg * _cam.fieldOfView);
            halfExtends.x = halfExtends.y * _cam.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }


    private void MessageListeners() {
        // should be called when everything is out of view
        
        foreach (ICameraListener listener in _listeners) {
            if (listener != null) {
                listener.UpdateLevel();
            }
        }
    }


    private void Awake() {
        _cam = GetComponent<Camera>();
        transform.localRotation = Quaternion.Euler(_orbitAngles);
        _focus = GameObject.Find("Player").transform;
    }

    private void LateUpdate() {
        if (_focus == null) return;

        UpdateFocusPoint();
        ConstrainAngles();

        Quaternion lookRotation = Quaternion.Euler(_orbitAngles);
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = _focusPoint - lookDirection * _distance;

        Vector3 rectOffset = lookDirection * _cam.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = _focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation,
                castDistance)) {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    private void UpdateFocusPoint() {
        _previousFocusPoint = _focusPoint;
        Vector3 targetPoint = _focus.position;
        if (_focusRadius > 0f) {
            float distance = Vector3.Distance(targetPoint, _focusPoint);
            _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, _focusRadius / distance);
        }
        else {
            _focusPoint = targetPoint;
        }
    }

    private void ConstrainAngles() {
        _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, _minVerticalAngle, _maxVericalAngle);

        if (_orbitAngles.y < 0f) {
            _orbitAngles.y += 360f;
        }
        else if (_orbitAngles.y >= 360f) {
            _orbitAngles.y -= 360f;
        }
    }
    
    public void PanCamera() { // on trigger enter
 
        if (!CameraIsPanning) {
            StartCoroutine(CameraPan());
        }
    }

    private IEnumerator CameraPan() {
        CameraIsPanning = true;
        MessageListeners();
        var targetAngle = _orbitAngles.y == _maxRotation ? _minRotation : _maxRotation;
        WaitForEndOfFrame endFrame = new WaitForEndOfFrame();
        while (Mathf.Abs(_orbitAngles.y - targetAngle) > 0.2f) {
            _orbitAngles.y = Mathf.Lerp(_orbitAngles.y, targetAngle, _cameraPanSmoothing * Time.deltaTime);
            yield return endFrame;
        }

        //invoke UI event for middle of screen to start walking
        _orbitAngles.y = targetAngle;
        CameraIsPanning = false;
    }
}

public interface ICameraListener {
    void UpdateLevel();
    CameraController Controller { get; set; }
}