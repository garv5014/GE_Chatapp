using System;
using System.Collections.Generic;

namespace Chatapp.Shared.Entities;

public partial class Message
{
  public int Id { get; set; }

  public string MessageText { get; set; } = null!;

  public string Username { get; set; } = null!;

  public DateTime CreatedAt { get; set; }

  public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();
}