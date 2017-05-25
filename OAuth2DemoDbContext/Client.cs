using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OAuth2DemoDbContext
{
    public class OAuth_Client
    {
        [Key]
        public int ClientId { get; set; }

        public string ClientIdentifier { get; set; }

        public string ClientSecret { get; set; }

        public string Callback { get; set; }

        public string Name { get; set; }

        public int ClientType { get; set; }

        public string SiteUrl { get; set; }

        public string AccountName { get; set; }

        public string AccountPassword { get; set; }

        public string Address { get; set; }

        public bool IsEnabled { get; set; }
    }
}