using System;
using System.Collections.Generic;

namespace Chatapp.Shared.Entities;

public partial class PictureLookup
{
  public int Id { get; set; }

  public int PictureId { get; set; }

  public string MachineName { get; set; } = null!;

  public virtual Picture Picture { get; set; } = null!;
}