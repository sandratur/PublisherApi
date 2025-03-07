using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using PublisherData;
using PublisherDomain;
namespace PubAPI;

public static class ArtistEndpoints
{
    public static void MapArtistEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Artist").WithTags(nameof(Artist));

        group.MapGet("/", async (PubContext db) =>
        {
            return await db.Artists.ToListAsync();
        })
        .WithName("GetAllArtists")
        .WithOpenApi();

        group.MapGet("/{ArtistId}", async Task<Results<Ok<Artist>, NotFound>> (int artistid, PubContext db) =>
        {
            return await db.Artists.AsNoTracking()
                .FirstOrDefaultAsync(model => model.ArtistId == artistid)
                is Artist model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetArtistById")
        .WithOpenApi();

        group.MapPut("/{ArtistId}", async Task<Results<Ok, NotFound>> (int artistid, Artist artist, PubContext db) =>
        {
            var affected = await db.Artists
                .Where(model => model.ArtistId == artistid)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.ArtistId, artist.ArtistId)
                    .SetProperty(m => m.FirstName, artist.FirstName)
                    .SetProperty(m => m.LastName, artist.LastName)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateArtist")
        .WithOpenApi();

        group.MapPost("/", async (Artist artist, PubContext db) =>
        {
            db.Artists.Add(artist);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Artist/{artist.ArtistId}",artist);
        })
        .WithName("CreateArtist")
        .WithOpenApi();

        group.MapDelete("/{ArtistId}", async Task<Results<Ok, NotFound>> (int artistid, PubContext db) =>
        {
            var affected = await db.Artists
                .Where(model => model.ArtistId == artistid)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteArtist")
        .WithOpenApi();
    }
}
