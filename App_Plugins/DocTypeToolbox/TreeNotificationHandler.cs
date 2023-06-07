using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace UmbracoDocTypeToolbox.App_Plugins.DocTypeToolbox;

public class TreeNotificationHandler : INotificationHandler<MenuRenderingNotification>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentTypeService _contentTypeService;

    public TreeNotificationHandler(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeService contentTypeService
        )
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentTypeService = contentTypeService;
    }

    public void Handle(MenuRenderingNotification notification)
    {
        // this example will add a custom menu item for all admin users

        // for all content tree nodes
        if (notification.TreeAlias.Equals("documentTypes") && _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsAdmin())
        {
            if (!int.TryParse(notification.NodeId, out int contentTypeId))
                return;

            var contentType = _contentTypeService.Get(contentTypeId);

            if (contentType == null || contentType.Alias == "Folder")
                return;

            // Creates a menu action that will open /umbraco/currentSection/itemAlias.html
            var menuItem = new Umbraco.Cms.Core.Models.Trees.MenuItem("applyComposition", "Apply Composition");

            // optional, if you don't want to follow the naming conventions, but do want to use a angular view
            // you can also use a direct path "../App_Plugins/my/long/url/to/view.html"
            menuItem.AdditionalData.Add("actionView", "/App_Plugins/DocTypeToolbox/menu-action.html");

            // sets the icon to icon-wine-glass
            menuItem.Icon = "merge";

            // insert at index 5
            notification.Menu.Items.Insert(5, menuItem);
        }
    }
}