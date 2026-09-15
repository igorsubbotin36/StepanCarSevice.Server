using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Application.Models
{
    public static class AuthErrors
    {
        public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
        public const string UserLocked = "AUTH_USER_LOCKED";
        public const string PhoneNotConfirmed = "AUTH_PHONE_NOT_CONFIRMED";
        public const string TokenExpired = "AUTH_TOKEN_EXPIRED";
        public const string TokenIsNotValid = "TOKEN_IS_NOT_VALID";
        public const string Forbidden = "AUTH_FORBIDDEN";
    }
    public static class RegisterErrors
    {
        public const string PasswordsDontMatch = "PASSWORDS_DONT_MATCH";
        public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
        public const string RoleIdNotFound = "ROLE_ID_NOT_FOUND";
    }
    public static class UserErrors
    {
        public const string NotFound = "USER_NOT_FOUND";
        public const string InvalidPhone = "USER_INVALID_PHONE";
        public const string WrongPassword = "USER_WRONG_PASSWORD";
    }

    public static class ValidationErrors
    {
        public const string RequiredField = "VALIDATION_REQUIRED_FIELD";
        public const string InvalidFormat = "VALIDATION_INVALID_FORMAT";
        public const string PasswordTooWeak = "VALIDATION_PASSWORD_TOO_WEAK";
    }
    public static class ModelErrors
    {
        public const string ModelNotFound = "MODEL_NOT_FOUND";
        public const string RequestedModelIsNull = "REQUESTED_MODEL_IS_NULL";
        public const string ModelAlreadyExists = "MODEL_ALREADY_EXISTS";
    }
    public static class MessageBusErrors
    {
        public const string MessageNotDelivered = "MESSAGE_NOT_DELIVERED";
    }
    public static class SystemErrors
    {
        public const string InternalError = "INTERNAL_ERROR";
        public const string DatabaseError = "DATABASE_ERROR";
        public const string ExternalServiceError = "EXTERNAL_SERVICE_ERROR";
    }
    public static class EntityErrors
    {
        public const string EntityNotFound = "ENTITY_NOT_FOUND";
    }
    public static class TenantErrors
    {
        public const string TenantIsNull = "TENANT_IS_NULL";
        public const string TenantNotFound = "TENANT_NOT_FOUND";
        public const string TenantAlreadyExists = "TENANT_ALREADY_EXISTS";
        public const string InvalidIdentifier = "TENANT_INVALID_IDENTIFIER";
        public const string TenantInactive = "TENANT_INACTIVE";
        public const string OwnerAlreadyHasTenant = "TENANT_OWNER_ALREADY_HAS_TENANT";
    }
}
