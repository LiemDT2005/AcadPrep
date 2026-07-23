using Microsoft.AspNetCore.Mvc;

namespace AcadPrep.WebUI.ViewComponents;

public class AiChatWidgetViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
