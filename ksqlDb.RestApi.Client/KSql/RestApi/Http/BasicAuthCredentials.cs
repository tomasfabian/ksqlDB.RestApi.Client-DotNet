namespace ksqlDB.RestApi.Client.KSql.RestApi.Http;

/// <summary>
/// Represents basic authentication credentials.
/// </summary>
public record BasicAuthCredentials
{
  /// <summary>
  /// Gets the schema for basic authentication.
  /// </summary>
  public const string Schema = "basic";

  /// <summary>
  /// Initializes a new instance of the <see cref="BasicAuthCredentials"/> class.
  /// </summary>
  /// <param name="userName">The username.</param>
  /// <param name="password">The password.</param>
  public BasicAuthCredentials(string userName, string password)
  {
    UserName = userName;
    Password = password;
  }

  /// <summary>
  /// Gets the username.
  /// </summary>
  public string UserName { get; internal set; }

  /// <summary>
  /// Gets the password.
  /// </summary>
  public string Password { get; internal set; }

  /// <summary>
  /// Creates a token for basic authentication.
  /// </summary>
  /// <returns>A base64 encoded string representing the username and password.</returns>
  internal string CreateToken()
  {
    string credentials = $"{UserName}:{Password}";

    var bytes = System.Text.Encoding.UTF8.GetBytes(credentials);

    string base64Credentials = Convert.ToBase64String(bytes, 0, bytes.Length);

    return base64Credentials;
  }
}
