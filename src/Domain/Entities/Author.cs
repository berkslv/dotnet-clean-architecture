using Domain.Entities.Common;

namespace Domain.Entities;

public class Author : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public DateTime Birth { get; set; }
    
    public DateTime? Death { get; set; }

    public IList<Book> Books { get; set; } = new List<Book>();
}