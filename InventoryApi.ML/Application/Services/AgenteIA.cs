using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using InventoryApi.ML.Application.DTO;
using InventoryApi.ML.Application.Interfaces;

namespace InventoryApi.ML.Application.Services
{

  public class AgenteIA : IAgenteIA
  {
    private readonly int mUmbralMinino;

    public AgenteIA(int pUmbralMinimo = 15)
    {
        mUmbralMinino = pUmbralMinimo;
    }

    public Task<List<RecomendacionDto>> AnalizarAsync(IEnumerable<OperacionSyncDto> Operaciones, Dictionary<int, int> InventarioActual)
    {
      var lRecomendaciones = new List<RecomendacionDto>();

      foreach (var lArticulo in InventarioActual)
      {
          var lSku = lArticulo.Key;
          var lStockActual = lArticulo.Value;

          // Filtrar operaciones recientes de ese SKU
          var lListaSku = Operaciones.Where(x => x.Sku == lSku).ToList();

          // Frecuencia de movimientos
          int lMovimientos = lListaSku.Count;

          if (lStockActual < mUmbralMinino)
          {
              lRecomendaciones.Add(new RecomendacionDto
              {
                  Sku = lSku,
                  StockActual = lStockActual,
                  Umbral = mUmbralMinino,
                  Recomendacion = $"⚠️ Reabastecer {lSku}. Stock actual: {lStockActual}, operaciones recientes: {lMovimientos}."
              });
          }
      }

      return Task.FromResult(lRecomendaciones);
    }
  }

}
