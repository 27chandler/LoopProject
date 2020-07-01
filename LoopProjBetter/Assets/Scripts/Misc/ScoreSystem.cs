using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSystem : MonoBehaviour
{
    private int score = 0;
    private Text score_text;
    // Start is called before the first frame update
    void Start()
    {
        score_text = GetComponent<Text>();
    }

    public void Add_Score(int i_score)
    {
        score += i_score;
    }

    public void Subtract_score(int i_score)
    {
        score -= i_score;
    }

    public void Set_Score(int i_score)
    {
        score = i_score;
    }

    // Update is called once per frame
    void Update()
    {
        score_text.text = "Score: " + score;
    }
}
