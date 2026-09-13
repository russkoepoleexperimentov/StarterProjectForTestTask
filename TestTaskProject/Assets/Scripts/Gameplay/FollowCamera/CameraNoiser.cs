using System;
using UnityEngine;
using Unity.Cinemachine;


public class CameraNoiser : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _startVelocity = 45;
    [SerializeField] private float _endVelocity = 100;
    [SerializeField] private CinemachineBasicMultiChannelPerlin _noise;
    
    private void Update()
    {
        var speedKph = _rigidbody.linearVelocity.magnitude * 3.6f;
        var factor = Mathf.InverseLerp(_startVelocity, _endVelocity, speedKph);
        _noise.AmplitudeGain = factor;
    }
}
