using System;

namespace InventoryApi.ML.Application.DTO;

public record BatchSyncDto(List<OperacionSyncDto> Operaciones);