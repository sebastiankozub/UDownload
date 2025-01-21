using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using UtubeRest.ViewModel;
namespace UtubeRest.Controllers;

public static class AudioManifestEndpoints
{
    public static void MapAudioManifestEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/AvManifestEndpoint").WithTags(nameof(AvManifest)+"Ep");

        group.MapGet("/", () =>
        {
            return new[] { new AvManifest { Id = "sXdfa_sf", Title = "fd", Description = "Longer description", Keywords = ["zdfsdf", "sdfsdfdsf"], AudioStreams = [], VideoStreams = [] } };
        })
        .WithName("GetAllAudioManifests")
        .WithOpenApi();

        group.MapGet("/{id}", (int id) =>
        {
            //return new AvManifest { ID = id };
        })
        .WithName("GetAudioManifestById")
        .WithOpenApi();

        //group.MapPut("/{id}", (int id, AvManifest input) =>
        //{
        //    return TypedResults.NoContent();
        //})
        //.WithName("UpdateAudioManifest")
        //.WithOpenApi();

        //group.MapPost("/", (AvManifest model) =>
        //{
        //    return TypedResults.Created($"/api/AudioManifests/{model.ID}", model);
        //})
        //.WithName("CreateAudioManifest")
        //.WithOpenApi();

        //group.MapDelete("/{id}", (int id) =>
        //{
        //    return TypedResults.Ok(new AvManifest { ID = id });
        //})
        //.WithName("DeleteAudioManifest")
        //.WithOpenApi();
    }
}
