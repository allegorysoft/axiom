using Allegory.Axiom.EntityFrameworkCore.ModelBuilding;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Allegory.Axiom.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    private const string TenancySideAnnotation = $"{nameof(TenancySideAnnotation)}";
    private const string ShouldCreateIndexesAnnotation = $"{nameof(ShouldCreateIndexesAnnotation)}";

    extension(IMutableModel model)
    {
        public bool ShouldCreateIndexes => model.FindAnnotation(ShouldCreateIndexesAnnotation)?.Value is true;

        public TenancySide TenancySide => model.FindAnnotation(TenancySideAnnotation)?.Value is TenancySide tenancySide
            ? tenancySide
            : TenancySide.Hybrid;
    }

    extension(ModelBuilder builder)
    {
        public bool ShouldCreateIndexes => builder.Model.ShouldCreateIndexes;

        public TenancySide TenancySide => builder.Model.TenancySide;

        public void ConfigureAxiom(DbContext context, bool createIndexes = true)
        {
            builder.Model.SetAnnotation(
                TenancySideAnnotation,
                TenancySideAttribute.Find(context.GetType()) ?? TenancySide.Hybrid);

            builder.Model.SetAnnotation(
                ShouldCreateIndexesAnnotation,
                createIndexes);

            GlobalModelBuilder.Instance.Build(builder, context);
        }
    }
}