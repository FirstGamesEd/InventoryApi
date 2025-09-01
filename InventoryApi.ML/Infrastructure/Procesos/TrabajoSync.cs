/// <summary>
/// Permite ejecutar procesos en segundo plano mientra la API se esta ejecutando
/// </summary>

using System;

using InventoryApi.ML.Application.Interfaces;

namespace InventoryApi.ML.Infrastructure.Procesos;

public class TrabajoSync : BackgroundService
{
  private readonly ILogger<TrabajoSync> mTrabajoSync;
  private readonly ICambios mCambiosInterfaz;

  public TrabajoSync(ILogger<TrabajoSync> pTrabajoSync, ICambios pCambiosInterfaz)
  {
    mTrabajoSync = pTrabajoSync;
    mCambiosInterfaz = pCambiosInterfaz;
  }

  protected override async Task ExecuteAsync(CancellationToken pTokenCancelacion)
  {
    long lPosicion = 0;
    while (!pTokenCancelacion.IsCancellationRequested)
    {
      try
      {
        var lLog = await mCambiosInterfaz.LeerAsync(lPosicion);
        if (lLog.Entrada.Count > 0)
        {
          lPosicion = lLog.SiguientePosicion;
          mTrabajoSync.LogInformation("SyncWorker observed {count} ops; next position {pos}", lLog.Entrada.Count, lPosicion);
        }
      }
      catch (Exception ex)
      {
        mTrabajoSync.LogError(ex, "SyncWorker error");
      }

      await Task.Delay(TimeSpan.FromSeconds(60), pTokenCancelacion); // simulate periodic sync (configurable)
    }
  }
}