using Moq;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

using InventoryApi.ML.Application.Interfaces;
using InventoryApi.ML.Application.DTO;
using InventoryApi.ML.Domain.Entities; 
using InventoryApi.ML.Domain.Exceptions; 

namespace InventoryApi.ML.Tests;

public class InventoryApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> mPrograma;

    public InventoryApiTests(WebApplicationFactory<Program> pPrograma)
    {
        mPrograma = pPrograma;        
    }

    [Fact]
    public async Task Get_Articulo_BySku_ReturnsArticulo()
    {
        var lMockInventario = new Mock<IInventario>();
        lMockInventario.Setup(x => x.ConsultarAsync(1))
                      .ReturnsAsync(new Articulo (1, "Test", 5, 1, 1, DateTimeOffset.Now));

        var lCliente = mPrograma.WithWebHostBuilder(lConstructor =>
        {
            lConstructor.ConfigureServices(lServicio =>
            {
                lServicio.AddSingleton(lMockInventario.Object);
            });
        }).CreateClient();

        var lPeticion = await lCliente.GetAsync("/Articulo/1");
        lPeticion.EnsureSuccessStatusCode();

        var lRespuesta = await lPeticion.Content.ReadFromJsonAsync<Articulo>();
        lRespuesta.Nombre.Should().Be("Test");
    }

    [Fact]
    public async Task Get_Articulo_BySku_NotFound_Returns404()
    {
        var lMockInventario = new Mock<IInventario>();
        lMockInventario.Setup(x => x.ConsultarAsync(99))
                      .ReturnsAsync((Articulo)null);

        var lCliente = mPrograma.WithWebHostBuilder(lConstructor =>
        {
            lConstructor.ConfigureServices(lServicio =>
            {
                lServicio.AddSingleton(lMockInventario.Object);
            });
        }).CreateClient();

        var lPeticion = await lCliente.GetAsync("/Articulo/99");
        lPeticion.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_Articulo_Crear_ReturnsCreated()
    {
        var lMockInventario = new Mock<IInventario>();
        lMockInventario.Setup(x => x.CrearAsync(It.IsAny<string>(), It.IsAny<int>()))
                      .ReturnsAsync(new Articulo (10, "Test", 15, 3, 4,  DateTimeOffset.Now));

        var lCliente = mPrograma.WithWebHostBuilder(lConstructor =>
        {
            lConstructor.ConfigureServices(lServicio =>
            {
                lServicio.AddSingleton(lMockInventario.Object);
            });
        }).CreateClient();

        var dto = new CrearDto (0, "Test", 10 );
        var lPeticion = await lCliente.PostAsJsonAsync("/Articulo", dto);

        lPeticion.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var lRespuesta = await lPeticion.Content.ReadFromJsonAsync<Articulo>();
        lRespuesta.Nombre.Should().Be("Test");
    }

    [Fact]
    public async Task Put_Articulo_Actualizar_ReturnsOk()
    {
        var lMockInventario = new Mock<IInventario>();
        lMockInventario.Setup(x => x.ActualizarAsync(
                            It.IsAny<int>(), 
                            It.IsAny<int>(), 
                            It.IsAny<string>(), 
                            It.IsAny<int>(), 
                            It.IsAny<string>(), 
                            It.IsAny<Guid>()
                            ))
                      .ReturnsAsync(new Articulo (1, "Test", 15, 3, 4,  DateTimeOffset.Now));

        var lCliente = mPrograma.WithWebHostBuilder(lConstructor =>
        {
            lConstructor.ConfigureServices(lServicio =>
            {
                lServicio.AddSingleton(lMockInventario.Object);
            });
        }).CreateClient();

        var lDto = new ActualizarDto(1, 5, "Motivo", 1, Guid.Parse("c1a1f1f2-aaab-bbbb-cccc-111111111111"), "Tienda-003");

        var lPeticion = await lCliente.PutAsJsonAsync("/Articulo/Actualizar", lDto);
        lPeticion.EnsureSuccessStatusCode();

        var lRespuesta = await lPeticion.Content.ReadFromJsonAsync<Articulo>();
        lRespuesta.Cantidad.Should().Be(15);
    }

    [Fact]
    public async Task Put_Articulo_Actualizar_Conflict_Returns409()
    {
        var lMockInventario = new Mock<IInventario>();
        lMockInventario.Setup(x => x.ActualizarAsync(
                            It.IsAny<int>(), 
                            It.IsAny<int>(), 
                            It.IsAny<string>(), 
                            It.IsAny<int>(), 
                            It.IsAny<string>(), 
                            It.IsAny<Guid>()
                            ))
                      .ThrowsAsync(new InvalidOperationException("Conflicto de concurrencia"));

        var lCliente = mPrograma.WithWebHostBuilder(lConstructor =>
        {
            lConstructor.ConfigureServices(lServicio =>
            {
                lServicio.AddSingleton(lMockInventario.Object);
            });
        }).CreateClient();

        var lDto = new ActualizarDto(1, 5, "Motivo", 1, Guid.Parse("c1a1f1f2-aaab-bbbb-cccc-111111111111"), "Tienda-003");

        var lRespuesta = await lCliente.PutAsJsonAsync("/Articulo/Actualizar", lDto);
        lRespuesta.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_Articulo_Reservar_ReturnsOk()
    {
        var lMockInventario = new Mock<IInventario>();
        lMockInventario.Setup(x => x.ReservarAsync(
                            It.IsAny<int>(), 
                            It.IsAny<int>(), 
                            It.IsAny<int>(), 
                            It.IsAny<string>(), 
                            It.IsAny<Guid>()                            
                            ))
                      .ReturnsAsync(new Articulo (1, "Test", 5, 5, 4,  DateTimeOffset.Now));

        var lCliente = mPrograma.WithWebHostBuilder(lConstructor =>
        {
            lConstructor.ConfigureServices(lServicio =>
            {
                lServicio.AddSingleton(lMockInventario.Object);
            });
        }).CreateClient();

        var lDto = new ReservarDto (1, 5, 1, Guid.Parse("c1a1f1f2-aaab-bbbb-cccc-111111111111"), "Tienda-004");
        var lPeticion = await lCliente.PutAsJsonAsync("/Articulo/Reservar", lDto);

        lPeticion.EnsureSuccessStatusCode();
        
        var lRespuesta = await lPeticion.Content.ReadFromJsonAsync<Articulo>();
        lRespuesta.Cantidad.Should().Be(5);
    }

    [Fact]
    public async Task Post_Sync_Enviar_ReturnsOk()
    {
        var lMockInventario = new Mock<IInventario>();

        var lOperacionesDto = new BatchSyncDto
        (
            new List<OperacionSyncDto>
            {
                new OperacionSyncDto
                {
                    Sku = 1,
                    Delta = 10,
                    Tipo = "Ajuste",
                    OperacionID = Guid.NewGuid(),
                    TiendaID = "Tienda-005",
                    Version = 5
                },
                new OperacionSyncDto
                {
                    Sku = 2,
                    Delta = 5,
                    Tipo = "Reserva",
                    OperacionID = Guid.NewGuid(),
                    TiendaID = "Tienda-006",
                    Version = 2
                },
                new OperacionSyncDto
                {
                    Sku = 2,
                    Delta = 2,
                    Tipo = "Ajuste",
                    OperacionID = Guid.NewGuid(),
                    TiendaID = "Tienda-003",
                    Version = 3
                }
                ,
                new OperacionSyncDto
                {
                    Sku = 3,
                    Delta = 2,
                    Tipo = "Reserva",
                    OperacionID = Guid.NewGuid(),
                    TiendaID = "Tienda-003",
                    Version = 2
                }
            }
        );

        lMockInventario.Setup(x => x.ProcesoAsync(It.IsAny<BatchSyncDto>()))
                      .ReturnsAsync(new RespuestaDto(lOperacionesDto.Operaciones.Count, 0));

        var lCliente = mPrograma.WithWebHostBuilder(lConstructor =>
        {
            lConstructor.ConfigureServices(lServicio =>
            {
                lServicio.AddSingleton(lMockInventario.Object);
            });
        }).CreateClient();

        // var lDto = new BatchSyncDto { Operaciones = lOperacionesDto.Operaciones };
        var lPeticion = await lCliente.PostAsJsonAsync("/sync/Enviar", lOperacionesDto);
        lPeticion.EnsureSuccessStatusCode();

        var lRespuesta = await lPeticion.Content.ReadFromJsonAsync<RespuestaDto>();
        lRespuesta.Exito.Should().Be(lOperacionesDto.Operaciones.Count);
    }

    [Fact]
    public async Task Get_Sync_Obtener_ReturnsLog()
    {
        var lMockCambios = new Mock<ICambios>();
        lMockCambios.Setup(x => x.LeerAsync(0, 200))
                   .ReturnsAsync(new CambioLogDto(1, new List<CambioEntradaDto>
                   {
                       new CambioEntradaDto(1, 
                                            new OperacionSyncDto                                              
                                            {
                                                Sku = 1,
                                                Delta = 10,
                                                Tipo = "Ajuste",
                                                OperacionID = Guid.Parse("c1a1f1f2-aaaa-bbbb-cccc-221111331113"),
                                                TiendaID = "Tienda-001",
                                                Version = 2
                                            })
                   }));

        var lCliente = mPrograma.WithWebHostBuilder(lConstructor =>
        {
            lConstructor.ConfigureServices(lServicio =>
            {
                lServicio.AddSingleton(lMockCambios.Object);
            });
        }).CreateClient();

        var lPeticion = await lCliente.GetAsync("/sync/Obtener?pPosicion=0");
        lPeticion.EnsureSuccessStatusCode();
        
        var lRespuesta = await lPeticion.Content.ReadFromJsonAsync<CambioLogDto>();
        
        Assert.NotNull(lRespuesta);
        Assert.Equal(1, lRespuesta.SiguientePosicion);
        Assert.Single(lRespuesta.Entrada);
    }

    [Fact]
    public async Task Get_Recomendar_Reabasto_Ok()
    {
        var lMockInventario = new Mock<IAgenteIA>();
        lMockInventario.Setup(x => x.AnalizarAsync(It.IsAny<IEnumerable<OperacionSyncDto>>(), It.IsAny<Dictionary<int, int>>()))
                      .ReturnsAsync(new List<RecomendacionDto> 
                      {
                        new RecomendacionDto
                        {
                            Sku = 3,
                            StockActual = 12,
                            Umbral = 15,
                            Recomendacion = "⚠️ Reabastecer 3. Stock actual: 12, operaciones recientes: 0."
                        }
                      });

        var lCliente = mPrograma.WithWebHostBuilder(lConstructor =>
        {
            lConstructor.ConfigureServices(lServicio =>
            {
                lServicio.AddSingleton(lMockInventario.Object);
            });
        }).CreateClient();
        

        var lOperaciones = new List<OperacionSyncDto>
        {
            new OperacionSyncDto
            {
                Sku = 3,
                Delta = 2,
                Tipo = "Reserva",
                OperacionID = Guid.NewGuid(),
                TiendaID = "Tienda-003",
                Version = 2
            }
        };

        var inventario = new Dictionary<int, int>
        {
            { 1, 5 } 
        };

        var lPeticion = await lCliente.GetAsync("/AgenteIA/Recomendacion");
        lPeticion.EnsureSuccessStatusCode();

        var lRespuesta = await lPeticion.Content.ReadFromJsonAsync<List<RecomendacionDto>>();

        Assert.Single(lRespuesta);
    }

}
