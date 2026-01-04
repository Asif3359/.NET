// string name = "Asif";
// name += " Ahmed";   // ❌ new object created
// // ❗ Bad in loops → memory + performance issues
// Console.WriteLine(name);



// String Interpolation (Best Practice)
using System.Text;

string firstName = "Asif";
string lastName = "Ahmed";

string fullName = $"{firstName} {lastName}";
Console.WriteLine(fullName);


string input = "  Asif Ahmed  ";

Console.WriteLine(input.Trim());
Console.WriteLine(input.ToUpper());
Console.WriteLine(input.ToLower());
Console.WriteLine(input.Replace("Ahmed", "A."));


string data = "Asif,24,3.60";

var parts = data.Split(',');

string name = parts[0];

if (int.TryParse(parts[1], out int age) &&
    double.TryParse(parts[2], out double cgpa))
{
    Console.WriteLine(name);
    Console.WriteLine(age);
    Console.WriteLine(cgpa);
}
else
{
    Console.WriteLine("Invalid input");
}



var sb = new StringBuilder();

for (int i = 0; i < 1000; i++)
{
    sb.Append(i);
}

string result = sb.ToString();
