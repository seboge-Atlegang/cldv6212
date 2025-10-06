using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailsFunctions.Functions;

public class BlobFunctions
{
    [Function("product-image-uploaded")]
    public void OnImageUploaded([BlobTrigger("%BLOB_CONTAINER%/{name}", Connection = "STORAGE_CONNECTION")] byte[] data, string name, FunctionContext ctx)
    {
        ctx.GetLogger("product-image-uploaded").LogInformation("Image uploaded: {Name}, {Bytes}", name, data.Length);
    }
}