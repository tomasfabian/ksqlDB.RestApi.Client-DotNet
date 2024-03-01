namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class JoinAliasGenerator
{
  private readonly Dictionary<string, string> aliasDictionary = new();

  public string GenerateAlias(string name)
  {
    if (aliasDictionary.TryGetValue(name, out var existingAlias))
    {
      return existingAlias;
    }

    var newAlias = CreateDistinctAliasFrom(name);
    aliasDictionary[name] = newAlias;

    return newAlias;
  }

  private string CreateDistinctAliasFrom(string name)
  {
    var aliasBase = name.FirstOrDefault(c => c != '`').ToString();
    int suffix = 0;

    var newAlias = aliasBase;

    while (aliasDictionary.Values.Contains(newAlias))
    {
      newAlias = $"{aliasBase}{++suffix}";
    }

    return newAlias;
  }
}
