# Informacion general

Este proyecto es una API con .Net 6 enfocada en una arquitectura en capas, con una implementacion de pruebas de componentes y unitarias con xUnit (Al dia de hoy solo estan desplegadas las pruebas unitarias y de componentes para un unico servicio y controlador). Permite la administracion de una empresa de transaporte, desde rutas, vehiculos hasta reservas. 

# Pasos para la configuracion de la Api.

##### Configuracion archivo appsettings:

1. **JwtSettings** - Configuracion de la cadena de conexion de base de datos.
- **Secret:** Este parametro debe llevar una clave para la encriptacion del token, se recomienda ademas por seguridad que la cadena incluya todo tipo de caracteres entre normales y especiales y que la longitud de la misma no sea inferior a los 50 caracteres.
- **ExpiryTime:** Tiempo de vigencia del token debe definirse en el formato HH:mm.
- **TokenIssuer:** Emisor del token, este puede llevar cualquier valor, que de preferencia sea diferente al valor de la propiedad ```Secret``` y ```TokenAudience```.
- **TokenAudience:** Audiencia del token, este puede llevar cualquier valor, que de preferencia sea diferente al valor de la propiedad ```Secret``` y ```TokenIssuer```.

2. **ConnectionStrings** - Configuracion de la cadena de conexion de base de datos.
- **DefaultConnection:** Se debe cambiar su valor actual por la configuracion del servidor de base de datos, ejemplo:
'Server=```{Servidor base de datos}```;Initial Catalog=```{Nombre base de datos}```;Persist Security Info=False;User ID=```{Usuario servidor base de datos}```;Password=```{Contraseña servidor base de datos}```;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=```{Timeout para consultas a base de datos}```'

3. **CachingSettings** - Configuracion de los servicios de cache
- **SlidingExpiration:** Tiempo en minutos de vigencia del cache en caso tal de que no se consulte el recurso cacheado
- **AbsoluteExpiration:** Tiempo en minutos de vigencia del cache en caso tal de que el recurso no se consulte constante o sea consultado de forma activa.


##### Configuracion archivo appsettings:

1. **JwtSettings** - Para cada uno de los parametros se puede conservar el mismo valor que se tenia en el archivo ```appsettings.json```

2. **ConnectionStrings** - Para cada uno de los parametros se puede conservar el mismo valor que se tenia en el archivo ```appsettings.json``` o en caso tal ajustar los valores basado en su homologo del archivo ```appsettings.json```.

3. **AuthTestingSettings** - Configuracion de los servicios de cache
- **Email:** Nombre del usuario que sera usado para el consumo de los metodos que requieran autenticacion.
- **Password:** Contraseña del usuario que sera usado para el consumo de los metodos que requieran autenticacion.


## Migraciones ##

Luego de la configuración previa de las cadenas de conexión, se puede proceder a la implementación de las migraciones. El proyecto ya cuenta con la primera migración creada y únicamente se requiere ejecutar el comando correspondiente, ya sea utilizando el CLI de .Net (dotnet ef database update) o la consola de administración de paquetes (update-database).


## Data semilla ##

Luego de implementar la migracion, se cuenta con un script que inserta la data semilla, el mismo se encuentra en la carpeta llamada ```GoTransport.Script.Sql``` en la cual hay dos scripts de semilla, se debe ejecutar el script con nombre ```INSERT_SEED``` para asegurar un correcto funcionamiento y tambien se encuentra el script opcional llamado ```INSERT_SEED_COLOMBIA``` que inserta los datos de departamentos y ciudades de Colombia

