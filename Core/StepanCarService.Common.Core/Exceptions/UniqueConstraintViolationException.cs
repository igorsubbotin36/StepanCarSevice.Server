namespace StepanCarService.Common.Core.Exceptions
{
    // Нарушение уникального индекса в БД (например, одновременная регистрация одного телефона)
    public class UniqueConstraintViolationException : Exception
    {
        public UniqueConstraintViolationException(Exception innerException)
            : base("Unique constraint violation", innerException)
        {
        }
    }
}
