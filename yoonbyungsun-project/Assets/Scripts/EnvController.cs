using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class EnvController : MonoBehaviour
{
    [Serializable]
    public class PlayerInfo
    {
        public BlockAgent agent;
        public Vector3 startPos;
        public Rigidbody agentRb;
    }

    public List<PlayerInfo> AgentList = new List<PlayerInfo>();
    public int maxEnvSteps = 2000;
    public GameObject ground;
    public GameObject door;
    public GameObject trap;

    private int trapDir = 1;
    private int resetTimer;
    private Bounds areaBounds;
    private float areaPosX;

    private SimpleMultiAgentGroup agentGroup;
    public int remainPlayers;
    private float spawnMarginMultiplier = 0.8f;

    private void Start()
    {
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        Time.timeScale = 20f;

        areaPosX = transform.position.x;
        areaBounds = ground.GetComponent<Collider>().bounds;
        agentGroup = new SimpleMultiAgentGroup();
        foreach (var block in AgentList)
        {
            block.startPos = block.agent.transform.position;
            block.agentRb = block.agent.GetComponent<Rigidbody>();
            agentGroup.RegisterAgent(block.agent);
        }
        remainPlayers = AgentList.Count;

        ResetScene();
    }

    private void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer > maxEnvSteps)
        {
            agentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        MoveTrap();
    }

    public void GoalReached()
    {
        agentGroup.AddGroupReward(1f);
        agentGroup.EndGroupEpisode();
    }

    public void OpenDoor()
    {
        door.gameObject.SetActive(false);
    }

    public void CloseDoor()
    {
        door.gameObject.SetActive(true);
    }

    public void KillAgent(BlockAgent agent)
    {
        remainPlayers--;
        if (remainPlayers == 0)
        {
            agentGroup.EndGroupEpisode();
            ResetScene();
        }
        else
        {
            agent.gameObject.SetActive(false);
            OpenDoor();
        }
    }

    public void MoveTrap()
    {
        if (trap.transform.position.x >= 12 + areaPosX || trap.transform.position.x <= -12 + areaPosX)
            trapDir *= -1;

        trap.transform.position = new Vector3(trap.transform.position.x + trapDir * 0.1f, trap.transform.position.y, trap.transform.position.z);
    }

    private List<Vector2> GetRandomSpawnPos()
    {
        List<Vector2> randPosList = new List<Vector2>();
        for (int i = 0; i < 4; i++)
        {
            Vector2 randPos = new Vector2();
            while (true)
            {
                randPos = new Vector2(ground.transform.position.x,
                                      ground.transform.position.z)
                        + new Vector2(Random.Range(-areaBounds.extents.x * spawnMarginMultiplier, areaBounds.extents.x * spawnMarginMultiplier),
                                      Random.Range(-areaBounds.extents.z * spawnMarginMultiplier, areaBounds.extents.z * spawnMarginMultiplier));

                bool repeat = false;
                foreach (Vector2 tempPos in randPosList)
                {
                    if (Vector2.Distance(tempPos, randPos) <= 5.0f)
                    {
                        repeat = true;
                        break;
                    }
                }
                if (!repeat) break;
            }
            randPosList.Add(randPos);
        }
        return randPosList;
    }

    private void ResetScene()
    {
        List<Vector2> randPosList = GetRandomSpawnPos();

        trap.transform.position = new Vector3(randPosList[0].x, 0.01f, randPosList[0].y);

        int index = 1;
        foreach (var agent in AgentList)
        {
            var pos = new Vector3(randPosList[index].x, 0.5f, randPosList[index].y);
            agent.agent.transform.position = pos;
            agent.agentRb.linearVelocity = Vector3.zero;
            agent.agentRb.angularVelocity = Vector3.zero;
            agent.agent.gameObject.SetActive(true);

            agentGroup.RegisterAgent(agent.agent);
            index++;
        }
        resetTimer = 0;
        remainPlayers = AgentList.Count;
        CloseDoor();
    }
}