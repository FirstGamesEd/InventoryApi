using System;

namespace InventoryApi.ML.Application.DTO;

public record ActualizarDto(int Sku, int Delta, string Motivo, int Version, Guid OperacionID, string TiendaID);