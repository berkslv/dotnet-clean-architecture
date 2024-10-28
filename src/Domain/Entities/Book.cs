using Domain.Entities.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Book : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    public BookCover Cover { get; set; }
    
    public Guid AuthorId { get; set; }
    
    public Author Author { get; set; } = new();
    
    public IList<Category> Categories { get; set; } = new List<Category>();
}