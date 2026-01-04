
// class Program
// {
//     static List<Student> students = new List<Student>();

//     static void Main()
//     {
//         Console.WriteLine("Welcome to Student Portal");

//         bool exit = false;

//         while (!exit)
//         {
//             Console.WriteLine("\n=== MENU ===");
//             Console.WriteLine("1. Add Student");
//             Console.WriteLine("2. View All Students");
//             Console.WriteLine("3. Search Student by ID");
//             Console.WriteLine("4. Remove Student");
//             Console.WriteLine("5. Update Student");
//             Console.WriteLine("6. Exit");

//             Console.Write("\nEnter option: ");
//             string input = Console.ReadLine();

//             if (int.TryParse(input, out int option))
//             {
//                 switch (option)
//                 {
//                     case 1:
//                         AddStudent();
//                         break;
//                     case 2:
//                         ViewAllStudents();
//                         break;
//                     case 3:
//                         SearchStudentById();
//                         break;
//                     case 4:
//                         RemoveStudent();
//                         break;
//                     case 5:
//                         UpdateStudent();
//                         break;
//                     case 6:
//                         exit = true;
//                         Console.WriteLine("Exiting... Thank you!");
//                         break;
//                     default:
//                         Console.WriteLine("Invalid option! Please choose 1-6.");
//                         break;
//                 }
//             }
//             else
//             {
//                 Console.WriteLine("Invalid input! Please enter a number.");
//             }
//         }
//     }

//     static void AddStudent()
//     {
//         Console.WriteLine("\n--- Add Student ---");

//         Console.Write("Enter Student ID: ");
//         int id = int.Parse(Console.ReadLine());

//         // Check if ID already exists
//         if (students.Exists(s => s.Id == id))
//         {
//             Console.WriteLine($"Student with ID {id} already exists!");
//             return;
//         }

//         Console.Write("Enter Name: ");
//         string name = Console.ReadLine();

//         Console.Write("Enter Age: ");
//         int age = int.Parse(Console.ReadLine());

//         Console.Write("Enter Grade: ");
//         string grade = Console.ReadLine();

//         Student newStudent = new Student(id, name, age, grade);
//         students.Add(newStudent);

//         Console.WriteLine("Student added successfully!");
//     }

//     static void ViewAllStudents()
//     {
//         Console.WriteLine("\n--- All Students ---");

//         if (students.Count == 0)
//         {
//             Console.WriteLine("No students found.");
//             return;
//         }

//         Console.WriteLine($"{"ID",-10} {"Name",-20} {"Age",-10} {"Grade",-10}");
//         Console.WriteLine(new string('-', 50));

//         foreach (var student in students)
//         {
//             Console.WriteLine($"{student.Id,-10} {student.Name,-20} {student.Age,-10} {student.Grade,-10}");
//         }

//         Console.WriteLine($"\nTotal Students: {students.Count}");
//     }

//     static void SearchStudentById()
//     {
//         Console.WriteLine("\n--- Search Student ---");

//         Console.Write("Enter Student ID to search: ");
//         if (int.TryParse(Console.ReadLine(), out int searchId))
//         {
//             Student student = students.Find(s => s.Id == searchId);

//             if (student != null)
//             {
//                 Console.WriteLine("\nStudent Found:");
//                 Console.WriteLine($"ID: {student.Id}");
//                 Console.WriteLine($"Name: {student.Name}");
//                 Console.WriteLine($"Age: {student.Age}");
//                 Console.WriteLine($"Grade: {student.Grade}");
//             }
//             else
//             {
//                 Console.WriteLine($"Student with ID {searchId} not found.");
//             }
//         }
//         else
//         {
//             Console.WriteLine("Invalid ID format!");
//         }
//     }

//     static void RemoveStudent()
//     {
//         Console.WriteLine("\n--- Remove Student ---");

//         Console.Write("Enter Student ID to remove: ");
//         if (int.TryParse(Console.ReadLine(), out int removeId))
//         {
//             Student student = students.Find(s => s.Id == removeId);

//             if (student != null)
//             {
//                 Console.WriteLine($"Remove student: {student.Name} (ID: {student.Id})?");
//                 Console.Write("Confirm (y/n): ");
//                 string confirm = Console.ReadLine().ToLower();

//                 if (confirm == "y" || confirm == "yes")
//                 {
//                     students.Remove(student);
//                     Console.WriteLine("Student removed successfully!");
//                 }
//                 else
//                 {
//                     Console.WriteLine("Operation cancelled.");
//                 }
//             }
//             else
//             {
//                 Console.WriteLine($"Student with ID {removeId} not found.");
//             }
//         }
//         else
//         {
//             Console.WriteLine("Invalid ID format!");
//         }
//     }

//     static void UpdateStudent()
//     {
//         Console.WriteLine("\n--- Update Student ---");

//         Console.Write("Enter Student ID to update: ");
//         if (int.TryParse(Console.ReadLine(), out int updateId))
//         {
//             Student student = students.Find(s => s.Id == updateId);

//             if (student != null)
//             {
//                 Console.WriteLine("\nCurrent Information:");
//                 Console.WriteLine($"1. Name: {student.Name}");
//                 Console.WriteLine($"2. Age: {student.Age}");
//                 Console.WriteLine($"3. Grade: {student.Grade}");

//                 Console.WriteLine("\nWhat would you like to update?");
//                 Console.WriteLine("1. Name");
//                 Console.WriteLine("2. Age");
//                 Console.WriteLine("3. Grade");
//                 Console.WriteLine("4. Cancel");

//                 Console.Write("Enter choice: ");
//                 if (int.TryParse(Console.ReadLine(), out int choice))
//                 {
//                     switch (choice)
//                     {
//                         case 1:
//                             Console.Write("Enter new name: ");
//                             student.Name = Console.ReadLine();
//                             Console.WriteLine("Name updated successfully!");
//                             break;
//                         case 2:
//                             Console.Write("Enter new age: ");
//                             if (int.TryParse(Console.ReadLine(), out int newAge))
//                             {
//                                 student.Age = newAge;
//                                 Console.WriteLine("Age updated successfully!");
//                             }
//                             else
//                             {
//                                 Console.WriteLine("Invalid age!");
//                             }
//                             break;
//                         case 3:
//                             Console.Write("Enter new grade: ");
//                             student.Grade = Console.ReadLine();
//                             Console.WriteLine("Grade updated successfully!");
//                             break;
//                         case 4:
//                             Console.WriteLine("Update cancelled.");
//                             break;
//                         default:
//                             Console.WriteLine("Invalid choice!");
//                             break;
//                     }
//                 }
//             }
//             else
//             {
//                 Console.WriteLine($"Student with ID {updateId} not found.");
//             }
//         }
//         else
//         {
//             Console.WriteLine("Invalid ID format!");
//         }
//     }
// }

