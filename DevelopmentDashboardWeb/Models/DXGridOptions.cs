﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevelopmentDashboardWeb.Models
{
    public class DXGridOptions
    {
        public List<Group> Group { get; set; }
        public int PageIndex { get; set; }
        public string SearchExpr { get; set; }
        public string SearchOperation { get; set; }
        public string SearchValue { get; set; }
        public int MyProperty { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }

    public class Group
    {
        public bool Desc { get; set; }
        public bool IsExpanded { get; set; }
        public string Selector { get; set; }
    }
}