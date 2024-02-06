using System;
using System.Collections.Generic;

namespace Chatapp.Shared.Entities;

public partial class Picture
{
  public int Id { get; set; }

  public int BelongsTo { get; set; }

  public string NameOfFile { get; set; } = null!;

  public virtual Message BelongsToNavigation { get; set; } = null!;

  public virtual ICollection<PictureLookup> PictureLookups { get; set; } = new List<PictureLookup>();
}