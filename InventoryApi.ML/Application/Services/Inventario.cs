using System;

using InventoryApi.ML.Application.DTO;
using InventoryApi.ML.Application.Interfaces;
using InventoryApi.ML.Domain.Exceptions;
using InventoryApi.ML.Domain.Entities;

namespace InventoryApi.ML.Application.Services;

public class Inventario : IInventario
{

  #region Miembros
  private readonly IInventarioRepo mInventarioRepoInterfaz;
  private readonly ICambios mCambiosInterfaz;
  #endregion

  #region Constructores
  public Inventario(IInventarioRepo pInventarioRepoInterfaz, ICambios pCambiosInterfaz)
  {
    this.mInventarioRepoInterfaz = pInventarioRepoInterfaz;
    this.mCambiosInterfaz = pCambiosInterfaz;
  }
  #endregion

  #region Metodos
  public Task<Articulo?> ConsultarAsync(int pSku) => this.mInventarioRepoInterfaz.ConsultarAsync(pSku);

  public Task<List<Articulo>> ConsultarTodoAsync() => this.mInventarioRepoInterfaz.ConsultarTodoAsync();

  public async Task<Articulo> CrearAsync(string pNombre, int pCantidad)
  {
    var lArticulo = await this.mInventarioRepoInterfaz.VerificarExistencia(pNombre);

    if(lArticulo is not null) throw new InvalidOperationException("El articulo ya se encuentra registrado. Favor de actualizar el inventario.");

    var lSkuConsecutivo = await this.mInventarioRepoInterfaz.ObtenerConsecutivo();

    var lArticuloNuevo = new Articulo(lSkuConsecutivo + 1, pNombre, pCantidad, 0, 1, DateTimeOffset.Now);

    return await this.mInventarioRepoInterfaz.UpsertAsync(() => null, () => lArticuloNuevo);
  }

  public async Task<Articulo> ActualizarAsync(int pSku, int pDelta, string pRazon, int pVersion, string pTiendaID, Guid pOperacionID)
  {
    if (pDelta == 0) throw new InvalidOperationException("Delta debe de ser diferente de cero.");    

    if (this.mCambiosInterfaz.Existe(pOperacionID)) throw new InvalidOperationException("Esta operación ya fue realizada. Favor de realizar una nueva");

    var lArticuloActualizado = await this.mInventarioRepoInterfaz.UpsertAsync(
      () => this.mInventarioRepoInterfaz.ConsultarAsync(pSku).GetAwaiter().GetResult(),
      () =>
      {
        var lArticulo = this.mInventarioRepoInterfaz.ConsultarAsync(pSku).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Articulo no encontrado");

        if (lArticulo.Version != pVersion) throw new Error("Conflicto de versión: alguien más actualizó este articulo", lArticulo);

        var lNuevaCantidad = lArticulo.Cantidad + pDelta;

        if (lNuevaCantidad < lArticulo.Reservado) throw new InvalidOperationException("La cantidad bajaría por debajo del monto reservado.");

        return lArticulo with { Cantidad = lNuevaCantidad, Version = lArticulo.Version + 1, FechaActualizacion = DateTimeOffset.Now };
      });

    await this.mCambiosInterfaz.UnirAsync(new DTO.OperacionSyncDto
    {
      Sku = pSku,
      Delta = pDelta,
      Tipo = "Ajuste",
      OperacionID = pOperacionID,
      TiendaID = pTiendaID,
      Version = pVersion
    });

    return lArticuloActualizado;
  }

  public async Task<Articulo> ReservarAsync(int pSku, int pCantidad, int pVersion, string pTiendaID, Guid pOperacionID)
  {
    if (pCantidad <= 0) throw new InvalidOperationException("La cantidad debe de ser mayor a cero");    

    if (this.mCambiosInterfaz.Existe(pOperacionID)) throw new InvalidOperationException("Esta operación ya fue realizada. Favor de realizar una nueva");

    var lArticuloReservado = await this.mInventarioRepoInterfaz.UpsertAsync(
      () => this.mInventarioRepoInterfaz.ConsultarAsync(pSku).GetAwaiter().GetResult(),
      () =>
      {
          var lArticulo = this.mInventarioRepoInterfaz.ConsultarAsync(pSku).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Articulo no encontrado");
          if (lArticulo.Version != pVersion) throw new Error("Conflicto de versión: alguien más actualizó este articulo", lArticulo);

          if (lArticulo.Cantidad - lArticulo.Reservado < pCantidad)
              throw new InvalidOperationException("La cantidad de articulos en stock no permite realizar reservaciones");

          return lArticulo with { Reservado = lArticulo.Reservado + pCantidad, Version = lArticulo.Version + 1, FechaActualizacion = DateTimeOffset.Now };
      });

    await this.mCambiosInterfaz.UnirAsync(new DTO.OperacionSyncDto
    {
      Sku = pSku,
      Delta = pCantidad,
      Tipo = "Reserva",
      OperacionID = pOperacionID,
      TiendaID = pTiendaID,
      Version = pVersion
    });

    return lArticuloReservado;
  }

  public async Task<RespuestaDto> ProcesoAsync(BatchSyncDto pProceso)
  {
    var lOperaciones = new List<OperacionSyncDto>();
    
    foreach (var lOperacion in pProceso.Operaciones)
    {
      if (lOperacion.Tipo.Equals("Ajuste"))
      {
        _ = await ActualizarAsync(lOperacion.Sku, lOperacion.Delta, "sync", lOperacion.Version, lOperacion.TiendaID, lOperacion.OperacionID);
      }
      else if (lOperacion.Tipo.Equals("Reserva"))
      {
        _ = await ReservarAsync(lOperacion.Sku, lOperacion.Delta, lOperacion.Version, lOperacion.TiendaID, lOperacion.OperacionID);
      }
      else
      {
        continue;
      }

      lOperaciones.Add(lOperacion);        
    }

    return new RespuestaDto(lOperaciones.Count, 0);
  }
  #endregion

}