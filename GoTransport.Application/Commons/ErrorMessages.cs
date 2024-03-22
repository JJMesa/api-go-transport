namespace GoTransport.Application.Commons;

public static class ErrorMessages
{
    public const string NotFound = "El recurso al que intentas acceder no existe.";
    public const string InvalidCredentials = "El usuario o contraseña proporcionados no son válidos.";
    public const string DuplicateDescription = "Ya existe otro registro con la misma descripción suministrada.";
    public const string Unauthorized = "El usuario no se encuentra autenticado.";
    public const string Forbidden = "El usuario no tiene permisos para acceder al recurso solicitado.";
    public const string UrlAndBodyIdNotEqual = "El ID proporcionado en la URL y en el cuerpo de la petición no coinciden.";
    public const string DuplicateRoute = "Ya existe otra ruta creada con el mismo punto de origen y el mismo punto de destino.";
    public const string DuplicatePlate = "Ya existe un vehículo con la misma placa.";
    public const string InvalidSchedule = "El horario en el cual intentas reservar no se encuentra disponible. Por favor, selecciona otro para continuar con el proceso.";
    public const string VehicleFull = "El horario en el cual intentas reservar ya no cuenta con asientos disponibles. Por favor, selecciona otro para continuar con el proceso.";
    public const string ReservationDatePassed = "La fecha de la reserva ya ha pasado. No puedes eliminarla.";
    public const string DuplicateEmail = "El correo electrónico que proporcionaste ya se encuentra registrado.";
    public const string InternalServerError = "Ha ocurrido un error. Por favor nuevamente o reporte el inconveniente.";
    public const string ConflictValidators = "Se han presentado errores de validación al intentar procesar la solicitud.";
    public const string CanceledTask = "La solicitud ha sido cancelada por parte del usuario.";
    public const string ConflictForeignKey = "No es posible realizar la acción debido a que parte de la información proporcionada no es válida.";
    public const string ConflictPrimaryKey = "No es posible realizar la acción debido a que ya hay un registro con la misma información.";
}