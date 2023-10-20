# Sistema de Entrenamiento para Detección de Phishing.

## Manual de Despliegue de la Aplicación

El sistema requiere de la instalación de:

- .NET Framework 7.0
- PostgreSQL 14 o superior

Estos componentes deben estar instalados para realizar tareas de configuración y restauración de datos.
El sistema se ha probado en entornos Windows, MacOS, Linux (Ubuntu, RHEL) y Docker.

Los entregables incluyen dos archivos de respaldo que sirven para facilitar el despliegue y
tener un entorno funcional:

- **phishtrain.backup**: Backup de la base de datos con información inicial
- **imagenes-ejercicios.zip**: Imágenes asociadas a los ejercicios de phishing

Se recomienda realizar el despliegue sobre Docker para garantizar la estabilidad
de las plataformas base.  

A continuación se detallan instrucciones de como deplegar el sistema en Linux y en Docker.

## Despliegue sobre Linux

Existen varias formas de configurar una aplicación .NET Core en Linux dependiendo de la
distribución y de cómo se quiera ejecutar el servicio. Recomendamos consultar la 
documentación de su sistema operativo específico para más opciones.

Para esta guía se asume una distribución compatible con CentOS, Red Hat Enterprise (RHEL) o Linux
más recientes como AlmaLinux. El código de los ejemplos se trabajó con Amazon Linux 2 en una
máquina alojada en Amazon AWS. Todas las operaciones se realizan en línea de comandos.

**NOTA** No se incluyen los pasos de instalación y configuración del servidor de base de datos
PostgreSQL, solo de la aplicación principal.

### Prerequisitos

Se recomienda tener una cuenta de usuario diferente a root para la instalación. Para los
ejemplos de trabajará con el usuario ec2-user propio de Amazon Linux.

El sistema requiere de .NET Framework version 7 o superior, para instalar esto en una instancia
nueva del sistema operativo se requiere adicionar el repositorio oficial de Microsoft al gestor de paquetes 
con el siguiente comando:

``sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm``

Instalamos el ejecutable del runtime de .Net Core:

``sudo yum install aspnetcore-runtime-7.0``

Para poder compilar y generar los ejecutables de la aplicación se requiere el sdk, lo instalamos también:

``sudo yum install dotnet-sdk-7.0``

Para verificar la instalación se puede ejecutar el siguiente comando y ver que no tenga errores:

``dotnet --version``

### Preparar la aplicación

Para poder crear el sitio web ejecutable de la aplicación se recomienda seguir los siguientes pasos:

1. Copiar el proyecto de código en una carpeta determinada en el servidor, p.ej ~/phishing
2. Ubicarse en la raiz de la carpeta
3. Instalar dependencias del proyecto con el comando: ``dotnet restore``. Este proceso puede tomar algún tiempo.
4. Una vez terminado, ingresar a la carpeta del proyecto web: ``cd Webapp/``
5. Publicar el proyecto en modo optimizado a una carpeta (en este caso /webphishing):  
   ``dotnet publish -c Release -o webphishing``  
   Este comando crea una carpeta llamada /webphishing que contiene los insumos y ejecutables de la aplicación así como la
   raiz del sitio web
6. *Opcional*, Crear un zip del sitio. En este caso se compacta la carpeta para poder moverla a otra ubicación.  
   Dentro de la carpeta Webapp, ejecutar: ``zip -r phishing-site.zip ./webphishing/``
7. Definir la ubicación del sitio web. Se recomienda mover la carpeta creada a una ubicación fuera de la carpeta
   Webapp e ingresar a esta carpeta. Para este ejemplo se movió la carpeta a ```/home/ec2-user/webphishing```.
8. *Opcional*, Descomprimir el archivo imagenes-ejercicios.zip en la carpeta ${raiz}/wwwroot/ejercicios para tener los
   archivos de imágenes de la información inicial. En el ejemplo estaría en ``/home/ec2-user/webphishing/wwwroot/ejercicios``

Para probar que la aplicación se ejecuta, dentro de la raiz ejecutar el comando ``dotnet Webapp.dll`` y 
observar la salida. Para terminar el proceso pulse Ctrl+C.

### Configuración de arranque

Una vez creado el sitio de producción es necesario crear la configuración mínima para que se pueda conectar
a la base de datos en un archivo llamado ``appsettings.json`` ubicado en la raiz de la aplicación.

Puede utilizar el siguiente ejemplo, deberá cambiar los parámetros de la cadena de conexión a la base de datos
para que apunte al servidor correspondiente.

```
{  
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "phishingDb": "Server=localhost;Port=5432;Database=phishtrain;User Id=postgres;Password=postgres"
  },
  "AllowedHosts": "*"
}
```

Una vez creado este archivo puede acceder a la aplicación por un navegador web por el puerto 5000 por defecto.
Para cambiar este puerto puede utilizar el flag --urls del comando dotnet al momento de correr el sistema, 
por ejemplo para correr el sistema en el puerto 80:

``dotnet Webapp.dll --urls "http://0.0.0.0:80"``

