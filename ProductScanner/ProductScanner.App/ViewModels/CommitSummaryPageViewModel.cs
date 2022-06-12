using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Commits;

namespace ProductScanner.App.ViewModels
{
    /// <summary>
    /// A control common to system, stores and vendors which shows full page for commit history - filtered to the given entity.
    /// </summary>
    public class CommitSummaryPageViewModel : ViewModelBase 
    {

        #region CommitRowViewModel Class
        /// <summary>
        /// Represents one row in the RadGrid.
        /// </summary>
        public class CommitRowViewModel : ObservableObject
        {
            private int _batchID = 0;
            public int BatchID
            {
                get
                {
                    return _batchID;
                }

                set
                {
                    if (_batchID == value)
                    {
                        return;
                    }

                    _batchID = value;
                    RaisePropertyChanged(() => BatchID);
                }
            }

            /// <summary>
            /// Date row added to SQL. Not the date eventually maybe committed to store.
            /// </summary>
            private DateTime _date = default(DateTime);     
            public DateTime Date
            {
                get
                {
                    return _date;
                }

                set
                {
                    if (_date == value)
                    {
                        return;
                    }

                    _date = value;
                    RaisePropertyChanged(() => Date);
                }
            }

            private IVendorModel _vendor = null;
            public IVendorModel Vendor
            {
                get
                {
                    return _vendor;
                }

                set
                {
                    if (_vendor == value)
                    {
                        return;
                    }

                    _vendor = value;
                    RaisePropertyChanged(() => Vendor);
                }
            }


            private CommitBatchType _batchType =  default(CommitBatchType);
            public CommitBatchType BatchType
            {
                get
                {
                    return _batchType;
                }

                set
                {
                    if (_batchType == value)
                    {
                        return;
                    }

                    _batchType = value;
                    RaisePropertyChanged(() => BatchType);
                }
            }

            private int _qtySubmitted = 0;
            public int QtySubmitted
            {
                get
                {
                    return _qtySubmitted;
                }

                set
                {
                    if (_qtySubmitted == value)
                    {
                        return;
                    }

                    _qtySubmitted = value;
                    RaisePropertyChanged(() => QtySubmitted);
                }
            }

            /// <summary>
            /// Set true so can know when to show visual indication that the qty finally
            /// committed did not match the qty originally submitted.
            /// </summary>
            private bool _isQtyCommittedDifferent = false;
            public bool IsQtyCommittedDifferent
            {
                get
                {
                    return _isQtyCommittedDifferent;
                }

                set
                {
                    if (_isQtyCommittedDifferent == value)
                    {
                        return;
                    }

                    _isQtyCommittedDifferent = value;
                    RaisePropertyChanged(() => IsQtyCommittedDifferent);
                }
            }

            private CommitBatchStatus _status = default(CommitBatchStatus);
            public CommitBatchStatus Status
            {
                get
                {
                    return _status;
                }

                set
                {
                    if (_status == value)
                    {
                        return;
                    }

                    _status = value;
                    RaisePropertyChanged(() => Status);
                }
            }

            /// <summary>
            /// Navigate to the page to show details.
            /// </summary>
            private RelayCommand _showBatchDetailsCommand;
            public RelayCommand ShowBatchDetailsCommand
            {
                get
                {
                    return _showBatchDetailsCommand
                        ?? (_showBatchDetailsCommand = new RelayCommand(
                        () =>
                        {
                            // although the same-looking page shows either way, in one case the user control is hosted on a store page,
                            // while the other one hosts on a vendor page. This simply affects how the UX navigation, breadcrumbs and headings behave.

                            if (IsNavigationRelativeToStore)
                            {
                                var msg = new RequestNavigationToContentPageType(Vendor.ParentStore, ContentPageTypes.StoreCommitBatch)
                                {
                                    ItemIdentifier = BatchID
                                };
                                Messenger.Default.Send(msg);
                            }
                            else
                            {
                                var msg = new RequestNavigationToContentPageType(Vendor, ContentPageTypes.VendorCommitBatch)
                                {
                                    ItemIdentifier = BatchID
                                };
                                Messenger.Default.Send(msg);
                            }
                        }));
                }
            }

            /// <summary>
            /// When we navigate to a specific batch, should it be done within
            /// the store context, as opposed to vendor context. Relates to breadcrumbs, etc.
            /// </summary>
            private bool IsNavigationRelativeToStore { get; set; }

