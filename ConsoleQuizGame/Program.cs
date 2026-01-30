using System.Collections.Generic;
using System.Text;


List<QuizQuestion> questions = new List<QuizQuestion>();

int lastRightAnswers = 0;
int lastTotalQuestions = 0;
bool hasResult = false;

Console.WriteLine("Welcome to console quiz game. Let's start\n");

while (true)
{
    ShowMenu();

    if (!int.TryParse(Console.ReadLine(), out int choice))
        continue;

    if (choice == 0)
        break;

    SwitchLogic(choice);
}


void SwitchLogic(int choice)
{
    switch (choice)
    {
        case 1:
            Console.Clear();
            StartQuiz();
            return;
        case 2:
            Console.Clear();
            ShowResults();
            return;
    }
}

void ShowMenu()
{
    string menuText = "\n\nWhat I can:\n" +
        "1. Start new quiz\n" +
        "2. Show results of last quiz\n" +
        "0. Exit program\n";
    Console.WriteLine(menuText);
}

void StartQuiz()
{
    Console.WriteLine("Choose difficulty:\n"+
        "1. Easy\n"+
        "2. Medium\n"+
        "3. Hard\n");
    int difficultyLevel = int.Parse(Console.ReadLine());

    string filePath = difficultyLevel switch
    {
        1 => "questions_easy.txt",
        2 => "questions_medium.txt",
        3 => "questions_hard.txt",
        _ => ""
    };
    questions = LoadQuestionsFromFile(filePath);

    if (questions.Count == 0)
    {
        Console.WriteLine("No questions loaded!");
        return;
    }

    Shuffle(questions);
    int questionNumber = 1;
    int rightAnswers = 0;
    int seconds = 5;

    while (questionNumber < questions.Count + 1)
    {
        Console.WriteLine($"Question number {questionNumber}:");
        var question = questions[questionNumber - 1];
        Console.WriteLine(question.Text);

        for (int i = 0; i < question.Answers.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {question.Answers[i]}");
        }

        int? answer = ReadAnswerWithCountdown(seconds);

        if (answer == null)
        {
            Console.WriteLine("No answer counted. Enter to continue");
            Console.ReadKey();
            questionNumber++;
            Console.Clear();
            continue;
        }

        if (answer < 1 || answer > question.Answers.Length)
        {
            Console.WriteLine("Wrong input! Try again.");
            Console.ReadKey();
            Console.Clear();
            continue;
        }

        Console.Clear();

        if (answer.Value == question.RightAnswerNumber)
        {
            rightAnswers++;
            Console.WriteLine("Correct!\n");
        }
        else
        {
            Console.WriteLine($"Wrong! Correct answer: {question.RightAnswerNumber} ({question.Answers[question.RightAnswerNumber - 1]})\n");
        }

        Console.ReadKey();
        Console.Clear();
        questionNumber++;
    }

    Console.WriteLine($"Your result: {rightAnswers}/{questions.Count}");

    lastRightAnswers = rightAnswers;
    lastTotalQuestions = questions.Count;
    hasResult = true;

    SaveResultToFile(rightAnswers, questions.Count);
}


void ShowResults()
{
    string filePath = "results.txt";

    if (!File.Exists(filePath))
    {
        Console.WriteLine("No history yet.");
        return;
    }

    Console.WriteLine("History:");
    Console.WriteLine(File.ReadAllText(filePath));
}

void Shuffle<T>(IList<T> questionsList)
{
    Random rnd = new Random();
    int n = questionsList.Count;
    while (n > 1)
    {
        n--;
        int k = rnd.Next(n+1);
        T value = questionsList[k];
        questionsList[k] = questionsList[n];
        questionsList[n] = value;
    }
}

void SaveResultToFile(int right, int total)
{
    string filePath = "results.txt";
    string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Score: {right}/{total}";
    File.AppendAllText(filePath, line+Environment.NewLine);
}

List<QuizQuestion> LoadQuestionsFromFile(string filePath)
{
    List<QuizQuestion> questions = new List<QuizQuestion>();

    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File not found: {filePath}");
        return questions;
    }

    string[] lines = File.ReadAllLines(filePath);

    foreach (string line in lines)
    {
        if (string.IsNullOrWhiteSpace(line))
            continue;

        string[] parts = line.Split('|');

        string text = parts[0];
        string[] answers = parts[1].Split(';');
        int rightAnswer = int.Parse(parts[2]);

        questions.Add(new QuizQuestion
        {
            Text = text,
            Answers = answers,
            RightAnswerNumber = rightAnswer
        });
    }

    return questions;
}


int? ReadAnswerWithCountdown(int seconds)
{
    DateTime endTime = DateTime.Now.AddSeconds(seconds);

    string input = "";
    int lastShownSeconds = -1;

    while (true)
    {
        int secondsLeft = (int)Math.Ceiling((endTime - DateTime.Now).TotalSeconds);

        if (secondsLeft < 0)
        {
            Console.WriteLine("\nTime is up!");
            return null;
        }

        if (secondsLeft != lastShownSeconds)
        {
            Console.Write($"\rWrite your answer ({secondsLeft}s left): {input}   ");
            lastShownSeconds = secondsLeft;
        }

        if (Console.KeyAvailable)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                if (int.TryParse(input, out int answer))
                    return answer;

                return null;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (input.Length > 0)
                    input = input.Substring(0, input.Length - 1);
            }
            else if (char.IsDigit(key.KeyChar))
            {
                input += key.KeyChar;
            }
        }

        Thread.Sleep(50);
    }
}

class QuizQuestion
{
    public string Text {  get; set; }
    public string[] Answers { get; set; }
    public int RightAnswerNumber { get; set; }
}