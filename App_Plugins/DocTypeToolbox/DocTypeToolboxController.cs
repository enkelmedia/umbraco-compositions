using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.Attributes;

namespace UmbracoDocTypeToolbox.App_Plugins.DocTypeToolbox
{
    [PluginController("DocTypeToolbox")]
    [JsonCamelCaseFormatter]
    public class DocTypeToolboxController : UmbracoAuthorizedApiController
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly ApplyCompositionRequestHandler _applyCompositionHandler;

        public DocTypeToolboxController(
            IContentTypeService contentTypeService,
            ApplyCompositionRequestHandler applyCompositionHandler
            )
        {
            _contentTypeService = contentTypeService;
            _applyCompositionHandler = applyCompositionHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id of the current content type</param>
        /// <returns></returns>
        public ActionResult<List<ContentTypeCompositionDto>> GetCompositions(string id)
        {
            var currentContentType = _contentTypeService.Get(int.Parse(id))!;

            var list = new List<ContentTypeCompositionDto>();

            var compositionTypes = _contentTypeService.GetAllElementTypes();
            foreach (var item in compositionTypes)
            {
                list.Add(new ContentTypeCompositionDto()
                {
                    Alias = item.Alias,
                    Enabled = currentContentType.ContentTypeComposition.Any(x=>x.Id == item.Id) == false,
                    Id = item.Id.ToString(),
                    Name = item.Name!
                });
            }
            
            return list;

        }

        [HttpPost]
        public ActionResult ApplyComposition(ContentTypeApplyCompositionDto model)
        {
            
            _applyCompositionHandler.ApplyComposition(int.Parse(model.ContentTypeId),int.Parse(model.CompositionId));

            return Ok();
        }
    }

    public class ContentTypeCompositionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool Enabled { get; set; }

    }

    public class ContentTypeApplyCompositionDto
    {
        public string ContentTypeId { get; set; }
        public string CompositionId { get; set; }
        
    }


}