            /// <summary>
            /// Populate this object using a summary SQL row as the input source.
            /// </summary>
            /// <param name="b"></param>
            public CommitRowViewModel(IAppModel app, CommitBatchSummary b, bool isNavigationRelativeToStore)
            {
                this.BatchID = b.Id;
                this.Date = b.Created;
                this.Vendor = app.Stores.SelectMany(e => e.Vendors).FirstOrDefault(e => e.Vendor.Id == b.VendorId);
                this.BatchType = b.BatchType;
                this.QtySubmitted = b.QtySubmitted;
                // if not yet committed to store, then always false, but if was committed, compare counts.
                this.IsQtyCommittedDifferent = b.SessionStatus == CommitBatchStatus.Committed ? b.QtySubmitted == b.QtyCommitted.GetValueOrDefault() ? false : true : false;
                this.Status = b.SessionStatus;
                this.IsNavigationRelativeToStore = isNavigationRelativeToStore;
            }

        }
    	#endregion


        private IScannerDatabaseConnector dbScanner;

        public CommitSummaryPageViewModel(IAppModel appModel, IScannerDatabaseConnector dbScanner)
        {
            this.AppModel = appModel;
            this.dbScanner = dbScanner;

            ShowStoreName = false;
            ShowVendorName = false;

            if (IsInDesignMode)
            {
                MakeFakeRecentCommits();
            }
        }


