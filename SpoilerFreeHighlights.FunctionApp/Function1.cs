using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SpoilerFreeHighlights.FunctionApp;

public class Function1
{
    private readonly ILogger _logger = Log.ForContext<Function1>();

    [Function(nameof(Function1))]
    public void Run([TimerTrigger("%ScheduleSyncFrequency%")] TimerInfo myTimer)
    {
        _logger.Information($"C# Timer trigger function executed at: {DateTime.Now}");
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.Information($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }
}
