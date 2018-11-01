﻿using System;

namespace DtpCore.ViewModel
{
    public class WorkflowView
    {
        public int DatabaseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public string State { get; set; }
        public DateTime LastExecution { get; set; }
        public DateTime NextExecution { get; set; }
    }
}
