interface IVehicle
{
    void Start();
    void Stop();
}


abstract class VehicleBase : IVehicle
{
    public string Brand { get; set; }  // public property
    protected string Model { get; set; }  // protected property

    public VehicleBase(string brand, string model)
    {
        Brand = brand;
        Model = model;
    }

    public abstract void Start();  // Must be implemented
    public abstract void Stop();

    public void ShowInfo()  // Concrete method
    {
        Console.WriteLine($"Brand: {Brand}, Model: {Model}");
    }
}

class Car : VehicleBase
{
    public int Seats { get; set; }

    public Car(string brand, string model, int seats) : base(brand, model)
    {
        Seats = seats;
    }

    public override void Start()
    {
        Console.WriteLine($"{Brand} Car is starting with {Seats} seats!");
    }

    public override void Stop()
    {
        Console.WriteLine($"{Brand} Car has stopped.");
    }
}

class Bike : VehicleBase
{
    public bool HasCarrier { get; set; }

    public Bike(string brand, string model, bool hasCarrier) : base(brand, model)
    {
        HasCarrier = hasCarrier;
    }

    public override void Start()
    {
        Console.WriteLine($"{Brand} Bike is starting. Carrier: {HasCarrier}");
    }

    public override void Stop()
    {
        Console.WriteLine($"{Brand} Bike has stopped.");
    }
}



class Program
{
    static void Main()
    {
        List<IVehicle> vehicles = new();

        bool running = true;
        while (running)
        {
            Console.WriteLine("\n--- Vehicle Management System ---");
            Console.WriteLine("1. Add Car");
            Console.WriteLine("2. Add Bike");
            Console.WriteLine("3. Show All Vehicles");
            Console.WriteLine("4. Exit");
            Console.Write("Choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Brand: ");
                    string carBrand = Console.ReadLine();
                    Console.Write("Model: ");
                    string carModel = Console.ReadLine();
                    Console.Write("Seats: ");
                    int seats = int.TryParse(Console.ReadLine(), out seats) ? seats : 4;

                    vehicles.Add(new Car(carBrand, carModel, seats));
                    Console.WriteLine("Car added!");
                    break;

                case "2":
                    Console.Write("Brand: ");
                    string bikeBrand = Console.ReadLine();
                    Console.Write("Model: ");
                    string bikeModel = Console.ReadLine();
                    Console.Write("Has Carrier (true/false): ");
                    bool hasCarrier = bool.TryParse(Console.ReadLine(), out hasCarrier) ? hasCarrier : false;

                    vehicles.Add(new Bike(bikeBrand, bikeModel, hasCarrier));
                    Console.WriteLine("Bike added!");
                    break;

                case "3":
                    foreach (var v in vehicles)
                    {
                        if (v is VehicleBase vb)
                        {
                            vb.ShowInfo();
                        }
                        v.Start();
                        v.Stop();
                        Console.WriteLine("----------------");
                    }
                    break;

                case "4":
                    running = false;
                    Console.WriteLine("Exiting...");
                    break;

                default:
                    Console.WriteLine("Invalid choice!");
                    break;
            }
        }
    }
}
