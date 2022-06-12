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
using System.Linq;
using MEFedMVVM.ViewModelLocator;
using Intersoft.Client.Framework;
using Intersoft.Client.Framework.Input;
using Intersoft.Client.UI.Aqua;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using MEFedMVVM.Services.CommonServices;
using System.ServiceModel.DomainServices.Client;
using Intersoft.Client.UI.Navigation;
using System.ComponentModel.Composition;
using Website.Services;
using System.Collections.Generic;
using Website;

namespace ControlPanel.ViewModels
{

    public class HomeScreenViewModel : ValidatingViewModelBase
    {
        public class UserPage
        {
            public string Name { get; set; }
            public string Icon { get; set; }
            public string PageTitle { get; set; }
            public string PageSubTitle { get; set; }
            public string Uri { get; set; }
            public StoreKeys? StoreKey { get; set; }
            public UserPage[] Children { get; set; }
        }

        public HomeScreenViewModel()
        {

            if (!ISControl.IsInDesignModeStatic)
            {
                AppService.Mediator.Register(this);
                ErrorMessage = string.Empty;
                PageTitle = string.Empty;
                PageSubTitle = string.Empty;
                LogoutCommand = new DelegateCommand(ExecuteLogoutCommand, CanExecuteLogoutCommand);
                SetPages();
                SelectedPage = null;
            }
            else
            {
                SetDesignTimeData();
            }
        }

