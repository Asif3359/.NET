using System;

class Alarm
{
    public delegate void Notify(); // Add public modifier
    public event Notify OnAlarm;

    public void Trigger()
    {
        Console.WriteLine("Alarm triggered!");
        OnAlarm?.Invoke();
    }
}

class Button
{
    public event Action OnClick; // Fixed typo: was Onclck

    public void Click()
    {
        Console.WriteLine("Button clicked!");
        OnClick?.Invoke(); // Fixed: was OnClick?.Invoke()
    }
}

class Program
{
    static void WakeUp()
    {
        Console.WriteLine("Wake up!");
    }
    static void TurnOnLights()
    {
        Console.WriteLine("Lights on");
    }

    static void CallPolice()
    {
        Console.WriteLine("Police notified");
    }

    // First method to subscribe
    static void ShowMessage()
    {
        Console.WriteLine("Button was clicked - Message 1");
    }

    // Second method to subscribe
    static void UpdateStatus()
    {
        Console.WriteLine("Status updated: Button clicked!");
    }

    public static void Main()
    {
        Console.WriteLine("=== Testing Alarm ===");
        Alarm alarm = new Alarm();
        alarm.OnAlarm += WakeUp;
        alarm.OnAlarm += TurnOnLights;
        alarm.OnAlarm += CallPolice;
        alarm.Trigger();

        Console.WriteLine("\n=== Testing Button ===");
        Button button = new Button();

        button.OnClick += ShowMessage;
        button.OnClick += UpdateStatus;

        // Trigger the click
        button.Click();

        Console.WriteLine("\n--- Adding another method dynamically ---");

        // Add another method at runtime
        button.OnClick += () => Console.WriteLine("Third handler: Action completed!");

        // Trigger click again to show all 3 methods
        button.Click();

        Console.WriteLine("\n--- Removing one method ---");

        // Remove one method
        button.OnClick -= ShowMessage;

        // Trigger click to show remaining methods
        button.Click();
    }
}