        private void MakeFakeRecentCommits()
        {
            var list = new List<CommitBatchSummary>()
            {
                new CommitBatchSummary
                {
                    Id = 100,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-1),
                    BatchType = CommitBatchType.Discontinued,
                    QtySubmitted = 1020,
                    SessionStatus= CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 101,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-2),
                    BatchType = CommitBatchType.NewProducts,
                    QtySubmitted = 10234,
                    SessionStatus= CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 102,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-3),
                    BatchType = CommitBatchType.InStock,
                    QtySubmitted = 233,
                    SessionStatus= CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 103,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-4),
                    BatchType = CommitBatchType.NewProducts,
                    QtySubmitted = 11456,
                    SessionStatus= CommitBatchStatus.Discarded,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },

                new CommitBatchSummary
                {
                    Id = 104,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-5),
                    BatchType = CommitBatchType.NewVariants,
                    QtySubmitted = 909,
                    SessionStatus= CommitBatchStatus.Committed,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },

            };

            SetItemsSource(list);
        }

        public void SetItemsSource(IEnumerable<CommitBatchSummary> items)
        {
            // FYI - turns out could have just passed in batchID numbers rather than
            // full summaries since we ended up needing logic to refresh by numbers.

            // if instructed to show vendor name, then (for now) assume we're being
            // hosted on a store page rather than a vendor page - this affects navigation 
            // when clicking to see individual batches.

            var listBatches = new List<CommitRowViewModel>();
            foreach (var batch in items)
                listBatches.Add(new CommitRowViewModel(AppModel, batch, ShowVendorName));

            Commits = new ObservableCollection<CommitRowViewModel>(listBatches);
            ItemsSource = Commits;
            Refresh(reloadBatches:false);
        }

        private void Refresh(bool reloadBatches)
        {
            if (reloadBatches)
            {
                Reload();
            }
            else
            {
                TotalCommitsCount = Commits.Count();
                PendingCommitsCount = Commits.Where(e => e.Status == CommitBatchStatus.Pending).Count();
                InvalidateButtons();
            }
        }

        private async void Reload()
        {
            IsBusy = true;

            // since may have just deleted discarded batches, then row count here could in theory be less than
            // shown on original screen 

            var batchSummaries = await dbScanner.GetCommitBatchSummariesAsync(Commits.Select(e => e.BatchID));

            var listBatches = new List<CommitRowViewModel>();
            foreach (var batch in batchSummaries)
                listBatches.Add(new CommitRowViewModel(AppModel, batch, ShowVendorName));

            Commits = new ObservableCollection<CommitRowViewModel>(listBatches);

            if (IsShowingAllRows)
            {
                ItemsSource = Commits;
            }
            else
            {
                var pendingOnly = Commits.Where(e => e.Status == CommitBatchStatus.Pending).ToList();
                ItemsSource = new ObservableCollection<CommitRowViewModel>(pendingOnly);
            }

            IsBusy = false;
            TotalCommitsCount = Commits.Count();
            PendingCommitsCount = Commits.Where(e => e.Status == CommitBatchStatus.Pending).Count();
            InvalidateButtons();
        }

        #region Local Methods

        private void InvalidateButtons()
        {
            CommitPendingCommand.RaiseCanExecuteChanged();
            DiscardPendingCommand.RaiseCanExecuteChanged();
            DeleteBatchesCommand.RaiseCanExecuteChanged();
        }


        private async void CommitPendingBatchesActivity()
        {
            var pendingBatches = (from b in Commits
                                  where b.Status == CommitBatchStatus.Pending
                                  orderby b.BatchID
                                  select new
                                  {
                                      b.BatchID,
                                      b.QtySubmitted,
                                      b.Vendor
                                  }).ToList();

            // so we can calc an aggregate pct complete
            int totalRecordsAllBatches = pendingBatches.Sum(e => e.QtySubmitted);

            var activity = new ActivityRequest()
            {
                //Caption = "My Caption",
                Title = string.Format("Commit {0:N0} Pending Batch{1}?", pendingBatches.Count(), pendingBatches.Count() == 1 ? string.Empty : "es"),
                IsIndeterminateProgress = false,
                PercentComplete = 0.0,
                IsCancellable = true,
                StatusMessage = string.Empty,
                IsAutoClose = false,
                CustomElement = new ProductScanner.App.Controls.CommitBatchInputs(),

                OnAccept = async (a) =>
                {
                    var inputs = (a.CustomElement as ProductScanner.App.Controls.CommitBatchInputs).DataContext as CommitBatchInputsViewModel;
                    inputs.IsDisabled = true;

                    int prevCompletedRecords = 0;

                    Progress<ActivityProgress> progressIndicator = new Progress<ActivityProgress>((p) =>
                    {
                        // Might need to ensure UX thread here...but if use dispatcher, then be aware that the
                        // thread switch will cause this method to return before the progress is updated, which creates
                        // a race condition on prevCompletedRecords since the new batch will actually start before
                        // the last progress update of the prior batch.

                        // so far, unexplainable, coming in here on a non-UX thread still seems to work.

                        // don't show messages from caller, we want to form our own since this is an group operation
                        if (a.CancelToken.IsCancellationRequested)
                            return;

                        // we want control over messages since this is a group operation, so don't print what comes from caller
                        if (p.Message != null)
                            return;

                        var totalCountCompleted = prevCompletedRecords + p.CountCompleted;
                        var pct = totalRecordsAllBatches == 0 ? 0 : ((double)totalCountCompleted * 100.0) / (double)totalRecordsAllBatches;

                        a.PercentComplete = pct;
                    });

                    try
                    {
                        string errMsg = null;
                        int batchIndex=1;
                        foreach (var batch in pendingBatches)
                        {
                            a.StatusMessage = string.Format("Committing {0:N0} records for batch {1} ({2} of {3})...", batch.QtySubmitted, batch.BatchID, batchIndex, pendingBatches.Count());

                            var result = await batch.Vendor.CommitBatchAsync(batch.BatchID, batch.QtySubmitted, inputs.IgnoreDuplicates, a.CancelToken, progressIndicator);

                            if (a.CancelToken.IsCancellationRequested)
                                break;

                            if (result != CommitBatchResult.Successful)
                            {
                                errMsg = string.Format("Batch {0} Error: {1}", batch.BatchID, result.DescriptionAttr());
                                break; // skip remaining 
                            }
                            else
                                PendingCommitsCount--;

                            batchIndex++;
                            prevCompletedRecords += batch.QtySubmitted;
                        }

                        // if was cancelled by user, then we don't need to set cancel here

                        if (!a.CancelToken.IsCancellationRequested)
                        {
                            if (errMsg == null)
                            {
                                a.PercentComplete = 100;
                                a.SetCompleted(ActivityResult.Success, "Finished committing all pending batches.");
                            }
                            else
                                a.SetCompleted(ActivityResult.Failed, errMsg);
                        }
                    }
                    catch (Exception Ex)
                    {
                        a.SetCompleted(ActivityResult.Failed, Ex.Message);
                    }
                    finally
                    {
                        // finalize the task, return from original await
                        a.FinishUp();
                    }

                    Refresh(reloadBatches: true);
                },

                OnCancel = (a) =>
                {
                    // typically won't need to do anything here

                    // if was cancelled by user, then no need to set anything on the UX, 
                    // since already done

                    // FinishUp() called automatically upon return from this method.
                    if (a.HasEverRun)
                        Refresh(reloadBatches: true);
                },
            };

            await activity.Show(activity);
        }

        private async void DiscardPendingBatchesActivity()
        {
            var allPendingBatches = (from b in Commits
                                  where b.Status == CommitBatchStatus.Pending
                                  orderby b.BatchID
                                  select new
                                  {
                                      b.BatchID,
                                      b.Vendor,
                                      b.Date,
                                  }).ToList();


            var inputsControl = new ProductScanner.App.Controls.DiscardBatchesInputs();
            var inputs = inputsControl.DataContext as DiscardBatchesInputsViewModel;

            Func<int, string> makeTitle = (cnt) =>
            {
                return string.Format("Discard {0:N0} Pending Batch{1}?", cnt, cnt == 1 ? string.Empty : "es");
            };

            var selectedBatches = allPendingBatches;

            Action updateSelectedBatches = () =>
            {
                selectedBatches = allPendingBatches.Where(b => b.Date < DateTime.Now.AddDays(0 - inputs.DayCount)).ToList();
            };

            updateSelectedBatches();

            var activity = new ActivityRequest()
            {
                //Caption = "My Caption",
                Title = makeTitle(selectedBatches.Count),
                IsIndeterminateProgress = true,
                PercentComplete = 0.0,
                IsCancellable = true,
                StatusMessage = string.Empty,
                IsAutoClose = false,
                CustomElement = inputsControl,
                IsAcceptanceDisabled = selectedBatches.Count() == 0,

                OnAccept = async (a) =>
                {
                        try
                        {
                            a.StatusMessage = "Discarding selected pending batches...";
                            await Task.Delay(700);

                            string errMsg = null;

                            foreach(var batch in selectedBatches)
                            {
                                var result = await batch.Vendor.DiscardBatchAsync(batch.BatchID);

                                if (a.CancelToken.IsCancellationRequested)
                                    break;

                                if (result != CommitBatchResult.Successful)
                                {
                                    errMsg = string.Format("Batch {0} Error: {1}", batch.BatchID, result.DescriptionAttr());
                                    break; // skip remaining 
                                }
                            }

                            // if was cancelled by user, then we don't need to set cancel here

                            if (!a.CancelToken.IsCancellationRequested)
                            {
                                if (errMsg == null)
                                    a.SetCompleted(ActivityResult.Success, "Finished discarding pending batches.");
                                else
                                    a.SetCompleted(ActivityResult.Failed, errMsg);
                            }
                        }
                        catch (Exception Ex)
                        {
                            a.SetCompleted(ActivityResult.Failed, Ex.Message);
                        }
                        finally
                        {
                            // finalize the task, return from original await
                            a.FinishUp();
                        }
                        Refresh(reloadBatches: true);
                },

                OnCancel = (a) =>
                {
                    // typically won't need to do anything here

                    // if was cancelled by user, then no need to set anything on the UX, 
                    // since already done

                    // FinishUp() called automatically upon return from this method.
                    if (a.HasEverRun)
                        Refresh(reloadBatches: true);
                },
            };

            // need to hook changes to day count and update the title and filtered selection

            inputs.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DayCount")
                {
                    updateSelectedBatches();
                    activity.Title = makeTitle(selectedBatches.Count());
                    activity.IsAcceptanceDisabled = selectedBatches.Count() == 0;
                }
            };

            await activity.Show(activity);
        }

        /// <summary>
        /// Clean up history when desired by deleting discarded batches.
        /// </summary>
        private async void DeleteBatchesActivity()
        {
            var allBatches = (from b in Commits
                                  orderby b.BatchID
                                  select new
                                  {
                                      b.Status,
                                      b.BatchID,
                                      b.Vendor,
                                      b.Date
                                  }).ToList();




            var inputsControl = new ProductScanner.App.Controls.DeleteDiscardedBatchesInputs();
            var inputs = inputsControl.DataContext as DeleteDiscardedBatchesInputsViewModel;

            Func<int, string> makeTitle = (cnt) =>
            {
                if (inputs.IsOnlyDiscardedBatches)
                    return string.Format("Delete {0:N0} Discarded Batch{1}?", cnt, cnt == 1 ? string.Empty : "es");
                else
                    return string.Format("Delete {0:N0} Batch{1}?", cnt, cnt == 1 ? string.Empty : "es");
            };

            var selectedBatches = allBatches;

            Action updateSelectedBatches = () =>
            { 
                if (inputs.IsOnlyDiscardedBatches)
                    selectedBatches = allBatches.Where(b => b.Status == CommitBatchStatus.Discarded && b.Date < DateTime.Now.AddDays(0 - inputs.DayCount)).ToList(); 
                else
                    selectedBatches = allBatches.Where(b => b.Date < DateTime.Now.AddDays(0 - inputs.DayCount)).ToList(); 
            };

            updateSelectedBatches();

            var activity = new ActivityRequest()
            {
                //Caption = "My Caption",
                Title = makeTitle(selectedBatches.Count),
                IsIndeterminateProgress = true,
                PercentComplete = 0.0,
                IsCancellable = true,
                StatusMessage = string.Empty,
                IsAutoClose = false,
                CustomElement = inputsControl,
                IsAcceptanceDisabled = selectedBatches.Count() == 0,

                OnAccept = async (a) =>
                {
                    inputs.IsDisabled = true;
                    

                    try
                    {

                        a.StatusMessage = "Deleting selected batches...";
                        await Task.Delay(700);

                        string errMsg = null;

                        foreach (var batch in selectedBatches)
                        {
                            var result = await batch.Vendor.DeleteBatchAsync(batch.BatchID);

                            if (a.CancelToken.IsCancellationRequested)
                                break;

                            if (result != CommitBatchResult.Successful)
                            {
                                errMsg = string.Format("Batch {0} Error: {1}", batch.BatchID, result.DescriptionAttr());
                                break; // skip remaining 
                            }
                        }

                        // if was cancelled by user, then we don't need to set cancel here

                        if (!a.CancelToken.IsCancellationRequested)
                        {
                            if (errMsg == null)
                                a.SetCompleted(ActivityResult.Success, "Finished deleting selected batches.");
                            else
                                a.SetCompleted(ActivityResult.Failed, errMsg);
                        }
                    }
                    catch (Exception Ex)
                    {
                        a.SetCompleted(ActivityResult.Failed, Ex.Message);
                    }
                    finally
                    {
                        // finalize the task, return from original await
                        a.FinishUp();
                    }
                    Refresh(reloadBatches: true);
                },

                OnCancel = (a) =>
                {
                    // typically won't need to do anything here

                    // if was cancelled by user, then no need to set anything on the UX, 
                    // since already done

                    // FinishUp() called automatically upon return from this method.
                    if (a.HasEverRun)
                        Refresh(reloadBatches: true);
                },
            };

            // need to hook changes to day count and update the title and filtered selection

            inputs.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DayCount" || e.PropertyName == "IsOnlyDiscardedBatches")
                {
                    updateSelectedBatches();
                    activity.Title = makeTitle(selectedBatches.Count());
                    activity.IsAcceptanceDisabled = selectedBatches.Count() == 0;
                }
            };

            await activity.Show(activity);
        }

        #endregion

        #region Public Properties

        private IAppModel _appModel = null;
        public IAppModel AppModel
        {
            get
            {
                return _appModel;
            }
            set
            {
                Set(() => AppModel, ref _appModel, value);
            }
        }


        /// <summary>
        /// For when reloading summaries after an activity.
        /// </summary>
        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                Set(() => IsBusy, ref _isBusy, value);
            }
        }

        /// <summary>
        /// Which view, all or just the pending
        /// </summary>
        private bool _isShowingAllRows = true;
        public bool IsShowingAllRows
        {
            get
            {
                return _isShowingAllRows;
            }

            set
            {
                if (_isShowingAllRows == value)
                {
                    return;
                }

                _isShowingAllRows = value;
                RaisePropertyChanged(() => IsShowingAllRows);
            }
        }
        private int _totalCommitsCount = 0;
        public int TotalCommitsCount
        {
            get
            {
                return _totalCommitsCount;
            }

            set
            {
                if (_totalCommitsCount == value)
                {
                    return;
                }

                _totalCommitsCount = value;
                RaisePropertyChanged(() => TotalCommitsCount);
            }
        }

        private int _pendingCommitsCount = 0;
        public int PendingCommitsCount
        {
            get
            {
                return _pendingCommitsCount;
            }

            set
            {
                if (_pendingCommitsCount == value)
                {
                    return;
                }

                _pendingCommitsCount = value;
                RaisePropertyChanged(() => PendingCommitsCount);
            }
        }



        /// <summary>
        /// All commits passed in - unfiltered.
        /// </summary>
        private ObservableCollection<CommitRowViewModel> _commits = null;
        public ObservableCollection<CommitRowViewModel> Commits
        {
            get
            {
                return _commits;
            }

            set
            {
                if (_commits == value)
                {
                    return;
                }

                _commits = value;
                RaisePropertyChanged(() => Commits);
            }
        }

        /// <summary>
        /// Filtered collection bound to RadGrid.
        /// </summary>
        private ObservableCollection<CommitRowViewModel> _itemsSource = null;
        public ObservableCollection<CommitRowViewModel> ItemsSource
        {
            get
            {
                return _itemsSource;
            }

            set
            {
                if (_itemsSource == value)
                {
                    return;
                }

                _itemsSource = value;
                RaisePropertyChanged(() => ItemsSource);
            }
        }

        /// <summary>
        /// Should RadGrid display the vendor name column.
        /// </summary>
        private bool _showVendorName = false;
        public bool ShowVendorName
        {
            get
            {
                return _showVendorName;
            }

            set
            {
                if (_showVendorName == value)
                {
                    return;
                }

                _showVendorName = value;
                RaisePropertyChanged(() => ShowVendorName);
            }
        }

        /// <summary>
        /// Should RadGrid display the store name column.
        /// </summary>
        private bool _showStoreName = false;
        public bool ShowStoreName
        {
            get
            {
                return _showStoreName;
            }

            set
            {
                if (_showStoreName == value)
                {
                    return;
                }

                _showStoreName = value;
                RaisePropertyChanged(() => ShowStoreName);
            }
        }

        #endregion

        #region Commands

        private RelayCommand _commitPendingCommand;
        public RelayCommand CommitPendingCommand
        {
            get
            {
                return _commitPendingCommand
                    ?? (_commitPendingCommand = new RelayCommand(
                    () =>
                    {
                        if (!CommitPendingCommand.CanExecute(null))
                        {
                            return;
                        }

                        CommitPendingBatchesActivity();
                        
                    },
                    () => PendingCommitsCount > 0));
            }
        }

        private RelayCommand _discardPendingCommand;
        public RelayCommand DiscardPendingCommand
        {
            get
            {
                return _discardPendingCommand
                    ?? (_discardPendingCommand = new RelayCommand(
                    () =>
                    {
                        if (!DiscardPendingCommand.CanExecute(null))
                        {
                            return;
                        }

                        DiscardPendingBatchesActivity();

                    },
                    () => PendingCommitsCount > 0));
            }
        }

        private RelayCommand _showAllRowsCommand;
        public RelayCommand ShowAllRowsCommand
        {
            get
            {
                return _showAllRowsCommand
                    ?? (_showAllRowsCommand = new RelayCommand(
                    () =>
                    {
                        if (!ShowAllRowsCommand.CanExecute(null))
                        {
                            return;
                        }

                        ItemsSource = Commits;
                        IsShowingAllRows = true;
                        
                    },
                    () => true));
            }
        }

        private RelayCommand _showPendingRowsCommand;
        public RelayCommand ShowPendingRowsCommand
        {
            get
            {
                return _showPendingRowsCommand
                    ?? (_showPendingRowsCommand = new RelayCommand(
                    () =>
                    {
                        if (!ShowPendingRowsCommand.CanExecute(null))
                        {
                            return;
                        }

                        var pendingOnly = Commits.Where(e => e.Status == CommitBatchStatus.Pending).ToList();
                        ItemsSource = new ObservableCollection<CommitRowViewModel>(pendingOnly);
                        IsShowingAllRows = false;
                        
                    },
                    () => true));
            }
        }

        private RelayCommand _deleteBatchesCommand;
        public RelayCommand DeleteBatchesCommand
        {
            get
            {
                return _deleteBatchesCommand
                    ?? (_deleteBatchesCommand = new RelayCommand(
                    () =>
                    {
                        if (!DeleteBatchesCommand.CanExecute(null))
                        {
                            return;
                        }

                        DeleteBatchesActivity();
                    },
                    () =>
                    {
                        if (Commits == null)
                            return false;

                        return Commits.Where(e => e.Status == CommitBatchStatus.Discarded).Count() > 0;
                    }));
            }
        }

        #endregion

    }
}