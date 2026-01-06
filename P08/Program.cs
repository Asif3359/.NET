public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double CGPA { get; private set; }

    public void UpdateCGPA(double cgpa)
    {
        if (cgpa < 0 || cgpa > 4)
            throw new ArgumentException();

        CGPA = cgpa;
    }
}



class Program
{
    static void Main()
    {
        Student s1 = new Student();
        s1.Id = 1;
        s1.Name = "Asif";
        s1.UpdateCGPA(3.60);

        Console.WriteLine(s1?.Name);
        Console.WriteLine(s1?.CGPA);



        Student a = new Student { Name = "Asif" };
        Student b = a;

        b.Name = "Rahim";

        Console.WriteLine(a?.Name); // Rahim
    }
}