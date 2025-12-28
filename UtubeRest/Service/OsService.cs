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

            var timeoutSignal = new CancellationTokenSource(TimeSpan.FromSeconds(12000)); // 2 min timeout 

            try
            {
                await process.WaitForExitAsync(timeoutSignal.Token);
                Console.WriteLine("Command has been Finished");
            }
            catch (OperationCanceledException)
            {
                process.Kill();
                Console.WriteLine("Command has been Terminated");
            }

            return sbOutput.ToString();
        }

        public async static Task RunUnixCommandAsync(string command, StreamWriter outputStreamWriter, StreamWriter errorStreamWriter)
        {
            UnicodeEncoding uniEncoding = new UnicodeEncoding();

            var sbOutput = new StringBuilder();
            var sbError = new StringBuilder();

            //var outputStreamWriter = new StreamWriter(outputStream);
            //var errorStreamWriter = new StreamWriter(errorStream);

            outputStreamWriter.AutoFlush = true;
            errorStreamWriter.AutoFlush = true;

            Console.WriteLine("Launch command");
            using var process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += (s, e) =>
            {
                sbOutput.AppendLine(e.Data);
                if (e.Data is not null)
                { 
                    outputStreamWriter.WriteLine(e.Data); 
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                sbError.AppendLine(e.Data);
                if (e.Data != null)
                {
                    errorStreamWriter.WriteLine(e.Data);
                }
            };

            //outputStream = process.StandardOutput.BaseStream;
            //errorStream = process.StandardError.BaseStream;


            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            //await process.StandardOutput.BaseStream.CopyToAsync(outputStream);
            //await process.StandardError.BaseStream.CopyToAsync(errorStream);

            var timeoutSignal = new CancellationTokenSource(TimeSpan.FromSeconds(240)); // 4 min timeout 

            try
            {
                await process.WaitForExitAsync(timeoutSignal.Token);
                
                ////await outputStreamWriter.FlushAsync();
                ////await errorStreamWriter.FlushAsync();
                //outputStreamWriter.Dispose();
                //errorStreamWriter.Dispose();
                //Console.WriteLine("Command has been Finished");
            }
            catch (OperationCanceledException)
            {
                process.Kill();
                //Console.WriteLine("Command has been Terminated");
            }
        }
    }
}
