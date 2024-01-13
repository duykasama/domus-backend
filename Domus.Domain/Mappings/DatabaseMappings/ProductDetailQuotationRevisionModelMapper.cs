using Domus.Domain.Entities;
using Domus.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domus.Domain.Mappings.DatabaseMappings;

public class ProductDetailQuotationRevisionModelMapper : IDatabaseModelMapper
{
    public void Map(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductDetailQuotationRevision>(entity =>
        {
            entity.ToTable("ProductDetail_QuotationRevision");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Version).HasDefaultValueSql("((0))");

            entity.HasOne(d => d.ProductDetailQuotation).WithMany(p => p.ProductDetailQuotationRevisions)
                .HasForeignKey(d => d.ProductDetailQuotationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductDe__Produ__5FB337D6");
        });
    }
}