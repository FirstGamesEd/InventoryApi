using System;

using InventoryApi.ML.Domain.Entities;

namespace InventoryApi.ML.Domain.Exceptions;

public class Error : Exception
{
  public Articulo ArticuloEX { get; }
  public Error(string Descipcion, Articulo Articulo) : base(Descipcion) => ArticuloEX = Articulo;
}