using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace demoLSP
{

    /*
     * Refer: https://docs.microsoft.com/en-us/visualstudio/extensibility/language-service-and-editor-extension-points?view=vs-2017
     * Define a new named content type for registering with the VS Editor. Our extension will only care about this content type.
     * Furthermore, we associate this new content type wth a file extension of our choice.
     * */

    public class BarContentTypeDefinition
    {
        [Export]
        [Name("bar")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition FooContentTypeDefinition;


        [Export]
        [FileExtension(".bar")]
        [ContentType("bar")]
        internal static FileExtensionToContentTypeDefinition FooFileExtensionDefinition;
    }
}
