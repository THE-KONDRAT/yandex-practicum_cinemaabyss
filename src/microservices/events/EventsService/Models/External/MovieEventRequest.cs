namespace EventsService.Models.External;

public class MovieEventRequest
{
    /*
    "movie_id": {{movieId}},
    "title": "Test Movie Event",
    "action": "viewed",
    "user_id": {{userId}}
    */
    public required int MovieId { get; init; }
    public required string Title { get; init; }
    public required string Action { get; init; }
    public int? UserId { get; init; }
    public float? Rating { get; init; }
    public string[]? Genres { get; init; }
    public string? Description { get; init; }
}