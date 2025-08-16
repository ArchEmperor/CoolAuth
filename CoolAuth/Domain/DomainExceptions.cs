using System.Net;

namespace CoolAuth.Domain;

public class DomainException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;


    public static DomainException UserAlreadyExists =>
        new ("User already exists", HttpStatusCode.Conflict);

    public static DomainException InvalidAuthToken =>
        new ("Invalid authentication token", HttpStatusCode.Unauthorized);
    public static DomainException InvalidMagicToken =>
        new ("Invalid or expired magic link", HttpStatusCode.Unauthorized);
    public static DomainException SessionExpired =>
        new ("Session already expired", HttpStatusCode.Unauthorized);

    public static DomainException InvalidCredentials =>
        new ("Invalid credentials", HttpStatusCode.Unauthorized);

}