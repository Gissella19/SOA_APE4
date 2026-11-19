# Cliente Web — React + Vite (Consulta de Vehículos y Ventas)

## 1. Requisitos
- Node.js (v18+) y npm
- NGINX activo en `http://127.0.0.1:8090` con servicios Java y FastAPI

## 2. Estructura
El proyecto React reside en `react-fetch-vehiculos/`.

## 3. Instalación de Dependencias
```powershell
cd react-fetch-vehiculos
npm install
```

## 4. Ejecución en Desarrollo
```powershell
cd react-fetch-vehiculos
npm run dev
```

## 5. Compilación para Producción
```powershell
cd react-fetch-vehiculos
npm run build
```

## 6. Función en la Arquitectura APE 04
Plataforma web de solo lectura:
- Consultar catálogo de vehículos con filtro por marca y detalle.
- Consultar historial de ventas desde `GET /api/ventas`.
- **Prohibido**: No registra ventas ni modifica datos.
