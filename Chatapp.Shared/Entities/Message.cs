namespace Chatapp.Shared.Entities;

public partial class Message
{
  public int Id { get; set; }

  public string MessageText { get; set; } = null!;

  public string Username { get; set; } = null!;

  public DateTime CreatedAt { get; set; }

  public string? Clientid { get; set; }

  public string? VectorDict { get; set; }

  public int? EventCount { get; set; }

  public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();
}