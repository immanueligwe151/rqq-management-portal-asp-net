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
    public class HomeController : Controller
    {
        private const string FirebaseApiKey = "AIzaSyCUiL8du5Kcr2qFvGMClaVvi_uKM8xGInA";
        private const string ProjectId = "rqq-management-project";

        // GET: Login - Show login page if not logged in
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserToken"] != null)
            {
                return RedirectToAction("Home", "Agent"); // redirect to admin home if already logged in
            }

            return View(); // show login page if not logged in
        }

        // POST: Login - Authenticate agent credentials
        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            string email = await GetEmailByUsername(username);
            if (email == null)
            {
                ViewBag.Error = "Invalid username or password, try again";
                return View();
            }

            string token = await SignInWithEmailPassword(email, password);
            if (token == null)
            {
                ViewBag.Error = "Invalid username or password, try again";
                return View();
            }

            // set session variables for logged-in agent
            Session["AgentToken"] = token;
            Session["AgentEmail"] = email;
            Session["AgentUsername"] = username;
            return RedirectToAction("Home", "Agent"); // redirect to agent home  after successful login
        }

        public ActionResult Logout()
        {
            Session.Clear(); // clears all session data
            Session.Abandon(); // abandon the session
            return RedirectToAction("Login", "Home"); // Redirect to general login page
        }

        private async Task<string> GetEmailByUsername(string username)
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
            return snapshot.GetValue<string>("email");
        }

        private async Task<string> SignInWithEmailPassword(string email, string password)
        {
            var payload = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}",
                    payload
                );

                if (!response.IsSuccessStatusCode)
                    return null;

                dynamic result = await response.Content.ReadAsAsync<dynamic>();
                return result.idToken;
            }
        }



        public ActionResult Index()
        {
            //return RedirectToAction("Login", "Home");
            return View();
            //this is to load the login page when user is logged out, and load the agent/admin home page when user is logged in
            //will come back to this later
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}