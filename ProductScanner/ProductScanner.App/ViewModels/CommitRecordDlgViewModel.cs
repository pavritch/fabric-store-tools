using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using InsideFabric.Data;

namespace ProductScanner.App.ViewModels
{
    #region CommitRecordImageViewModel Class
    public class CommitRecordImageViewModel : ViewModelBase
    {
        private long rawImageSizeBytes;
        private int imageWidth;
        private int imageHeight;

        public CommitRecordImageViewModel(ProductImage productImage)
        {

            IsDefault = string.Format("Default: {0}", productImage.IsDefault ? "Yes" : "No");
            ImageUrl = productImage.SourceUrl;
            Filename = productImage.Filename;
            ImageVariant = "Variant: " + productImage.ImageVariant;
            DisplayOrder = string.Format("Display Order: {0}", productImage.DisplayOrder);

            Dimensions = "Dimensions: Loading...";
            ImageSize = "Size: Loading...";

            if (!IsInDesignMode)
            {
                BrowseImageCommand.RaiseCanExecuteChanged();

                Task.Run(async () =>
                {
                    var src = await LoadImageSourceAsync(ImageUrl);
                    await DispatcherHelper.RunAsync(() =>
                    {
                        ImageSource = src;
                    });
                });
            }
        }


        private string _imageUrl = null;
        public string ImageUrl
        {
            get
            {
                return _imageUrl;
            }
            set
            {
                Set(() => ImageUrl, ref _imageUrl, value);
            }
        }

        private string _isDefault = null;
        public string IsDefault
        {
            get
            {
                return _isDefault;
            }
            set
            {
                Set(() => IsDefault, ref _isDefault, value);
            }
        }

        private string _displayOrder = null;
        public string DisplayOrder
        {
            get
            {
                return _displayOrder;
            }
            set
            {
                Set(() => DisplayOrder, ref _displayOrder, value);
            }
        }

