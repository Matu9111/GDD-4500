using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GDD4500.LAB01
{
    public class BulletHandler : MonoBehaviour
    {

        [SerializeField] private float _LifeTime = 10f;
        [SerializeField] private float _Speed = 5f;
        [SerializeField] private float _BulletRadius = 2.5f;
        [SerializeField] private Collider _Collider;
        [SerializeField] private Rigidbody _Rigidbody;
        [SerializeField] private Material _ExplosionMaterial;

        private IEnumerator Start()
        {
            Destroy(this.gameObject, _LifeTime);
            yield return new WaitForSeconds(0.5f);
            _Collider.enabled = true;
        }

        public void OnCollisionEnter(Collision collision)
        {
            collision.collider.gameObject.SendMessageUpwards("OnBulletHit", this.gameObject, SendMessageOptions.DontRequireReceiver);
            
            _Collider.enabled = false;
            Destroy(_Rigidbody);

            GetComponent<MeshRenderer>().material = _ExplosionMaterial;
            
            StartCoroutine(GrowAndCheckColliders());
        }

    private IEnumerator GrowAndCheckColliders()
    {
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(_BulletRadius, _BulletRadius, _BulletRadius);
        float growDuration = .25f; // Duration over which the bullet grows
        float elapsedTime = 0f;

        while (elapsedTime < growDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / growDuration);
            elapsedTime += Time.deltaTime;

            // Check for colliders within the bullet's current scale
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, transform.localScale.x / 2);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider != _Collider) // Avoid self-collision
                {
                    hitCollider.gameObject.SendMessageUpwards("OnBulletHit", this.gameObject, SendMessageOptions.DontRequireReceiver);
                }
            }

            yield return null;
        }

        // Ensure final scale is set
        transform.localScale = targetScale;

        Destroy(this.gameObject);
    }
    }
}
