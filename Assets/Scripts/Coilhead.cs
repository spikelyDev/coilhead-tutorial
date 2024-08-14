using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

#nullable enable
public class Coilhead : MonoBehaviour {
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float speed;
    private AIPath aiPath;
    private AIDestinationSetter aiDestinationSetter;
    private GameObject? currentTarget;

    private void Awake() {
        aiPath = FindObjectOfType<AIPath>();
        aiDestinationSetter = FindObjectOfType<AIDestinationSetter>();

        // Set movement properties
        if(aiPath != null) {
            aiPath.maxSpeed = speed;
        }
    }

    private void Update() {
        // Fetch all possible player targets nearby
        List<PlayerController> possibleTargets = GetPlayersWithinRadius();

        // Check if any of the possibleTargets are looking at me
        bool isAnyoneLookingAtMe = IsAnyoneLookingAtMe(possibleTargets);

        // If nobody is looking at me, and there is at least one possible target
        if(!isAnyoneLookingAtMe && currentTarget == null && possibleTargets.Count > 0) {
            int randomTargetIndex = Random.Range(0, possibleTargets.Count);
            SetCurrentTarget(possibleTargets[randomTargetIndex].gameObject);
        } // Else if a possible target is looking at me, stop moving.
        else if(isAnyoneLookingAtMe) {
            SetCurrentTarget(null);
        }
    }

    private void SetCurrentTarget(GameObject? target) {
        currentTarget = target;
        if(target != null) {
            aiDestinationSetter.target = target.transform;
            aiPath.canMove = true;
        } else {
            aiDestinationSetter.target = null;
            aiPath.canMove = false;
        }
    }

    private List<PlayerController> GetPlayersWithinRadius() {
        Collider[] hitColliders = Physics.OverlapBox(
            gameObject.transform.position,
            transform.localScale * 20, // The size of our radius
            Quaternion.identity,
            targetLayer
        );
        return hitColliders.ToList().ConvertAll(a => a.GetComponent<PlayerController>());
    }

    // Returns true if any players in the list are looking at this gameObject
    // otherwise, returns false
    private bool IsAnyoneLookingAtMe(List<PlayerController> players) {
        for(int i = 0; i < players.Count; i++) {
            if(players[i].TryGetComponent(out PlayerController possibleTarget)) {
                if(IsGameObjectInView(possibleTarget.playerCamera)) {
                    return true;
                }
            }
        }
        return false;
    }

    // Returns whether the current gameObject is within view of the given Camera
    private bool IsGameObjectInView(Camera cam) {
        // Check if the object is within camera bounds
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Bounds bounds = boxCollider.bounds;
        if(!GeometryUtility.TestPlanesAABB(planes, bounds)) {
            return false;
        }

        // Check if the object is visible within the camera (not occluded)
        Vector3[] corners = new Vector3[8];
        bounds.GetCorners(corners); 

        foreach(Vector3 corner in corners) {
            Vector3 direction = corner - cam.transform.position;
            if(Physics.Raycast(cam.transform.position, direction, out RaycastHit hit)) {
                if(hit.collider.gameObject == gameObject) {
                    return true;
                }
            }
        }
        return false;
    }
}
