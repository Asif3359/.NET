
class Student
{
    public string Name { get; set; }
    public double CGPA { get; set; }
}

class Program
{
    public static void Main()
    {
        List<int> numbers = new() { 2, 4, 6, 7, 9, 10 };
        List<int> marks = new() { 20, 40, 60, 70, 90, 100 };

        var even = numbers.Where(n => n % 2 == 0);

        var squares = numbers.Select(n => n * n);

        var sorted = numbers.OrderBy(n => n);
        var desc = numbers.OrderByDescending(n => n);

        var firstEven = numbers.First(n => n % 2 == 0);
        var safeEven = numbers.FirstOrDefault(n => n % 2 == 0);

        bool hasFail = marks.Any(m => m < 60);
        bool allPassed = marks.All(m => m >= 60);

        int count = numbers.Count();
        int sum = numbers.Sum();
        int max = numbers.Max();

        List<Student> students = new()
        {
            new Student { Name = "Asif", CGPA = 3.8 },
            new Student { Name = "Rahim", CGPA = 2.9 }
        };
        var toppers = students.Where(s => s.CGPA >= 3.5).Select(s => s.Name);
        var result = students.Where(s => s.CGPA >= 3.0).OrderByDescending(s => s.CGPA).Select(s => s.Name);

        

    }
}