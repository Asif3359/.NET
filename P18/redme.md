🔥 **Day 19 – Func, Action, Predicate (Deep Dive)**
This is **CORE INTERVIEW + REAL PROJECT SKILL**.
You’ll see these **everywhere**: LINQ, ASP.NET Core, EF Core, middleware, events.

---

# 🧠 1️⃣ What Are They?

| Type          | Return | Purpose                  |
| ------------- | ------ | ------------------------ |
| **Func**      | Yes    | Calculate / return value |
| **Action**    | No     | Perform task             |
| **Predicate** | `bool` | Check condition          |

📌 All are **generic delegates**

---

# 🔹 2️⃣ Func – Deep Dive

### ✅ Syntax

```csharp
Func<T1, T2, ..., TResult>
```

### Example

```csharp
Func<int, int, int> add = (a, b) => a + b;
Console.WriteLine(add(10, 20));
```

### With 1 parameter

```csharp
Func<int, int> square = x => x * x;
```

### With NO parameters

```csharp
Func<DateTime> now = () => DateTime.Now;
```

---

## 🔹 Func in Real Project

```csharp
List<int> numbers = new() { 1, 2, 3, 4, 5 };

var result = numbers.Select(n => n * 2);
```

Behind the scenes:

```csharp
Select(Func<int, int> selector)
```

---

# 🔹 3️⃣ Action – Deep Dive

### Syntax

```csharp
Action<T1, T2, ...>
```

### Example

```csharp
Action<string> print = msg => Console.WriteLine(msg);
print("Hello Asif");
```

### Multiple parameters

```csharp
Action<int, int> logSum = (a, b) =>
{
    Console.WriteLine(a + b);
};
```

### No parameters

```csharp
Action greet = () => Console.WriteLine("Welcome!");
```

---

## 🔹 Action in Real Project

```csharp
Action<string> logger = message =>
{
    Console.WriteLine($"LOG: {message}");
};
```

Used in:

* Logging
* Event handlers
* Middleware

---

# 🔹 4️⃣ Predicate – Deep Dive

### Definition

```csharp
Predicate<T>  // returns bool
```

### Example

```csharp
Predicate<int> isEven = x => x % 2 == 0;

Console.WriteLine(isEven(6)); // true
```

---

## 🔹 Predicate with Collections

```csharp
List<int> marks = new() { 45, 67, 89, 32 };

var passed = marks.FindAll(m => m >= 60);

foreach (var m in passed)
{
    Console.WriteLine(m);
}
```

---

# 🔹 5️⃣ Func vs Action vs Predicate (INTERVIEW GOLD)

| Scenario           | Use       |
| ------------------ | --------- |
| Need return value  | Func      |
| Just perform task  | Action    |
| True / False check | Predicate |

---

# 🔹 6️⃣ Replace Delegate with Func/Action

### ❌ Old

```csharp
delegate int Calc(int x, int y);
```

### ✅ Modern

```csharp
Func<int, int, int> calc = (x, y) => x + y;
```

---

# 🔹 7️⃣ Passing Func/Action as Method Parameter

```csharp
static void Process(Func<int, int> operation)
{
    Console.WriteLine(operation(10));
}

Process(x => x * 2);
```

---

# 🔹 8️⃣ Combining with Events

```csharp
Action<double> OnPaymentSuccess;

OnPaymentSuccess += amount =>
{
    Console.WriteLine($"Payment of {amount} completed");
};
```

---

# 🧪 PRACTICE TASKS (MANDATORY)

### 📝 Task 1

Create:

```csharp
Func<int, bool> isPrime
```

---

### 📝 Task 2

Create:

```csharp
Action<string> saveLog
```

---

### 📝 Task 3

Create a list of students and:

* Use **Predicate** to filter CGPA ≥ 3.5

---

# 🧠 INTERVIEW QUESTIONS

❓ Why use Func instead of method?
✔ Flexibility
✔ Pass logic dynamically
✔ Cleaner code

❓ Max parameters?
✔ Func: up to 16 parameters
✔ Action: up to 16 parameters

---

## ✅ YOU COMPLETED

✔ Day 19 – Func, Action, Predicate

---

### 🔜 NEXT

```
Day 20 – LINQ Basics
```

Say **YES** when ready 🚀
