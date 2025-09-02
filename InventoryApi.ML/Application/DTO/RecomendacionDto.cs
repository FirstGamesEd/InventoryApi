using System;

namespace InventoryApi.ML.Application.DTO;

public class RecomendacionDto
{
  public int Sku { get; set; }
  public int StockActual { get; set; }
  public int Umbral { get; set; }
  public string Recomendacion { get; set; } = string.Empty;
}