using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Car.View
{
    public class CarWheelView : MonoBehaviour
    {
        [field: Header("Shape")] 
        [field: SerializeField] public float Radius { get; set; } = 0.3f;
        [field: SerializeField] public float Mass { get; set; } = 20f;
        
        [field: Header("Suspension")]
        [field: SerializeField] public float SpringStiffness { get; set; } = 30000f;
        [field: SerializeField] public float SpringDamper { get; set; } = 1500f;
        [field: SerializeField] public float SpringMaxLength { get; set; } = .5f;
        
        // x: [0..1] input slip ratio, y: output slip
        [field: Header("Grip")]
        [field: SerializeField] public AnimationCurve ForwardSlipRemap { get; set; } 
        [field: SerializeField] public AnimationCurve SidewaysSlipRemap { get; set; }


        [Header("Visuals")] 
        [SerializeField] private Transform _visual;

        private float _rollRadians = 0;
        
        
        private Vector3 WheelForward => Quaternion.Euler(0f, SteerAngle, 0f) * transform.forward;
        private Vector3 WheelRight => Quaternion.Euler(0f, SteerAngle, 0f) * transform.right;

        private float _lastSpringLength;

        private float _lngSlip;
        private float _latSlip;
        private float _slipVelocity;

        private float _feedbackImpulse;

        
        private Rigidbody _carRigidBody;
        private float _wheelAngularVelocity = 0f;
        
        // api
        public float SteerAngle { get; set; } = 0;
        public float MotorTorque { get; set; } = 0;
        public float BrakeTorque { get; set; } = 0;
        public float RPM => _wheelAngularVelocity * 30 / Mathf.PI;
        public float FeedbackImpulse => _feedbackImpulse;
        public float Inertia => 0.5f * Mass * Radius * Radius;

        // slip integration constants
        private const float RELAX_LNG = .01f; 
        private const float RELAX_LAT = .01f; 

        private void Awake()
        {
            _carRigidBody = GetComponentInParent<Rigidbody>();

            if (!_carRigidBody)
            {
                Debug.LogError($"Parent rigidbody for wheel '{gameObject.name}' not found! Disabling game object.");
                gameObject.SetActive(false);
            }

            if (!_visual && transform.childCount > 0)
            {
                _visual = transform.GetChild(0);
            }
        }

        public void AddAcceleration(float angularAcceleration)
        {
            _wheelAngularVelocity += angularAcceleration;
        }

        public void ApplyVisual() {  }

        private void FixedUpdate()
        {
            var springDir = transform.up;
            var wheelDidHit = WheelCast(
                transform.position, 
                -springDir, 
                SpringMaxLength,
                Radius,
                out var springLength,
                out var hit
                );

            var localForce = Vector3.zero;
            Vector3 loadForceVector;
            var rayEndPoint = transform.position - springDir * (springLength + Radius);

            if (wheelDidHit)
            {
                var springDepth = SpringMaxLength - springLength;
                var springForce = springDepth * SpringStiffness;
                
                var springSpeed = (_lastSpringLength - springLength) / Time.fixedDeltaTime;
                var damperForce = SpringDamper * springSpeed;
                
                var suspensionForce = Mathf.Max(0, springForce + damperForce);
                localForce.y = suspensionForce;
                loadForceVector = hit.normal * suspensionForce;
            }
            else
            {
                localForce.y = 0;
                loadForceVector = Vector3.zero;
            }
            
            // contact calculation
            var contactForward = wheelDidHit ? Vector3.ProjectOnPlane(WheelForward, hit.normal) : Vector3.zero;
            var contactRight = wheelDidHit ? Vector3.ProjectOnPlane(WheelRight, hit.normal) : Vector3.zero;
            var contactObjectVelocity = wheelDidHit && hit.rigidbody ? hit.rigidbody.GetPointVelocity(hit.point) : Vector3.zero;
            var contactVelocity = wheelDidHit
                ? Vector3.ProjectOnPlane(_carRigidBody.GetPointVelocity(hit.point) - contactObjectVelocity, hit.normal)
                : Vector3.zero;
            var contactLatVelocity = wheelDidHit ? Vector3.Dot(contactVelocity, contactRight) : 0f;
            var contactLngVelocity = wheelDidHit ? Vector3.Dot(contactVelocity, contactForward) : 0f;
            
            var tireMaxForce = localForce.y;
            
            
            var tWheel = (_wheelAngularVelocity - contactLngVelocity / Radius) * Inertia / Time.fixedDeltaTime;
            tWheel = Mathf.Clamp(tWheel, -tireMaxForce, tireMaxForce);

            var wheelLinearVelocity = _wheelAngularVelocity * Radius;
            var vWheelDelta = wheelLinearVelocity - contactLngVelocity;
            var contactLngVelocityAbs = Mathf.Abs(contactLngVelocity);

            var lngSlipRatio = contactLngVelocityAbs == 0f ? 0f : vWheelDelta / contactLngVelocityAbs;
            var latSlipRatio = CalculateLatSlipRatio(contactLngVelocity, contactLatVelocity);

            
            _slipVelocity = Mathf.Sqrt(vWheelDelta * vWheelDelta + contactLatVelocity * contactLatVelocity);
            
            // friction circle
            var slipUsage = Mathf.Sqrt(lngSlipRatio * lngSlipRatio + latSlipRatio * latSlipRatio);
            
            if (slipUsage > 1)
            {
                lngSlipRatio /= slipUsage;
                latSlipRatio /= slipUsage;
            }
            
            var desiredLatSlip = SidewaysSlipRemap.Evaluate(latSlipRatio) * -Mathf.Sign(contactLatVelocity);
            var desiredLngSlip = ForwardSlipRemap.Evaluate(Mathf.Abs(lngSlipRatio)) * Mathf.Sign(lngSlipRatio);
            
            // slip integration
            var lngSlipCoeff = Mathf.Clamp01(Mathf.Abs(vWheelDelta) / RELAX_LNG * Time.fixedDeltaTime);
            _lngSlip += (desiredLngSlip - _lngSlip) * lngSlipCoeff;
            
            var latSlipCoeff  = Mathf.Clamp01(Mathf.Abs(contactLatVelocity) / RELAX_LAT * Time.fixedDeltaTime);
            _latSlip += (desiredLatSlip - _latSlip) * latSlipCoeff;

            var angularAcceleration = -tWheel / Inertia;
            _wheelAngularVelocity += angularAcceleration * Time.fixedDeltaTime;

            var rollingResistanceCoeff = 0.0005f; // asphalt
            var tResistance = _wheelAngularVelocity * (localForce.y - Physics.gravity.y) * rollingResistanceCoeff * -1f;
            angularAcceleration = (MotorTorque + tResistance) / Inertia;
            _wheelAngularVelocity += angularAcceleration * Time.fixedDeltaTime;
            
            angularAcceleration = BrakeTorque / Inertia;
            _wheelAngularVelocity +=
                Mathf.Min(
                    Mathf.Abs(_wheelAngularVelocity),
                    Mathf.Abs(angularAcceleration) * Time.fixedDeltaTime
                ) * -Mathf.Sign(_wheelAngularVelocity);

            _rollRadians += _wheelAngularVelocity * Time.fixedDeltaTime;
            _rollRadians %= Mathf.PI * 2;
            
            localForce.x = _latSlip * tireMaxForce;
            localForce.z = _lngSlip * tireMaxForce;
            
            var latForceVector = contactRight * localForce.x;
            var lngForceVector = contactForward * localForce.z;

            _carRigidBody.AddForceAtPosition(loadForceVector + lngForceVector + latForceVector, rayEndPoint,
                ForceMode.Force);
            
            Debug.DrawRay(rayEndPoint, contactRight * _latSlip, Color.blue);
            Debug.DrawRay(rayEndPoint, contactForward * _lngSlip, Color.blue);
            Debug.DrawRay(rayEndPoint, WheelForward * Mathf.Sign(MotorTorque), Color.green);
            Debug.DrawRay(rayEndPoint, WheelForward * Mathf.Sign(BrakeTorque), Color.red);

            if (_visual)
            {
                _visual.position = transform.position - springDir * springLength;
                _visual.localRotation = Quaternion.Euler(_rollRadians * Mathf.Rad2Deg, SteerAngle, 0);
            }
            
            // impulse calculations 
            var totalFrictionTorque = tResistance + BrakeTorque;
            var totalFrictionTorqueImpulse = totalFrictionTorque * Time.fixedDeltaTime;
            var roadTorqueImpulse = localForce.z * Radius * Time.fixedDeltaTime;
            var clampedSelfImpulse = Mathf.Clamp(_wheelAngularVelocity * Inertia, -totalFrictionTorqueImpulse, totalFrictionTorqueImpulse);
            _feedbackImpulse = - (clampedSelfImpulse + roadTorqueImpulse);
            
            
            _lastSpringLength = springLength;
        }

        public float GetSlipVelocity() => _slipVelocity;

        private float CalculateLatSlipRatio(float vLong, float vLat)
        {
            if (Mathf.Approximately(vLat, 0f))
            {
                return 0f;
            }

            var slipAngleRad = Mathf.Atan2(Mathf.Abs(vLat), Mathf.Abs(vLong));
            return Mathf.Clamp01(slipAngleRad / (Mathf.PI * 0.5f));
        }

        private bool WheelCast(Vector3 origin, Vector3 direction, float suspensionLength, float wheelRadius, 
            out float springLength, 
            out RaycastHit hit)
        {
            var isGrounded = Physics.Raycast(origin, direction, out hit, suspensionLength + wheelRadius);

            if (!isGrounded)
            {
                springLength = suspensionLength;
                return false;
            }
    
            springLength = hit.distance - wheelRadius;
            return true;
        }
    }
}
