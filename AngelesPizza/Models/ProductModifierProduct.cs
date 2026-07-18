namespace AngelesPizza.Models
{
    public class ProductModifierProduct
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int ProductModifierId { get; set; }
        public ProductModifier ProductModifier { get; set; } = null!;
    }
}