# 📦 Inventory API – Code Challenge (Mercado Libre)

## 🚀 Descripción
Este proyecto implementa una **API de inventario** siguiendo principios de **Clean Architecture**.  
El objetivo es simular una base de datos utilizando **JSON en disco**, exponer endpoints para sincronización de stock y registrar operaciones en un **log persistente**.  

Adicionalmente, se integra un componente básico de **IA** para sugerir acciones de reabasto en base a las operaciones recibidas.  

---

## 🏗️ Arquitectura
El proyecto está dividido en capas:  

- **Domain**: Entidades y reglas de negocio.  
- **Application**: Casos de uso, interfaces, DTOs.  
- **Infrastructure**: Persistencia (JSON/TXT), logs y servicios externos.  
- **API**: Endpoints expuestos con `Minimal APIs`.  
- **Tests**: Pruebas unitarias e integración.  

---

## ⚙️ Requisitos
- [Docker](https://www.docker.com/)  
- [Postman](https://www.postman.com/) (opcional, para probar la API)  

---

## ▶️ Ejecución
1. Clonar el repositorio:  
   git clone https://github.com/edubo1896/InventoryApi.git
   cd inventory-api

2. restaurar dependencias
   dotnet restore
   
3. Ejecutar la API
   dotnet run --project src/InventoryApi

4. Acceder a la API   
   http://localhost:5194/

---

## Endpoints
1. Consultar todo (GET)
	http://localhost:5194/Articulo
	
2. Consultar por SKU (GET)
	http://localhost:5194/Articulo/{sku}

3. Crear Articulo (POST)
	http://localhost:5194/Articulo
	
	{
	  "Nombre": "Camisa Gris",
	  "CantidadInicial": 13
	}
	
3. Actualizar (PUT)
	http://localhost:5194/Articulo/Actualizar

	{
	  "Sku": 1,
	  "Delta": -5,
	  "Motivo": "Ajuste",
	  "Version": 3,
	  "OperacionID": "c1a1f1f2-aaaa-bbbb-cccc-221111331115",
	  "TiendaID": "Tienda-001"
	}

4. Reservar (PUT)
	http://localhost:5194/Articulo/Reservar

	{
	  "Sku": 1,
	  "Cantidad": 5,
	  "Version": 4,
	  "TiendaID": "Tienda-001",
	  "OperacionID": "c1a1f1f2-aaaa-bbbb-cccc-111111111111"  
	}

5. Sincronización de Operaciones (POST)
	http://localhost:5194/sync/Enviar

6. Obtener Logs de Cambios (GET)
	http://localhost:5194/sync/Obtener?pPosicion=0
	
7. Recomendacion IA
	http://localhost:5194/AgenteIA/Recomendacion

---

## Testing

1. Ejecutar
	dotnet test

1.1 Ejecutar con Reporte de Cobertura
	dotnet test --collect:"XPlat Code Coverage"
	
---

## Logs

Cada operacion registrada se guarda en un archivo local

	/app/logs/cambios.json

	Ejemplo:
	
	[
	  {
		"Posicion": 1,
		"Operacion": {
		  "Sku": 1,
		  "Delta": 10,
		  "Tipo": "Ajuste",
		  "OperacionID": "c1a1f1f2-aaaa-bbbb-cccc-221111331113",
		  "TiendaID": "Tienda-001",
		  "Version": 2,
		  "FechaMovimiento": "2025-09-01T01:41:37.8676284-06:00"
		}
	  }
	]

--

## IA integrada

El proyecto incluye un módulo sencillo de IA que:

- Analiza la frecuencia de operaciones por SKU.
- Recomienda reabasto automático cuando el stock baja de un umbral.
