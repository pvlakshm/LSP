# An implementation of the Language Server Protocol supporting hover semantics
Here are the key files in these projects:

## [__demoLSP__](demoLSP)
This is the client.
1) [BarContentTypeDefinition.cs](demoLSP/BarContentTypeDefinition.cs) - Defines and registers a contenttype (in this case any file with the extension __".bar"__).
2) [BarLanguageClient.cs](demoLSP/BarLanguageClient.cs) - client for the LSP extension. Activation is handled in ActivateAsync. Explicitly locates and starts server. Client-server communication is over named pipes.
3) [source.extension.manifest](demoLSP/source.extension.manifest) - VSIX manifest file, identifying required assets, etc.

## [__demoLSPServerUI__](demoLSPServerUI)
This is the server.
1) [MainWindowViewModel.cs](demoLSPServerUI/MainWindowViewModel.cs) - represents the server process. Uses the same pipes (above) to connect to the client.
the language service implemention is split across the following 2 classes:
2) [LanguageServer.cs](demoLSPServerUI/LanguageServer.cs) - this is the server implementation that will communicate back
3) [LanguageServerTarget.cs](demoLSPServerUI/LanguageServerTarget.cs) - methods on this class will receive notifications and act upon them. The Initialize implementation here will be used to report back the capabilities of our language service. In this case, we will support only hover. Accordingly when the user is hovering over text in the editor we will receive a notification on OnTestDocumentHover(...) here.

## __Usage__
1) Open demoLSP.sln in Visual Studio (please use VS 15.9).
2) Hit Ctrl F5 to start. Build should succeed, and an experimental instance of VS will start (VS-Exp).
3) In VS-Exp, locate the file "test.bar" and open it - you should see a file with "test" written twice.
This will cause our extension to be activated, which in turn will launch the LSP server. A window will pop up -(minimize it)
4) Take the mouse and hover over the text in the file.
You should see a message from our server show up as an informational message on the editor reporting the file name and the hover position.
