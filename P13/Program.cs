class Person
{
    public string Name;       // Accessible everywhere
    private int Age;          // Only inside Person

    public void SetAge(int age)
    {
        Age = age;           // Private field can be modified via method
    }

    public int GetAge() => Age;
}


class Program
{
    public static void Main()
    {
        var p = new Person();
        p.Name = "Asif";       // ✅ OK
                               // p.Age = 24;          // ❌ Error
        p.SetAge(24);           // ✅ OK
        Console.WriteLine(p.GetAge());
    }
}