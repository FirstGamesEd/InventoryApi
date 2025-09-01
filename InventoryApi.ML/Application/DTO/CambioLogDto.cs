using System;

namespace InventoryApi.ML.Application.DTO;

public record CambioLogDto(long SiguientePosicion, List<CambioEntradaDto> Entrada);