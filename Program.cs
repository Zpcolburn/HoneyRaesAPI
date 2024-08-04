using HoneyRaesAPI.Models;

List<Customer> customers = new List<Customer> ()
{
    new Customer()
    {
        Name = "Felicia",
        Id = 15,
        Address = "1520 Cyprus Lane, Nashville, TN"
    },

    new Customer()
    {
        Name = "Fletcher",
        Id = 37,
        Address = "3023 Dogwood Trail, Spring Hill, TN"
    },

    new Customer()
    {
        Name = "Brit",
        Id = 53,
        Address = "8951 SW 85th St, Miami, FL"
    }
};

List<Employee> employees = new List<Employee>() 
{
    new Employee()
    {
        Name = "Jake",
        Id = 09,
        Specialty = "Grade setter"
    },

    new Employee()
    {
        Name = "Zach",
        Id = 27,
        Specialty = "Jack of all trades"
    }
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket>()
{
    new ServiceTicket()
    {
        Id = 12,
        CustomerId = 53,
        EmployeeId = 27,
        Description = "Network outage in the main office",
        Emergency = true,
        DateCompleted =  new DateTime(2024, 07, 20)
    },

    new ServiceTicket()
    {
        Id = 45,
        CustomerId = 37,
        EmployeeId = 0,
        Description = "Printer malfunction on the 3rd floor",
        Emergency = false,
        DateCompleted =  new DateTime(2024, 07, 18)
    },

    new ServiceTicket()
    {
        Id = 28,
        CustomerId = 18,
        EmployeeId = 0,
        Description = "Software installation request for the finance department",
        Emergency = false,
        DateCompleted =  new DateTime(2024, 07, 22)
    },

    new ServiceTicket()
    {
        Id = 67,
        CustomerId = 53,
        EmployeeId = 09,
        Description = "Security breach in the IT system",
        Emergency = true,
        DateCompleted =  new DateTime()
    },

    new ServiceTicket()
    {
        Id = 89,
        CustomerId = 37,
        EmployeeId = 27,
        Description = "HVAC system maintenance in the server room",
        Emergency = true,
        DateCompleted =  new DateTime(2024, 09, 07)
    },
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(e => e.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    return Results.Ok(serviceTicket);
});

app.MapGet("employees", () =>
{
    return employees;
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapGet("customers", () =>
{
    return customers;
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(e => e.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(customer);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.Run();