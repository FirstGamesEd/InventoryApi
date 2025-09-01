using System;
using System.Collections.Generic;
using System.Text.Json;
using InventoryApi.ML.Application.DTO;
using InventoryApi.ML.Application.Interfaces;

namespace InventoryApi.ML.Infrastructure.Log;

public class Cambios : ICambios
{

  #region Miembros
  private readonly List<CambioEntradaDto> mLog;
  private readonly HashSet<Guid> mVisto;
  private long mPosicion = 0;
  private readonly object mCandado = new();
  private readonly string mArchivoLog;
  #endregion

  #region Constructores
  public Cambios(string pRutaArchivo)
  {
    this.mLog = new List<CambioEntradaDto>();
    mVisto = new HashSet<Guid>();

    this.mArchivoLog = pRutaArchivo;

    if (File.Exists(pRutaArchivo))
    {
      var lArchivoJson = File.ReadAllText(pRutaArchivo);
      var lEntradas = JsonSerializer.Deserialize<List<CambioEntradaDto>>(lArchivoJson);

      if (lEntradas != null && lEntradas.Count > 0)
      {
        mLog = lEntradas;
        mPosicion = lEntradas.Last().Posicion;
        mVisto = new HashSet<Guid>(lEntradas.Select(e => e.Operacion.OperacionID));
      }
    }
  }  
  #endregion

  #region Metodos
  public Task<long> UnirAsync(OperacionSyncDto pOperacion)
  {
    lock(this.mCandado)
    {
      if(this.mVisto.Contains(pOperacion.OperacionID)) return Task.FromResult(this.mPosicion);

      this.mPosicion++;
      this.mLog.Add(new CambioEntradaDto(this.mPosicion, pOperacion));
      this.mVisto.Add(pOperacion.OperacionID);
      
      File.WriteAllText(this.mArchivoLog, JsonSerializer.Serialize(this.mLog, new JsonSerializerOptions { WriteIndented = true }));

      return Task.FromResult(this.mPosicion);
    }
  }

  public Task<CambioLogDto> LeerAsync(long pPosicion, int pPaginaTamanio = 200)
  {
    lock(this.mCandado)
    {
      var lEntradas = this.mLog.Where(x => x.Posicion > pPosicion).Take(pPaginaTamanio).ToList();
      var lSiguienteEntrada = lEntradas.Count == 0 ? pPosicion : lEntradas.Last().Posicion;

      return Task.FromResult(new CambioLogDto(lSiguienteEntrada, lEntradas));
    }
  }

  public bool Existe(Guid pOperacionID)
  {
    lock(this.mCandado)
    {
      return this.mVisto.Contains(pOperacionID);
    }
  }  
  #endregion

}