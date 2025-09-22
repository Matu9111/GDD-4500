using UnityEngine;

namespace GDD4500.LAB01
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMoveMechanic : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _Acceleration = 10;
        [SerializeField] private float _MaxSpeed = 20;
        [SerializeField] private float _Deceleration = 0.85f;



        private Rigidbody _rigidbody;
        private PlayerInputHandler _inputHandler;

        private Vector3 _moveVector;

        public void Initialize(PlayerInputHandler handler)
        {
            _inputHandler = handler;
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void DoMove(Vector2 value)
        {
            _moveVector.x += value.x;
            _moveVector.z += value.y;

            _moveVector = Vector3.ClampMagnitude(_moveVector, _MaxSpeed);

            this.transform.forward = _moveVector;



        }

        public void Update()
        {
            _moveVector *= _Deceleration;
        }

        public void FixedUpdate()
        {
            if (_rigidbody == null) return;
            _rigidbody.AddForce(_moveVector * _Acceleration);
        }
    }
}
