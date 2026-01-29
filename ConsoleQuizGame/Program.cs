using System.Collections.Generic;
using System.Text;


List<QuizQuestion> questions = new List<QuizQuestion>
{
    new QuizQuestion
    {
        Text = "The capital of Great Britain?\n",
        Answers = new[] { "Berlin", "Moscow", "Paris", "London" },
        RightAnswerNumber = 4
    },
    new QuizQuestion
    {
        Text = "2+2",
        Answers = new[] { "22", "4", "6", "2" },
        RightAnswerNumber = 2
    },
    new QuizQuestion
    {
        Text = "3+2",
        Answers = new[] { "22", "4", "6", "2" },
        RightAnswerNumber = 2
    },
    new QuizQuestion
    {
        Text = "4+2",
        Answers = new[] { "22", "4", "6", "2" },
        RightAnswerNumber = 2
    },
    new QuizQuestion
    {
        Text = "5+2",
        Answers = new[] { "22", "4", "6", "2" },
        RightAnswerNumber = 2
    },
};

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