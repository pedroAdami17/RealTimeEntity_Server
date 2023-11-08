using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    float durationUntilNextBalloon;

    LinkedList<int> connectedClientIDs;

    int lastUsedRefId;

    LinkedList<BalloonInfo> activeBallooIDs;

    void Start()
    {
        connectedClientIDs = new LinkedList<int>();
        NetworkServerProcessing.SetGameLogic(this);
        activeBallooIDs = new LinkedList<BalloonInfo>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            NetworkServerProcessing.SendMessageToClient("2,Hello client's world, sincerely your network server", 0, TransportPipeline.ReliableAndInOrder);

        durationUntilNextBalloon -= Time.deltaTime;

        if (durationUntilNextBalloon < 0)
        {
            lastUsedRefId++;

            durationUntilNextBalloon = 1f;

            float screenPositionXPercent = Random.Range(0.0f, 1.0f);
            float screenPositionYPercent = Random.Range(0.0f, 1.0f);

            string msg = ServerToClientSignifiers.SpawnBalloon + "," + screenPositionXPercent + "," + screenPositionYPercent + "," + lastUsedRefId;

            foreach (int cid in connectedClientIDs)
            {
                NetworkServerProcessing.SendMessageToClient(msg, cid, TransportPipeline.ReliableAndInOrder);
            }
            activeBallooIDs.AddLast(new BalloonInfo(screenPositionXPercent, screenPositionYPercent, lastUsedRefId));
        }
    }

    public void AddClientConenction(int clientID)
    {
        connectedClientIDs.AddLast(clientID);

        foreach (BalloonInfo bi in activeBallooIDs)
        {
            string msg = ServerToClientSignifiers.SpawnBalloon + "," + bi.percentX + "," + bi.percentY + "," + bi.id;
            NetworkServerProcessing.SendMessageToClient(msg, clientID, TransportPipeline.ReliableAndInOrder);
        }
    }

    public void RemoveClientConnection(int clientID)
    {
        connectedClientIDs.Remove(clientID);
    }

    public void ProcessBalloonClick(int balloonID)
    {
        BalloonInfo bi = findBalloonWithID(balloonID);
        if (bi != null)
        {
            activeBallooIDs.Remove(bi);
            string msg = ServerToClientSignifiers.BalloonPopped + "," + balloonID;

            foreach (int cid in connectedClientIDs)
            {
                NetworkServerProcessing.SendMessageToClient(msg, cid, TransportPipeline.ReliableAndInOrder);
            }
        }
    }

    private BalloonInfo findBalloonWithID(int id)
    {
        foreach (BalloonInfo bi in activeBallooIDs)
        {
            if (bi.id == id)
                return bi;
        }

        return null;
    }
}


public class BalloonInfo
{
    public float percentX, percentY;

    public int id;

    public BalloonInfo(float percentX, float percentY, int id)
    {
        id = id;
        percentX = percentX;
        percentY = percentY;
    }
}