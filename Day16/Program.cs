class Program
{
    public static void Main()
    {
        MyDelegate d = SayHello;
        d();

        MathOperation op;

        op = Add;
        Console.WriteLine(op(5, 3));   // 8

        op = Multiply;
        Console.WriteLine(op(5, 3));   // 15


        Notify notify = Email;
        notify += SMS;
        notify += Push;

        notify();

        Console.WriteLine(Calculate(10, 5, Add));
        Console.WriteLine(Calculate(10, 5, Multiply));

    }
    static void SayHello()
    {
        Console.WriteLine("Hello from delegate!");
    }
    static int Add(int a, int b)
    {
        return a + b;
    }

    static int Multiply(int a, int b)
    {
        return a * b;
    }
    static void Email()
    {
        Console.WriteLine("Email sent");
    }

    static void SMS()
    {
        Console.WriteLine("SMS sent");
    }

    static void Push()
    {
        Console.WriteLine("Push notification sent");
    }
    delegate void MyDelegate();
    delegate int MathOperation(int a, int b);
    delegate void Notify();
    delegate int Operation(int x, int y);

    static int Calculate(int a, int b, Operation op)
    {
        return op(a, b);
    }


}