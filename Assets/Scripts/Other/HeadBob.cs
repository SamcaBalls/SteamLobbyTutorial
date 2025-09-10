using SteamLobbyTutorial;
using UnityEngine;

public class Headbob : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool _enable = true;
    [SerializeField, Range(0, 0.1f)] public float _Amplitude = 0.015f;
    [SerializeField, Range(0, 30)] public float _BaseFrequency = 10.0f;
    [SerializeField] private Transform _camera = null;
    [SerializeField] private Transform _cameraHolder = null;
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

    private Vector3 FootStepMotion(float adjustedFrequency)
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * adjustedFrequency) * _Amplitude;
        pos.x += Mathf.Cos(Time.time * adjustedFrequency / 2) * _Amplitude * 2;
        return pos;
    }

    private void CheckMotion()
    {
        // rychlost hráče v rovině
        float speed = new Vector3(movScript.controller.velocity.x, 0, movScript.controller.velocity.z).magnitude;

        if (speed < 0.1f) return; // stojí → žádný bobbing

        // přizpůsobení frekvence rychlosti
        float adjustedFrequency = _BaseFrequency * (speed / movScript.moveSpeed);
        PlayMotion(FootStepMotion(adjustedFrequency));
    }

    private void PlayMotion(Vector3 motion)
    {
        _camera.localPosition += motion;
    }

    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x,
                                  transform.position.y + _cameraHolder.localPosition.y,
                                  transform.position.z);
        pos += _cameraHolder.forward * 15.0f;
        return pos;
    }

    private void ResetPosition()
    {
        if (_camera.localPosition == _startPos) return;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, _startPos, 1 * Time.deltaTime);
    }
}
