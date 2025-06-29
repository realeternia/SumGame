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
        string question = "";
        List<int> numbers = new List<int>();
        List<char> operators = new List<char>();
        int maxNumber = 10;
        int maxNumber2 = 10; // maxNUmber2是30%概率出现的数字上限，其他70%按maxNumber
        int numberCount = 2;
        bool reverseQuestion = false;

        // 根据难度设置参数
        if (difficultyLevel == 1) {
            maxNumber = 10;
        } else if (difficultyLevel == 2) {
            maxNumber = 10;
        } else if (difficultyLevel == 3) {
            maxNumber = 10;
            maxNumber2 = 20;
        } else if (difficultyLevel == 4) {
            maxNumber = 10;
            maxNumber2 = 20;
        } else if (difficultyLevel == 5) {
            maxNumber = 20;
            maxNumber2 = 20;
            if(Random.Range(1, 100) < 30) numberCount = 3;
        } else if (difficultyLevel == 6) {
            maxNumber = 20;
            maxNumber2 = 50;
            if(Random.Range(1, 100) < 30) numberCount = 3;
        } else if (difficultyLevel == 7) {
            maxNumber = 50;
            maxNumber2 = 50;
            if(Random.Range(1, 100) < 30) numberCount = 3;
            reverseQuestion = Random.Range(0, 4) == 0;
        } else if (difficultyLevel == 8) {
            maxNumber = 50;
            maxNumber2 = 80;
            if(Random.Range(1, 100) < 30) numberCount = 3;
            reverseQuestion = Random.Range(0, 2) == 0;
        } else if (difficultyLevel == 9) {
            maxNumber = 80;
            maxNumber2 = 30;
            if(Random.Range(1, 100) < 40) numberCount = 3;
            reverseQuestion = Random.Range(0, 2) == 0;            
        } else if (difficultyLevel == 10) {
            maxNumber = 100;
            maxNumber2 = 50;
            if(Random.Range(1, 100) < 24) numberCount = 4;
            reverseQuestion = Random.Range(0, 2) == 0;
        }

        // 生成数字
        for (int i = 0; i < numberCount; i++) {
            int upperBound = Random.Range(1, 101) <= 30 ? maxNumber2 : maxNumber;
            numbers.Add(Random.Range(1, upperBound + 1));
        }

        // 生成运算符
        if (difficultyLevel == 1) {
            for (int i = 0; i < numberCount - 1; i++) {
                operators.Add('+');
            }
        } else {
            for (int i = 0; i < numberCount - 1; i++) {
                operators.Add(Random.Range(0, 2) == 0 ? '+' : '-');
            }
        }

        // 当难度为3且有减法运算时，保证大数在前
        if (difficultyLevel <= 3) {
            for (int i = 0; i < operators.Count; i++) {
                if (operators[i] == '-') {
                    if (numbers[i] < numbers[i + 1]) {
                        int temp = numbers[i];
                        numbers[i] = numbers[i + 1];
                        numbers[i + 1] = temp;
                    }
                }
            }
        }

        // 计算正确答案
        correctAnswer = numbers[0];
        for (int i = 0; i < operators.Count; i++) {
            if (operators[i] == '+') {
                correctAnswer += numbers[i + 1];
            } else {
                correctAnswer -= numbers[i + 1];
            }
        }

        if (!reverseQuestion) {
            // 生成普通问题
            question = numbers[0].ToString();
            for (int i = 0; i < operators.Count; i++) {
                question += " " + operators[i] + " " + numbers[i + 1];
            }
            question += " = ?";
        }
        else
        {
            question = "?";
            for (int i = 0; i < operators.Count; i++) {
                if(operators[i] == '+')
                    question += " - " + numbers[i + 1];
                else
                    question += " + " + numbers[i + 1];
            }
            question += " = " + numbers[0];
        }

        questionText.text = question;

        // 设置答案选项
        List<int> answers = new List<int>();
        answers.Add(correctAnswer);
        while (answers.Count < 5) {
            int wrongAnswer = correctAnswer + Random.Range(-10, 11);
            if (wrongAnswer != correctAnswer && !answers.Contains(wrongAnswer)) {
                answers.Add(wrongAnswer);
            }
        }
        answers = ShuffleList(answers);
        for (int i = 0; i < 5; i++) {
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
        if(correctCount + wrongCount >= 20)
        {    
            isMsgBoxShowing = true;
            MsgBox.Show("恭喜你完成了20题\n"+resultText.text+"\n耗时"+timerText.text, () => {
                RestartGame();
            });
        }
        GenerateQuestion();
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