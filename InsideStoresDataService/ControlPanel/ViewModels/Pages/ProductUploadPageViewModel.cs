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
using ControlPanel.Views;
using System.Windows.Threading;
using Website;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Input;


namespace ControlPanel.ViewModels
{


    public class ProductUploadPageViewModel : ValidatingViewModelBase, IViewModelPageNavigation
    {
        ControlPanelDomainContext ctx;

        public ProductUploadPageViewModel()
        {

            if (!ISControl.IsInDesignModeStatic)
            {
                AppService.Mediator.Register(this);
                Init();
            }
            else
            {
                SetDesignTimeData();
            }

        }

        private void Init()
        {
            IsBusy = false;
            IsProcessing = false;
            ErrorMessage = string.Empty;
            IsSuccessful = false;
            IsCompleted = false;
            ProgressMessage = string.Empty;
        }

        private void SetDesignTimeData()
        {
#if DEBUG
            IsBusy = true;
            IsProcessing = true;
            ErrorMessage = "This is my very bad error.";
            IsSuccessful = true;
            IsCompleted = true;
            ProgressMessage = "Uploading...";
#endif
        }

        public void SetRadUpload(Telerik.Windows.Controls.RadUpload ctrl )
        {
            RadUpload1 = ctrl;
            RadUpload1.MaxFileSize = (long)1024000 * 1024; // 1GB
            // must hook methods
            RadUpload1.FileTooLarge += new FileTooLargeEventHandler(radUpload_FileTooLarge);
            RadUpload1.FileUploadFailed += new FileUploadFailedEventHandler(radUpload_FileUploadFailed);
            RadUpload1.FileUploaded += new FilesUploadedEventHandler(radUpload_FileUploaded);
            RadUpload1.ProgressChanged += new RoutedEventHandler(radUpload_ProgressChanged);
            RadUpload1.FileUploadStarting += new EventHandler<FileUploadStartingEventArgs>(radUpload_FileUploadStarting);
            RadUpload1.UploadCanceled += RadUpload1_UploadCanceled;
        }

        void RadUpload1_UploadCanceled(object sender, RoutedEventArgs e)
        {
            EndUpload(false, "Operation cancelled.");
        }

        private void EndUpload(bool isSuccess, string msg=null)
        {
            IsProcessing = false;
            //RadUpload1.Foreground = new SolidColorBrush("#FF959595".ToColor());

            if (IsBusy)
            {
                //UploadMessage = "Finished";
                AppSvc.Current.RunActionWithDelay(1000, () =>
                {
                    IsBusy = false;
                });
            }

            if (isSuccess)
            {
                IsSuccessful = true;
                ErrorMessage = string.Empty;

                if (!string.IsNullOrWhiteSpace(msg))
                    ProgressMessage = msg;
                else
                    ProgressMessage = "Operation completed successfully.";
                //SetStatusImage(StatusState.Success);
            }
            else
            {
                ProgressMessage = string.Empty;
                if (!string.IsNullOrWhiteSpace(msg))
                    ErrorMessage = msg;

                //SetStatusImage(StatusState.Warning);
            }
        }

        void radUpload_FileUploadStarting(object sender, FileUploadStartingEventArgs e)
        {
            IsBusy = true;
            IsSuccessful = false;
            IsProcessing = false;
            ProgressMessage = "Uploading file...";
            ErrorMessage = string.Empty;
            //RadUpload1.Foreground = new SolidColorBrush("#FF202020".ToColor());

        }

        void radUpload_FileUploaded(object sender, FileUploadedEventArgs e)
        {
            if (!IsBusy)
                return;

            // RadUploadSelectedFile uploadedFile = e.SelectedFile;
            var props = e.HandlerData.CustomData;

            var isValid = bool.Parse(props["IsValid"].ToString());
            var filename = props["Filename"].ToString();

            if (isValid)
            {
                IsProcessing = true;
                ProgressMessage = "Processing images...";
                BeginProcessingImages(filename);
            }
            else
            {
                if (props.ContainsKey("Exception"))
                {
                    var exceptionMsg = props["Exception"].ToString();
                    EndUpload(false, exceptionMsg);
                }
                else
                {
                    EndUpload(false, "Error processing file.");
                }
            }
        }

        void radUpload_ProgressChanged(object sender, RoutedEventArgs e)
        {
            //var upload = sender as RadUpload;

            //if (upload != null && upload.CurrentSession != null)
            //{
            //    var pct = upload.CurrentSession.CurrentFileProgress;
            //}
        }

        void radUpload_FileUploadFailed(object sender, FileUploadFailedEventArgs e)
        {
            EndUpload(false, "Operation failed.");
        }

        void radUpload_FileTooLarge(object sender, FileEventArgs e)
        {
            EndUpload(false, "File is too large.");
        }

        private void BeginProcessingImages(string filename)
        {
            ctx.ProcessProductUpload(StoreKey, filename, (result) =>
            {
                if (result.HasError)
                {
                    result.MarkErrorAsHandled();
                    AppService.SetErrorMessage(result.Error.Message);
                    EndUpload(false); 
                    return;
                }

                var text = result.Value;


                if (text.StartsWith("OK:"))
                {
                    var ary = text.Split(':');
                    EndUpload(true, string.Format("Operation completed successfully. {0} images processed.", ary[1]));
                }
                else if (text.StartsWith("OK"))
                {
                    EndUpload(true); 
                }
                else
                {
                    EndUpload(false, text);
                }
                
            }, null);            
        }


        private Website.StoreKeys storeKey;

        public Website.StoreKeys StoreKey
        {
            get { return storeKey; }
            set
            {
                if (storeKey != value)
                {
                    storeKey = value;
                    RaisePropertyChanged(() => StoreKey);
                }
            }
        }

        private bool isProcessing;

        public bool IsProcessing
        {
            get { return isProcessing; }
            set
            {
                if (isProcessing != value)
                {
                    isProcessing = value;
                    RaisePropertyChanged(() => IsProcessing);
                }
            }
        }
        
        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    RaisePropertyChanged(() => IsBusy);
                }
            }
        }

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

        private string progressMessage;

        public string ProgressMessage
        {
            get { return progressMessage; }
            set
            {
                if (progressMessage != value)
                {
                    progressMessage = value;
                    RaisePropertyChanged(() => ProgressMessage);
                }
            }
        }

        private bool isCompleted;

        public bool IsCompleted
        {
            get { return isCompleted; }
            set
            {
                if (isCompleted != value)
                {
                    isCompleted = value;
                    RaisePropertyChanged(() => IsCompleted);
                }
            }
        }

        private bool isSuccessful;

        public bool IsSuccessful
        {
            get { return isSuccessful; }
            set
            {
                if (isSuccessful != value)
                {
                    isSuccessful = value;
                    RaisePropertyChanged(() => IsSuccessful);
                }
            }
        }

        private Telerik.Windows.Controls.RadUpload radUpload1;

        public Telerik.Windows.Controls.RadUpload RadUpload1
        {
            get { return radUpload1; }
            set
            {
                if (radUpload1 != value)
                {
                    radUpload1 = value;
                    RaisePropertyChanged(() => RadUpload1);
                }
            }
        }
        
        #region Command Actions


        #endregion



        public void OnNavigatedTo(UXViewPage page, NavigationEventArgs e)
        {
            try
            {
                var key = (StoreKeys?)e.ExtraData;
                StoreKey = key.Value;

                ctx = new Website.Services.ControlPanelDomainContext();

            }
            catch (Exception Ex)
            {
                AppService.SetErrorMessage(Ex.Message);
            }
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ctx = null;
        }

    }
}
