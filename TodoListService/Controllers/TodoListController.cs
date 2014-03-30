using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Collections.Concurrent;
using TodoListService.Models;
using System.Security.Claims;

namespace TodoListService.Controllers
{
    [Authorize]
    public class TodoListController : ApiController
    {
        //
        // To Do items list for all users.  Since the list is stored in memory, it will go away if the service is cycled.
        //
        static ConcurrentBag<TodoItem> todoBag = new ConcurrentBag<TodoItem>();

        // GET api/todolist
        public IEnumerable<TodoItem> Get()
        {
            //
            // The Scope claim tells you what permissions the client application has in the service.
            // In this case we look for a scope value of user_impersonation, or full access to the service as the user.
            //
            Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (scopeClaim != null)
            {
                if (scopeClaim.Value != "user_impersonation")
                {
                    throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
                }
            }

            // A user's To Do list is keyed off of the NameIdentifier claim, which contains an immutable, unique identifier for the user.
            Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            return from todo in todoBag
                   where todo.Owner == subject.Value
                   select todo;
        }

        // POST api/todolist
        public void Post(TodoItem todo)
        {
            Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (scopeClaim != null)
            {
                if (scopeClaim.Value != "user_impersonation")
                {
                    throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
                }
            }

            if (null != todo && !string.IsNullOrWhiteSpace(todo.Title))
            {
                todoBag.Add(new TodoItem { Title = todo.Title, Owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value });
            }
        }
    }
}
