using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Page api.
    /// </summary>
    [Authorize]
    [Route("api/page")]
    public class PageApiController : BaseController
    {
        /// <summary>
        /// Page provider.
        /// </summary>
        private PageProvider provider { get; set; }

        /// <summary>
        /// The Page api controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public PageApiController(Services.WcmsAppContext appContext)
            : base(appContext)
        {
            provider = new PageProvider(AppContext);
        }

        /// <summary>
        /// GET: api/page
        /// GET: api/page/list
        /// Get site pages.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [HttpGet("list")]
        public async Task<List<JsonPage>> Get()
        {
            try
            {
                IEnumerable<Page> pages = await provider?.Get(false, null, true);
                return (pages == null)
                    ? null
                    : _ToJsonPageList(pages, new List<JsonPage>());
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception while getting pages - HttpGet:/api/page: {0}", e.Message);
                return null;
            }
        }

        /// <summary>
        /// GET: api/page/{id}
        /// Get the page specified by the id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<JsonPage> Get(int id)
        {
            try
            {
                Page page = await provider?.Get(id);
                return (page == null)
                    ? null
                    : new JsonPage(page);
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception while getting page {0} - HttpGet:/api/page/<id>: {1}", id, e.Message);
                return null;
            }
        }

        /// <summary>
        /// PUT: api/page
        /// Add\update an existing page.
        /// </summary>
        /// <param name="page"></param>
        [Authorize(ClaimValueRole.Administrator)]
        [Authorize(ClaimValueRole.Publicator)]
        [Authorize(ClaimValueRole.Contributor)]
        [HttpPut]
        public async Task<int> Put([FromBody]JsonPage page)
        {
            try
            {
                return 0;
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception while adding\\updating page {0} - HttpPut:/api/page: {1}", page?.Id ?? -1, e.Message);
                return 0;
            }
        }

        /// <summary>
        /// DELETE: api/page/{id}
        /// Delete an existing page.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(ClaimValueRole.Administrator)]
        [Authorize(ClaimValueRole.Publicator)]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(int id)
        {
            try
            {
                return false;
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception while deleting page {0} - HttpDelete:/api/page/<id>: {1}", id, e.Message);
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="pages"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<JsonPage> _ToJsonPageList(IEnumerable<Page> pages, List<JsonPage> list)
        {
            if (pages != null)
            {
                foreach (Page page in pages)
                {
                    JsonPage jsonPg = new JsonPage(page);
                    list.Add(jsonPg);
                }
            }
            return list;
        }
    }
}
