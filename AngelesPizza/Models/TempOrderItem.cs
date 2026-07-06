namespace AngelesPizza.Models
{
    public class TempOrderItem
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Price { get; set; }

        public string? Notes { get; set; }

        public List<int> ModifierIds { get; set; } = new();

        public List<string> ModifierNames { get; set; } = new();

        public int ExtraCost { get; set; }

        public int Total => Price + ExtraCost;
    }
}