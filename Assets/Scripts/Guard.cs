using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard :MonoBehaviour {

    public Transform pathHolder;

    public float waitTime = .3f;
    public float speed = 6f;
    public float turnSpeed = 90;

    public Light spotLight;
    public float viewDistance;
    float viewAngle;
    float timeToSpotPlayer = .5f;
    float playerVisibleTimer;

    public LayerMask viewMask;

    Transform player;
    Color originalSpotLightColour;
    public AudioSource source;
    public AudioClip clip;
    bool alert;

    public static event System.Action OnPlayerSpotted;


    void Start () {

        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalSpotLightColour = spotLight.color;

        viewAngle = spotLight.spotAngle;

        Vector3[] waypointPosition = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypointPosition.Length; i++) {
            waypointPosition[i] = pathHolder.GetChild(i).position;
        }

        StartCoroutine(FollowPath(waypointPosition));
    }

    private void Update () {
        if (CanSeePlayer()) {
            playerVisibleTimer += Time.deltaTime;
        } else {
            playerVisibleTimer -= Time.deltaTime;
        }
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        spotLight.color = Color.Lerp(originalSpotLightColour, Color.red, playerVisibleTimer / timeToSpotPlayer);

        if (playerVisibleTimer >= timeToSpotPlayer) {
            if (OnPlayerSpotted != null) {
                OnPlayerSpotted();
                if (!alert) {
                    source.PlayOneShot(clip);
                    alert = true;
                }
            }
        }
    }

    bool CanSeePlayer () {
        if (Vector3.Distance(transform.position, player.position - player.localScale) <= viewDistance) {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleBetweenGuardAndPlayer < viewAngle / 2f) {
                if (!Physics.Linecast(transform.position, player.position, viewMask)) {
                    return true;
                }
            }

        }
        return false;
    }

    IEnumerator FollowPath (Vector3[] waypoints) {
        Vector3 fixedPosition = new Vector3(0, transform.localScale.y, 0);
        transform.position = waypoints[0] + fixedPosition;

        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex] + fixedPosition;
        transform.LookAt(targetWaypoint);

        while (true) {
            if (!CanSeePlayer()) {
                transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
                if (transform.position == targetWaypoint) {
                    targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                    targetWaypoint = waypoints[targetWaypointIndex] + fixedPosition;
                    yield return new WaitForSeconds(waitTime);
                    yield return StartCoroutine(TurnToFace(targetWaypoint));
                }
            }
            yield return null;
        }
    }

    IEnumerator TurnToFace (Vector3 lookTarget) {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = Mathf.Atan2(dirToLookTarget.x, dirToLookTarget.z) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f) {
            if (!CanSeePlayer()) {
                float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
                transform.eulerAngles = Vector3.up * angle;
            }
            yield return null;
        }
    }

    private void OnDrawGizmos () {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;
        foreach (Transform waypoint in pathHolder) {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
}
