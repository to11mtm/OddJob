using WebSharper.AspNetCore;
using WebSharper.Sitelets;

namespace GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore
{
    public class  OddJobSitelet : ISiteletService
    {
        public Sitelet<object> Sitelet
        {
            get
            {
                return new Server().Main;
            }
        }
    }
}