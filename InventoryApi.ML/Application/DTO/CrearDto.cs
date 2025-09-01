using System;

namespace InventoryApi.ML.Application.DTO;

public record CrearDto(int Sku, string Nombre, int CantidadInicial);