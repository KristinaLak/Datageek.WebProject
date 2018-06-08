using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DataGeek.BusinessLogic.Common
{
    public class EmailHunterSource
    {
        // Private fields
        private String _Domain;
        private String _URI;
        private String _ExtractedOn;

        // Attributes
        public String Domain
        {
            set
            {
                if (value != null)
                    _Domain = value;
            }
            get { return _Domain; }
        }
        public String Uri
        {
            set
            {
                if (value != null)
                    _URI = value;
            }
            get { return _URI; }
        }
        public String Extracted_On
        {
            set
            {
                if (value != null)
                    _ExtractedOn = value;
            }
            get { return _ExtractedOn; }
        }

        // Constructor
        public EmailHunterSource()
        {

        }
    }
}