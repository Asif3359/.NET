// Variables & Data Types
int age = 24;
double cgpa = 3.60;
char grade = 'A';
bool isstudent = true;
string name = "Asif Ahammad";

Console.WriteLine(name);
Console.WriteLine(isstudent ? "student" : "Worker");
Console.WriteLine(cgpa);
Console.WriteLine(age);
Console.WriteLine(grade);



// Type Inference (var)
var student = "Asif Ahammad";
var gpa = 4.83;

Console.WriteLine(student);
Console.WriteLine(gpa);


// Operators

int a = 10;
int b = 3;

Console.WriteLine(a + b);
Console.WriteLine(a - b);
Console.WriteLine(a * b);
Console.WriteLine(a / b);
Console.WriteLine(a % b);


// Comparison Operators
Console.WriteLine(a > b);
Console.WriteLine(a < b);
Console.WriteLine(a == b);
Console.WriteLine(a != b);


// Logical Operators

bool x = true;
bool y = false;

Console.WriteLine(x && y); // AND
Console.WriteLine(x || y); // OR
Console.WriteLine(!x);     // NOT
Console.WriteLine(!y);     // NOT

int futureAge = age + 5;
Console.WriteLine($"Age after 5 years: {futureAge}");
