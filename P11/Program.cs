public class Employee
{
    public virtual double CalculateSalary()
    {
        return 0;
    }
}

public class FullTimeEmployee : Employee
{
    public override double CalculateSalary()
    {
        return 50000;
    }
}

public class PartTimeEmployee : Employee
{
    public override double CalculateSalary()
    {
        return 20000;
    }
}


class Program
{
    public static void Main()
    {
        List<Employee> employees = new()
        {
            new FullTimeEmployee(),
            new PartTimeEmployee()
        };

        foreach (Employee emp in employees)
        {
            Console.WriteLine(emp.CalculateSalary());
        }
    }
}