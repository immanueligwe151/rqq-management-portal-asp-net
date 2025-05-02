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
    public class AdminController : Controller
    {
        private const string FirebaseApiKey = "AIzaSyCUiL8du5Kcr2qFvGMClaVvi_uKM8xGInA";
        private const string ProjectId = "rqq-management-project";

        // GET: Login - Show login page if not logged in
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["AdminToken"] != null)
            {
                return RedirectToAction("Home", "Admin"); // redirect to admin home if already logged in
            }

            return View(); // show login page if not logged in
        }

        // POST: Login - Authenticate admin credentials
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

            // set session variables for logged-in admin
            Session["AdminToken"] = token;
            Session["AdminEmail"] = email;
            return RedirectToAction("Home", "Admin"); // redirect to admin home  after successful login
        }

        // home page for logged-in admin
        public ActionResult Home()
        {
            if (Session["AdminToken"] == null)
            {
                return RedirectToAction("Login"); // redirect to login if not logged in
            }

            ViewBag.Message = "Welcome to the Admin Portal.";
            return View(); // Show the home page for logged-in admin
        }

        private async Task<string> GetEmailByUsername(string username)
        {
            FirestoreDb db = FirestoreDb.Create(ProjectId);


            // fetching document as it's saved by username
            DocumentReference docRef = db.Collection("admin").Document(username);
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
    }
}