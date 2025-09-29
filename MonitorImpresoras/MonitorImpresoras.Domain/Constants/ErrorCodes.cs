namespace MonitorImpresoras.Domain.Constants
{
    /// <summary>
    /// Códigos de error estandarizados para la API MonitorImpresoras
    /// Formato: MI-XXXX donde:
    /// - MI: MonitorImpresoras
    /// - XXXX: Código numérico único
    /// </summary>
    public static class ErrorCodes
    {
        // Errores de Impresoras (1000-1999)
        public const string PrinterNotFound = "MI-1001";
        public const string PrinterAlreadyExists = "MI-1002";
        public const string PrinterConnectionFailed = "MI-1003";
        public const string InvalidPrinterData = "MI-1004";
        public const string PrinterOffline = "MI-1005";

        // Errores de Base de Datos (2000-2999)
        public const string DatabaseConnectionError = "MI-2001";
        public const string DatabaseConstraintViolation = "MI-2002";
        public const string DatabaseTransactionError = "MI-2003";
        public const string EntityNotFound = "MI-2004";
        public const string DuplicateEntity = "MI-2005";

        // Errores de Autenticación y Autorización (3000-3999)
        public const string AuthenticationFailed = "MI-3001";
        public const string InvalidCredentials = "MI-3002";
        public const string TokenExpired = "MI-3003";
        public const string InsufficientPermissions = "MI-3004";
        public const string AccountLocked = "MI-3005";
        public const string RefreshTokenInvalid = "MI-3006";
        public const string SessionExpired = "MI-3007";

        // Errores de Validación (4000-4999)
        public const string ValidationFailed = "MI-4001";
        public const string InvalidInputFormat = "MI-4002";
        public const string MissingRequiredField = "MI-4003";
        public const string InvalidFieldValue = "MI-4004";
        public const string FieldTooLong = "MI-4005";

        // Errores de Sistema (5000-5999)
        public const string UnexpectedError = "MI-5001";
        public const string ServiceUnavailable = "MI-5002";
        public const string ExternalServiceError = "MI-5003";
        public const string ConfigurationError = "MI-5004";
        public const string FileSystemError = "MI-5005";

        // Errores de SNMP (6000-6999)
        public const string SnmpConnectionFailed = "MI-6001";
        public const string SnmpTimeout = "MI-6002";
        public const string SnmpInvalidResponse = "MI-6003";
        public const string SnmpAuthenticationFailed = "MI-6004";
    }

    /// <summary>
    /// Mensajes de error correspondientes a los códigos de error
    /// </summary>
    public static class ErrorMessages
    {
        public static readonly Dictionary<string, string> Messages = new()
        {
            // Errores de Impresoras
            [ErrorCodes.PrinterNotFound] = "Impresora no encontrada",
            [ErrorCodes.PrinterAlreadyExists] = "Ya existe una impresora con estos datos",
            [ErrorCodes.PrinterConnectionFailed] = "No se pudo conectar con la impresora",
            [ErrorCodes.InvalidPrinterData] = "Los datos de la impresora no son válidos",
            [ErrorCodes.PrinterOffline] = "La impresora está fuera de línea",

            // Errores de Base de Datos
            [ErrorCodes.DatabaseConnectionError] = "Error de conexión con la base de datos",
            [ErrorCodes.DatabaseConstraintViolation] = "Violación de restricción en la base de datos",
            [ErrorCodes.DatabaseTransactionError] = "Error en la transacción de base de datos",
            [ErrorCodes.EntityNotFound] = "Entidad no encontrada",
            [ErrorCodes.DuplicateEntity] = "Entidad duplicada",

            // Errores de Autenticación y Autorización
            [ErrorCodes.AuthenticationFailed] = "Error de autenticación",
            [ErrorCodes.InvalidCredentials] = "Credenciales inválidas",
            [ErrorCodes.TokenExpired] = "Token expirado",
            [ErrorCodes.InsufficientPermissions] = "Permisos insuficientes",
            [ErrorCodes.AccountLocked] = "Cuenta bloqueada",
            [ErrorCodes.RefreshTokenInvalid] = "Token de refresco inválido",
            [ErrorCodes.SessionExpired] = "Sesión expirada",

            // Errores de Validación
            [ErrorCodes.ValidationFailed] = "Error de validación",
            [ErrorCodes.InvalidInputFormat] = "Formato de entrada inválido",
            [ErrorCodes.MissingRequiredField] = "Campo requerido faltante",
            [ErrorCodes.InvalidFieldValue] = "Valor de campo inválido",
            [ErrorCodes.FieldTooLong] = "Campo demasiado largo",

            // Errores de Sistema
            [ErrorCodes.UnexpectedError] = "Error inesperado del sistema",
            [ErrorCodes.ServiceUnavailable] = "Servicio no disponible",
            [ErrorCodes.ExternalServiceError] = "Error en servicio externo",
            [ErrorCodes.ConfigurationError] = "Error de configuración",
            [ErrorCodes.FileSystemError] = "Error del sistema de archivos",

            // Errores de SNMP
            [ErrorCodes.SnmpConnectionFailed] = "No se pudo conectar via SNMP",
            [ErrorCodes.SnmpTimeout] = "Timeout de conexión SNMP",
            [ErrorCodes.SnmpInvalidResponse] = "Respuesta SNMP inválida",
            [ErrorCodes.SnmpAuthenticationFailed] = "Error de autenticación SNMP"
        };
    }
}
