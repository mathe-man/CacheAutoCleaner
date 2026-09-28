namespace CacheOperator;

public static class Operator
{
    private static CancellationTokenSource? _cts;
    private static Task? _standbyTask;

    
    public static void CleanCache()
    {
        foreach (var elem in Memory.Load())
            elem.Delete();
        
    }

    public static void StartParallelStandby(int cleaningIntervalSeconds)
    {
        if (_standbyTask is { IsCompleted: false })
        {
            StopParallelStandby();
            StartParallelStandby(cleaningIntervalSeconds);
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _standbyTask = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(cleaningIntervalSeconds));

            try
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    try
                    {
                        CleanCache();
                    }
                    catch (Exception ex)
                    {
                        // An exception shouldn't stop the loop
                        Console.WriteLine($"Cleaning exception : {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Stop called, nothing to do
            }
        }, token);
    }
    
    public static void StopParallelStandby()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}