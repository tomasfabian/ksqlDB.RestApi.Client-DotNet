using System;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Http
{
  public record BasicAuthCredentials
  {
    public BasicAuthCredentials(string userName, string password)
    {
      UserName = userName;
      Password = password;
    }

    internal BasicAuthCredentials()
    {
    }

    public string UserName { get; internal set; }
    public string Password { get; internal set; }

    public string Schema => "basic";

    internal string CreateToken()
    {
      string credentials = $"{UserName}:{Password}";

      var bytes = System.Text.Encoding.UTF8.GetBytes(credentials);

      string base64Credentials = Convert.ToBase64String(bytes, 0, bytes.Length);
      
      return base64Credentials;
    }
  }
}