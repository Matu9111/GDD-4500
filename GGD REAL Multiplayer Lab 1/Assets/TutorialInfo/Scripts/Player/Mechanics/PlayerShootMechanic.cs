using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDD4500.LAB01
{
    public class PlayerShootMechanic : MonoBehaviour
    {
        [SerializeField] private Transform _ShootPoint;
        [SerializeField] private GameObject _BulletPrefab;

        [Header("Settings")]
        [SerializeField] private float _ChargeRate = 2f;
        [SerializeField] private float _MinShootForce = 1f;
        [SerializeField] private float _MaxShootForce = 10f;
        [SerializeField] private float _ArcHeight = 1f;
        [SerializeField] private float _ShootModifier = 0.1f;

        private PlayerInputHandler _inputHandler;

        private List<Vector3> _ArcPoints = new List<Vector3>();

        public void Initialize(PlayerInputHandler playerInputHandler)
        {
            _inputHandler = playerInputHandler;
        }

        private float _currentChargeTime = 0f;
        private bool _isCharging = false;

        public void OnCharge()
        {
            _isCharging = true;
            _currentChargeTime = 0f;
        }

        public void Update()
        {
            if (_isCharging)
            {
                _currentChargeTime += Time.deltaTime * _ChargeRate;
                //float chargeRatio = Mathf.Clamp01(_currentChargeTime / _MaxShootForce);
                float shootForce = Mathf.Lerp(_MinShootForce, _MaxShootForce, _currentChargeTime);

                // Calculate the arc points
                _ArcPoints.Clear();
                Vector3 startPoint = _ShootPoint.position;
                Vector3 velocity = _ShootPoint.forward * shootForce;
                for (float t = 0; t < 2f; t += 0.1f)
                {
                    Vector3 point = startPoint + velocity * t + 0.5f * Physics.gravity * t * t;
                    point.y += _ArcHeight * Mathf.Sin(Mathf.Clamp01(t / 2f) * Mathf.PI);
                    _ArcPoints.Add(point);
                }
            }
        }

        public void OnRelease()
        {
            if (_isCharging)
            {
                _isCharging = false;
                float chargeRatio = Mathf.Clamp01(_currentChargeTime / _MaxShootForce);
                float shootForce = Mathf.Lerp(_MinShootForce, _MaxShootForce, chargeRatio);

                // Instantiate and launch the projectile
                GameObject bullet = Instantiate(_BulletPrefab, _ShootPoint.position, _ShootPoint.rotation);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    if (_ArcPoints.Count >= 3)
                    {
                        Vector3 direction = (_ArcPoints[2] - _ArcPoints[1]).normalized;
                        bulletRb.AddForce(direction * shootForce * _ShootModifier, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}
