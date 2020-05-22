//// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
////  Author:                     Joe Audette
////  Created:                    2014-10-13
////	Last Modified:              2015-10-10
//// 

//using Microsoft.AspNet.Mvc;
//using Microsoft.AspNet.Mvc.Razor;
//using Microsoft.AspNet.Razor.Runtime;
//using Microsoft.Framework.OptionsModel;
////using Microsoft.AspNet.Mvc.Razor.OptionDescriptors;
//using System.Collections.Generic;

//namespace Services
//{
//    /// <summary>
//    /// Subclassing the razor view engine here to be able to customize the
//    /// locations and priority where views are searched.
//    /// 
//    /// https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/RazorViewEngine.cs
//    /// {0} represents the name of the view
//    /// {1} represents the name of the controller
//    /// {2} represents the name of the area
//    /// </summary>
//    public class ViewEngine : RazorViewEngine
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pageFactory"></param>
//        /// <param name="viewFactory"></param>
//        /// <param name="optionsAccessor"></param>
//        /// <param name="viewLocationCache"></param>
//        /// <param name="appContext"></param>
//        public ViewEngine(
//            IRazorPageFactory pageFactory,
//            IRazorViewFactory viewFactory,
//            IOptions<RazorViewEngineOptions> optionsAccessor,
//            IViewLocationCache viewLocationCache,
//            WcmsAppContext appContext)
//            : base(pageFactory, viewFactory, optionsAccessor, viewLocationCache)
//        {
//            AppContext = appContext;
//        }

//        private const string ViewExtension = ".cshtml";

//        private static readonly IEnumerable<string> _viewLocationFormats = new[]
//        {
//            "/Views/{1}/{0}" + ViewExtension,
//            "/Views/Shared/{0}" + ViewExtension,
//            "/Views/Sys/{1}/{0}" + ViewExtension,
//            "/Views/Sys/Shared/{0}" + ViewExtension,

//        };

//        private static readonly IEnumerable<string> _areaViewLocationFormats = new[]
//        {
//            "/Areas/{2}/Views/{1}/{0}" + ViewExtension,
//            "/Areas/{2}/Views/Shared/{0}" + ViewExtension,
//            "/Views/Shared/{0}" + ViewExtension,
//            "/Areas/{2}/Views/Sys/{1}/{0}" + ViewExtension,
//            "/Areas/{2}/Views/Sys/Shared/{0}" + ViewExtension,
//            "/Views/Sys/Shared/{0}" + ViewExtension,
//        };

//        /// <summary>
//        /// The application context.
//        /// </summary>
//        public WcmsAppContext AppContext { get; private set; }

//        public override IEnumerable<string> ViewLocationFormats
//        {
//            get { return _viewLocationFormats; }
//        }

//        public override IEnumerable<string> AreaViewLocationFormats
//        {
//            get {
//                if (AppContext != null 
//                    && AppContext.Module.UseArea == true
//                    && AppContext.ModuleController.HaveLayout == true)
//                {
//                    // Use only the area locations... 
//                    return new[]
//                    {
//                        $"/Areas/{AppContext.Module.Name}/Views/{{1}}/{{0}}{ViewExtension}",
//                        $"/Areas/{AppContext.Module.Name}/Views/Shared/{{0}}{ViewExtension}"
//                    };
//                }
//                return _areaViewLocationFormats;
//            }
//        }

//    }
//}
