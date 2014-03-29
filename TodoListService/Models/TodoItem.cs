using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoListService.Models
{
    public class TodoItem
    {
        public string Title { get; set; }
        public string Owner { get; set; }
    }
}