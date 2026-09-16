using System.Diagnostics;

namespace StepanCarService.TestKit;

// Ожидание условия вместо фиксированных пауз: для асинхронных процессов (события, фоновые службы)
public static class Eventually
{
    public static async Task<bool> WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout,
        bool throwOnTimeout = true, TimeSpan? pollInterval = null, string? because = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            if (await condition())
                return true;
            if (stopwatch.Elapsed >= timeout)
            {
                if (throwOnTimeout)
                    throw new TimeoutException($"Условие не выполнилось за {timeout.TotalSeconds:0.#} с{(because == null ? "" : $": {because}")}");
                return false;
            }
            await Task.Delay(interval);
        }
    }
}
