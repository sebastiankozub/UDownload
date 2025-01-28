using System.Diagnostics;
using System.Text;

namespace UtubeRest.Service
{
    public class OsService
    {

        public static string RunUnixCommand(string command)
        {
            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command}\"";
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
            var sbOutput = new StringBuilder();
            var sbError = new StringBuilder();

            Console.WriteLine("Launch command");
            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += (s, e) 
                => sbOutput.AppendLine(e.Data);

            process.ErrorDataReceived += (s, e) 
                => sbError.AppendLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var timeoutSignal = new CancellationTokenSource(TimeSpan.FromSeconds(120)); // 2 min timeout 

            try
            {
                await process.WaitForExitAsync(timeoutSignal.Token);
                Console.WriteLine("Command has been Finished");
            }
            catch (OperationCanceledException)
            {
                process.Kill();
                Console.WriteLine("Copmmand has been Terminated");
            }

            return sbOutput.ToString();
        }
    }
}
