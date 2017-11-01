﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;


public class FetchRobots : MonoBehaviour {

    MQTT mMqtt = new MQTT();

    Dictionary<string, Robot> robots = new Dictionary<string, Robot>();
    Dictionary<string, SpawnRobot> robotShape = new Dictionary<string, SpawnRobot>();

    Queue robotsToSpawn = new Queue();

    private const string baseTopic = "vrep/pose/robot/";

    // Use this for initialization
    void Start ()
    {
        mMqtt.Connect();
        mMqtt.Subscribe(baseTopic + "+");
        mMqtt.AddCallback(onMqttMessage);
    }

    private void onMqttMessage(object sender, MqttMsgPublishEventArgs e)
    {
        string subTopic = e.Topic.Replace(baseTopic, "");
        string message = System.Text.Encoding.UTF8.GetString(e.Message).Replace("\n", "");
        Debug.Log(subTopic + ": " + message);

        Robot newRobot = JsonUtility.FromJson<Robot>(message);

        if (robots.ContainsKey(subTopic)) {
            robots[subTopic] = newRobot;
        }
        else {
            robots.Add(subTopic, newRobot);
        }

        robotsToSpawn.Enqueue(subTopic);
    }

    // Update is called once per frame
    void Update ()
    {
        while(robotsToSpawn.Count > 0)
        {
            if ( !robotShape.ContainsKey((string)robotsToSpawn.Peek()))
            {
                robotShape.Add((string)robotsToSpawn.Peek(), gameObject.AddComponent<SpawnRobot>());
            }

            robotShape[(string)robotsToSpawn.Peek()].Position = robots[(string)robotsToSpawn.Peek()].Position;
            robotShape[(string)robotsToSpawn.Peek()].Orientation = robots[(string)robotsToSpawn.Peek()].Orientation;

            robotsToSpawn.Dequeue();
        }
    }

    public class Robot
    {
        public Vector3 Position;
        public Quaternion Orientation;
    }
}
