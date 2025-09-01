using System;
using System.Text.Json;
using InventoryApi.ML.Application.Interfaces;
using InventoryApi.ML.Domain.Entities;

namespace InventoryApi.ML.Infrastructure.Repositories;

public class InventarioRepo : IInventarioRepo
{
  
  #region Miembros
  private readonly string mRutaArchivo;
  private readonly object mCandado = new ();
  #endregion 

  #region Constructores
  public InventarioRepo (string pRutaArchivo)
  {
    this.mRutaArchivo = pRutaArchivo;

    if(!File.Exists (mRutaArchivo)) File.WriteAllText (this.mRutaArchivo, "[]");
  }
  #endregion

  #region Metodos
  public Task<Articulo?> ConsultarAsync(int pSku)
  {
    var lArticulos = Load();
    return Task.FromResult(lArticulos.FirstOrDefault(x => x.Sku == pSku));
  }

  public Task<List<Articulo>> ConsultarTodoAsync()
  {
    var lArticulos = Load();
    return Task.FromResult(lArticulos.ToList());
  }

  public Task<Articulo> UpsertAsync(Func<Articulo?> pCantidad, Func<Articulo?> pArticulo)
  {
    lock(this.mCandado)
    {
      var lArticulos = Load();

      var lCantidad = pCantidad();
      var lNuevoArticulo = pArticulo();

      if(lNuevoArticulo is null) throw new InvalidOperationException ("No existe un articulo por actualizar o agregar, favor de proprocionar información");

      var lArticuloExistente = lArticulos.Find(x => x.Nombre.Equals(lNuevoArticulo.Nombre, StringComparison.CurrentCultureIgnoreCase));

      var lIndice = lArticulos.FindIndex(x => x.Sku == lNuevoArticulo.Sku);

      if(lIndice >= 0)
      {
        lArticulos[lIndice] = lNuevoArticulo;
      }
      else
      {
        lArticulos.Add(lNuevoArticulo);
      }

      Save(lArticulos);

      return Task.FromResult(lNuevoArticulo);
    }
  }

  public Task<int> ObtenerConsecutivo()
  {
    var lArticulos = Load();
    return Task.FromResult(lArticulos.Count);
  }

  public Task<Articulo?> VerificarExistencia(string pNombre)
  {
    var lArticulos = Load();
    return Task.FromResult(lArticulos.Find(x => x.Nombre.Equals(pNombre, StringComparison.CurrentCultureIgnoreCase)));
  }

  private List<Articulo> Load()
  {
    lock (this.mCandado)
    {
      var lJson = File.ReadAllText(this.mRutaArchivo);
      return JsonSerializer.Deserialize<List<Articulo>>(lJson) ?? new List<Articulo>();
    }
  }

  private void Save(List<Articulo> pArticulos)
  {
    lock(this.mCandado)
    {
      var lJson = JsonSerializer.Serialize(pArticulos, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(this.mRutaArchivo, lJson);
    }
  }
  #endregion

}
