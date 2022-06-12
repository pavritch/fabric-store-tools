using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.App.ViewModels
{
  public class VendorPropertiesViewModel : VendorContentPageViewModel
  {

#if DEBUG
        public VendorPropertiesViewModel()
            : this(new DesignVendorModel() { Name = "Kravet", VendorId = 5 })
        {

        }
#endif
    public VendorPropertiesViewModel(IVendorModel vendor)
        : base(vendor)
    {
      PageType = ContentPageTypes.VendorProperties;
      PageSubTitle = "Properties";
      BreadcrumbTemplate = "{Home}/{Store}/{Vendor}/Properties";
      IsNavigationJumpTarget = true;

      var export = new VendorExportRecord(vendor);
      TextProperties = export.ToString();

      if (!IsInDesignMode)
      {
        Task.Run(async () =>
        {
                  // need to populate the value in the UI thread
                  var props = await Vendor.GetVendorPropertiesAsync();
          await DispatcherHelper.RunAsync(() =>
                  {
              VendorProperties = props;
            });
        });
      }
      else
      {
        VendorProperties = new VendorProperties()
        {
          Name = "My Vendor Name",
          Username = "dddddd",
          Password = "Mdddd$Password",
          Status = VendorStatus.AutoPilot
        };
        IsSaved = true;
      }
    }

    #region Local Methods


    private async void SaveProperties()
    {
      // very loose error checking here 

      IsSaving = true;
      var result = await Vendor.SaveVendorPropertiesAsync(VendorProperties);
      IsSaving = false;
      IsSaved = result;
    }

    private void InvalidateButtons()
    {
      IsValid = AreInputsValid();
      SaveCommand.RaiseCanExecuteChanged();
      ManualButtonCommand.RaiseCanExecuteChanged();
      AutopilotButtonCommand.RaiseCanExecuteChanged();
      DisabledButtonCommand.RaiseCanExecuteChanged();
    }

    private bool AreInputsValid()
    {
      if (string.IsNullOrWhiteSpace(VendorProperties.Name))
        return false;

      if (string.IsNullOrWhiteSpace(VendorProperties.Username))
        return false;

      if (string.IsNullOrWhiteSpace(VendorProperties.Password))
        return false;

      return true;
    }

    private void ChangeStatusSetting(VendorStatus newStatus)
    {
      switch (newStatus)
      {
        case VendorStatus.Manual:
          IsManualStatus = true;
          IsAutopilotStatus = false;
          IsDisabledStatus = false;
          break;

        case VendorStatus.AutoPilot:
          IsManualStatus = false;
          IsAutopilotStatus = true;
          IsDisabledStatus = false;
          break;

        default:
          IsManualStatus = false;
          IsAutopilotStatus = false;
          IsDisabledStatus = true;
          break;
      }
      VendorProperties.Status = newStatus;
    }


    void VendorProperties_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      IsSaved = false;
      InvalidateButtons();
    }

    #endregion

    #region Public Properties

    private VendorProperties _vendorPropertes = null;
    public VendorProperties VendorProperties
    {
      get
      {
        return _vendorPropertes;
      }
      set
      {
        if (Set(() => VendorProperties, ref _vendorPropertes, value))
        {
          var old = _vendorPropertes;

          if (old != null)
            old.PropertyChanged -= VendorProperties_PropertyChanged;

          if (value != null)
            VendorProperties.PropertyChanged += VendorProperties_PropertyChanged;
        }

        if (value != null)
          ChangeStatusSetting(value.Status);

        InvalidateButtons();
      }
    }

    private bool _isManualStatus = false;
    public bool IsManualStatus
    {
      get
      {
        return _isManualStatus;
      }
      set
      {
        Set(() => IsManualStatus, ref _isManualStatus, value);
      }
    }

    private bool _isAutopilotStatus = false;
    public bool IsAutopilotStatus
    {
      get
      {
        return _isAutopilotStatus;
      }
      set
      {
        Set(() => IsAutopilotStatus, ref _isAutopilotStatus, value);
      }
    }

    private bool _isDisabledStatus = false;
    public bool IsDisabledStatus
    {
      get
      {
        return _isDisabledStatus;
      }
      set
      {
        Set(() => IsDisabledStatus, ref _isDisabledStatus, value);
      }
    }

    /// <summary>
    /// Triggers the saved state in UX. (show icon)
    /// </summary>
    private bool _isSaved = false;
    public bool IsSaved
    {
      get
      {
        return _isSaved;
      }
      set
      {
        Set(() => IsSaved, ref _isSaved, value);
      }
    }

    private bool _isSaving = false;
    public bool IsSaving
    {
      get
      {
        return _isSaving;
      }
      set
      {
        Set(() => IsSaving, ref _isSaving, value);
      }
    }

    /// <summary>
    /// Are user inputs valid.
    /// </summary>
    private bool _isValid = false;
    public bool IsValid
    {
      get
      {
        return _isValid;
      }
      set
      {
        Set(() => IsValid, ref _isValid, value);
      }
    }

    private string _textProperties = string.Empty;
    public string TextProperties
    {
      get
      {
        return _textProperties;
      }
      set
      {
        Set(() => TextProperties, ref _textProperties, value);
      }
    }
    #endregion

    #region Commands
    private RelayCommand _saveCommand;
    public RelayCommand SaveCommand
    {
      get
      {
        return _saveCommand
            ?? (_saveCommand = new RelayCommand(
            () =>
            {
              if (!SaveCommand.CanExecute(null))
              {
                return;
              }

              SaveProperties();

            },
            () => IsValid && !IsSaving));
      }
    }

    private RelayCommand _manualButtonCommand;
    public RelayCommand ManualButtonCommand
    {
      get
      {
        return _manualButtonCommand
            ?? (_manualButtonCommand = new RelayCommand(
            () =>
            {
              if (!ManualButtonCommand.CanExecute(null))
              {
                return;
              }

              ChangeStatusSetting(VendorStatus.Manual);

            },
            () => true));
      }
    }

    private RelayCommand _autopilotButtonCommand;
    public RelayCommand AutopilotButtonCommand
    {
      get
      {
        return _autopilotButtonCommand
            ?? (_autopilotButtonCommand = new RelayCommand(
            () =>
            {
              if (!AutopilotButtonCommand.CanExecute(null))
              {
                return;
              }

              ChangeStatusSetting(VendorStatus.AutoPilot);

            },
            () => true));
      }
    }

    private RelayCommand _disabledButtonCommand;
    public RelayCommand DisabledButtonCommand
    {
      get
      {
        return _disabledButtonCommand
            ?? (_disabledButtonCommand = new RelayCommand(
            () =>
            {
              if (!DisabledButtonCommand.CanExecute(null))
              {
                return;
              }

              ChangeStatusSetting(VendorStatus.Disabled);

            },
            () => true));
      }
    }
    #endregion
  }
}