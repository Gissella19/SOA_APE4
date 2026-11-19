# APE 04 — Arquitectura Multicanal de Vehículos, Compradores, Ventas y Notificaciones

## 1. Descripción General
Integración distribuida orientada a servicios que comunica cuatro clientes heterogéneos (Windows Forms, PWA Móvil, React Web y JavaFX) con tres microservicios backend (Java Spring Boot x2, Python FastAPI y Java Notificaciones), utilizando NGINX como punto único de entrada REST (puerto 8090) con balanceo 3:1, persistencia relacional en MySQL y mensajería asíncrona mediante RabbitMQ.

## 2. Mapa de Puertos y Servicios
| Componente | Puerto | Protocolo / Rol |
|---|---|---|
| **NGINX** | `8090` | API Gateway / Proxy inverso REST local |
| **API Vehículos & Auth (Instancia A)** | `8080` | Spring Boot (Weight 3) |
| **API Vehículos & Auth (Instancia B)** | `8082` | Spring Boot (Weight 1) |
| **API Ventas & Compradores** | `8002` | Python FastAPI + Uvicorn |
| **Servicio de Notificaciones** | `8081` | Spring Boot + RabbitMQ Consumer |
| **RabbitMQ** | `5672` / `15672` | AMQP / Panel de administración |
| **MySQL / MariaDB** | `3306` | Base de datos `venta_vehiculos` |
| **React Web (Vite)** | `5173` | Cliente Web de Consulta |
| **PWA Móvil** | `5174` (o local) | Cliente Móvil Exclusivo de Venta |

## 3. Estructura de Proyectos
- `AplicacionEscritorioWINDOWS_FORMS/`: Cliente administrativo C# .NET 8 (Crear vehículos/compradores; listar).
- `AplicacionMovilPWA/`: PWA Móvil con login OAuth2 y registro exclusivo de ventas con `Idempotency-Key`.
- `AplicacionWebREACT/`: Catálogo web React + Vite con filtros y consulta de ventas (solo lectura).
- `AplicacionEscritorioJAVA_FX/`: Dashboard JavaFX de notificaciones en tiempo real consumidas de RabbitMQ.
- `java_venta-vehiculos-api_security/`: Microservicio Spring Boot de autenticación JWT y gestión de vehículos.
- `python_ventas-api/`: Microservicio FastAPI de compradores, validación de ventas y publicación a RabbitMQ.
- `java_notificaciones-service/`: Microservicio Spring Boot que consume eventos de RabbitMQ y expone historial.
- `nginx.conf`: Configuración del proxy inverso y balanceador 3:1.

## 4. Orden de Arranque
1. **MySQL**: Iniciar servicio local o XAMPP en el puerto `3306`.
2. **RabbitMQ**: Iniciar servicio o contenedor Docker (`5672`/`15672`).
3. **Instancias Java Vehículos**:
   - Terminal 1: `java -jar target/venta-vehiculos-api-0.0.1-SNAPSHOT.jar --server.port=8080`
   - Terminal 2: `java -jar target/venta-vehiculos-api-0.0.1-SNAPSHOT.jar --server.port=8082`
4. **NGINX**: `cd C:\nginx && .\nginx.exe` (escucha en `8090`).
5. **FastAPI Ventas**: `.\.venv\Scripts\python.exe -m uvicorn app.main:app --port 8002 --reload`.
6. **Servicio Notificaciones**: `mvn spring-boot:run` en `java_notificaciones-service` (puerto `8081`).
7. **Clientes**: Ejecutar Windows Forms, React Web, PWA Móvil y JavaFX según el flujo de prueba.
