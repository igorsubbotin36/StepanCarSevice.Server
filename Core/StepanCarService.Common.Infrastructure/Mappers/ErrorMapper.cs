using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Application.Models;
using System.Net;

namespace StepanCarService.Common.Infastructure.Mappers
{
    public class ErrorMapper : IErrorMapper
    {
        private readonly Dictionary<string, (HttpStatusCode statusCode, string message)> _errorMappings;

        public ErrorMapper()
        {
            _errorMappings = new()
            {
                // Auth errors
                [AuthErrors.InvalidCredentials] = (HttpStatusCode.Unauthorized,
                    "Неверный логин или пароль"),
                [AuthErrors.UserLocked] = (HttpStatusCode.Forbidden,
                    "Аккаунт заблокирован"),
                [AuthErrors.PhoneNotConfirmed] = (HttpStatusCode.BadRequest,
                    "Телефон не подтвержден"),
                [AuthErrors.TokenExpired] = (HttpStatusCode.Unauthorized,
                    "Токен истек"),
                [AuthErrors.TokenIsNotValid] = (HttpStatusCode.BadRequest,
                    "Токен неверен"),

                //Register errors
                [RegisterErrors.UserAlreadyExists] = (HttpStatusCode.Conflict,
                    "Пользователь с таким номером телефона уже существует"),
                [RegisterErrors.PasswordsDontMatch] = (HttpStatusCode.BadRequest,
                    "Пароли не совпадают"),
                [RegisterErrors.RoleIdNotFound] = (HttpStatusCode.InternalServerError,
                    "Не найден Id роли при регистрации"),

                // User errors
                [UserErrors.NotFound] = (HttpStatusCode.NotFound,
                "Пользователь не найден"),
                [UserErrors.InvalidPhone] = (HttpStatusCode.BadRequest,
                "Неверный формат номера телефона"),

                // Validation errors
                [ValidationErrors.RequiredField] = (HttpStatusCode.BadRequest,
                "Обязательное поле не заполнено"),
                [ValidationErrors.InvalidFormat] = (HttpStatusCode.BadRequest,
                "Неверный формат данных"),
                [ValidationErrors.PasswordTooWeak] = (HttpStatusCode.BadRequest,
                "Пароль слишком слабый. Используйте буквы, цифры и специальные символы"),

                // DetailErrors
                [ModelErrors.ModelNotFound] = (HttpStatusCode.NotFound,
                "Модель не найдена"),
                [ModelErrors.RequestedModelIsNull] = (HttpStatusCode.BadRequest,
                "Переданная модель для обновления пустая"),
                [ModelErrors.ModelAlreadyExists] = (HttpStatusCode.Conflict,
                "Модель уже существует в БД"),

                // MessageBusErrors
                [MessageBusErrors.MessageNotDelivered] = (HttpStatusCode.InternalServerError,
                "Ошибка публикации сообщения"),

                // System errors
                [SystemErrors.InternalError] = (HttpStatusCode.InternalServerError,
                "Внутренняя ошибка сервера"),
                [SystemErrors.DatabaseError] = (HttpStatusCode.InternalServerError,
                "Ошибка базы данных"),
                [SystemErrors.ExternalServiceError] = (HttpStatusCode.ServiceUnavailable,
                "Сервис временно недоступен"),

                //Entity Erros
                [EntityErrors.EntityNotFound] = (HttpStatusCode.NotFound,
                "Не найдено в бд"),

                //Tenant errors
                [TenantErrors.TenantIsNull] = (HttpStatusCode.BadRequest, 
                    "Переданный тенант пустой"),
                [TenantErrors.TenantNotFound] = (HttpStatusCode.NotFound,
                        "Тенант не найден")
            };
        }

        public (HttpStatusCode statusCode, string message) Map(string errorCode)
        {
            return _errorMappings.TryGetValue(errorCode, out var mapping)
                ? mapping
                : (HttpStatusCode.InternalServerError, "Произошла непредвиденная ошибка");
        }
    }
}
