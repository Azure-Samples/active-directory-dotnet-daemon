using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;

namespace TodoListDaemon
{
    class Program
    {
        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The App Key is a credential used by the application to authenticate to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        //
        const string aadInstance = "https://login.windows.net/{0}";
        const string tenant = "skwantoso.com";
        const string clientId = "a2f3e366-3dd9-4597-8445-ee8cab4eb65a";
        const string appKey = "NgT0Xhr71BoWyVgnNzmLAeo2TfRREh2nlQDx4gztOLk=";

        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        //
        // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
        // To contact the To Do list service we need it's URL as well.
        //
        const string todoListResourceId = "https://skwantoso.com/TodoListService";
        const string todoListBaseAddress = "https://localhost:44321";

        private static HttpClient httpClient = new HttpClient();
        private static AuthenticationContext authContext = new AuthenticationContext(authority);
        private static ClientCredential clientCredential = new ClientCredential(clientId, appKey);

        static void Main(string[] args)
        {
            //
            // Call the To Do service 10 times with a 6 second delay between calls.
            //
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(3000);
                PostTodo().Wait();
                Thread.Sleep(3000);
                GetTodo().Wait();
            }
        }

        static async Task PostTodo()
        {
            //
            // Get an access token from Azure AD using client credentials.
            // If the attempt to get a token fails because the server is unavailable, retry twice after 3 seconds each.
            //
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;

            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    result = authContext.AcquireToken(todoListResourceId, clientCredential);
                }
                catch (ActiveDirectoryAuthenticationException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }

                    Console.Write(
                        String.Format("An error occurred while acquiring a token\nTime: {0}\nError: {1}\nRetry: {2}\n\n",
                        DateTime.Now.ToString(),
                        ex.ToString(),
                        retry.ToString()));
                }

            } while ((retry == true) && (retryCount < 3));

            if (result == null)
            {
                Console.Write("Canceling attempt to contact To Do list service.\n\n");
                return;
            }

            //
            // Post an item to the To Do list service.
            //

            // Add the access token to the authorization header of the request.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Forms encode To Do item and POST to the todo list web api.
            string timeNow = DateTime.Now.ToString();
            Console.Write("Posting to To Do list at " + timeNow + "\n");
            string todoText = "Task at time: " + timeNow;
            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", todoText) });
            HttpResponseMessage response = await httpClient.PostAsync(todoListBaseAddress + "/api/todolist", content);

            if (response.IsSuccessStatusCode == true)
            {
                Console.Write("Successfully posted new To Do item:  " + todoText + "\n\n");
            }
            else
            {
                Console.Write("Failed to post a new To Do item\nError:  "+ response.ReasonPhrase + "\n\n");
            }
        }

        static async Task GetTodo()
        {
            //
            // Get an access token from Azure AD using client credentials.
            // If the attempt to get a token fails because the server is unavailable, retry twice after 3 seconds each.
            //
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;

            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    result = authContext.AcquireToken(todoListResourceId, clientCredential);
                }
                catch (ActiveDirectoryAuthenticationException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }

                    Console.Write(
                        String.Format("An error occurred while acquiring a token\nTime: {0}\nError: {1}\nRetry: {2}\n\n",
                        DateTime.Now.ToString(),
                        ex.ToString(),
                        retry.ToString()));
                }

            } while ((retry == true) && (retryCount < 3));

            if (result == null)
            {
                Console.Write("Canceling attempt to contact To Do list service.\n\n");
                return;
            }
            
            //
            // Read items from the To Do list service.
            //

            // Add the access token to the authorization header of the request.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Call the To Do list service.
            Console.Write("Retrieving To Do list at " + DateTime.Now.ToString() + "\n");
            HttpResponseMessage response = await httpClient.GetAsync(todoListBaseAddress + "/api/todolist");

            if (response.IsSuccessStatusCode)
            {
                // Read the response and output it to the console.
                string s = await response.Content.ReadAsStringAsync();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<TodoItem> toDoArray = serializer.Deserialize<List<TodoItem>>(s);

                int count = 0;
                foreach (TodoItem item in toDoArray)
                {
                    Console.Write("Owner: " + item.Owner + "\nItem:  " + item.Title + "\n");
                    count++;
                }

                Console.Write("Total item count:  " + count + "\n\n");
            }
            else
            {
                Console.Write("Failed to retrieve To Do list\nError:  " + response.ReasonPhrase + "\n\n");
            }
        }
    }
}
