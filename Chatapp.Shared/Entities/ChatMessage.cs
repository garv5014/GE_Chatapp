using System;
using System.Collections.Generic;

namespace Chatapp.Shared.Entities;

public partial class ChatMessage
{
    public int Id { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
