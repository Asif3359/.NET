
using System.Drawing;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

interface IShape
{
    double GetArea();
    double GetPerimeter();
}
class Rectangle : IShape
{
    public double Width { get; set; }
    public double Height { get; set; }

    public double GetArea() => Width * Height;
    public double GetPerimeter() => 2 * (Width + Height);
}

class Circle : IShape
{
    public double Radius { get; set; }

    public double GetArea() => Math.PI * Radius * Radius;
    public double GetPerimeter() => 2 * Math.PI * Radius;
}



class Program
{
    public static void Main()
    {
        Rectangle r1 = new Rectangle();
        r1.Width = 2;
        r1.Height = 2;

        Console.WriteLine($"{r1.GetArea()} , {r1.GetPerimeter()}");

        Circle c1 = new Circle();
        c1.Radius = 4;
        Console.WriteLine($"{c1.GetArea()} , {c1.GetPerimeter()}");


    }
}