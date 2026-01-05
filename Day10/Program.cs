public class Person
{
    public int Id { get; }
    public string Name { get; }

    public Person(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public void PrintInfo()
    {
        Console.WriteLine($"Id: {Id}, Name: {Name}");
    }
    public virtual string GetRole()
    {
        return "Person";
    }
}

public class Student : Person
{
    public double CGPA { get; }

    public Student(int id, string name, double cgpa)
        : base(id, name)
    {
        CGPA = cgpa;
    }
    public override string GetRole()
    {
        return "Student";
    }
}



class Program
{
    public static void Main()
    {
        Student s = new Student(1, "Asif", 3.6);

        s.PrintInfo();   // inherited method
        Console.WriteLine(s.CGPA);

        Console.WriteLine(s.GetRole());
    }
}