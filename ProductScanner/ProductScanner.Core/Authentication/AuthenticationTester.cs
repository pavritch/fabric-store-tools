using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleInjector;

namespace ProductScanner.Core.Authentication
{
    // used to run authentication for all vendors at once
    public class AuthenticationTester : IAuthenticationTester
    {
        private readonly Container _container;
        public AuthenticationTester(Container container)
        {
            _container = container;
        }

        public async Task RunAll()
        {
            var vendors = Vendor.GetAll();
            var tasks = new List<Task<AuthenticationResult>>();
            var associatedVendors = new Dictionary<int, Vendor>();
            foreach (var vendor in vendors)
            {
                var auth = _container.GetInstance(typeof (IVendorAuthenticator<>).MakeGenericType(vendor.GetType())) as IVendorAuthenticator;
                if (auth == null) continue;

                if (auth.GetType().Name.Contains("NullVendorAuthenticator")) continue;

                var task = auth.LoginAsync();
                tasks.Add(task);
                associatedVendors.Add(task.Id, vendor);

                //var res = await auth.LoginAsync(credentials);
                //System.Console.WriteLine("Authenticating for " + vendor + (res.Success ? " success" : " failed"));
            }

            while (tasks.Count > 0)
            {
                // Identify the first task that completes.
                var firstFinishedTask = await Task.WhenAny(tasks);

                // ***Remove the selected task from the list so that you don't 
                // process it more than once.
                tasks.Remove(firstFinishedTask);

                // Await the completed task. 
                AuthenticationResult res = await firstFinishedTask;
                System.Console.WriteLine("Authenticating for " + associatedVendors[firstFinishedTask.Id].DisplayName + (res.IsSuccessful ? " success" : " failed"));
                //resultsTextBox.Text += String.Format("\r\nLength of the download:  {0}", length);
            }
        }
    }
}