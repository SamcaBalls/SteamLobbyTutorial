using SteamLobbyTutorial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Headbob : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool _enable = true;
    [SerializeField, Range(0, 0.1f)] public float _Amplitude = 0.015f; [SerializeField, Range(0, 30)] public float _frequency = 10.0f;
    [SerializeField] private Transform _camera = null; [SerializeField] private Transform _cameraHolder = null;
    [SerializeField] private PlayerMovementHandler movScript;

    private Vector3 _startPos;



    private void Awake()
    {
        _startPos = _camera.localPosition;
    }

    void Update()
    {
        if (!_enable) return;
        CheckMotion();
        ResetPosition();
        _camera.LookAt(FocusTarget());
    }
    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * _frequency) * _Amplitude;
        pos.x += Mathf.Cos(Time.time * _frequency / 2) * _Amplitude * 2;
        return pos;
    }
    private void CheckMotion()
    {
        float speed = new Vector3(movScript.controller.velocity.x, 0, movScript.controller.velocity.z).magnitude;
        //if (!movScript.isGrounded) return;
        if (movScript.controller.velocity == Vector3.zero) return;
        PlayMotion(FootStepMotion());
    }
    private void PlayMotion(Vector3 motion)
    {
        _camera.localPosition += motion;
    }

    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + _cameraHolder.localPosition.y, transform.position.z);
        pos += _cameraHolder.forward * 15.0f;
        return pos;
    }
    private void ResetPosition()
    {
        if (_camera.localPosition == _startPos) return;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, _startPos, 1 * Time.deltaTime);
    }
}
