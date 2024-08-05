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
        Specialty = "IT Specialist"
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
        EmployeeId = 09,
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
        EmployeeId = 09,
        Description = "Software installation request for the finance department",
        Emergency = false,
        DateCompleted =  new DateTime(2024, 07, 22)
    },

    new ServiceTicket()
    {
        Id = 67,
        CustomerId = 53,
        EmployeeId = 0,
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
    serviceTicket.Customer = customers.FirstOrDefault(e => e.Id == serviceTicket.CustomerId);
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
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTickets.Remove(serviceTicket);
    return Results.NoContent();
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPut("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (ticketToComplete == null)
    {
        return Results.NotFound();
    }
    ticketToComplete.DateCompleted = DateTime.Today;
    return Results.Ok(ticketToComplete);
});

app.MapGet("/api/serviceTickets/emergencies", () =>

{
    List<ServiceTicket> emergencies = serviceTickets.Where(st => st.Emergency == true && st.DateCompleted == null).ToList();
    return Results.Ok(emergencies);
});

app.MapGet("/apiserviceTickets/unassigned", () =>
{
    List<ServiceTicket> unassigned = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    return Results.Ok(unassigned);

});

app.MapGet("/customer/inactive", () =>
{
    DateTime lastYear = DateTime.Now.AddYears(-1);

    List<Customer> inactiveCustomers = customers
                   .Where(c => !serviceTickets
                   .Any(st => st.CustomerId == c.Id && st.DateCompleted.HasValue && st.DateCompleted.Value > lastYear))
                   .ToList();

    return Results.Ok(inactiveCustomers);
});


app.MapGet("/api/employee/available", () =>
{
    var availableEmployees = serviceTickets.Where(st => st.DateCompleted == null).Select(st => st.EmployeeId).ToList();
    var available = employees.Where(st => !availableEmployees.Contains(st.Id)).ToList();
    return Results.Ok(available);
});

app.MapGet("/employees/{id}/customers", (int id) =>
{
    var customerIds = serviceTickets.Where(st => st.EmployeeId == id).Select(st => st.CustomerId).Distinct().ToList();
    var customersForEmployee = customers.Where(c => customerIds.Contains(c.Id)).ToList();
    return Results.Ok(customersForEmployee);
});

app.MapGet("/employee/month", () =>
{
    var completedTicketCountByEmployee = serviceTickets
        .Where(st => st.EmployeeId != null && st.DateCompleted != null)
        .GroupBy(st => st.EmployeeId) 
        .Select(st => new
        {
            EmployeeId = st.Key,
            ServiceTicketCount = st.Count() 
        })
        .OrderByDescending(c => c.ServiceTicketCount)
        .FirstOrDefault();

    if (completedTicketCountByEmployee == null)
    {
        return Results.NotFound();
    }

    var topTicketCountByEmployee = employees.FirstOrDefault(e => e.Id == completedTicketCountByEmployee.EmployeeId);

    return Results.Ok(topTicketCountByEmployee);
});

app.MapGet("/api/serviceTickets/review", () =>
{
    List<ServiceTicket> completedTickets = serviceTickets
    .Where(st => st.DateCompleted.HasValue)
    .OrderBy(st => st.DateCompleted)
    .ToList();

    foreach (var ticket in completedTickets)
    {
        ticket.Customer = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
        ticket.Employee = employees.FirstOrDefault(e => e.Id == ticket.EmployeeId);
    }

    return Results.Ok(completedTickets);
});


app.Run();