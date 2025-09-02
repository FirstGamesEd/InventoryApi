using System;

using InventoryApi.ML.Application.DTO;

namespace InventoryApi.ML.Application.Interfaces;

public interface IAgenteIA
{
  Task<List<RecomendacionDto>> AnalizarAsync(IEnumerable<OperacionSyncDto> Operaciones, Dictionary<int, int> InventarioActual);
}