using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamJsonRpc;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace demoLSPServerLibrary
{
    /*
     * This is our LanguageServiceTarget. This will receive notifications from the LSP and act upon them.
     * Every method publshed over JsonRpc is indicated with a corresponding attribute.
     * The very first method invoked will be Initialize - this is where the language server can advertise
     * its capabilities. Advertising your capabilities will further ensure that you get notified only as
     * appropriate.
     * 
     * In this case, our language server only supports 1 capability - "hover".
     * When the user is hovering over text in a file in the editor, and if that files matches the content type
     * our LSP client cared about, then VS will activate the client, the client will start the server, and
     * establish the piped connection and the server will start getting notificaitons as appropriate.
     */ 
    public class LanguageServerTarget
    {
        private readonly LanguageServer server;
        public event EventHandler Initialized;
        private MarkupKind HoverContentFormat;

        public LanguageServerTarget(LanguageServer server)
        {
            this.server = server;
        }

        [JsonRpcMethod(Methods.InitializeName)]
        public object Initialize(JToken arg)
        {
            /* We want to support the hover capability. When we get a hover request we will respond
             * with the appropriate text.
             * Check the client's declared capabilities to identify what hover format the client expects
             * before returning a hover response from the server. Stash away the expected format for use
             * when composing the hover response.
             */
            var b = arg.ToObject<InitializeParams>();
            MarkupKind[] m = b.Capabilities.TextDocument.Hover.ContentFormat;

            // in the case of Visual Studio, this is what I am observing. Capturing that as an assertion
            // just in case things change.
            System.Diagnostics.Debug.Assert(m != null &&
                                            m.Length == 1 &&
                                            m[0] == MarkupKind.PlainText);

            HoverContentFormat = m[0];

            /*
             *"hover" is the only capability that we wil report back as supported.
             */
            var capabilities = new ServerCapabilities();
            capabilities.HoverProvider = true;

            var result = new InitializeResult();
            result.Capabilities = capabilities;

            Initialized?.Invoke(this, new EventArgs());

            return result;
        }

        /*
         * We got a notfication for hover.
         * The incoming params will contain the file name and the position at which the cursor is hovering.
         * 
         * At this point we coudl offer a variety of features:
         * (1) we could potentially take the position, and traverse to/fro from the position until we can
         * isolate a word and return that word to be shown as a tooltip by the client
         * (2) Or, we could return some metadata about the word,
         * (3) etc.
         * 
         * In this case I am just going to pass on the param to our server.
         */
        [JsonRpcMethod(Methods.TextDocumentHoverName)]
        public void OnTextDocumentHover(JToken arg)
        {
            var parameter = arg.ToObject<TextDocumentPositionParams>();
            server.OnTextDocumentHovered(parameter, HoverContentFormat);
        }

        [JsonRpcMethod(Methods.ShutdownName)]
        public object Shutdown()
        {
            return null;
        }

        [JsonRpcMethod(Methods.ExitName)]
        public void Exit()
        {
            server.Exit();
        }
    }
}
