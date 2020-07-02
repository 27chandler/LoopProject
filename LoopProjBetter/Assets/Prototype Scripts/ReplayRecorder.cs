using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayRecorder : MonoBehaviour
{
    public class ReplayData
    {
        public float timestamp;
        public Vector3 player_pos;
        public Quaternion player_rot;
    }

    private Dictionary<int, ReplayData> replay_timeline = new Dictionary<int, ReplayData>();
    private int timestamp_key = 0;
    private float time = 0.0f;

    private bool is_playback = false;

    // For logging data
    [SerializeField] private Transform player_transform;
    [SerializeField] private Transform player_rotation_transform;

    [Space]
    // For playback
    [SerializeField] private GameObject dummy_player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if (Input.GetKey(KeyCode.Space))
        {
            is_playback = true;
            time = 0.0f;
            timestamp_key = 0;
        }

        if (is_playback)
            Playback();
        else
            Record();
    }

    private void Record()
    {
        replay_timeline.Add(replay_timeline.Count, Save_Values());
    }

    // Saves data to be played back later
    private ReplayData Save_Values()
    {
        ReplayData new_replay = new ReplayData();
        new_replay.player_pos = player_transform.position;
        new_replay.player_rot = player_rotation_transform.rotation;
        new_replay.timestamp = time;

        return new_replay;
    }

    private void Playback()
    {
        Set_Values(timestamp_key);

        if (time > replay_timeline[timestamp_key].timestamp)
            timestamp_key++;
    }

    private void Set_Values(int i_index)
    {
        dummy_player.transform.position = replay_timeline[i_index].player_pos;
        dummy_player.transform.rotation = replay_timeline[i_index].player_rot;
    }
}
