using System;

using InventoryApi.ML.Application.DTO;

namespace InventoryApi.ML.Application.Interfaces;

public interface ICambios
{
  Task<long> UnirAsync(OperacionSyncDto pOperacion);
  Task<CambioLogDto> LeerAsync(long pPosicion, int pPaginaTamanio = 200);
  bool Existe(Guid pOperacionID);
}