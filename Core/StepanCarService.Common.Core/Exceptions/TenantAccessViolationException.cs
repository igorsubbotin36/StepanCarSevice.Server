namespace StepanCarService.Common.Core.Exceptions
{
    // Попытка создать, изменить или удалить данные тенанта вне этого тенанта
    public class TenantAccessViolationException : Exception
    {
        public TenantAccessViolationException(string message) : base(message)
        {
        }
    }
}
