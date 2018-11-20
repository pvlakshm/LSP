using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using StreamJsonRpc;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using System.Threading;

namespace demoLSPServerLibrary
{
    /*
     * This is our LanguageServer. It is composed of this LanguageServer class, and
     * a LanguageServerTarget class. The LanguageServerTarget class will be the one
     * receving LSP notifications, and acting upon them - that way i have separate
     * abstraction that can proffer and implement the logic to support various
     * Intellisense-like services. It will rely on this LanguageServer class to talk
     * back to the client etc.
     *
     * So, here we instantiate the LanguageServerTarget and attach it the named pipes;
     * thus it can recieve LSP notifications.
     * The JsonRpc object returned by the call to Attach can then on be used to invoke
     * remote methods.
     * 
     * The rest of the methods here are to construct the payloads and use them in any
     * remote method invocations.
     */

    public class LanguageServer
    {
        private readonly JsonRpc rpc;
        private readonly LanguageServerTarget target;
        private readonly ManualResetEvent disconnectEvent = new ManualResetEvent(false);

        public event EventHandler Disconnected;

        public LanguageServer(Stream sender, Stream reader)
        {
            this.target = new LanguageServerTarget(this);
            this.rpc = JsonRpc.Attach(sender, reader, this.target);
        }

        /*
         * We could potentially provide a variety of features here.
         * But I will keep this simple. I unpack the parameters to extract the file name and hover position
         * and use that as the payload for my remote method call.
         */
        internal void OnTextDocumentHovered(TextDocumentPositionParams parameter, MarkupKind m)
        {
            string markupkind = m.ToString();
            string msg = string.Empty;

            if (m == MarkupKind.PlainText)
            {
                msg = "The bar LSP - " +
                            " File: " + parameter.TextDocument.Uri.AbsolutePath +
                            " Line: " + parameter.Position.Line +
                            " Character: " + parameter.Position.Character +
                            " HoverContentFormat: " + markupkind;
            }
            else if (m == MarkupKind.Markdown)
            {
                // should never be taken in Visual Studio as per my observation. See assertion in
                // LanguageServerTarget.Initialize().
                // Even if we force this path via the debugger, the below text is not interpreted
                // as markdown when displayed in VS.
                msg = "The bar LSP - " +
                        " **File:** " + parameter.TextDocument.Uri.AbsolutePath +
                        " _Line:_ " + parameter.Position.Line +
                        " _Character:_ " + parameter.Position.Character +
                        " **HoverContentFormat:** " + markupkind;
            }
            else
            {
                msg = "The bar LSP - " +
                      " HoverContentFormat: " + "unknown.";
                System.Diagnostics.Debug.Fail("unknown hover content format"); // just for me to break in and attach the debugger.
            }

            // Send a informational message
            this.ShowMessage(msg, MessageType.Info);
        }

        /*
         * This will be remote method invocation.
         * Availalbe methods to invoke are provided by the LSP protocol via "Methods".
         * I could have invoked a method on the LSP Client but instead am directly invoking
         * WindowShowMessageName - this is just a short cut to see if the e2e feature is working.
         * You shoud see a gold bar in the VS Editor with message string displayed.
         */
        public void ShowMessage(string message, MessageType messageType)
        {
            ShowMessageParams parameter = new ShowMessageParams
            {
                Message = message,
                MessageType = messageType
            };
            this.rpc.NotifyWithParameterObjectAsync(Methods.WindowShowMessageName, parameter);
        }

        public void Exit()
        {
            this.disconnectEvent.Set();

            Disconnected?.Invoke(this, new EventArgs());
        }
    }
}
