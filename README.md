# Banco API — Pruebas de API con Newman y k6

Proyecto grupal del curso **Calidad y Pruebas de Software (UPT)**. Una API REST en .NET 8 probada
automáticamente con dos herramientas en GitHub Actions:

| Herramienta | Tipo de prueba | Qué valida | Responsable |
| --- | --- | --- | --- |
| [Newman](https://github.com/postmanlabs/newman) (Postman) | Funcional | Status, datos, esquema JSON y reglas de negocio de cada endpoint | Victor Carretero |
| [k6](https://k6.io) | Carga y rendimiento | Tiempos de respuesta y errores con hasta 20 usuarios simultáneos | Gerald Deybi Zevallos Pinto |

## La API

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/health` | Estado de la API |
| GET | `/api/cuentas` | Lista las cuentas |
| GET | `/api/cuentas/{id}` | Obtiene una cuenta (404 si no existe) |
| POST | `/api/cuentas` | Crea una cuenta (`titular`, `saldoInicial`) |
| POST | `/api/cuentas/{id}/depositos` | Deposita un `monto` |
| POST | `/api/cuentas/{id}/retiros` | Retira un `monto` (422 si no hay saldo) |
| DELETE | `/api/cuentas/{id}` | Elimina una cuenta |

## Pipeline

El workflow [`pruebas-api.yml`](.github/workflows/pruebas-api.yml) se ejecuta en cada `push` o `pull request` a `main`:

```
push → build (.NET 8) ─┬→ Newman: levanta la API + 12 pruebas funcionales ─┐
                       └→ k6: levanta la API + prueba de carga (20 VUs)    ─┴→ reportes → GitHub Pages
```

## Reportes publicados

https://viccarretero.github.io/banco-api-pruebas/

## Ejecutar en local

```bash
# API
dotnet run --project src/BancoApi --urls http://localhost:5000

# Newman
npm install -g newman newman-reporter-htmlextra
newman run tests/postman/banco-api.postman_collection.json -e tests/postman/local.postman_environment.json -r cli,htmlextra

# k6
k6 run tests/k6/carga.js
```

## Integrantes

- Victor Carretero
- Gerald Deybi Zevallos Pinto
