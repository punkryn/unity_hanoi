using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public GameObject gazePlot;
    GazePlotter gazePlotter;

    float curSelectDelay = 0f;
    float maxSelectDelay = 1.5f;

    float curTime = 0f;

    public GameObject[] stickSet;

    float centerBottomX;
    float centerBottomY;
    float centerTopX;
    float centerTopY;

    float leftBottomX;
    float leftBottomY;
    float leftTopX;
    float leftTopY;

    float rightBottomX;
    float rightBottomY;
    float rightTopX;
    float rightTopY;

    bool isOnLeft = false;
    bool isOnCenter = false;
    bool isOnRight = false;

    bool isSelected = false;
    string preSelected = null;

    Stack<GameObject> leftStack;
    Stack<GameObject> centerStack;
    Stack<GameObject> rightStack;

    public GameObject[] plates;
    public Text mode;

    int[] leftArr;
    int[] rightAnswer;
    int[] centerAnswer;
    int rightPtr = -1;
    int centerPtr = -1;
    int leftPtr;

    public Text time;

    public GameObject pauseSet;
    bool isPaused = false;

    public GameObject clearSet;
    public Text clearTime;

    GameObject preObject;
    SpriteRenderer preHover = null;

    private void Awake()
    {
        gazePlotter = gazePlot.GetComponent<GazePlotter>();

        StickInitialize();

        leftStack = new Stack<GameObject>();
        centerStack = new Stack<GameObject>();
        rightStack = new Stack<GameObject>();

        leftArr = new int[plates.Length];
        rightAnswer = new int[plates.Length];
        centerAnswer = new int[plates.Length];

        leftStack.Clear();
        centerStack.Clear();
        rightStack.Clear();

        foreach(var plate in plates)
        {
            leftStack.Push(plate);
        }

        for(int i = 1; i <= leftArr.Length; i++)
        {
            leftArr[i - 1] = i;
        }
        leftPtr = leftArr.Length - 1;
    }

    void StickInitialize()
    {
        float unitX = 2f;
        float unitY = 5f;
        centerBottomX = stickSet[0].transform.position.x - unitX;
        centerTopX = stickSet[0].transform.position.x + unitX;
        centerBottomY = stickSet[0].transform.position.y - unitY;
        centerTopY = stickSet[0].transform.position.y + unitY;

        rightBottomX = stickSet[1].transform.position.x - unitX;
        rightTopX = stickSet[1].transform.position.x + unitX;
        rightBottomY = stickSet[1].transform.position.y - unitY;
        rightTopY = stickSet[1].transform.position.y + unitY;

        leftBottomX = stickSet[2].transform.position.x - unitX;
        leftTopX = stickSet[2].transform.position.x + unitX;
        leftBottomY = stickSet[2].transform.position.y - unitY;
        leftTopY = stickSet[2].transform.position.y + unitY;
    }

    private void Update()
    {
        ShowTime();
        if (isSelected)
        {
            followEyes();
            MovePlate();
        }
        else
        {
            HoveringEffect();
            PosCheck();
        }

        if (CheckAnswer())
        {
            Debug.Log("success");
            mode.text = "SUCCESS";
            clearTime.text = string.Format("{0:n0} 초", curTime);
            //GameClearPause();
            Invoke("GameClearPause", 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GamePause();
        }
    }

    bool CheckAnswer()
    {
        if(centerPtr + 1 == plates.Length)
        {
            for(int i = 0; i < plates.Length; i++)
            {
                if(i + 1 != centerAnswer[i])
                {
                    return false;
                }
            }
            return true;
        }

        if(rightPtr + 1 == plates.Length)
        {
            for(int j = 0; j < plates.Length; j++)
            {
                if(j + 1 != rightAnswer[j])
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }

    void ShowTime()
    {
        curTime += Time.deltaTime;
        time.text = string.Format("{0:n0}", curTime);
    }

    void PosCheck()
    {
        mode.text = "선택";
        // center
        if (gazePlotter.transform.position.x > centerBottomX && gazePlotter.transform.position.x < centerTopX
            && gazePlotter.transform.position.y > centerBottomY && gazePlotter.transform.position.y < centerTopY)
        {
            MakeTimeDelay();
            if (centerStack.Count <= 0)
                return;

            if (!isOnCenter)
            {
                isOnCenter = true;
                isOnLeft = false;
                isOnRight = false;
                curSelectDelay = 0;
            }
            else
            {
                if(curSelectDelay > maxSelectDelay)
                {
                    Debug.Log("select center");
                    isSelected = true;
                    curSelectDelay = 0;

                    preSelected = "center";
                    SelectedEffect();
                }
            }
        }
        // left
        else if(gazePlotter.transform.position.x > leftBottomX && gazePlotter.transform.position.x < leftTopX &&
            gazePlotter.transform.position.y > leftBottomY && gazePlotter.transform.position.y < leftTopY)
        {
            MakeTimeDelay();
            if (leftStack.Count <= 0)
                return;

            if (!isOnLeft)
            {
                isOnLeft = true;
                isOnCenter = false;
                isOnRight = false;
                curSelectDelay = 0;
            }
            else
            {
                if (curSelectDelay > maxSelectDelay)
                {
                    Debug.Log("select left");
                    isSelected = true;
                    curSelectDelay = 0;

                    preSelected = "left";
                    SelectedEffect();
                }
            }
        }
        // right
        else if(gazePlotter.transform.position.x > rightBottomX && gazePlotter.transform.position.x < rightTopX
            && gazePlotter.transform.position.y > rightBottomY && gazePlotter.transform.position.y < rightTopY)
        {
            MakeTimeDelay();
            if (rightStack.Count <= 0)
                return;

            if (!isOnRight)
            {
                isOnRight = true;
                isOnLeft = false;
                isOnCenter = false;
                curSelectDelay = 0;
            }
            else
            {
                if (curSelectDelay > maxSelectDelay)
                {
                    Debug.Log("select right");
                    isSelected = true;
                    curSelectDelay = 0;

                    preSelected = "right";
                    SelectedEffect();
                }
            }
        }
        else
        {
            curSelectDelay = 0;
            isOnRight = false;
            isOnCenter = false;
            isOnLeft = false;
        }
    }

    void MovePlate()
    {
        mode.text = "이동";
        MakeTimeDelay();

        bool tmpleft = isOnLeft;
        bool tmpcenter = isOnCenter;
        bool tmpright = isOnRight;

        CheckGazePosition();

        if (!((tmpleft && isOnLeft) || (tmpcenter && isOnCenter) || (tmpright && isOnRight)))
            curSelectDelay = 0;

        if (maxSelectDelay > curSelectDelay)
        {
            return;
        }

        float y = -3.25f;
        // center로 이동
        if (gazePlotter.transform.position.x > centerBottomX && gazePlotter.transform.position.x < centerTopX
            && gazePlotter.transform.position.y > centerBottomY && gazePlotter.transform.position.y < centerTopY)
        {
            float x = stickSet[0].transform.position.x;
            if(preSelected == "left")
            {
                if (leftStack.Count <= 0)
                    return;

                centerAnswer[++centerPtr] = leftArr[leftPtr--];

                GameObject plate = leftStack.Pop();
                centerStack.Push(plate);
                centerStack.Peek().transform.position = new Vector3(x, y + 1.05f * (centerStack.Count - 1));
                isSelected = false;


            }
            else if(preSelected == "right")
            {
                if (rightStack.Count <= 0)
                    return;

                centerAnswer[++centerPtr] = rightAnswer[rightPtr--];

                GameObject plate = rightStack.Pop();
                centerStack.Push(plate);
                centerStack.Peek().transform.position = new Vector3(x, y + 1.05f * (centerStack.Count - 1));
                isSelected = false;
            }
            else
            {
                centerStack.Peek().transform.position = new Vector3(x, y + 1.05f * (centerStack.Count - 1));
                isSelected = false;
            }
        }
        // left로 이동
        else if (gazePlotter.transform.position.x > leftBottomX && gazePlotter.transform.position.x < leftTopX &&
            gazePlotter.transform.position.y > leftBottomY && gazePlotter.transform.position.y < leftTopY)
        {
            float x = stickSet[2].transform.position.x;

            if (preSelected == "center")
            {
                if (centerStack.Count <= 0)
                    return;

                leftArr[++leftPtr] = centerAnswer[centerPtr--];

                GameObject plate = centerStack.Pop();
                leftStack.Push(plate);
                leftStack.Peek().transform.position = new Vector3(x, y + 1.05f * (leftStack.Count - 1));
                isSelected = false;
            }
            else if (preSelected == "right")
            {
                if (rightStack.Count <= 0)
                    return;

                leftArr[++leftPtr] = rightAnswer[rightPtr--];

                GameObject plate = rightStack.Pop();
                leftStack.Push(plate);
                leftStack.Peek().transform.position = new Vector3(x, y + 1.05f * (leftStack.Count - 1));
                isSelected = false;
            }
            else
            {
                leftStack.Peek().transform.position = new Vector3(x, y + 1.05f * (leftStack.Count - 1));
                isSelected = false;
            }
        }
        // right로 이동
        else if (gazePlotter.transform.position.x > rightBottomX && gazePlotter.transform.position.x < rightTopX
            && gazePlotter.transform.position.y > rightBottomY && gazePlotter.transform.position.y < rightTopY)
        {
            float x = stickSet[1].transform.position.x;

            if (preSelected == "center")
            {
                if (centerStack.Count <= 0)
                    return;

                rightAnswer[++rightPtr] = centerAnswer[centerPtr--];

                GameObject plate = centerStack.Pop();
                rightStack.Push(plate);
                rightStack.Peek().transform.position = new Vector3(x, y + 1.05f * (rightStack.Count - 1));
                isSelected = false;
            }
            else if (preSelected == "left")
            {
                if (leftStack.Count <= 0)
                    return;

                rightAnswer[++rightPtr] = leftArr[leftPtr--];

                GameObject plate = leftStack.Pop();
                rightStack.Push(plate);
                rightStack.Peek().transform.position = new Vector3(x, y + 1.05f * (rightStack.Count - 1));
                isSelected = false;
            }
            else
            {
                rightStack.Peek().transform.position = new Vector3(x, y + 1.05f * (rightStack.Count - 1));
                isSelected = false;
            }
        }

        curSelectDelay = 0;
    }

    void MakeTimeDelay()
    {
        curSelectDelay += Time.deltaTime;
    }

    void GamePause()
    {
        if (isPaused)
        {
            Time.timeScale = 1;
            isPaused = false;

            pauseSet.SetActive(false);
        }
        else
        {
            Time.timeScale = 0;
            isPaused = true;

            pauseSet.SetActive(true);
        }
        
    }

    public void GamePauseOff()
    {
        Time.timeScale = 1;
        isPaused = false;

        pauseSet.SetActive(false);
        clearSet.SetActive(false);
    }

    public void GameRestart()
    {
        GamePauseOff();
        SceneManager.LoadScene(0);
    }

    void GameClearPause()
    {
        Time.timeScale = 0;

        clearSet.SetActive(true);
    }

    void SelectedEffect()
    {
        if (!isSelected)
            return;

        SpriteRenderer sPeek;
        float opacity = 0.85f;
        switch (preSelected)
        {
            case "center":
                preObject = centerStack.Peek();
                break;
            case "left":
                preObject = leftStack.Peek();
                break;
            case "right":
                preObject = rightStack.Peek();
                break;
            default:
                preObject = null;
                break;
        }

        sPeek = preObject.GetComponent<SpriteRenderer>();
        sPeek.color = new Color(1, 1, 1, opacity);

        Invoke("SelectedEffectOff", 0.3f);
    }

    void SelectedEffectOff()
    {
        SpriteRenderer sPeek;
        float opacity = 1f;

        sPeek = preObject.GetComponent<SpriteRenderer>();
        sPeek.color = new Color(1, 1, 1, opacity);

        Invoke("SelectedEffect", 0.3f);
    }

    void HoveringEffect()
    {
        GameObject peek;
        SpriteRenderer sPeek;
        float opacity = 1f;
        float hover = 0.8f;

        if (isOnLeft && leftStack.Count > 0)
        {
            if (preHover)
                preHover.color = new Color(1, 1, 1, 1);

            peek = leftStack.Peek();
            sPeek = peek.GetComponent<SpriteRenderer>();
            sPeek.color = new Color(hover, hover, hover, opacity);
            preHover = sPeek;
        }
        else if(isOnCenter && centerStack.Count > 0)
        {
            if (preHover)
                preHover.color = new Color(1, 1, 1, 1);

            peek = centerStack.Peek();
            sPeek = peek.GetComponent<SpriteRenderer>();
            sPeek.color = new Color(hover, hover, hover, opacity);
            preHover = sPeek;
        }
        else if(isOnRight && rightStack.Count > 0)
        {
            if (preHover)
                preHover.color = new Color(1, 1, 1, 1);

            peek = rightStack.Peek();
            sPeek = peek.GetComponent<SpriteRenderer>();
            sPeek.color = new Color(hover, hover, hover, opacity);
            preHover = sPeek;
        }
        else
        {
            if (preHover)
                preHover.color = new Color(1, 1, 1, 1);
        }
    }

    void followEyes()
    {
        GameObject peek;
        switch (preSelected)
        {
            case "left":
                peek = leftStack.Peek();
                break;
            case "center":
                peek = centerStack.Peek();
                break;
            case "right":
                peek = rightStack.Peek();
                break;
            default:
                peek = null;
                break;
        }

        peek.transform.position = gazePlotter.transform.position;
    }

    void CheckGazePosition()
    {
        // center
        if (gazePlotter.transform.position.x > centerBottomX && gazePlotter.transform.position.x < centerTopX
            && gazePlotter.transform.position.y > centerBottomY && gazePlotter.transform.position.y < centerTopY)
        {
            isOnCenter = true;
            isOnLeft = false;
            isOnRight = false;
        }
        // left
        else if (gazePlotter.transform.position.x > leftBottomX && gazePlotter.transform.position.x < leftTopX &&
            gazePlotter.transform.position.y > leftBottomY && gazePlotter.transform.position.y < leftTopY)
        {
            isOnCenter = false;
            isOnLeft = true;
            isOnRight = false;
        }
        // right
        else if (gazePlotter.transform.position.x > rightBottomX && gazePlotter.transform.position.x < rightTopX
            && gazePlotter.transform.position.y > rightBottomY && gazePlotter.transform.position.y < rightTopY)
        {
            isOnCenter = false;
            isOnLeft = false;
            isOnRight = true;
        }
        else
        {
            isOnCenter = false;
            isOnLeft = false;
            isOnRight = false;
        }
    }

    public void ShowAnswer()
    {
        SceneManager.LoadScene(1);
    }
}
