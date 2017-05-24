using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoScheduler.Entities;
using AutoScheduler.Models;
using Microsoft.AspNet.Identity;
using WebApi.Utilities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using AutoScheduler.Repository;
using Newtonsoft.Json;
using Microsoft.Xrm.Tooling.Connector;

namespace AutoScheduler.Controllers
{
    [Authorize]
    [RequireHttps]
    public class CRMConnectionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CRMConnections
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var crmConnections = db.CrmConnections.Where(c => c.UserId == userId);
            return View(crmConnections
                .Select(c=> new
                { Id = c.Id, UserId = c.UserId, Name = c.Name ,connectionKey = c.connectionKey })
                .AsEnumerable()
                .Select(c=> new CRMConnection
                { Id = c.Id, UserId = c.UserId, Name = c.Name ,connectionKey = EncryptionHelper.Decrypt(c.connectionKey) })
                .ToList());
        }

        // GET: CRMConnections/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            CRMConnection cRMConnection = db.CrmConnections.Find(id);
            if (cRMConnection == null || cRMConnection.UserId != userId)
            {
                return HttpNotFound();
            }
            cRMConnection.connectionKey = EncryptionHelper.Decrypt(cRMConnection.connectionKey);
            return View(cRMConnection);
        }

        // GET: CRMConnections/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            ViewBag.UserId = new SelectList(db.CrmConnections.Where(c=> c.UserId == userId), "Id", "connectionKey");
            return View();
        }

        // POST: CRMConnections/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,connectionKey,Name,UserId")] CRMConnection cRMConnection)
        {
            var userId = User.Identity.GetUserId();
            var name = cRMConnection.Name;
            cRMConnection.UserId = userId;
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                var key = EncryptionHelper.Encrypt(cRMConnection.connectionKey);
                var newConnection = new CRMConnection();
                newConnection.connectionKey = key;
                newConnection.UserId = userId;
                newConnection.Name = name;
                db.CrmConnections.Add(newConnection);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.CrmConnections.Where(c => c.UserId == userId), "Id", "connectionKey", User.Identity.GetUserId());
            return View(cRMConnection);
        }

        // GET: CRMConnections/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            CRMConnection cRMConnection = db.CrmConnections.Find(id);
            if (cRMConnection == null || cRMConnection.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            cRMConnection.connectionKey = EncryptionHelper.Decrypt(cRMConnection.connectionKey);
            ViewBag.UserId = new SelectList(db.CrmConnections.Where(c => c.UserId == userId), "Id", "connectionKey", cRMConnection.UserId);
            return View(cRMConnection);
        }

        // POST: CRMConnections/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,connectionKey,UserId")] CRMConnection cRMConnection)
        {
            var userId = User.Identity.GetUserId();
            CRMConnection checkValue = db.CrmConnections.Find(cRMConnection.Id);
            if (checkValue == null || checkValue.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                checkValue.Name = cRMConnection.Name;
                checkValue.connectionKey = EncryptionHelper.Encrypt(cRMConnection.connectionKey);
                db.Entry(checkValue).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.CrmConnections.Where(c => c.UserId == userId), "Id", "connectionKey", cRMConnection.UserId);
            return View(cRMConnection);
        }

        // GET: CRMConnections/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CRMConnection cRMConnection = db.CrmConnections.Find(id);
            var userId = User.Identity.GetUserId();
            if (cRMConnection == null || cRMConnection.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(cRMConnection);
        }

        // POST: CRMConnections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CRMConnection cRMConnection = db.CrmConnections.Find(id);
            var userId = User.Identity.GetUserId();
            if (cRMConnection == null || cRMConnection.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            db.CrmConnections.Remove(cRMConnection);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public string GetCRMWorkflows(string entityname, int id)
        {
            var userId = User.Identity.GetUserId();
            var repo = new CRMRepository();
            var conn = repo.GetConnectionById(id, userId);
            if (conn == null) return null;
            var service = new CrmServiceClient(conn);

            QueryByAttribute qba = new QueryByAttribute
            {
                EntityName = "workflow",
                ColumnSet = new ColumnSet("name")
            };

            qba.Attributes.Add("primaryentity");
            qba.Values.Add(entityname);
            qba.AddAttributeValue("type", 1);

            EntityCollection wfs = service.RetrieveMultiple(qba);

            List<Entity> wfList = new List<Entity>();

            foreach (Entity entity in wfs.Entities)
            {
                if (entity.Contains("name"))
                    wfList.Add(entity);
            }
            var result = wfList.Select(v => new { Value = v.Id, Text = v["name"] });
            return JsonConvert.SerializeObject(result);
        }
        public JsonResult GetCRMEntities(int id)
        {
            var userId = User.Identity.GetUserId();
            var repo = new CRMRepository();
            var conn = repo.GetConnectionById(id, userId);
            if (conn == null) return null;
            var service = new CrmServiceClient(conn);
            List<EntityMetadata> entities = new List<EntityMetadata>();

            RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest
            {
                RetrieveAsIfPublished = true,
                EntityFilters = EntityFilters.Entity
            };

            RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)service.Execute(request);

            foreach (EntityMetadata emd in response.EntityMetadata)
            {
                if (emd.DisplayName.UserLocalizedLabel != null && (emd.IsCustomizable.Value || emd.IsManaged.Value == false))
                {
                    entities.Add(emd);
                }
            }
            var objects = entities.OrderBy(e => e.LogicalName).Select(e => new { Value = e.ObjectTypeCode, Text = e.LogicalName });

            return Json(objects, JsonRequestBehavior.AllowGet);
        }

        public string GetCRMViews(int objectTypeCode, int id)
        {
            var userId = User.Identity.GetUserId();
            var repo = new CRMRepository();
            var conn = repo.GetConnectionById(id, userId);
            if (conn == null) return null;
            var service = new CrmServiceClient(conn);

            QueryByAttribute qba = new QueryByAttribute
            {
                EntityName = "savedquery",
                ColumnSet = new ColumnSet("fetchxml", "name")
            };

            qba.Attributes.Add("returnedtypecode");
            qba.Values.Add(objectTypeCode);

            EntityCollection views = service.RetrieveMultiple(qba);

            List<Entity> viewsList = new List<Entity>();

            foreach (Entity entity in views.Entities)
            {
                if (entity.Contains("fetchxml"))
                viewsList.Add(entity);
            }
            var result = viewsList.Select(v => new { Value = v["fetchxml"], Text = v["name"] });
            return JsonConvert.SerializeObject(result);
        }

        public ActionResult GetViewData(Entity view, int id)
        {
            var userId = User.Identity.GetUserId();
            var repo = new CRMRepository();
            return View(repo.GetEntitiesFromView(view, id, userId));
        }


        public ActionResult GetViewData(string fetchXml, int id)
        {
            var userId = User.Identity.GetUserId();
            var repo = new CRMRepository();
            return View(repo.GetEntitiesFromFetch(fetchXml, id, userId));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