        private string _filename = null;
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                Set(() => Filename, ref _filename, value);
            }
        }

        private string _dimensions = null;
        public string Dimensions
        {
            get
            {
                return _dimensions;
            }
            set
            {
                Set(() => Dimensions, ref _dimensions, value);
            }
        }

        private string _imageSize = null;
        public string ImageSize
        {
            get
            {
                return _imageSize;
            }
            set
            {
                Set(() => ImageSize, ref _imageSize, value);
            }
        }

        private string _imageVariant = null;
        public string ImageVariant
        {
            get
            {
                return _imageVariant;
            }
            set
            {
                Set(() => ImageVariant, ref _imageVariant, value);
            }
        }

        private ImageSource _imageSource = null;
        public ImageSource ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                Set(() => ImageSource, ref _imageSource, value);
                if (value != null)
                {
                    Dimensions = string.Format("Dimensions: {0:N0} x {1:N0}", imageWidth, imageHeight);
                    ImageSize = string.Format("Size: {0}", rawImageSizeBytes.ToFileSize());
                    HasImage = true;
                }
                else
                {
                    Dimensions = "Dimensions: Unknown";
                    ImageSize = "Size: Unknown";
                    HasImage = false;
                }
            }
        }


        private bool _hasImage = true;
        public bool HasImage
        {
            get
            {
                return _hasImage;
            }
            set
            {
                Set(() => HasImage, ref _hasImage, value);
            }
        }

        private RelayCommand _browseImageCommand;
        public RelayCommand BrowseImageCommand
        {
            get
            {
                return _browseImageCommand
                    ?? (_browseImageCommand = new RelayCommand(
                    () =>
                    {
                        if (!BrowseImageCommand.CanExecute(null))
                        {
                            return;
                        }

                        MessengerInstance.Send(new RequestLaunchBrowser(ImageUrl));
                    },
                    () => !string.IsNullOrWhiteSpace(ImageUrl)));
            }
        }

        private async Task<ImageSource> LoadImageSourceAsync(string address)
        {
            ImageSource imgSource = null;

            try
            {
                MemoryStream ms = new MemoryStream(await new WebClient().DownloadDataTaskAsync(new Uri(address, UriKind.RelativeOrAbsolute)));
                rawImageSizeBytes = ms.Length;

                using (var img = System.Drawing.Image.FromStream(ms))
                {
                    imageWidth = img.Width;
                    imageHeight = img.Height;
                }
                ms.Seek(0, SeekOrigin.Begin);
                ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
                imgSource = (ImageSource)imageSourceConverter.ConvertFrom(ms);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return imgSource;
        }



    } 
    #endregion

    public class CommitRecordDlgViewModel : ViewModelBase
    {

#if DEBUG
        public CommitRecordDlgViewModel()
        {
            // will only be called in design mode
            if (IsInDesignMode)
            {
                var r = new CommitRecordDetails
                {
                    Title = "DL-42456-451",
                    FullName = "42456-451 PAPAYA BY DURALEE",
                    ImageUrl = "http://www.insidefabric.com/images/product/large/037655-ashworth-oak-by-robert-allen.jpg", // "http://www.insidefabric.com/images/product/large/42456-451-papaya-by-duralee.jpg",
                    VendorUrl = "http://www.insidestores.com",
                    StoreUrl = "http://www.insidefabric.com/p-1073112-42456-451-papaya-by-duralee.aspx",
                    ProductImages = new List<ProductImage>()
                        {
                            new ProductImage()
                            {
                                DisplayOrder = 0,
                                Filename = "1106898-verena-plaid-si-423-verena-plaid-sienna-by-lee-jofa.jpg",
                                ImageVariant = "Primary",
                                IsDefault = true,
                                ProcessingInstructions = null,
                                SourceUrl = "http://www.insidefabric.com/images/product/large/FS-173277.jpg",
                            },

                            new ProductImage()
                            {
                                DisplayOrder = 1,
                                Filename = "1055254-34354-20-belgian-linen-smoke-by-clarence-house.jpg",
                                ImageVariant = "Alternate",
                                IsDefault = false,
                                ProcessingInstructions = null,
                                SourceUrl = "http://www.insidefabric.com/images/product/large/verena-plaid-si-423-verena-plaid-sienna-by-lee-jofa.jpg",
                            },
                        },
                        ProductVariants = null,
                    JSON = "This is my fake JSON.\nIs this on another line?\nI hope so.",
                };

                Record = r;

                var list = r.ProductImages.Select(e => new CommitRecordImageViewModel(e)).ToList();
                Images = new ObservableCollection<CommitRecordImageViewModel>(list);
                HasAnyImages = true;
            }
        }
#endif

        public CommitRecordDlgViewModel(ICommitRecordDetails record)
        {
            Record = record;

            if (record.ProductImages != null)
            {
                var list = record.ProductImages.Select(e => new CommitRecordImageViewModel(e)).ToList();
                Images = new ObservableCollection<CommitRecordImageViewModel>(list);
                HasAnyImages = record.ProductImages.Count() > 0;
            }
        }

        private void InvalidateCommands()
        {
            if (Record != null)
            {
                HasProductImage = !string.IsNullOrWhiteSpace(Record.ImageUrl);
                ShowImagesTab = Record.ProductImages != null;
                ShowVariantsTab = Record.ProductVariants != null;
            }

            BrowseStoreUrlCommand.RaiseCanExecuteChanged();
            BrowseVendorUrlCommand.RaiseCanExecuteChanged();
        }

        private ICommitRecordDetails _record = null;
        public ICommitRecordDetails Record
        {
            get
            {
                return _record;
            }
            set
            {
                Set(() => Record, ref _record, value);

                if (!IsInDesignMode)
                    InvalidateCommands();
            }
        }

        // if any at all - so can know to show different UX screen
        private bool _hasAnyImages = false;
        public bool HasAnyImages
        {
            get
            {
                return _hasAnyImages;
            }
            set
            {
                Set(() => HasAnyImages, ref _hasAnyImages, value);
            }
        }

        private ObservableCollection<CommitRecordImageViewModel> _images = null;
        public ObservableCollection<CommitRecordImageViewModel> Images
        {
            get
            {
                return _images;
            }
            set
            {
                Set(() => Images, ref _images, value);
            }
        }

        private bool _showImagesTab = false;
        public bool ShowImagesTab
        {
            get
            {
                return _showImagesTab;
            }
            set
            {
                Set(() => ShowImagesTab, ref _showImagesTab, value);
            }
        }


        private bool _showVariantsTab = false;
        public bool ShowVariantsTab
        {
            get
            {
                return _showVariantsTab;
            }
            set
            {
                Set(() => ShowVariantsTab, ref _showVariantsTab, value);
            }
        }

        private bool _hasProductImage = false;
        public bool HasProductImage
        {
            get
            {
                return _hasProductImage;
            }
            set
            {
                Set(() => HasProductImage, ref _hasProductImage, value);
            }
        }

        private RelayCommand _browseStoreUrlCommand;

        /// <summary>
        /// Gets the BrowseStoreUrlCommand.
        /// </summary>
        public RelayCommand BrowseStoreUrlCommand
        {
            get
            {
                return _browseStoreUrlCommand
                    ?? (_browseStoreUrlCommand = new RelayCommand(
                    () =>
                    {
                        if (!BrowseStoreUrlCommand.CanExecute(null))
                        {
                            return;
                        }

                        MessengerInstance.Send(new RequestLaunchBrowser(Record.StoreUrl));
                    },
                    () => !string.IsNullOrEmpty(Record.StoreUrl)));
            }
        }

        private RelayCommand _browseVendorUrlCommand;

        /// <summary>
        /// Gets the BrowseVendorUrlCommand.
        /// </summary>
        public RelayCommand BrowseVendorUrlCommand
        {
            get
            {
                return _browseVendorUrlCommand
                    ?? (_browseVendorUrlCommand = new RelayCommand(
                    () =>
                    {
                        if (!BrowseVendorUrlCommand.CanExecute(null))
                        {
                            return;
                        }

                        MessengerInstance.Send(new RequestLaunchBrowser(Record.VendorUrl));
                        
                    },
                    () => !string.IsNullOrEmpty(Record.VendorUrl)));
            }
        }
    }
}