        private void SetPages()
        {
            Items = new List<UserPage>
            {
               new UserPage
               {
                   Name="Dashboard",
                   Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_blue.png",
                   Uri = "/Home",
                   PageTitle = "Dashboard",
                   PageSubTitle = "Home Screen",
                   Children = new UserPage[]
                   {

                       new UserPage
                       {
                           Name="Sales by Store",
                           PageTitle = "Dashboard",
                           PageSubTitle = "Sales by Store Summary",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/SalesByStoreSummary",
                       },
                       new UserPage
                       {
                           Name="Combined Sales",
                           PageTitle = "Dashboard",
                           PageSubTitle = "Combined Sales Summary",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/CombinedSalesSummary",
                       },
                       
                     //new UserPage
                     //{
                     //  Name="Cross Marketing",
                     //  Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                     //  Uri = "/Blank",
                     //},
                   },
               },

               //
               // fabric
               //

               new UserPage
               {
                   Name="Inside Fabric",
                   PageTitle = "Inside Fabric",
                   PageSubTitle = "Dashboard",
                   Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_blue.png",
                   Uri = "/InsideFabricStoreDashboard",
                   StoreKey = StoreKeys.InsideFabric,
                   Children = new UserPage[]
                   {
                       new UserPage
                       {
                           Name="Website Page Views",
                           PageTitle = "Inside Fabric",
                           PageSubTitle = "Page Views",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideFabricPageViews",
                           StoreKey = StoreKeys.InsideFabric,
                       },
                       new UserPage
                       {
                           Name="Manufacturer Counts Bar Chart",
                           PageTitle = "Inside Fabric",
                           PageSubTitle = "Products by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideFabricManufacturerCounts",
                           StoreKey = StoreKeys.InsideFabric,
                       },
                       new UserPage
                       {
                           Name="Manufacturer Counts Pie Chart",
                           PageTitle = "Inside Fabric",
                           PageSubTitle = "Products by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideFabricManufacturerCountsPieChart",
                           StoreKey = StoreKeys.InsideFabric,
                       },
                       new UserPage
                       {
                           Name="SQL Search Analytics",
                           PageTitle = "Inside Fabric",
                           PageSubTitle = "SQL Search Analytics",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideFabricSqlSearchMetrics",
                           StoreKey = StoreKeys.InsideFabric,
                       },
                       new UserPage
                       {
                           Name="Sales Summary",
                           PageTitle = "Inside Fabric",
                           PageSubTitle = "Sales Summary",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideFabricSalesSummary",
                           StoreKey = StoreKeys.InsideFabric,
                       },
                       new UserPage
                       {
                           Name="Sales by Manufacturer",
                           PageTitle = "Inside Fabric",
                           PageSubTitle = "Sales by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideFabricSalesByManufacturer",
                           StoreKey = StoreKeys.InsideFabric,
                       },

                       new UserPage
                       {
                           Name="Sales Comparison by Manufacturer",
                           PageTitle = "Inside Fabric",
                           PageSubTitle = "Sales Comparison by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideFabricSalesComparisonByManufacturer",
                           StoreKey = StoreKeys.InsideFabric,
                       },

                        //new UserPage
                        //{
                        //    Name="Cross Marketing",
                        //    Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                        //},
                   },
               },

               //
               // inside avenue
               //

               new UserPage
               {
                   Name="Inside Avenue",
                   Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_blue.png",
                   PageTitle = "Inside Avenue",
                   PageSubTitle = "Dashboard",
                   Uri = "/InsideAvenueStoreDashboard",
                   StoreKey = StoreKeys.InsideAvenue,
                   Children = new UserPage[]
                   {
                       new UserPage
                       {
                           Name="Website Page Views",
                           PageTitle = "Inside Avenue",
                           PageSubTitle = "Page Views",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideAvenuePageViews",
                           StoreKey = StoreKeys.InsideAvenue,
                       },

                       new UserPage
                       {
                           Name="Manufacturer Counts Bar Chart",
                           PageTitle = "Inside Avenue",
                           PageSubTitle = "Products by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideAvenueManufacturerCounts",
                           StoreKey = StoreKeys.InsideAvenue,
                       },
                       new UserPage
                       {
                           Name="Manufacturer Counts Pie Chart",
                           PageTitle = "Inside Avenue",
                           PageSubTitle = "Products by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideAvenueManufacturerCountsPieChart",
                           StoreKey = StoreKeys.InsideAvenue,
                       },
                       new UserPage
                       {
                           Name="SQL Search Analytics",
                           PageTitle = "Inside Avenue",
                           PageSubTitle = "SQL Search Analytics",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideAvenueSqlSearchMetrics",
                           StoreKey = StoreKeys.InsideAvenue,
                       },
                       new UserPage
                       {
                           Name="Sales Summary",
                           PageTitle = "Inside Avenue",
                           PageSubTitle = "Sales Summary",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideAvenueSalesSummary",
                           StoreKey = StoreKeys.InsideAvenue,
                       },
                       // obsolete with new InsideAvenue scanning
                       //new UserPage
                       //{
                       //    Name="Product Upload",
                       //    PageTitle = "Inside Avenue",
                       //    PageSubTitle = "Product Upload",
                       //    Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                       //    Uri = "/InsideAvenueProductUpload",
                       //    StoreKey = StoreKeys.InsideAvenue,
                       //},

                     //new UserPage
                     //{
                     //  Name="Cross Marketing",
                     //  Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                     //},
                   },
               },


               //
               // wallpaper
               //

               new UserPage
               {
                   Name="Inside Wallpaper",
                   Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_blue.png",
                   PageTitle = "Inside Wallpaper",
                   PageSubTitle = "Dashboard",
                   Uri = "/InsideWallpaperStoreDashboard",
                   StoreKey = StoreKeys.InsideWallpaper,
                   Children = new UserPage[]
                   {
                       new UserPage
                       {
                           Name="Website Page Views",
                           PageTitle = "Inside Wallpaper",
                           PageSubTitle = "Page Views",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideWallpaperPageViews",
                           StoreKey = StoreKeys.InsideWallpaper,
                       },

                       new UserPage
                       {
                           Name="Manufacturer Counts Bar Chart",
                           PageTitle = "Inside Wallpaper",
                           PageSubTitle = "Products by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideWallpaperManufacturerCounts",
                           StoreKey = StoreKeys.InsideWallpaper,
                       },
                       new UserPage
                       {
                           Name="Manufacturer Counts Pie Chart",
                           PageTitle = "Inside Wallpaper",
                           PageSubTitle = "Products by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideWallpaperManufacturerCountsPieChart",
                           StoreKey = StoreKeys.InsideWallpaper,
                       },


                       new UserPage
                       {
                           Name="SQL Search Analytics",
                           PageTitle = "Inside Wallpaper",
                           PageSubTitle = "SQL Search Analytics",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideWallpaperSqlSearchMetrics",
                           StoreKey = StoreKeys.InsideWallpaper,
                       },

                       new UserPage
                       {
                           Name="Sales Summary",
                           PageTitle = "Inside Wallpaper",
                           PageSubTitle = "Sales Summary",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideWallpaperSalesSummary",
                           StoreKey = StoreKeys.InsideWallpaper,
                       },

                       new UserPage
                       {
                           Name="Sales by Manufacturer",
                           PageTitle = "Inside Wallpaper",
                           PageSubTitle = "Sales by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideWallpaperSalesByManufacturer",
                           StoreKey = StoreKeys.InsideWallpaper,
                       },

                       new UserPage
                       {
                           Name="Sales Comparison by Manufacturer",
                           PageTitle = "Inside Wallpaper",
                           PageSubTitle = "Sales Comparison by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideWallpaperSalesComparisonByManufacturer",
                           StoreKey = StoreKeys.InsideWallpaper,
                       },


                     //new UserPage
                     //{
                     //  Name="Cross Marketing",
                     //  Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                     //},
                   },
               },

               //
               // rugs
               //

               new UserPage
               {
                   Name="Inside Rugs",
                   Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_blue.png",
                   PageTitle = "Inside Rugs",
                   PageSubTitle = "Dashboard",
                   Uri = "/InsideRugsStoreDashboard",
                   StoreKey = StoreKeys.InsideRugs,
                   Children = new UserPage[]
                   {
                       new UserPage
                       {
                           Name="Website Page Views",
                           PageTitle = "Inside Rugs",
                           PageSubTitle = "Page Views",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideRugsPageViews",
                           StoreKey = StoreKeys.InsideRugs,
                       },
                       new UserPage
                       {
                           Name="Manufacturer Counts Bar Chart",
                           PageTitle = "Inside Rugs",
                           PageSubTitle = "Products by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideRugsManufacturerCounts",
                           StoreKey = StoreKeys.InsideRugs,
                       },
                       new UserPage
                       {
                           Name="Manufacturer Counts Pie Chart",
                           PageTitle = "Inside Rugs",
                           PageSubTitle = "Products by Manufacturer",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideRugsManufacturerCountsPieChart",
                           StoreKey = StoreKeys.InsideRugs,
                       },

                       new UserPage
                       {
                           Name="SQL Search Analytics",
                           PageTitle = "Inside Rugs",
                           PageSubTitle = "SQL Search Analytics",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideRugsSqlSearchMetrics",
                           StoreKey = StoreKeys.InsideRugs,
                       },

                       new UserPage
                       {
                           Name="Sales Summary",
                           PageTitle = "Inside Rugs",
                           PageSubTitle = "Sales Summary",
                           Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                           Uri = "/InsideRugsSalesSummary",
                           StoreKey = StoreKeys.InsideRugs,
                       },

                     //new UserPage
                     //{
                     //  Name="Cross Marketing",
                     //  Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                     //},
                   },
               },
            };
        }

