# Cliente de Escritorio — JavaFX (Monitor de Notificaciones)

## 1. Requisitos
- JDK 17
- Apache Maven 3.8+
- Servicio de notificaciones activo en `http://127.0.0.1:8081` (o vía NGINX en `http://127.0.0.1:8090/api/notificaciones`)

## 2. Estructura
El código fuente Maven y JavaFX reside en `consultarvehiculojavafx/`.

## 3. Compilación
```powershell
cd consultarvehiculojavafx
mvn clean compile
```

## 4. Ejecución
```powershell
cd consultarvehiculojavafx
mvn javafx:run
```

## 5. Función en la Arquitectura APE 04
Visualizar las notificaciones de ventas en tiempo real consumidas desde RabbitMQ mediante el microservicio `java_notificaciones-service`, sin refresco manual continuo.
