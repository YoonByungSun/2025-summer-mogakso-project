using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BlockAgent : Agent
{
    private Rigidbody agentRb;
    private EnvController ec;
    private float runSpeed = 2.5f;

    public float waitingTime = 0.01f;
    float currTime = 0f;

    public override void Initialize()
    {
        ec = GetComponentInParent<EnvController>();
        agentRb = GetComponent<Rigidbody>();

        Academy.Instance.AgentPreStep += WaitTimeInference;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ec.remainPlayers);
        sensor.AddObservation(agentRb.linearVelocity);
    }

    private void MoveAgent(ActionSegment<int> act)
    {
        var moveDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                moveDir = transform.forward * 1f;
                break;
            case 2:
                moveDir = transform.forward * -1f;
                break;
            case 3:
                moveDir = transform.right * -1f;
                break;
            case 4:
                moveDir = transform.right * 1f;
                break;
        }
        agentRb.AddForce(moveDir * runSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("goal"))
        {
            ec.GoalReached();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("trap"))
        {
            ec.KillAgent(this);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
            discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.S))
            discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.A))
            discreteActionsOut[0] = 3;
        else if (Input.GetKey(KeyCode.D))
            discreteActionsOut[0] = 4;
    }

    public void WaitTimeInference(int action)
    {
        if (Academy.Instance.IsCommunicatorOn)
            RequestDecision();
        else
        {
            if (currTime >= waitingTime)
            {
                currTime = 0f;
                RequestDecision();
            }
            else
            {
                currTime += Time.fixedDeltaTime;
            }
        }
    }
}