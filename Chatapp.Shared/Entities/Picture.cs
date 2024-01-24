namespace Chatapp.Shared.Entities;

public partial class Picture
{
  public int Id { get; set; }

  public int BelongsTo { get; set; }

  public string NameOfFile { get; set; } = null!;

  public virtual Message BelongsToNavigation { get; set; } = null!;
}