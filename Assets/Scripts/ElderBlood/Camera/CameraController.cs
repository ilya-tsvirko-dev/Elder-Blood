using System;
using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _target;
    [SerializeField] private float _speed;
    [SerializeField] private Vector3 _offset;
    [Space]
    [SerializeField] private float radius = 1f;
    [Space] 
    [SerializeField] private bool _debug;
    [SerializeField] private int segments = 32;
    [SerializeField] private float _zPosition = 0f;
    [SerializeField] private LineRenderer lineRenderer;
    [Space]
    [SerializeField] private float _magn;
    
    private Vector3 _lastPosition;
    
    private void Start()
    {
        if (_target != null)
            SnapToTarget();

        if (_debug)
        {
            lineRenderer.gameObject.SetActive(true);
            
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = segments;

            Vector3[] points = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                points[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                points[i].z = _zPosition;
            }

            lineRenderer.SetPositions(points);
        }
        else
        {
            lineRenderer.gameObject.SetActive(false);
        }
    }

    private void OnValidate()   
    {
        if (_debug)
        {
            lineRenderer.gameObject.SetActive(true);
            
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = segments;

            Vector3[] points = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                points[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                points[i].z = _zPosition;
            }

            lineRenderer.SetPositions(points);
        }
        else
        {
            lineRenderer.gameObject.SetActive(false);
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        SnapToTarget();
    }

    private void SnapToTarget()
    {
        _mainCamera.transform.position = _target.position + _offset;
        _lastPosition = _mainCamera.transform.position;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        _mainCamera.transform.position =  GetCameraPosition();
    }

    private Vector3 GetCameraPosition()
    {
        _magn = (_lastPosition - (_target.position + _offset)).magnitude;
        
        if (_magn < radius)
        {
            return _lastPosition;
        }
        
        _lastPosition = Vector3.Lerp(_lastPosition, _target.position + _offset, _speed * Time.deltaTime);
        
        return _lastPosition;
    }
}
