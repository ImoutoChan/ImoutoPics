namespace ImoutoPicsBot.Data.Models;

public class Media
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string Md5 { get; set; } = default!;
    
    public bool IsPosted { get; set; }
    
    public long? PostedChat { get; set; }
    
    public int? PostedMessageId { get; set; }
    
    public DateTimeOffset AddedOn { get; set; } 
    
    public DateTimeOffset? PostedOn { get; set; } 
}
