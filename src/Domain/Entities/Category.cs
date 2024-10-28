using Domain.Entities.Common;

namespace Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public IList<Book> Books { get; set; } = new List<Book>();
}