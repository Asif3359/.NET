// Advanced if / else (Guard Clauses)
int marks = 60;
string grade = "";

if (marks < 0 || marks > 100)
{
    Console.WriteLine("Invalid Marks");
    return;
}

if (marks >= 80)
{
    grade = "A+";
    Console.WriteLine("A+");
}
else if (marks >= 70)
{
    grade = "A";
    Console.WriteLine("A");
}
else if (marks >= 60)
{
    grade = "A-";
    Console.WriteLine("A-");
}
else
{
    Console.WriteLine("Fail");
}



// Modern C# switch Expression (Recommended)


string result = grade switch
{
    "A+" => "Excellent",
    "A" => "Good",
    "A-" => "Average",
    _ => "Fail"
};

Console.WriteLine(result);


string status = marks switch
{
    >= 80 => "A+",
    >= 70 => "A",
    >= 60 => "A-",
    _ => "Failed"
};

Console.WriteLine(status);

// Loop Best Practices (Avoid Classic Mistakes)

for (int i = 0; i <= 5; i++)
{
    Console.WriteLine($"Iteration {i}");
}

int balance = 100;

while (balance > 0)
{
    balance -= 20;
    Console.WriteLine($"Balance: {balance}");
}

// foreach (MOST USED in .NET APIs)
string[] names = { "Asif", "Rahim", "Karim" };

foreach (var item in names)
{
    Console.WriteLine(item);
}

for (int i = 1; i <= 10; i++)
{
    if (i == 5) continue;
    if (i == 9) break;

    Console.WriteLine(i);
}
