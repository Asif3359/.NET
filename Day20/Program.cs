class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double CGPA { get; set; }
    public bool IsActive { get; set; }
}

class Program
{
    public static void Main()
    {
        List<Student> students = new()
        {
            new Student { Id = 1, Name = "Asif", CGPA = 3.85, IsActive = true },
            new Student { Id = 2, Name = "Rahim", CGPA = 2.90, IsActive = false },
            new Student { Id = 3, Name = "Karim", CGPA = 3.40, IsActive = true },
            new Student { Id = 4, Name = "Sakib", CGPA = 3.95, IsActive = true },
            new Student { Id = 5, Name = "Nabil", CGPA = 2.50, IsActive = false }
        };

        var isActive = students.Where(n => n.IsActive == true);
        foreach (var item in isActive)
        {
            Console.WriteLine(item.Name);
        }
        Console.WriteLine("\n");
        var sorted = students.OrderByDescending(n => n.CGPA);
        Console.WriteLine(sorted.First().Name);



    }
}