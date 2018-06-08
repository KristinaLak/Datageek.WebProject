using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataGeek.BusinessLogic.Common
{
    public class EmailHunterEmailResponse
    {
        // Private fields
        private String _Status;
        private String _Email;
        private int? _Score = 0;
        private List<EmailHunterSource> _Sources;

        // Attributes
        public String Status
        {
            set
            {
                if (value != null)
                    _Status = value;
            }
            get { return _Status; }
        }
        public String Email
        {
            set
            {
                if (value != null)
                    _Email = value;
            }
            get { return _Email; }
        }
        public int? Score
        {
            set
            {
                int test_int = 0;
                if (value != null && Int32.TryParse(value.ToString(), out test_int))
                    _Score = test_int;
            }
            get { return _Score; }
        }
        public List<EmailHunterSource> Sources
        {
            set
            {
                if (value != null)
                    _Sources = value;
            }
            get { return _Sources; }
        }

        // Constructor
        public EmailHunterEmailResponse()
        {

        }
    }
}