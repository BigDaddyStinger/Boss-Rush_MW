using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Navigator : MonoBehaviour
{
    NavMeshAgent agent;
    public List<Vector3> PathNodes { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        PathNodes = new List<Vector3>();
    }

    public bool CalculatePathToPosition(Vector3 targetPosition)
    {
        if (agent == null)
        {
            Debug.Log("agent is null");
            return false;
        }

        if (targetPosition == null)
        {
            Debug.Log("target position is null");
            return false;
        }

        agent.enabled = true;

        NavMeshPath tempPath = new NavMeshPath();
        NavMesh.SamplePosition(targetPosition, out NavMeshHit nmHit, 10, NavMesh.AllAreas);

        if (agent.CalculatePath(nmHit.position, tempPath))
        {
            Debug.Log(tempPath.status);
            PathNodes.Clear();

            Debug.Log("found path that is " + tempPath.corners.Length + " corners");

            foreach (Vector3 v in tempPath.corners)
                PathNodes.Add(v);

            agent.enabled = false;

            return true;
        }

        agent.enabled = false;

        //Debug.Log("found no path");
        return false;

    }

    private void OnDrawGizmos()
    {
        if (PathNodes == null)
            return;

        if(PathNodes.Count != 0)
        {
            Gizmos.color = Color.red;
            foreach (var v in PathNodes)
                Gizmos.DrawSphere(v, 0.25f);
        }
    }
}
