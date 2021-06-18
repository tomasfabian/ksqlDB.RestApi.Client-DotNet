using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Sample.Data;
using Microsoft.AspNetCore.Components;

namespace Blazor.Sample.Pages
{
  public partial class SqlServerComponent
  {
    [Inject] private ApplicationDbContext DbContext { get; init; }

    protected override async Task OnInitializedAsync()
    {
      var sensors = await DbContext.Sensors.ToListAsync();

      await base.OnInitializedAsync();
    }
  }
}