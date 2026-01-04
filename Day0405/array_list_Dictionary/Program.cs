// Arrays (Use Carefully)
int[] scores = { 65, 78, 90, 55 };

foreach (var score in scores)
{
    Console.WriteLine(score);
}



// Lists (MOST USED)
List<int> marks = new() { 60, 72, 85, 49 };

marks.Add(95);
marks.Remove(49);

var passed = marks.Where(m => m >= 60);

foreach (var m in passed)
{
    Console.WriteLine(m);
}


// Dictionaries (Key–Value Mapping)
Dictionary<int, string> students = new()
{
    { 1, "Asif" },
    { 2, "Rahim" }
};

students[3] = "Karim";
students.Add(4,"Adf");


foreach (var item in students)
{
    Console.WriteLine($"{item.Key}, {item.Value}");
}

