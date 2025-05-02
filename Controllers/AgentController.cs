using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Cloud.Firestore;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace rqq_management_portal_asp_net.Controllers
{
    public class AgentController : Controller
    {
        private const string FirebaseApiKey = "AIzaSyCUiL8du5Kcr2qFvGMClaVvi_uKM8xGInA";
        private const string ProjectId = "rqq-management-project";

        // GET: Agent
        public async Task<ActionResult> Home()
        {
            if (Session["AgentUsername"] != null)
            {
                string username = Session["AgentUsername"] as string; // obtain stored username in session
                string agentName = await GetUserDetails(username, "name"); // get the name using the username
                ViewBag.AgentName = agentName;
                return View();
            }

            return RedirectToAction("Login", "Home");
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
    }
}