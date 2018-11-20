using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;

namespace demoLSP
{
    /*
     * The below 2 attributes tell VS the content type for which to load and activate our LSP Client.
     * We are using MEF - hence we will also have indicated that this demoLSP project is a MEF asset in the vsix manifest.
     */
    [ContentType("bar")]
    [Export(typeof(ILanguageClient))]
    public class BarLanguageClient : ILanguageClient
    {
        public string Name => "Bar Language Server";

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => null;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            /*
             * Put a break point here. Then F5, and an experimental intance of VS starts up. In that, open a file
             * name say "test.bar" and this break point should be hit.
             * 
             * We go on to explicitly locate the server, and start it.
             * Communicaton between this client and the server processes is over named pipes.
             * The client will "write" to the "input" of the server, and "read" from the "output" of the server.
             * Once the server process is started, the client will wait for these connections to be established.
             * MainWindowViewModel.cs in demoLSPServerUI has the corresponding implementation for the server.
             */
            ProcessStartInfo info = new ProcessStartInfo();
            var programPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"demoLSPServerUI.exe");
            info.FileName = programPath;
            info.WorkingDirectory = Path.GetDirectoryName(programPath);
            info.UseShellExecute = false;

            var stdInPipeName = @"output";
            var stdOutPipeName = @"input";

            var pipeAccessRule = new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            var pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(pipeAccessRule);

            var bufferSize = 256;
            var readerPipe = new NamedPipeServerStream(stdInPipeName, PipeDirection.InOut, 4, PipeTransmissionMode.Message, PipeOptions.Asynchronous, bufferSize, bufferSize, pipeSecurity);
            var writerPipe = new NamedPipeServerStream(stdOutPipeName, PipeDirection.InOut, 4, PipeTransmissionMode.Message, PipeOptions.Asynchronous, bufferSize, bufferSize, pipeSecurity);

            Process process = new Process();
            process.StartInfo = info;
            try
            {
                if (process.Start())
                {
                    await readerPipe.WaitForConnectionAsync(token);
                    await writerPipe.WaitForConnectionAsync(token);

                    return new Connection(readerPipe, writerPipe);
                }
            }
            catch (Exception ex)
            {
                // just for me to debug in case we are unable to start the process.
                Debugger.Break();
            }

            return null;
        }

        public Task OnLoadedAsync()
        {
            return StartAsync?.InvokeAsync(this, EventArgs.Empty);
        }

        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnServerInitializeFailedAsync(Exception e)
        {
            return Task.CompletedTask;
        }
    }
}
