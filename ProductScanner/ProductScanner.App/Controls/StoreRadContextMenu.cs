using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace ProductScanner.App.Controls
{
    public class StoreRadContextMenu : RadContextMenu
    {
        public StoreRadContextMenu()
        {
            Loaded += StoreRadContextMenu_Loaded;
        }

        void StoreRadContextMenu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var menu = sender as RadContextMenu;

            var store = menu.DataContext as IStoreModel;
            if (store == null)
                return;

            var dc = new ProductScanner.App.ViewModels.StoreContextMenuViewModel(store);
            menu.DataContext = dc;    
        
            // doing this here rather than in Xaml is simply to prevent all the binding errors reported
            // to output when transitioning the datacontext to the vm we want

            foreach(RadMenuItem item in menu.Items)
            {
                if (item.Header == null)
                    continue;

                switch(item.Header as string)
                {
                    // scanning 

                    case "Start All":
                        item.Command = dc.StartCommand;
                        break;

                    case "Suspend All":
                        item.Command = dc.SuspendCommand;
                        break;

                    case "Resume All":
                        item.Command = dc.ResumeCommand;
                        break;

                    case "Cancel All":
                        item.Command = dc.CancelCommand;
                        break;

                    // pending batches

                    case "Commit Pending":
                        item.Command = dc.CommitPendingBatchesCommand;
                        break;

                    case "Discard Pending":
                        item.Command = dc.DiscardPendingBatchesCommand;
                        break;

                    case "Delete Batches":
                        item.Command = dc.DeletePendingBatchesCommand;
                        break;

                    // misc 

                    case "Clear Logs":
                        item.Command = dc.ClearLogCommand;
                        break;

                    case "Delete Cache":
                        item.Command = dc.DeleteCachedFilesCommand;
                        break;

                }
            }
        }
    }
}
