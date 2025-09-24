using UnityEngine;
using GDD4500.LAB01;
using TMPro;



public class ObstaclePath : MonoBehaviour
{
    // used to display the you lose text
    [SerializeField] private TextMeshProUGUI loseText;
    [SerializeField] private float delayBeforeReturn = 2f;

    // stores waypoints to travel
    public Transform[] waypoints;
    public float speed = 5f;
    public float scaleSpeed = 1f;
    public float scaleAmount = 1f; // How much to grow/shrink
    private int currentIndex = 0;
    private Vector3 originalScale;

    // save original size
    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        MoveAlongPath();
        AnimateScale();
    }

    // travel between the selected points, and repeat back to original
    void MoveAlongPath()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }

    // grow and shrink the size of the objects
    void AnimateScale()
    {
        float scaleOffset = Mathf.PingPong(Time.time * scaleSpeed, 1f); // range: 0 to 1
        float scaleFactor = 1f + scaleOffset * scaleAmount; // e.g., 1 + 0.3 = 1.3x size
        transform.localScale = originalScale * scaleFactor;

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
            PlayerManager.Instance.ReturnToLobby("Lose");
            Debug.Log("Player destroyed on collision.");
        }
    }






}
