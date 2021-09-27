using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Answer : MonoBehaviour
{
    public float curMoveDelay = 0f;
    public float maxMoveDelay = 1f;

    public GameObject[] plates;
    public GameObject[] stickSet;

    Stack<GameObject> leftStack;
    Stack<GameObject> centerStack;
    Stack<GameObject> rightStack;

    Queue<string> answerQ;
    private void Awake()
    {
        leftStack = new Stack<GameObject>();
        centerStack = new Stack<GameObject>();
        rightStack = new Stack<GameObject>();

        answerQ = new Queue<string>();

        leftStack.Clear();
        centerStack.Clear();
        rightStack.Clear();

        answerQ.Clear();

        foreach (var plate in plates)
        {
            leftStack.Push(plate);
        }
    }

    private void Start()
    {
        //Debug.Log("start");
        //hanoi(plates.Length, "left", "right", "center");
        hanoi(plates.Length, "left", "right", "center");
    }

    private void Update()
    {
        MovePlate();
        AddTime();
    }

    void hanoi(int N, string start, string to, string via)
    {
        if(N == 1)
        {
            move(start, to);
            return;
        }
        hanoi(N - 1, start, via, to);
        move(start, to);
        hanoi(N - 1, via, to, start);
    }

    void move(string start, string to)
    {
        //Debug.Log(start + " " + to);

        answerQ.Enqueue(start + "," + to);
    }

    void MovePlate()
    {
        if (maxMoveDelay > curMoveDelay)
            return;

        if (answerQ.Count <= 0)
            return;

        string str = answerQ.Dequeue();

        string[] op = str.Split(',');
        string start = op[0];
        string to = op[1];
        Debug.Log(start + to);

        Stack<GameObject> s;
        Stack<GameObject> e;

        float y = -3.25f;
        float x;

        switch (start)
        {
            case "left":
                s = leftStack;
                break;
            case "center":
                s = centerStack;
                break;
            case "right":
                s = rightStack;
                break;
            default:
                s = null;
                break;
        }

        switch (to)
        {
            case "left":
                e = leftStack;
                x = stickSet[2].transform.position.x;
                break;
            case "center":
                e = centerStack;
                x = stickSet[0].transform.position.x;
                break;
            case "right":
                e = rightStack;
                x = stickSet[1].transform.position.x;
                break;
            default:
                e = null;
                x = 0;
                break;
        }

        GameObject peek = s.Pop();
        e.Push(peek);
        e.Peek().transform.position = new Vector3(x, y + 1.05f * (e.Count - 1));

        curMoveDelay = 0;
    }

    void AddTime()
    {
        curMoveDelay += Time.deltaTime;
    }

    public void BackScene()
    {
        SceneManager.LoadScene(0);
    }
}
