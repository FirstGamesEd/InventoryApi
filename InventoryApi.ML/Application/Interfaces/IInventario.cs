using System;

using InventoryApi.ML.Application.DTO;
using InventoryApi.ML.Domain.Entities;

namespace InventoryApi.ML.Application.Interfaces;

public interface IInventario
{
  Task<Articulo?> ConsultarAsync(int pSku);
  Task<List<Articulo>> ConsultarTodoAsync();
  Task<Articulo> CrearAsync(string pNombre, int pCantidad);
  Task<Articulo> ActualizarAsync(int pSku, int pDelta, string pRazon, int pVersion, string pTiendaID, Guid pOperacionID);
  Task<Articulo> ReservarAsync(int pSku, int pCantidad, int pVersion, string pTiendaID, Guid pOperacionID);
  Task<RespuestaDto> ProcesoAsync(BatchSyncDto pProceso);
}