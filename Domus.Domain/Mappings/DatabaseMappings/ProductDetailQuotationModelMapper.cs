using Domus.Domain.Entities;
using Domus.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domus.Domain.Mappings.DatabaseMappings;

public class ProductDetailQuotationModelMapper : IDatabaseModelMapper
{
    public void Map(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductDetailQuotation>(entity =>
        {
            entity.ToTable("ProductDetail_Quotation");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.MonetaryUnit).HasMaxLength(256);
            entity.Property(e => e.QuantityType).HasMaxLength(256);

            entity.HasOne(d => d.ProductDetail).WithMany(p => p.ProductDetailQuotations)
                .HasForeignKey(d => d.ProductDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductDe__Produ__5EBF139D");

            entity.HasOne(d => d.Quotation).WithMany(p => p.ProductDetailQuotations)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductDe__Quota__5DCAEF64");
        });
    }
}