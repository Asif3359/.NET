class Program
{
    delegate int MathOperation(int a, int b);
    public static void Main()
    {
        MathOperation add = (a, b) => a + b;
        Console.WriteLine(add(5, 3));

        Func<int, int, int> multiply = (a, b) => a * b;
        Console.WriteLine(multiply(4, 5));

        Predicate<int> isEven = x => x % 2 == 0;
        Console.WriteLine(isEven(10)); // true

        Func<int, int> square = x =>
        {
            int result = x * x;
            return result;
        };
        Console.WriteLine(square(4));


        List<int> numbers = new() { 1, 2, 3, 4, 5 };

        var evens = numbers.Where(n => n % 2 == 0);

        foreach (var n in evens)
        {
            Console.WriteLine(n);
        }
    }
}