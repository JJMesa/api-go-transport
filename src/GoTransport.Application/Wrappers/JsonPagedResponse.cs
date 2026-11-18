using System.Text.Json.Serialization;

namespace GoTransport.Application.Wrappers;

public partial class JsonPagedResponse<TEntity> : JsonResponse<TEntity>
{
    [JsonIgnore]
    public Metadata? Metadata { get; set; }
}