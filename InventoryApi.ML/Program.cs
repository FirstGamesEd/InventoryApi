using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using InventoryApi.ML.Application.Interfaces;
using InventoryApi.ML.Application.Services;
using InventoryApi.ML.Application.DTO;
using InventoryApi.ML.Domain.Exceptions;
using InventoryApi.ML.Infrastructure.Repositories;
using InventoryApi.ML.Infrastructure.Log;
using InventoryApi.ML.Infrastructure.Procesos;

public partial class Program
{
  public static void Main(string[] args)
  {

    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.UseUrls("http://0.0.0.0:80");

    var lJsonArchivoBD = builder.Configuration.GetSection("InventarioBD:RutaArchivo").Value;
    var lJsonArchivoLog = builder.Configuration.GetSection("InventarioLog:RutaArchivo").Value;

    builder.Services.AddSingleton<IInventarioRepo>(new InventarioRepo(lJsonArchivoBD));
    builder.Services.AddSingleton<ICambios>(new Cambios(lJsonArchivoLog));
    builder.Services.AddSingleton<IInventario, Inventario>();

    builder.Services.AddHostedService<TrabajoSync>();

    var app = builder.Build();

    app.MapGet("/Estado", () => Results.Ok(new { status = "ok" }));

    app.MapGet("/Articulo", async (IInventario x) => Results.Ok(await x.ConsultarTodoAsync()));

    app.MapGet("/Articulo/{sku}", async (int Sku, IInventario x) =>
    {
      var lArticulo = await x.ConsultarAsync(Sku);
      return lArticulo is null ? Results.NotFound() : Results.Ok(lArticulo);
    });

    app.MapPost("/Articulo", async (CrearDto dto, IInventario x) =>
    {
      var lArticuloCreado = await x.CrearAsync(dto.Nombre, dto.CantidadInicial);
      return Results.Created($"/Articulo/{lArticuloCreado.Sku}", lArticuloCreado);
    });

    app.MapPut("/Articulo/Actualizar", async (ActualizarDto dto, IInventario x) =>
    {
      try
      {
        var lArticulo = await x.ActualizarAsync(dto.Sku, dto.Delta, dto.Motivo, dto.Version, dto.TiendaID, dto.OperacionID);
        return Results.Ok(lArticulo);
      }
      catch (InvalidOperationException pError)
      {
        return Results.Conflict(new { Descripcion = pError.Message });
      }
      catch (Error pError)
      {
        return Results.Conflict(new { Descripcion = pError.Message, Articulo = pError.ArticuloEX });
      }
      
    });

    app.MapPut("/Articulo/Reservar", async (ReservarDto dto, IInventario x) =>
    {
      try
      {
        var lArticulo = await x.ReservarAsync(dto.Sku, dto.Cantidad, dto.Version, dto.TiendaID, dto.OperacionID);
        return Results.Ok(lArticulo);
      }
      catch (InvalidOperationException pError)
      {
        return Results.Conflict(new { Descripcion = pError.Message });
      }
      catch (Error pError)
      {
        return Results.Conflict(new { Descripcion = pError.Message, Articulo = pError.ArticuloEX });
      }
      
    });

    app.MapPost("/sync/Enviar", async (BatchSyncDto pProceso, IInventario x) =>
    {
      var lRespuesta = await x.ProcesoAsync(pProceso);

      return Results.Ok(lRespuesta);
    });


    app.MapGet("/sync/Obtener", async (long pPosicion, ICambios pCambios) =>
    {
      var lLog = await pCambios.LeerAsync(pPosicion);
      return Results.Ok(lLog);
    });

    app.Run();

  }
}

