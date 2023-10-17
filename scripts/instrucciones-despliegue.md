# Sistema de Entrenamiento para Detección de Phishing.

## Manual de Despliegue de la Aplicación

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

**NOTA**: Si el host es Linux, compruebe los permisos de la carpeta creada por Docker para que pueda 
copiar los archivos en los siguientes pasos, es posible que tenga que acceder como el usuario root.

1. Copiar el proyecto de código en una carpeta determinada en el servidor.
2. Ubicar el archivo docker-compose.yml en la carpeta raiz del proyecto.
3. Abrir una linea de comandos a esta carpeta y correr el siguiente comando:  
   ``docker-compose up -d``
4. Se creará una carpeta llamada /deploy en la carpeta raiz, ingresar a la misma
5. Copiar el archivo de respaldo ``phishtrain.backup`` en la carpeta /deploy/backups
6. Ejecutar el archivo ``restore-db-docker.bat`` si es Windows o ``restore-db-docker.sh``
   si es Linux para restaurar la base de datos
7. Descomprimir el archivo ``imagenes-ejercicios.zip`` en la carpeta /deploy/imagenes
8. Acceder al servidor por un navegador por el puerto 8080, por ejemplo:  
   http://localhost:8080

Si existen problemas con el despliegue, revisar los logs de los contenedores utilizando
el comando ``docker logs {nombre_contenedor}`` o alguna herramienta visual como
Docker Desktop.

Editando los parámetros del archivo docker-compose.yml se puede personalizar el
despliegue, por ejemplo cambiar el puerto 8080 del servicio webapp al puerto 80
para un entorno de producción.

### Configuración inicial

Por defecto, el sistema tiene una cuenta de super administrador con nombre de usuario 'admin' y 
contraseña 'admin'. Se recomienda ingresar al sistema y cambiar la contraseña de este usuario
desde el menú 'Mi Perfil' en la parte superior derecha o en el módulo de administración de usuarios
en el menú lateral del lado izquierdo, opción 'Administración' > 'Usuarios'.

### Respaldos

Se recomienda crear un plan de respaldos automáticos tanto de la base de datos como de los
archivos cargados (imágenes). Con la configuración por defecto, es posible simplemente copiar
todo el contenido de la carpeta ``/deploy`` a otra ubicación de preferencia en otra máquina externa.

Si se quiere automatizar cada imsumo por separado, se recomienda crear una tarea programada en el
sistema operativo o con otra herramienta que permita automatizar el proceso.

En el caso de Linux se puede usar el comando cron para correr las tareas de backup o el programador
de tareas en el caso de Windows. A continuación, un ejemplo de los comandos para respaldar la
base de datos y la carpeta de archivos:

***Respaldo base de datos:***

El siguiente comando crea un respaldo de la base de datos en la carpeta /backups mapeada en docker-compose.yml,
el archivo resultante tiene en el nombre la fecha actual de sistema.

``docker exec -it phishdb bash -c "pg_dump -U postgres --format custom --blobs
--verbose --dbname phishtrain > /backups/phishdb-`date -I`.backup"``

Este comando llama a la utilidad pg_dump dentro del contenedor postgresql.

NOTA: El comando se ejecutó en la línea de comandos de Windows por lo que las comillas dobles
para delimitar la sentencia interna son requeridas, también funciona en Linux de la misma forma.

***Respaldo archivos***

Se recomienda utilizar alguna utilidad de compresión para respaldar los archivos de la carpeta
/imagenes, por ejemplo en Linux se puede utilizar el comando zip. Dentro de la carpeta /deploy,
ejecutar el siguiente comando:

``zip -r phish-imagenes.zip ./imagenes``

Se creará un archivo llamado phish-imagenes.zip con todas las imágenes subidas al sistema como
respaldo de esta información.

### Actualizaciones

Cuando se realicen mantenimientos y mejoras en el sistema, se puede redesplegar en Docker utilizando
el comando de reconstrucción de los servicios. Es muy recomendable sacar respaldos de la base de datos
y de la carpeta ``/imagenes`` antes de realizar cualquier actualización como se detalló anteriormente.

Si existen cambios en el código del sistema es necesario recompilar la aplicación y reconstruir
el contenedor phishapp. Copie el nuevo código en el servidor antes de proceder con los comandos de
actualización de Docker.

Primero, se debe detener los servicios utilizando el comando

``docker compose stop``

Puede detener solo el servicio de la aplicación web utilizando:

``docker compose stop webapp``

Ejecutar el siguiente comando en la carpeta donde está el archivo docker-compose.yml:

``docker compose build --no-cache``

Para reconstruir solo la aplicación web, especificar el nombre del servicio como está 
descrito en el archivo docker-compose.yml, por ejemplo:

``docker compose build --no-cache webapp``

Una vez se termine de actualizar la aplicación, se puede ejecutar los servicios normalmente 
usando ``docker compose up -d`` o alguna interfaz de usuario para Docker.

### Recursos adicionales

- Despliegue de aplicación .NET Core en CentOS
    - https://www.netmentor.es/entrada/desplegar-aspnetcore-centos

- Ejemplos de despliegue de aplicaciones .NET sobre Docker:
    - https://learn.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=windows&pivots=dotnet-7-0
    - https://medium.com/@ersen/step-by-step-dockerizing-net-core-api-a2490752a3d2

- Imagen Oficial PostgreSQL, opciones de configuración:
    - https://hub.docker.com/_/postgres

- Respaldos de bases Postgresql en contenedores Docker:
    - https://stackoverflow.com/questions/24718706/backup-restore-a-dockerized-postgresql-database