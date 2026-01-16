using CDMValidation.Core.Models;
using System.Diagnostics;

namespace CDMValidation.CLI;

/// <summary>
/// Reports validation progress to the console with a progress bar and time estimates.
/// </summary>
public class ProgressReporter : IProgress<ValidationProgress>
{
    private readonly Stopwatch _stopwatch = new();
    private readonly object _lock = new();
    private string _lastPhase = string.Empty;
    private int _lastCurrent = 0;
    private int _consoleWidth;
    private bool _isJsonOutput;

    public ProgressReporter(bool isJsonOutput = false)
    {
        _isJsonOutput = isJsonOutput;
        try
        {
            _consoleWidth = Console.WindowWidth;
        }
        catch
        {
            _consoleWidth = 80; // Default if console width can't be determined
        }
        _stopwatch.Start();
    }

    public void Report(ValidationProgress progress)
    {
        // Don't show progress bar in JSON mode
        if (_isJsonOutput)
            return;

        lock (_lock)
        {
            // Reset stopwatch when starting a new phase
            if (progress.Phase != _lastPhase)
            {
                _lastPhase = progress.Phase;
                _lastCurrent = 0;
                _stopwatch.Restart();
            }

            _lastCurrent = progress.Current;

            // Calculate estimated time remaining
            TimeSpan? estimatedTimeRemaining = null;
            if (progress.Current > 0 && progress.Total > 0)
            {
                double percentComplete = (double)progress.Current / progress.Total;
                if (percentComplete > 0)
                {
                    var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
                    var totalEstimatedSeconds = elapsedSeconds / percentComplete;
                    var remainingSeconds = totalEstimatedSeconds - elapsedSeconds;

                    if (remainingSeconds > 0)
                    {
                        estimatedTimeRemaining = TimeSpan.FromSeconds(remainingSeconds);
                    }
                }
            }

            // Clear current line
            Console.Write("\r" + new string(' ', Math.Min(_consoleWidth - 1, 120)));
            Console.Write("\r");

            // Build progress bar
            int barWidth = Math.Min(40, (_consoleWidth - 60)); // Reserve space for text
            if (barWidth < 10) barWidth = 10;

            int filledWidth = progress.Total > 0
                ? (int)((double)progress.Current / progress.Total * barWidth)
                : 0;

            string bar = "[" +
                         new string('█', Math.Min(filledWidth, barWidth)) +
                         new string('░', Math.Max(0, barWidth - filledWidth)) +
                         "]";

            string percentage = progress.Total > 0
                ? $"{progress.PercentComplete:F1}%"
                : "0.0%";

            string phaseInfo = $"{progress.Phase}: {progress.Current:N0}/{progress.Total:N0}";

            string timeInfo = estimatedTimeRemaining.HasValue
                ? $"ETA: {FormatTimeSpan(estimatedTimeRemaining.Value)}"
                : "";

            // Truncate phase info if too long
            int maxPhaseLength = _consoleWidth - barWidth - percentage.Length - timeInfo.Length - 10;
            if (maxPhaseLength > 0 && phaseInfo.Length > maxPhaseLength)
            {
                phaseInfo = phaseInfo.Substring(0, maxPhaseLength - 3) + "...";
            }

            // Print progress
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(bar);
            Console.ResetColor();
            Console.Write($" {percentage}");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($" {phaseInfo}");
            Console.ResetColor();

            if (!string.IsNullOrEmpty(timeInfo))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($" {timeInfo}");
                Console.ResetColor();
            }
        }
    }

    public void Complete()
    {
        if (!_isJsonOutput)
        {
            lock (_lock)
            {
                Console.WriteLine(); // Move to next line after progress bar
                _stopwatch.Stop();
            }
        }
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        }
        else if (timeSpan.TotalMinutes >= 1)
        {
            return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
        }
        else
        {
            return $"{timeSpan.Seconds}s";
        }
    }
}
