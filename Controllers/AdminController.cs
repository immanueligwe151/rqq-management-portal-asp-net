using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Cloud.Firestore;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using rqq_management_portal_asp_net.Models;

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

        public ActionResult Logout()
        {
            Session.Clear(); // clears all session data
            Session.Abandon(); // abandon the session
            return RedirectToAction("Login", "Home"); // Redirect to general login page
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
            Session["AdminUsername"] = username;
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

        // GET: Create Agent Account
        [HttpGet]
        public ActionResult CreateAgentAccount()
        {
            if (Session["AdminToken"] == null)
            {
                return RedirectToAction("Login", "Home"); //redirect to login if not logged in
            }

            ViewBag.Message = "Create Agent Account";
            return View();
        }

        // POST: Create Agent Account
        [HttpPost]
        public async Task<ActionResult> CreateAgentAccount(CreateAgentAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // creates user in firebase auth
            var uid = await CreateUserInFirebase(model.Email, model.Password);

            // generates username based on account type
            string username = GenerateUsername(model.AccountType);

            // save account details to Firestore
            FirestoreDb db = FirestoreDb.Create(ProjectId);
            DocumentReference docRef = db.Collection(model.AccountType).Document(username);

            await docRef.SetAsync(new
            {
                UID = uid,
                name = model.Name,
                email = model.Email,
                username = username,
            });

            // sends data to the view
            ViewBag.Message = "Account created successfully! Below are the details:";
            ViewBag.Name = model.Name;
            ViewBag.Email = model.Email;
            ViewBag.Username = username;
            ViewBag.AccountType = model.AccountType;
            ViewBag.Password = model.Password;

            return View(); // re-renders the same view with the success message
        }

        private async Task<string> CreateUserInFirebase(string email, string password)
        {
            var payload = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsJsonAsync(
                        $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FirebaseApiKey}",
                        payload
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Firebase error: {errorContent}");
                    }

                    dynamic result = await response.Content.ReadAsAsync<dynamic>();
                    return result.localId; // returns the UID
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating user in Firebase", ex);
                }
            }
        }

        private string GenerateUsername(string accountType)
        {
            Random random = new Random();
            string letters = new string(Enumerable.Range(0, 3).Select(_ => (char)random.Next(65, 91)).ToArray()); // random 3 letters
            string digits = random.Next(1000, 9999).ToString(); // random 4 digits

            if (accountType == "agent")
            {
                return letters.ToLower() + digits;  // format: ab123c
            }
            else if (accountType == "admin")
            {
                return "RK" + digits;  // format: RK1234
            }

            return "Unknown";
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