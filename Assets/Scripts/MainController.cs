using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MainController : MonoBehaviour
{
    public TMP_Text questionText;
    public TMP_Text timerText;
    public TMP_Text resultText;
    public Button restartButton;
    public GameObject[] panels;
    private int correctAnswer;
    private AudioSource audioSource; //音源
    public AudioClip soundOk; //音效文件
    public AudioClip soundFail; //音效文件
    public MyMessageBox MsgBox;
    private float elapsedTime = 0f;
    private int correctCount = 0;
    private int wrongCount = 0;
    private bool isMsgBoxShowing = false;
    private bool isGamePaused = false; // 新增游戏暂停状态

    public Button difficultAddButton;
    public Button difficultMinorButton;
    public TMP_Text difficultText;
    private int difficultyLevel = 3;

    // Start is called before the first frame update
    void Start()
    {
        BindUI();
        GenerateQuestion();
        audioSource = GetComponent<AudioSource>();
        elapsedTime = 0f;
        correctCount = 0;
        wrongCount = 0;
        isMsgBoxShowing = false;
        UpdateResultText();
        restartButton.onClick.AddListener(RestartGame);
        difficultAddButton.onClick.AddListener(IncreaseDifficulty);
        difficultMinorButton.onClick.AddListener(DecreaseDifficulty);
        UpdateDifficultyText();
    }

    private void BindUI()
    {
        // 创建答案按钮
        for (int i = 0; i < 5; i++)
        {
            var answerButton = panels[i].GetComponent<Button>();
            answerButton.onClick.AddListener(() => CheckAnswer(answerButton));
        }
    }

    private void GenerateQuestion()
    {
        if (isGamePaused) return; // 游戏暂停时不生成新问题
        int num1 = Random.Range(1, 10);
        int num2 = Random.Range(1, 10);
        int op = Random.Range(0, 2);
        string question = "";
        if (op == 0)
        {
            correctAnswer = num1 + num2;
            question = $"{num1} + {num2} = ?";
        }
        else
        {
            correctAnswer = num1 - num2;
            question = $"{num1} - {num2} = ?";
        }
        questionText.text = question;

        // 设置答案选项
        List<int> answers = new List<int>();
        answers.Add(correctAnswer);
        while (answers.Count < 5)
        {
            int wrongAnswer = correctAnswer + Random.Range(-5, 6);
            if (wrongAnswer != correctAnswer && !answers.Contains(wrongAnswer))
            {
                answers.Add(wrongAnswer);
            }
        }
        answers = ShuffleList(answers);
        for (int i = 0; i < 5; i++)
        {
            panels[i].GetComponentInChildren<TMP_Text>().text = answers[i].ToString();
        }
    }

    private List<int> ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return list;
    }

    private void CheckAnswer(Button button)
    {
        if (isGamePaused) return; // 游戏暂停时不检查答案
        int selectedAnswer = int.Parse(button.GetComponentInChildren<TMP_Text>().text);
        if (selectedAnswer == correctAnswer)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(soundOk);
            correctCount++;
        }
        else
        {
            audioSource.Stop();
            audioSource.PlayOneShot(soundFail);
            wrongCount++;
        }
        UpdateResultText();
        GenerateQuestion();

        if(correctCount + wrongCount >= 20)
        {    
            isMsgBoxShowing = true;
            MsgBox.Show("恭喜你完成了20题\n"+resultText.text+"\n耗时"+timerText.text, () => {
                RestartGame();
            });
        }
    }

    private void UpdateResultText()
    {
        resultText.text = $"{correctCount}对{wrongCount}错";
    }

    private void RestartGame()
    {
        elapsedTime = 0f;
        correctCount = 0;
        wrongCount = 0;
        isMsgBoxShowing = false;
        UpdateResultText();
        GenerateQuestion();
        isGamePaused = false; // 点击重启按钮恢复游戏
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMsgBoxShowing && !isGamePaused) {
            elapsedTime += Time.deltaTime;
            int hours = Mathf.FloorToInt(elapsedTime / 3600);
            int minutes = Mathf.FloorToInt((elapsedTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }

    private void IncreaseDifficulty()
    {
        difficultyLevel = Mathf.Min(difficultyLevel + 1, 10);
        UpdateDifficultyText();
        isGamePaused = true; // 点击难度提升按钮暂停游戏
    }

    private void DecreaseDifficulty()
    {
        difficultyLevel = Mathf.Max(difficultyLevel - 1, 1);
        UpdateDifficultyText();
        isGamePaused = true; // 点击难度降低按钮暂停游戏
    }

    private void UpdateDifficultyText()
    {
        difficultText.text = $"{difficultyLevel}级";
    }
}