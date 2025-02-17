using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Coflnet.Sky.Mayor.Models;

public record Candidate(
    [property: JsonPropertyName("key")] string key,
    [property: JsonPropertyName("name")] string name,
    [property: JsonPropertyName("perks")] IReadOnlyList<Perk> perks,
    [property: JsonPropertyName("votes")] int votes
);

public record Current(
    [property: JsonPropertyName("year")] int year,
    [property: JsonPropertyName("candidates")] IReadOnlyList<Candidate> candidates
);

public record Election(
    [property: JsonPropertyName("year")] int year,
    [property: JsonPropertyName("candidates")] IReadOnlyList<Candidate> candidates
);

public record Mayor(
    [property: JsonPropertyName("key")] string key,
    [property: JsonPropertyName("name")] string name,
    [property: JsonPropertyName("perks")] IReadOnlyList<Perk> perks,
    [property: JsonPropertyName("minister")] Minister minister,
    [property: JsonPropertyName("election")] Election election
);

public record Minister(
    [property: JsonPropertyName("key")] string key,
    [property: JsonPropertyName("name")] string name,
    [property: JsonPropertyName("perk")] Perk perk
);

public record Perk(
    [property: JsonPropertyName("name")] string name,
    [property: JsonPropertyName("description")] string description,
    [property: JsonPropertyName("minister")] bool minister
);

public record Perk2(
    [property: JsonPropertyName("name")] string name,
    [property: JsonPropertyName("description")] string description,
    [property: JsonPropertyName("minister")] bool minister
);

public record ElectionResult(
    [property: JsonPropertyName("success")] bool success,
    [property: JsonPropertyName("lastUpdated")] long lastUpdated,
    [property: JsonPropertyName("mayor")] Mayor mayor,
    [property: JsonPropertyName("current")] Current current
);