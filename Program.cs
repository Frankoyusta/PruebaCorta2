using chairs_dotnet7_api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("chairlist"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

var chairs = app.MapGroup("api/chair");

//TODO: ASIGNACION DE RUTAS A LOS ENDPOINTS

// GETS
chairs.MapGet("/", GetAllChairs);
chairs.MapGet("/{name}",GetChairByName);
// PUTS
chairs.MapPut("/{id}",updateChair);
chairs.MapPut("/{id}/stock",updateStockChair);
// POST
chairs.MapPost("/",createChair);
chairs.MapPost("/purchase", purchaseChair);
// DELETE
chairs.MapDelete("/{id}", deleteChair);
app.Run();

//TODO: ENDPOINTS SOLICITADOS
static async Task<IResult> GetAllChairs(DataContext db)
{
    var listOfChairs = await db.Chairs.ToListAsync();
    return TypedResults.Ok(listOfChairs);
}

static async Task<IResult> GetChairByName(DataContext db,string name)
{
    var chairExist = await db.Chairs.FirstOrDefaultAsync(t => t.Nombre == name);

    if(chairExist == null){
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(chairExist);
}



static async Task<IResult> createChair(DataContext db, Chair chair)
{

    var chairExist = await db.Chairs.FirstOrDefaultAsync(t => t.Nombre == chair.Nombre);

    if (chairExist != null)
    {
        return TypedResults.BadRequest();
    }

    await db.Chairs.AddAsync(chair);
    await db.SaveChangesAsync();
    return TypedResults.Ok(chair);
}



static async Task<IResult> updateChair(DataContext db,int id)
{
    var chairExist = await db.Chairs.FindAsync(id);
    return TypedResults.Ok();
}

static async Task<IResult> updateStockChair(DataContext db, int id, [FromBody] int Stock)
{
    var chairExist = await db.Chairs.FindAsync(id);
    
    if(chairExist == null){
        return TypedResults.NotFound();
    }

    chairExist.Stock = chairExist.Stock + Stock;

    await db.SaveChangesAsync();

    return TypedResults.Ok(chairExist);

}

static async Task<IResult> purchaseChair(DataContext db, int id,int cant, int totalPay)
{
    var chairExist = await db.Chairs.FindAsync(id);


    if(chairExist == null){
        return TypedResults.BadRequest();
    }

    var precio = chairExist.Precio;
    var totalPayReal = precio*cant;
    var stockChair = chairExist.Stock;

    if(stockChair < cant){
        return TypedResults.BadRequest();
    }
    if(totalPay != totalPayReal){
        return TypedResults.BadRequest();
    }

    chairExist.Stock = stockChair - cant;
    await db.SaveChangesAsync();
    return TypedResults.Ok(chairExist);
}


static async Task<IResult> deleteChair(DataContext db,int id)
{
    var chairExist = await db.Chairs.FindAsync(id);

    if(chairExist == null){
        return TypedResults.NotFound();
    }
    db.Chairs.Remove(chairExist);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}