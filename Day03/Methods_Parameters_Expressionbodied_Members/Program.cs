// Methods (Advanced Usage)

static int Add(int a, int b)
{
    return a + b;
}

int sum = Add(5, 7);
Console.WriteLine(sum);

// Expression-Bodied Methods 🔥 (Very Common)

static int Multiply(int a, int b) => a * b;

int mul = Multiply(5, 7);
Console.WriteLine(mul);


// Methods with Multiple Return Conditions
static string GetGrade(int marks)
{
    if (marks < 0 || marks > 100)
        return "Invalid";

    if (marks >= 80) return "A+";
    if (marks >= 70) return "A";
    if (marks >= 60) return "A-";

    return "Fail";
}


string grade = GetGrade(80);
Console.WriteLine(grade);


// Optional Parameters (Real API Usage)
static void PrintStudent(
    string name,
    int age,
    string department = "CSE")
{
    Console.WriteLine($"{name}, {age}, {department}");
}

PrintStudent("Asif", 24);
PrintStudent("Rahim", 22, "EEE");

// Named Parameters (Readable Calls)
PrintStudent(
    name: "Karim",
    age: 23,
    department: "BBA");



// ref vs out (Frequently Asked in Interviews)
static void Increase(ref int value)
{
    value += 10;
}

int number = 5;
Increase(ref number);
Console.WriteLine(number);


static bool TryParseAge(string input, out int age)
{
    return int.TryParse(input, out age);
}

int age;

// bool success = TryParseAge("24t", out age);

if (TryParseAge("24t", out age))
{
    Console.WriteLine($"Parsed age: {age}");
}
else
{
    Console.WriteLine("Invalid age input");
}



static string GetStudentStatus(int marks)
{
    if (marks < 0 || marks > 100)
        return "Invalid";

    if (marks >= 80) return "Merit";
    if (marks >= 70) return "Pass";
    if (marks >= 60) return "Fail";

    return "Fail";
}


string grd = GetStudentStatus(80);
Console.WriteLine(grd);


static double CalculateSalary(double baseSalary, double bonus = 0)
{
    return baseSalary+bonus;
}

double salary = CalculateSalary(baseSalary:20000, bonus:2000);
Console.WriteLine(salary);