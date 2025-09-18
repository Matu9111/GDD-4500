using System;
using UnityEngine;
using TMPro;

namespace GDD4500.LAB02
{
    [RequireComponent(typeof(Rigidbody))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private float _Force = 10f;
        [SerializeField] private float _MaxSpeed = 10f;


        [SerializeField] private ParticleSystem _ExplosionParticles;

        [SerializeField] private MeshRenderer _PlayerMaterial;
        [SerializeField] private TextMeshPro _PlayerName;

        private Rigidbody _rigidbody;   
        private Server _server;
        internal void Initialize(string name, Server server)
        {
            _server = server;

            _rigidbody = GetComponent<Rigidbody>();
            this.name = name;

            _PlayerMaterial.material.color = UnityEngine.Random.ColorHSV(0, 1, 0, 1, .5f, .85f);

            _PlayerName.text = name;

            Vector3 randomUpwardDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), -1f, UnityEngine.Random.Range(-1f, 1f)).normalized;
            transform.forward = randomUpwardDirection;
            _rigidbody.AddForce(randomUpwardDirection * _Force, ForceMode.Impulse);
        }

        public void Update()
        {
            _PlayerName.transform.eulerAngles = Vector3.zero;
            _PlayerName.transform.position = this.transform.position + Vector3.up;
        }

        public void AddImpulse()
        {
            Vector3 randomUpwardDirection = new Vector3(UnityEngine.Random.Range(-.25f, .25f), 1f, UnityEngine.Random.Range(-1f, 1f)).normalized;
            _rigidbody.AddForce(Vector3.up * _Force , ForceMode.Impulse);
            _rigidbody.linearVelocity = Vector3.ClampMagnitude(_rigidbody.linearVelocity, _MaxSpeed);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Finish"))
            {
                _server.OnPlayerDestroyed(this);
                Destroy(this.gameObject);
            }
        }
    }
}
