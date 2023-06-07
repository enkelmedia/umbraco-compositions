using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;

namespace UmbracoDocTypeToolbox.App_Plugins.DocTypeToolbox;

public class Composer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<MenuRenderingNotification, TreeNotificationHandler>();

        builder.Services.AddTransient<ApplyCompositionRequestHandler>();
    }
}