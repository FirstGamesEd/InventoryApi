using System;

namespace InventoryApi.ML.Application.DTO;

public record ReservarDto(int Sku, int Cantidad, int Version, Guid OperacionID, string TiendaID);