#### Configuración como servicio

Idealmente, la aplicación debe correr como un servicio y estar siempre disponible sin intervención del usuario
administrador. Para esto recomendamos el uso del programa supervisord que permite gestionar servicios
de forma organizada y eficiente.

Para instalar este componente se requiere activar el repositorio EPEL disponible para casi toda
distribución de Linux actual :

- Amazon Linux: ``sudo amazon-linux-extras install epel``
- Para CentOS o RHEL 7: ``sudo yum install epel-release``
- AlmaLinux: ``sudo dnf install epel-release``

Si el sistema pide confirmación poner si (y o yes) en todas las preguntas. Seguido, instalar
supervisord:

``sudo yum install supervisor``

Una vez instalado se debe habilitar el servicio con los siguiente dos comandos:

```
sudo systemctl enable supervisord.service
sudo systemctl start supervisord
```

Verifique que el archivo /etc/supervisord.conf exista y si no, refiérase a la documentación de
supervisord para como crearlo, pero normalmente esto se crea en la instalación 
y no es necesario hacer más.

Ahora se debe crear una descripción del servicio de nuestra aplicación web. Si existe una carpeta
en la ubicación /etc/supervisord.d, se puede crear aquí un archivo llamado por ejemplo webphishing.ini
y poner el código a continuación.

También es posible poner este código al final del archivo /etc/supervisord.conf

Por favor revise las rutas de cada programa, en el caso de la aplicación web, se colocó
dentro de la carpeta /home/ec2-user/webphishing y el ejecutable de dotnet está en /usr/bin.

Ejemplo de servicio supervisord:

```
[program:webphishing]
command=/usr/bin/dotnet /home/ec2-user/webphishing/Webapp.dll
directory=/home/ec2-user/webphishing
autostart=true
autorestart=true
stderr_logfile=/var/log/webphishing.err.log
stdout_logfile=/var/log/webphishing.out.log
environment=ASPNETCORE_ENVIRONMENT=Production,ASPNETCORE_URLS="http://*:5000/"
user=ec2-user
stopsignal=INT
```

Nótese que se configuró el servicio para que corra bajo el contexto del usuario ec2-user y no como root
por cuestiones de seguridad y para facilitar actualizaciones. Ajuste al usuario correspondiente.

Adicionalmente, la salida de la consola estándar y de errores se guarda en dos achivos dentro de la carpeta
/var/logs.

Una vez hecho esto, reiniciar el servicio de supervisord:

``sudo systemctl restart supervisord``

Si el comando responde con un error, se puede verificar el detalle utilizando:

``sudo systemctl status supervisord -l``

Y corregir cualquier error que se presente, por lo general la sintaxis de la configuración del servicio.

Finalmente, el control de los servicios de puede realizar desde la consola de supervisord a la cual
se puede ingresar con:

``sudo supervisorctl``

Para más información de como administrar servicios, refíerase a la documentación de supervisord.

## Despliegue en Docker

Se requiere de la instalación del servicio Docker y del módulo docker-compose para
utilizar los scripts incluidos en este proyecto. Pasos comunes para instalar Docker:

1. Actualizar paquetes: ``sudo yum update -y ``  
   Para Amazon Linux, habilitar el repositorio: ``sudo amazon-linux-extras install docker``
2. Instalar Docker: ``sudo yum install docker``
3. Instalar Docker compose: ``sudo yum install docker-compose``
4. Habilitar el servicio:   
   ``sudo systemctl enable docker``  
   ``sudo systemctl start docker``

Verificar que el servicio funcione con ``docker --version``. Para opciones adicionales, revise la
documentación disponible en internet sobre Docker.

En la carpeta del proyecto, se proporciona un archivo de definición de servicios para docker compose 
que creará dos contenedores:

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

Adicionalmente, en otras plataformas el comando suele estar separado de la forma ``docker compose``, 
tomar en cuenta este detalle.

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
para un entorno de producción sin proxy.

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

***NOTA***: El comando se ejecutó en la línea de comandos de Windows por lo que las comillas dobles
para delimitar la sentencia interna son requeridas, también funciona en Linux de la misma forma.

***Respaldo archivos***

Se recomienda utilizar alguna utilidad de compresión para respaldar los archivos de la carpeta
/imagenes, por ejemplo en Linux se puede utilizar el comando zip. Dentro de la carpeta /deploy,
ejecutar el siguiente comando:

``zip -r phish-imagenes.zip ./imagenes``

Se creará un archivo llamado phish-imagenes.zip con todas las imágenes subidas al sistema como
respaldo de esta información.

### Actualizaciones

Cuando existan cambios en el código de la aplicación, se puede redesplegar siguiendo los mismos pasos
anteriormente descritos pero con la nueva carpeta del proyecto, ya sea reconstruyendo el proyecto
de forma manual con el comando dotnet o reconstruyendo los contenedores Docker si se utilizó esta plataforma.

Es muy recomendable sacar respaldos de la base de datos y de la carpeta ``/imagenes`` (o ``/wwwroot/ejercicios``)
antes de realizar cualquier actualización.

