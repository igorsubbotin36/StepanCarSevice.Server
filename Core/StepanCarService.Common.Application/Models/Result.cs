using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Application.Models
{
    public abstract class Result
    {
        public bool IsSuccess { get; }
        public string ErrorCode { get; }

        protected Result(bool isSuccess, string errorCode)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
        }

        public static Result Success()
            => new Result<object>(default, true, null);

        public static Result<T> Success<T>(T value)
            => new Result<T>(value, true, null);

        public static Result Failure(string errorCode)
            => new Result<object>(default, false, errorCode);

        public static Result<T> Failure<T>(string errorCode)
            => new Result<T>(default, false, errorCode);
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        internal Result(T value, bool isSuccess, string errorCode)
            : base(isSuccess, errorCode)
        {
            Value = value;
        }
    }
}
