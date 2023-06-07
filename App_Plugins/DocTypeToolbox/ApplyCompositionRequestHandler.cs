using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;

namespace UmbracoDocTypeToolbox.App_Plugins.DocTypeToolbox;

public class ApplyCompositionRequestHandler
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IScopeProvider _scopeProvider;
    private readonly AppCaches _appCaches;

    public ApplyCompositionRequestHandler(
        IContentTypeService contentTypeService,
        IScopeProvider scopeProvider,
        AppCaches appCaches
        )
    {
        _contentTypeService = contentTypeService;
        _scopeProvider = scopeProvider;
        _appCaches = appCaches;
    }

    public void ApplyComposition(int contentTypeId, int compositionContentTypeId)
    {
        var contentType = _contentTypeService.Get(contentTypeId)!;
        var composition = _contentTypeService.Get(compositionContentTypeId)!;

        var progress = new List<CompositionPropertyProgress>();

        foreach (var property in composition.PropertyTypes)
        {
            
            var progressItem = new CompositionPropertyProgress()
            {
                Alias = property.Alias,
                CompositionPropertyId = property.Id
            };

            var propertyWithSameAliasOnContentType = contentType.PropertyTypes.FirstOrDefault(x => x.Alias.Equals(property.Alias));
            if (propertyWithSameAliasOnContentType != null)
            {
                progressItem.ContentTypePropertyId = propertyWithSameAliasOnContentType.Id;
            }

            progress.Add(progressItem);
        }

        // If there is no overlap we can just apply the composition
        if (!progress.Any(x => x.ContentTypePropertyId.HasValue))
        {
            contentType.AddContentType(composition);
            _contentTypeService.Save(contentType);

            return;
        }

        var propertiesToMoveData = progress.Where(x => x.ContentTypePropertyId.HasValue).ToList();

        using (var scope = _scopeProvider.CreateScope())
        {
            // Change data-connection from the "old" property on the contentType
            // to the new property on the composition.
            foreach (var property in propertiesToMoveData)
            {
                scope.Database.ExecuteScalar<int>($"UPDATE [umbracoPropertyData] SET [propertyTypeId] = {property.CompositionPropertyId} WHERE [propertyTypeId] = {property.ContentTypePropertyId!.Value}");
            }

            // Delete the properties from the content type.
            foreach (var property in propertiesToMoveData)
            {
                scope.Database.ExecuteScalar<int>($"DELETE FROM [cmsPropertyType] WHERE id = {property.ContentTypePropertyId}");
            }

            // Apply the composition
            scope.Database.ExecuteScalar<int>($"INSERT INTO [cmsContentType2ContentType] (parentContentTypeId,childContentTypeId) VALUES({composition.Id},{contentType.Id})");

            scope.Complete();

        }

        // This will reset the caches for DocumentTypes etc.
        _appCaches.IsolatedCaches.ClearAllCaches();

        // Reset runtime cache just to be safe
        _appCaches.RuntimeCache.Clear();

    }

    class CompositionPropertyProgress
    {
        public int CompositionPropertyId { get; set; }
        public int? ContentTypePropertyId { get; set; }
        public string Alias { get; set; }
    }
}