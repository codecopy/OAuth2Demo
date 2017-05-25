using OAuth2DemoDbContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;

namespace OAuth2DemoClientManage.Controllers
{
    public class ClientController : ApiController
    {
        public IEnumerable<OAuth_Client> GetClients()
        {
            using (var db = new OAuthDbContext())
            {
                var clients = db.Clients.ToArray();
                return clients;
            }
        }

        public OAuth_Client GetAccountClient(string accountName)
        {
            using (var db = new OAuthDbContext())
            {
                var client = db.Clients.FirstOrDefault(o => o.AccountName == accountName);
                return client;
            }
        }

        //change information
        [AcceptVerbs("POST")]
        public OPResult Update(OAuth_Client client)
        {
            using (var db = new OAuthDbContext())
            {
                var exists = db.Clients.Any(o => o.ClientId != client.ClientId && (o.ClientIdentifier == client.ClientIdentifier || client.Name == o.Name));
                if (exists)
                {
                    return new OPResult { IsSucceed = false, Message = "已存在相同名称或相同标识的其它商户" };
                }
                db.Entry(client).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult { IsSucceed = true };
        }

        [AcceptVerbs("DELETE")]
        public OPResult Delete(int clientId)
        {
            using (var db = new OAuthDbContext())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    var auths = db.ClientAuthorizations.Where(o => o.ClientId == clientId).ToArray();
                    var client = db.Clients.Find(clientId);
                    db.ClientAuthorizations.RemoveRange(auths);
                    db.Clients.Remove(client);
                    try
                    {
                        db.SaveChanges();
                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult { IsSucceed = false, Message = e.Message };
                    }
                }
            }
            return new OPResult { IsSucceed = true };
        }

        [AcceptVerbs("POST")]
        public OPResult Create(OAuth_Client client)
        {
            using (var db = new OAuthDbContext())
            {
                var c = db.Clients.FirstOrDefault(o => o.ClientIdentifier == client.ClientIdentifier || client.Name == o.Name);
                if (c != null)
                {
                    if (c.ClientIdentifier == client.ClientIdentifier)
                        return new OPResult { IsSucceed = false, Message = "已存在相同标识的商户" };
                    else
                        return new OPResult { IsSucceed = false, Message = "已存在相同名称的商户" };
                }
                client = db.Clients.Add(client);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult<int> { IsSucceed = true, Result = client.ClientId };
        }

        public IEnumerable<OAuth_ClientOpenApi> GetClientApis(int clientId)
        {
            using (var db = new OAuthDbContext())
            {
                var apis = db.ClientOpenApis.Where(o => o.ClientId == clientId).ToArray();
                return apis;
            }
        }

        //get api authority
        public HttpResponseMessage GetApis()
        {
            var httpClient = new HttpClient();
            using (var resourceResponse = httpClient.GetAsync(MvcApplication.OpenApiAddress))
            {
                return resourceResponse.Result;
            }
        }

        [AcceptVerbs("POST")]
        public OPResult SaveClientAPIs(int clientId, IEnumerable<string> apis)
        {
            using (var db = new OAuthDbContext())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    var capis = db.ClientOpenApis.Where(o => o.ClientId == clientId).ToArray();
                    db.ClientOpenApis.RemoveRange(capis);
                    foreach (var api in apis)
                    {
                        db.ClientOpenApis.Add(new OAuth_ClientOpenApi { ClientId = clientId, OpenApi = api });
                    }
                    try
                    {
                        db.SaveChanges();
                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult { IsSucceed = false, Message = e.Message };
                    }
                }
            }
            return new OPResult { IsSucceed = true };
        }
    }
}
