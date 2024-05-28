using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BallAgent : Agent
{
    public GameObject _ball;
    private Rigidbody _ballRb;
    private BoxCollider[] _colliders;
    private GameObject _bottom;
    private Vector3 _ballPosition;

    public override void Initialize()
    {
        _ballRb = _ball.GetComponent<Rigidbody>();
        _ballPosition = _ball.transform.localPosition;
        _colliders = GetComponents<BoxCollider>();
        _bottom = GetComponentInChildren<MeshRenderer>().gameObject;
    }

    /// <summary>
    /// Checks if ball leaves level bounds
    /// </summary>
    private void Update()
    {
        if (_ballRb.gameObject.transform.position.y < _bottom.transform.position.y - 1) {
            Debug.Log("LOSE");
            SetReward(-1f);
            EndEpisode();
        }
    }

    /// <summary>
    /// Lists varaiables that are watched
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.rotation.z);
        sensor.AddObservation(Vector3.Distance(_ball.transform.position,_colliders[2].transform.position));
        sensor.AddObservation(_ball.transform.position);
        sensor.AddObservation(_ballRb.velocity);
    }

    /// <summary>
    /// Transforms the receved action into a rotation on the Z axis
    /// </summary>
    /// <param name="actionBuffers"></param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var actionZ = 2f * Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);

        if ((gameObject.transform.rotation.z < 0.25f && actionZ > 0f) || (gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
            gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
    }

    /// <summary>
    /// Resets the level to default valus
    /// </summary>
    public override void OnEpisodeBegin()
    {
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        _ballRb.velocity = new Vector3(0f, 0f, 0f);
        _ball.transform.localPosition = _ballPosition;
    }

    /// <summary>
    /// Function that handles player input in heuristic mode
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
    }

    /// <summary>
    /// If a trigger is hit check if it was the right one
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if(_colliders[2].bounds.Intersects(other.bounds))
        {
            SetReward(1f);
            Debug.Log("WIN");
        }else
        {
            Debug.Log("LOSE");
            SetReward(-1f);
        }
        
        EndEpisode();
    }
}
