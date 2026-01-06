public class Student
{
    public int Id { get; }
    public string Name { get; }
    public double CGPA { get; private set; }

    public Student(int id, string name, double cgpa)
    {
        if (cgpa < 0 || cgpa > 4)
            throw new ArgumentException("Invalid CGPA");

        Id = id;
        Name = name;
        CGPA = cgpa;
    }
    public Student(int id, string name)
    {
        Id = id;
        Name = name;
        CGPA = 0.0;
    }
}


class Program
{
    public static void Main()
    {
        Student st1 = new Student(id: 1, name: "Asif Ahammad");
        Console.WriteLine(st1.Id);
        Console.WriteLine(st1.Name);
        Console.WriteLine(st1.CGPA);

        Student st2 = new Student(id: 1, name: "Asif Ahammad", cgpa:3.60);
        Console.WriteLine(st2.Id);
        Console.WriteLine(st2.Name);
        Console.WriteLine(st2.CGPA);
    }
}