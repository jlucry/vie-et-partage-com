using Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services
{
    /// <summary>
    /// Post registration field.
    /// </summary>
    public class JsonPostRegistrationField : PostRegistrationField
    {
        public JsonPostRegistrationField() : base()
        {

        }

        public JsonPostRegistrationField(PostRegistrationField field) : base()
        {
            if (field != null)
            {
                UId = field.UId;
                Title = field.Title;
                Position = field.Position;
                Type = field.Type;
                Mandatory = field.Mandatory;
                Choose = field.Choose;
                Choose2 = field.Choose2;
                Details = field.Details;
            }
        }

        public string Value { get; set; }
        public string Value2 { get; set; }

        public bool IsError { get; set; }
        public bool IsError2 { get; set; }
    }
}