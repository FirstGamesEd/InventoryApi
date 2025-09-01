using System;

namespace InventoryApi.ML.Application.DTO;

public record CambioEntradaDto(long Posicion, OperacionSyncDto Operacion);