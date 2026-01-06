
class InvalidAgeException : Exception
{
    public InvalidAgeException(string message) : base(message)
    {
    }
}

class Program
{
    public static void Main()
    {
        try
        {
            int a = 10;
            int b = 0;
            int result = a / b;   // ❌ Runtime error
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong!");
            Console.WriteLine(ex.Message);
        }

        try
        {
            int number = int.Parse("abc");
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid number format");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.WriteLine("Cleanup done");
        }

        try
        {
            Withdraw(1000, 2000);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            ValidateAge(12);
        }
        catch (InvalidAgeException ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            Console.Write("Enter number: ");
            int number = int.Parse(Console.ReadLine());

            Console.WriteLine($"Square: {number * number}");
        }
        catch (FormatException)
        {
            Console.WriteLine("Please enter a valid number");
        }


    }
    static void Withdraw(double balance, double amount)
    {
        if (amount > balance)
        {
            throw new InvalidOperationException("Insufficient balance");
        }

        Console.WriteLine("Withdrawal successful");
    }

    static void ValidateAge(int age)
    {
        if (age < 18)
        {
            throw new InvalidAgeException("Age must be 18 or above");
        }
    }

}