using System;

namespace InventoryApi.ML.Application.DTO;

public record OperacionSyncDto
{
    public required int Sku { get; init; }
    public required int Delta { get; init; } 
    public required string Tipo{ get; init; }
    public required Guid OperacionID { get; init; }
    public required string TiendaID { get; init; }
    public int Version { get; init; }
    public DateTimeOffset FechaMovimiento { get; init; } = DateTimeOffset.Now;
}