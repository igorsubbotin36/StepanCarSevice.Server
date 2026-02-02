using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens.Experimental;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Infrastructure.Services
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

                // System errors
                ["INTERNAL_ERROR"] = (HttpStatusCode.InternalServerError,
                "Внутренняя ошибка сервера"),
                ["DATABASE_ERROR"] = (HttpStatusCode.InternalServerError,
                "Ошибка базы данных"),
                ["EXTERNAL_SERVICE_ERROR"] = (HttpStatusCode.ServiceUnavailable,
                "Сервис временно недоступен"),
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
