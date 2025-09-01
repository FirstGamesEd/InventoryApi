using System;

namespace InventoryApi.ML.Domain.Entities;

public record Articulo (int Sku, string Nombre, int Cantidad, int Reservado, int Version, DateTimeOffset FechaActualizacion);