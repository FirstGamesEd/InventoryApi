using System;

using InventoryApi.ML.Domain.Entities;

namespace InventoryApi.ML.Application.Interfaces;

public interface IInventarioRepo
{
    Task<Articulo?> ConsultarAsync(int pSku);
    Task<List<Articulo>> ConsultarTodoAsync();
    Task<Articulo> UpsertAsync(Func<Articulo?> pCantidad, Func<Articulo?> pArticulo);
    Task<int> ObtenerConsecutivo();
    Task<Articulo?> VerificarExistencia(string pNombre);
}