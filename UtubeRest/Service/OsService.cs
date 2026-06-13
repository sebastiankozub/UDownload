using System.Diagnostics;
using System.Text;

namespace UtubeRest.Service
{
    public sealed record CommandExecutionResult(int ExitCode, string Output, string Error);

    public class OsService
    {

        public static string RunUnixCommand(string command)
        {
            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.ArgumentList.Add("-c");
            process.StartInfo.ArgumentList.Add(command);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            return output;
        }

        public async static Task<string> RunUnixCommandAsync(string command)
        {
            var result = await RunUnixCommandWithResultAsync(command);
            return result.Output;
        }

        public async static Task RunUnixCommandAsync(string command, StreamWriter outputStreamWriter, StreamWriter errorStreamWriter)
        {
            outputStreamWriter.AutoFlush = true;
            errorStreamWriter.AutoFlush = true;

            await RunUnixCommandStreamingAsync(
                command,
                line => outputStreamWriter.WriteLineAsync(line),
                line => errorStreamWriter.WriteLineAsync(line));
        }

        public async static Task RunUnixCommandStreamingAsync(
            string command,
            Func<string, Task> onOutput,
            Func<string, Task>? onError = null,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Launch command");

            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.ArgumentList.Add("-c");
            process.StartInfo.ArgumentList.Add(command);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            using var timeoutSignal = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSignal.CancelAfter(TimeSpan.FromSeconds(240));

            process.Start();

            try
            {
                await Task.WhenAll(
                    process.WaitForExitAsync(timeoutSignal.Token),
                    PumpReaderAsync(process.StandardOutput, onOutput, timeoutSignal.Token),
                    PumpReaderAsync(process.StandardError, onError, timeoutSignal.Token));
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
        }

        public async static Task<CommandExecutionResult> RunUnixCommandWithResultAsync(
            string command,
            CancellationToken cancellationToken = default)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.ArgumentList.Add("-c");
            process.StartInfo.ArgumentList.Add(command);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            using var timeoutSignal = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSignal.CancelAfter(TimeSpan.FromSeconds(240));

            process.Start();

            try
            {
                await Task.WhenAll(
                    process.WaitForExitAsync(timeoutSignal.Token),
                    PumpReaderAsync(
                        process.StandardOutput,
                        line =>
                        {
                            outputBuilder.AppendLine(line);
                            return Task.CompletedTask;
                        },
                        timeoutSignal.Token),
                    PumpReaderAsync(
                        process.StandardError,
                        line =>
                        {
                            errorBuilder.AppendLine(line);
                            return Task.CompletedTask;
                        },
                        timeoutSignal.Token));
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }

                throw;
            }

            return new CommandExecutionResult(process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
        }

        private static async Task PumpReaderAsync(
            StreamReader reader,
            Func<string, Task>? onLine,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null)
                {
                    break;
                }

                if (onLine is not null)
                {
                    await onLine(line);
                }
            }
        }
    }
}
