using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ClasicConsole.Reports
{
    public interface IAppService
    {

    }
    public interface IWorkingContext
    {
        /// <summary>
        /// Get a CMS user currently executing application code
        /// </summary>
       // CmsUser CurrentUser { get; }

        /// <summary>
        /// Get the current HTTP context 
        /// </summary>
        HttpContext HttpContext { get; }
    }
}
