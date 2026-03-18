
namespace Lumen.Domain
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;

        // Navigation
        public ICollection<Photo> Photos { get; set; } = [];
    }
}
