using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using MEFedMVVM.Services.Contracts;
using MEFedMVVM.ViewModelLocator;
using System.ComponentModel.Composition;
using Intersoft.Client.UI.Navigation;
using System.Diagnostics;
using Intersoft.Client.Framework;
using System.Windows.Browser;
using Website;
using Website.Services;
using System.Threading.Tasks;

namespace ControlPanel
{
    // Application services
    // http://msdn.microsoft.com/en-us/library/dd833084(v=vs.95).aspx

    /// <summary>
    /// App-wide service provider for any global support which is needed.
    /// </summary>
    [Export(typeof(AppSvc))]
    public class AppSvc : IApplicationLifetimeAware, IApplicationService
    {
        private static AppSvc _appSvc = null;
        private readonly Dictionary<Website.StoreKeys, List<SalesSummaryMetric>> salesSummaryData;
        private readonly Dictionary<Website.StoreKeys, List<ManufacturerIdentity>> manufacturersData;

        /// <summary>
        /// Init params passed in from HTML.
        /// </summary>
        public Dictionary<string, string> ApplicationInitParams { get; private set; }

        public static AppSvc Current
        {
            get
            {
                if (_appSvc == null)
                    _appSvc = ViewModelRepository.Instance.Resolver.Container.GetExportedValue<AppSvc>();

                return _appSvc;
            }
        }

        [Import]
        public IMediator Mediator {get;set;}

        public UXFrame PageContentFrame { get; set; }

        public AppSvc()
        {
            salesSummaryData = new Dictionary<Website.StoreKeys, List<SalesSummaryMetric>>();
            manufacturersData = new Dictionary<Website.StoreKeys, List<ManufacturerIdentity>>();

            // load up all the sales data


            RunActionWithDelay(50, () =>
                {
                    try
                    {
                        var ctx = new Website.Services.ControlPanelDomainContext();

                        ctx.GetManufacturerNames(StoreKeys.InsideFabric, (result) =>
                        {
                            if (result.HasError)
                            {
                                result.MarkErrorAsHandled();
                                SetErrorMessage(result.Error.Message);
                                return;
                            }

                            manufacturersData.Add(StoreKeys.InsideFabric, result.Value);

                        }, null);

                        ctx.GetManufacturerNames(StoreKeys.InsideAvenue, (result) =>
                        {
                            if (result.HasError)
                            {
                                result.MarkErrorAsHandled();
                                SetErrorMessage(result.Error.Message);
                                return;
                            }

                            manufacturersData.Add(StoreKeys.InsideAvenue, result.Value);

                        }, null);

                        ctx.GetManufacturerNames(StoreKeys.InsideWallpaper, (result) =>
                        {
                            if (result.HasError)
                            {
                                result.MarkErrorAsHandled();
                                SetErrorMessage(result.Error.Message);
                                return;
                            }

                            manufacturersData.Add(StoreKeys.InsideWallpaper, result.Value);

                        }, null);

                        ctx.GetManufacturerNames(StoreKeys.InsideRugs, (result) =>
                        {
                            if (result.HasError)
                            {
                                result.MarkErrorAsHandled();
                                SetErrorMessage(result.Error.Message);
                                return;
                            }

                            manufacturersData.Add(StoreKeys.InsideRugs, result.Value);

                        }, null);

                        ctx.GetAllSalesSummaryMetrics(StoreKeys.InsideFabric, (result) =>
                        {
                            if (result.HasError)
                            {
                                result.MarkErrorAsHandled();
                                SetErrorMessage(result.Error.Message); 
                                return;
                            }

                            salesSummaryData.Add(StoreKeys.InsideFabric, result.Value);
                                
                        }, null);

                        ctx.GetAllSalesSummaryMetrics(StoreKeys.InsideAvenue, (result) =>
                        {
                            if (result.HasError)
                            {
                                result.MarkErrorAsHandled();
                                SetErrorMessage(result.Error.Message);
                                return;
                            }

                            salesSummaryData.Add(StoreKeys.InsideAvenue, result.Value);

                        }, null);


                        ctx.GetAllSalesSummaryMetrics(StoreKeys.InsideRugs, (result) =>
                        {
                            if (result.HasError)
                            {
                                result.MarkErrorAsHandled();
                                SetErrorMessage(result.Error.Message);
                                return;
                            }

                            salesSummaryData.Add(StoreKeys.InsideRugs, result.Value);

                        }, null);

                        ctx.GetAllSalesSummaryMetrics(StoreKeys.InsideWallpaper, (result) =>
                        {
                            if (result.HasError)
                            {
                                result.MarkErrorAsHandled();
                                SetErrorMessage(result.Error.Message);
                                return;
                            }

                            salesSummaryData.Add(StoreKeys.InsideWallpaper, result.Value);

                        }, null);

                    }
                    catch (Exception Ex)
                    {
                        Debug.WriteLine(Ex.ToString());
                        SetErrorMessage(Ex.Message);
                    }                        

                });

        }

        private void InitializeDesignTimeSessionContext()
        {
        }

        // used to hold a prefetched set of sales metrics
        public List<SalesSummaryMetric> GetSalesSummaryData(Website.StoreKeys storeKey)
        {
            return salesSummaryData[storeKey];
        }

        public List<ManufacturerIdentity> GetManufacturers(Website.StoreKeys storeKey)
        {
            return manufacturersData[storeKey];
        }

        /// <summary>
        /// For internal navigation amongst the silverlight pages.
        /// </summary>
        /// <remarks>
        /// Not intended for launching browsers.
        /// </remarks>
        /// <param name="Url"></param>
        public void NavigatePage(string Url, StoreKeys? storeKey=null)
        {
            if (PageContentFrame != null)
                PageContentFrame.Navigate(new Uri(Url, UriKind.RelativeOrAbsolute), storeKey);
        }


        public void RunActionWithDelay(int DelayMS, Action action)
        {
            if (ISControl.IsInDesignModeStatic)
                return;
            
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object notUsed)
            {
                System.Threading.Thread.Sleep(DelayMS);
                Deployment.Current.Dispatcher.BeginInvoke(action);
            });
        }

        public void SetPageTitle(string title, string subtitle=null)
        {
            Mediator.NotifyColleagues<string>(MediatorMessages.SetPageTitle, title);
            Mediator.NotifyColleagues<string>(MediatorMessages.SetPageSubTitle, subtitle);
        }

        public void SetErrorMessage(string msg)
        {
            Mediator.NotifyColleagues<string>(MediatorMessages.SetErrorMessage, msg);                
        }

        public void ClearErrorMessage()
        {
            Mediator.NotifyColleagues<object>(MediatorMessages.ClearErrorMessage, null);
        }

        #region IApplicationLifetimeAware Members

        public void Exited()
        {
        }

        public void Exiting()
        {
        }

        public void Started()
        {
        }

        public void Starting()
        {
        }

        #endregion

        #region IApplicationService Members

        public void StartService(ApplicationServiceContext context)
        {
            ApplicationInitParams = context.ApplicationInitParams;

            var host = Application.Current.Host;
            host.Content.FullScreenChanged += new EventHandler(Content_FullScreenChanged);
        }


        public void StopService()
        {
        }

        #endregion


        void Content_FullScreenChanged(object sender, EventArgs e)
        {
            var host = Application.Current.Host;
            if (host.Content.IsFullScreen)
                Mediator.NotifyColleagues<object>(MediatorMessages.BeginFullScreenMode, null);
            else
                Mediator.NotifyColleagues<object>(MediatorMessages.EndFullScreenMode, null);
        }

    }


}
