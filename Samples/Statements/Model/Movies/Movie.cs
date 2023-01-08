﻿using ksqlDB.RestApi.Client.KSql.Query;

namespace Statements.Model.Movies;

public class Movie : Record
{
  public string Title { get; set; } = null!;
  public int Id { get; set; }
  public int Release_Year { get; set; }
}