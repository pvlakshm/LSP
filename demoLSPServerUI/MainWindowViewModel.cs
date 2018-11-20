using System.IO.Pipes;
using System.Windows;
using System.Windows.Threading;
using demoLSPServerLibrary;

namespace demoLSPServerUI
{
    /*
     * This represents our server process.
     * (I chose making it a Windows application so that I don't have to worry about keeping it alive post-instantiation.
     * This could have as well been implemented as a console .exe but then i would have had to have special code just
     * to keep it alive)
     * 
     * Corresponding to the ActivateAsync in BarLanguageClient.cs, we establish and connect to the named pipes;
     * this will be the medium for the client and server processes to communicate.
     * The LanguageServer is then instantiated and provided these pipes to communicate over.
     */
    public class MainWindowViewModel
    {
        private readonly LanguageServer languageServer;

        // instantiated from MainWindow.xaml.cs 
        public MainWindowViewModel()
        {
            //System.Diagnostics.Debug.Fail("Test"); // just for me to break in and attach the debugger.

            var stdInPipeName = @"input";
            var stdOutPipeName = @"output";

            var pipeAccessRule = new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            var pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(pipeAccessRule);

            var readerPipe = new NamedPipeClientStream(stdInPipeName);
            var writerPipe = new NamedPipeClientStream(stdOutPipeName);

            readerPipe.Connect();
            writerPipe.Connect();

            this.languageServer = new LanguageServer(writerPipe, readerPipe);
            this.languageServer.Disconnected += OnDisconnected;
        }

        private void OnDisconnected(object sender, System.EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }
    }
}