        private void SetDesignTimeData()
        {
            ErrorMessage = "This is a dummy error message.";
            PageTitle = "Inside Fabric";
            PageSubTitle = "Products by Manufacturer";
            Items = new List<UserPage>
            {
               new UserPage
               {
                   Name="First Item",
                   Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_blue.png",
                   Children = new UserPage[]
                   {
                     new UserPage
                     {
                       Name="Sub Item 1",
                       Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                     },
                     new UserPage
                     {
                       Name="Sub Item 2",
                       Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                     }
                   },
               },
               new UserPage
               {
                   Name="Second Item",
                   Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_blue.png",
                   Children = new UserPage[]
                   {
                     new UserPage
                     {
                       Name="Sub Item A",
                       Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                     },
                     new UserPage
                     {
                       Name="Sub Item B",
                       Icon = "/ControlPanel;component/Assets/Images/bullet_ball_glass_yellow.png",
                     }
                   },
               },
            };
        }


        public DelegateCommand LogoutCommand { get; private set; }


        private string errorMessage;

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    RaisePropertyChanged(() => ErrorMessage);
                }
            }
        }

        private string pageTitle;

        public string PageTitle
        {
            get { return pageTitle; }
            set
            {
                if (pageTitle != value)
                {
                    pageTitle = value;
                    RaisePropertyChanged(() => PageTitle);
                }
            }
        }

        private string pageSubTitle;

        public string PageSubTitle
        {
            get { return pageSubTitle; }
            set
            {
                if (pageSubTitle != value)
                {
                    pageSubTitle = value;
                    RaisePropertyChanged(() => PageSubTitle);
                }
            }
        }
        


        private UserPage selectedPage;

        public UserPage SelectedPage
        {
            get { return selectedPage; }
            set
            {
                if (selectedPage != value)
                {
                    selectedPage = value;
                    RaisePropertyChanged(() => SelectedPage);
                    ErrorMessage = string.Empty;
                    if (selectedPage != null)
                    {
                        PageTitle = selectedPage.PageTitle ?? string.Empty;
                        PageSubTitle = selectedPage.PageSubTitle ?? string.Empty;
                        if (selectedPage.Uri != null)
                            AppService.NavigatePage(selectedPage.Uri, selectedPage.StoreKey);
                        else
                            AppService.NavigatePage("/Blank");
                    }
                }
            }
        }
        
        private List<UserPage> items;

        public List<UserPage> Items
        {
            get { return items; }
            set
            {
                if (items != value)
                {
                    items = value;
                    RaisePropertyChanged(() => Items);
                }
            }
        }

        private void SetHomePage()
        {
            SelectedPage = Items.Where(e => e.Uri == "/Home").FirstOrDefault();
        }

        #region Command Actions

        private bool CanExecuteLogoutCommand(object parameter)
        {
            return true;
        }


        private void ExecuteLogoutCommand(object parameter)
        {
            SelectedPage = null;
            AppService.NavigatePage("/Blank");
            AppService.RunActionWithDelay(600, 
                () => AppService.Mediator.NotifyColleagues<object>(MediatorMessages.ApplicationLogout, null));
        }

        #endregion

        [MediatorMessageSink(MediatorMessages.SetPageTitle, ParameterType = typeof(string))]
        public void OnSetPageTitle(string title)
        {
            PageTitle = title;
        }

        [MediatorMessageSink(MediatorMessages.SetPageSubTitle, ParameterType = typeof(string))]
        public void OnSetPageSubTitle(string subtitle)
        {
            PageSubTitle = subtitle;
        }


        [MediatorMessageSink(MediatorMessages.SetErrorMessage, ParameterType = typeof(string))]
        public void OnSetErrorMessage(string msg)
        {
            ErrorMessage = msg;
        }

        [MediatorMessageSink(MediatorMessages.ClearErrorMessage, ParameterType = typeof(object))]
        public void OnClearErrorMessage(object o)
        {
            ErrorMessage = string.Empty;
        }

        [MediatorMessageSink(MediatorMessages.DisplayHomeScreen, ParameterType = typeof(object))]
        public void OnDisplayHomeScreen(object o)
        {
            PageTitle = string.Empty;
            ErrorMessage = string.Empty;
            SetHomePage();
        }

    }
}