En el caso de Docker, es necesario recompilar la aplicación y reconstruir
el contenedor phishapp. Copie el nuevo código en el servidor antes de proceder con los comandos de
actualización de Docker.

Primero, se debe detener los servicios utilizando el comando

``docker-compose stop``

Puede detener solo el servicio de la aplicación web utilizando:

``docker-compose stop webapp``

Ejecutar el siguiente comando en la carpeta donde está el archivo docker-compose.yml:

``docker-compose build --no-cache``

Para reconstruir solo la aplicación web, especificar el nombre del servicio como está 
descrito en el archivo docker-compose.yml, por ejemplo:

``docker-compose build --no-cache webapp``

Una vez se termine de actualizar la aplicación, se puede ejecutar los servicios normalmente 
usando ``docker-compose up -d`` o alguna interfaz de usuario para Docker.

### Opcional: Ruteo de peticiones por Nginx

En entornos de producción, se recomienda no exponer la aplicación directamente al internet si no, hacerla
pasar por lo que se conoce como un proxy reverso. Esta configuración hace que un servidor web primario 
reciba las peticiones de los clientes y las enrute hacia otros servicios internos proporcionando un nivel
más de seguridad y opciones adicionales como la gestión de certificados ssl sin tener que modificar la
aplicación base.

El servidor web más utilizado para este fin es Nginx el cual está disponible para todos los sistemas 
operativos más populares y está probado en la industria.

Existen muchas formas de configuración para este servicio por lo que en esta guía solo se detalla la forma más
cómun de realizar el enrutamiento. Para configuraciones adicionales, refiérase a la documentación de Nginx y
del sistema operativo utilizado.

1. Instalar Nginx. 
   - Para Amazon linux 2, es necesario habilitar el repositorio con ``sudo amazon-linux-extras enable nginx1``  
   - Instalar el paquete: ``sudo yum -y install nginx``
2. Verificar instalación: ``nginx -v``
3. Habilitar el servicio:   
   ``sudo systemctl enable nginx``  
   ``sudo systemctl start nginx``
4. Como usuario root, editar la configuración en el archivo /etc/nginx/nginx.conf (ver más adelante)
5. Reiniciar el servicio: ``sudo systemctl restart nginx``

La configuración más básica que se debe incorporar en el archivo nginx.conf es cambiar la sección ``location /``
dentro de la sección server como se muestra a continuación:

```
server {
  listen 80;
  server_name localhost;
  
  ... mas comandos

  # Esta seccion hace el ruteo
  location / {
      proxy_pass         http://localhost:5000;
      proxy_http_version 1.1;
      proxy_set_header   Upgrade $http_upgrade;
      proxy_set_header   Connection keep-alive;
      proxy_set_header   Host $host;
      proxy_cache_bypass $http_upgrade;
      proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
      proxy_set_header   X-Forwarded-Proto $scheme;
  }
  
  ... mas comandos
```

En este caso se enrutan todas las peticiones al puerto 80 a la dirección http://localhost:5000, la que para 
esta guía  corresponde a la aplicación desplegada de forma nativa. Si se utiliza el servicio Docker, 
la dirección por defecto sería http://localhost:8080.

Nginx como puerta de entrada permite tener una capa adicional de protección así como un software especializado
para tareas como limitador de peticiones, ruteo avanzado y redirección HTTPS entre muchas otras disponibles.

## Configuración inicial

Por defecto, el sistema tiene una cuenta de super administrador con nombre de usuario 'admin' y
contraseña 'admin'. Se recomienda ingresar al sistema y cambiar la contraseña de este usuario
desde el menú 'Mi Perfil' en la parte superior derecha o en el módulo de administración de usuarios
en el menú lateral del lado izquierdo, opción 'Administración' > 'Usuarios'.

## Recursos adicionales

- Desplegar aplicación .NET Core en Amazon Linux 2 (incluye supervisord y nginx)
    - https://docs.servicestack.net/deploy-netcore-to-amazon-linux-2-ami#etcnginxconf.dmy-app.org.conf
    - https://coderjony.com/blogs/hosting-aspnet-core-on-amazon-linux-ec2

- Despliegue de aplicación .NET Core en Linux
    - https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-7.0&tabs=linux-ubuntu

- Despliegue de aplicación .NET Core en CentOS
    - https://www.netmentor.es/entrada/desplegar-aspnetcore-centos

- Supervisor, control de procesos en Linux
    - http://supervisord.org/

- Ejemplos de despliegue de aplicaciones .NET sobre Docker:
    - https://learn.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=windows&pivots=dotnet-7-0
    - https://medium.com/@ersen/step-by-step-dockerizing-net-core-api-a2490752a3d2

- Imagen Oficial PostgreSQL, opciones de configuración:
    - https://hub.docker.com/_/postgres

- Respaldos de bases Postgresql en contenedores Docker:
    - https://stackoverflow.com/questions/24718706/backup-restore-a-dockerized-postgresql-database