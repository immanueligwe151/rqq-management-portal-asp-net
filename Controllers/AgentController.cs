using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Cloud.Firestore;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using System.Web.Script.Serialization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using System.IO;
using static Google.Cloud.Storage.V1.UrlSigner;

namespace rqq_management_portal_asp_net.Controllers
{
    public class AgentController : Controller
    {
        private const string FirebaseApiKey = "AIzaSyCUiL8du5Kcr2qFvGMClaVvi_uKM8xGInA";
        private const string ProjectId = "rqq-management-project";

        static private readonly string BucketName = "rqq-management-project.firebasestorage.app";

        // GET: Agent
        public async Task<ActionResult> Home()
        {
            if (Session["AgentUsername"] != null)
            {
                string username = Session["AgentUsername"] as string; // obtain stored username in session
                string agentName = await GetUserDetails(username, "name"); // get the name using the username
                string uName = await GetUserDetails(username, "username");
                ViewBag.AgentName = agentName;
                ViewBag.UName = uName;
                return View();
            }

            return RedirectToAction("Login", "Home");
        }

        public ActionResult AssignedRequests()
        {
            return View();
        }

        public ActionResult ViewCompletedRequests()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear(); // clears all session data
            Session.Abandon(); // abandon the session
            return RedirectToAction("Login", "Home"); // Redirect to general login page
        }

        private async Task<string> GetUserDetails(string username, string detailToRetrieve)
        {
            FirestoreDb db = FirestoreDb.Create(ProjectId);


            // fetching document as it's saved by username
            DocumentReference docRef = db.Collection("agent").Document(username);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            // checks if the document exists
            if (!snapshot.Exists)
            {
                return null;
            }

            // return the email field from the document
            return snapshot.GetValue<string>(detailToRetrieve);
        }

        [Obsolete]
        public JsonResult GetImages(string path)
        {
            try
            {
                var credential = GoogleCredential.FromFile(
                    Server.MapPath("~/App_Data/rqq-management-project-firebase-adminsdk-fbsvc-a228a51ee0.json")
                );
                var signer = UrlSigner.FromServiceAccountCredential(credential.UnderlyingCredential as ServiceAccountCredential);
                var storageClient = StorageClient.Create(credential);

                var files = storageClient.ListObjects(BucketName, path);
                var urls = new List<string>();

                foreach (var file in files)
                {
                    var url = signer.Sign(
                        BucketName,
                        file.Name,
                        TimeSpan.FromMinutes(15),
                        HttpMethod.Get
                    );
                    urls.Add(url);
                }

                return Json(urls, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}