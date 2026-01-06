class Program
{
    static async Task<string> GetMessageAsync()
    {
        await Task.Delay(2000);
        return "Hello after delay";
    }
    static async Task<int> GetNumberAsync()
    {
        await Task.Delay(1000);
        return 42;
    }
    static async Task<string> GetUserAsync()
    {
        await Task.Delay(1500); // simulate DB
        return "Asif";
    }

    public static async Task Main()
    {
        string message = await GetMessageAsync();
        Console.WriteLine(message);

        int result = await GetNumberAsync();
        Console.WriteLine(result);

        string user = await GetUserAsync();
        Console.WriteLine(user);
    }
}