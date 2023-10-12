## Despliegue de la aplicación

El sistema requiere de la instalación de:

- .NET Framework 7.0
- PostgreSQL 14 o superior

Estos componentes deben estar instalados sobre el sistema operativo para luego
realizar tareas de configuración y restauración de datos. El sistema se ha probado
en entornos Windows, MacOS, Linux (Ubuntu, RHEL) y Docker.

Los entregables incluyen dos archivos que sirven para facilitar el despliegue y
tener un entorno funcional:

- **phishtrain.backup**: Backup de la base de datos con información inicial
- **imagenes-ejercicios.zip**: Imágenes asociadas a los ejercicios de phishing

La forma recomendada de desplegar este sistema es utilizando la plataforma Docker
como se detalla en la siguiente sección.

### Despliegue en Docker

Se requiere de la instalación del servicio Docker y del módulo docker-compose para
utilizar los scripts incluidos en este proyecto.

Se proporciona un archivo de definición de servicios para docker compose que creará dos
contenedores:

- **db** : Base de datos
    - Imagen PostgreSQL 15 alpine
    - Nombre contenedor ``phishdb``
    - Volúmen externo para la carpeta de datos de la base
    - Volúmen externo para una carpeta de backups
    - Base de datos llamada ``phishtrain``
    - Mapeo de puerto 54329 para conexiones externas
- **webapp** : Aplicación web
    - Contenedor custom usando un Dockerfile dentro del proyecto /WebApp
    - Nombre contenedor ``phishapp``
    - Imágenes de .NET 7.0 para compilar la aplicación (sdk) y para despliegue (aspnet)
    - Volúmen externo para las imágenes subidas desde web
    - Configurado para producción, cadena de conexión a la base incluida
    - Mapeo de puerto 8080 para conexiones externas

A continuación se detalla el proceso de despliegue por defecto:

1. Ubicar el archivo docker-compose.yml en la carpeta raiz del proyecto
2. Abrir una linea de comandos a esta carpeta y correr el siguiente comando:  
   ``docker-compose up -d``
3. Se creará una carpeta llamada /deploy en la carpeta raiz, ingresar
4. Copiar el archivo de respaldo ``phishtrain.backup`` en la carpeta /deploy/backups
5. Ejecutar el archivo ``restore-db-docker.bat`` si es Windows o ``restore-db-docker.sh``
   si es Linux para restaurar la base de datos
6. Descomprimir el archivo ``imagenes-ejercicios.zip`` en la carpeta /deploy/imagenes
7. Acceder al servidor por un navegador por el puerto 8080, por ejemplo:  
   http://localhost:8080

Si existen problemas con el despliegue, revisar los logs de los contenedores utilizando
el comando ``docker logs {nombre_contenedor}`` o alguna herramienta visual como
Docker Desktop.

Editando los parámetros del archivo docker-compose.yml se puede personalizar el
despliegue, por ejemplo cambiar el puerto 8080 del servicio webapp al puerto 80
para un entorno de producción.

### Recursos adicionales

- Despliegue de aplicación .NET Core en CentOS
    - https://www.netmentor.es/entrada/desplegar-aspnetcore-centos

- Ejemplos de despliegue de aplicaciones .NET sobre Docker:
    - https://learn.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=windows&pivots=dotnet-7-0
    - https://medium.com/@ersen/step-by-step-dockerizing-net-core-api-a2490752a3d2

- Imagen Oficial PostgreSQL, opciones de configuración:
    - https://hub.docker.com/_/postgres
