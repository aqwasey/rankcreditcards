using System.Web;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using CCSortApp.Models;
using CCSortApp.Validators;
using System.IO;
using System.Text.Json;
using CCSortApp.Utility;

namespace CCSortApp.Controllers {
    public class CreditCardController : Controller {
        CardOptions conq = new CardOptions();
        CreditCardService service = new CreditCardService();

        const string UPLOADS = "Uploads";

        /* Methods that emits database actions */
        public JsonResult SingleEntry(string cardNo) {
            // Identify the type of a card
            string identity = conq.CardIdentifier(cardNo);

            // check if card numbers contains invalid characters
            if(identity.Contains("Invalid") || identity.Contains("invalid")) {
                return Json(identity);
            }

            // check if the card type is UNKNOWN
            if(identity != "UNKNOWN") {
                int cardExist = service.existCard(cardNo);
                if (cardExist == 0) {
                    // create a new credit card object and pass values to it
                    CreditCard newCard = new CreditCard(identity, cardNo, DateTime.Now, "Typed-In");
                    // Save or record card number into the database
                    var results = service.addCard(newCard);
                    // return the results from the save action
                    return Json(results);
                }else {
                    // notify user text if card already exist
                    return Json("Sorry! Credit Card already exist");
                }
            }
            // when an UNKNOWN card is found return the result
            return Json(identity);
        }
        
        [HttpGet]
        public JsonResult OverallSummary() {
            List<object> cards = service.getSummary();
            return Json(cards);
        }

        [HttpPost]
        public JsonResult BatchEntry(IFormCollection forco) {
            
            IFormCollection loc = forco;
            IFormFile? rawfile = forco.Files["rawfile"];

            var filePath = Path.Combine(UPLOADS, rawfile!.FileName.ToString());
            new TempFile().create(rawfile, UPLOADS);
            
            // identify the extension of the uploaded file
            FileInfo fi = new FileInfo(filePath);
            string fext = fi.Extension;
            long flen = fi.Length;
            
            // check if file is empty or file has not data in it
            if(flen == 0) {
                return Json("No Card(s) found");
            }

            // define the allowed file extension
            Array allowed_extension = new string[] { ".csv", ".txt" };

            // search througth the allowed extension
            int found_ext = Array.BinarySearch(allowed_extension, fext);

            // check if the return value qualifies the file
            // to be an accepted type
            if (found_ext >= 0) {

                // return or retrieve the overall prepared cards (VALID & INVALID)
                List<object> raw = conq.PrepCreditCards(fi.FullName, found_ext);

                // return the found results from the file
                List<string> allValid_cards = (List<string>) raw[0];
                List<string> allInvalid_cards = (List<string>) raw[1];
                
                // get all credit card numbers already saved in the database for comparison
                List<string> exist_cards = service.cardNums();

                // check the credit card numbers which already exist in the database
                List<string> non_duplicates = conq.Regroup(exist_cards, allValid_cards);

                // obtain the card identifier such AMEX, DISCOVER, MASTERCARD or VISA
                // for each card in the list
                Dictionary<string, string> final_valid_cards = conq.CardsIdentifier(non_duplicates);
                
                // save the clean list of cards in the database
                string status = service.addCardBatch(final_valid_cards);

                // delete temporary file after usage
                new TempFile().trash(fi.FullName);

                if(status.Contains("Added")) {
                    
                    // determine if duplicates were found between the old and new list of cards
                    int dupCount;
                    if (allValid_cards.Count != non_duplicates.Count) {
                        dupCount = non_duplicates.Count;
                    }else {
                        dupCount = 0;
                    }

                    // variables to return results
                    List<Dictionary<string, string>> info = new List<Dictionary<string, string>>();
                    info.Add(new Dictionary<string, string>() {{"status", status }});
                    info.Add(new Dictionary<string, string>() {{"valid", Convert.ToString(allValid_cards.Count)}});
                    info.Add(new Dictionary<string, string>() {{"invalid", Convert.ToString(allInvalid_cards.Count)}});
                    info.Add(new Dictionary<string, string>() {{"duplicates", dupCount.ToString()}});
                    return Json(info); 
                }else {
                    return Json("Error on batch file operation due to \n" + status);
                }
            }else {
                // when a wrong or incorrect file type is uploaded
                return Json("Upload file is not allowed.");
            }
        }

        [HttpGet]
        public ActionResult Index() {
            return View();
        }

        [HttpGet]
        public ActionResult Failed() {
            return View();
        }

        [HttpGet]
        public ActionResult Upload() {
            return View();
        }
        
        [HttpGet]
        public ActionResult<CreditCard> Sort() {
            return View();
        }
        
        [HttpGet]
        public ActionResult<CreditCard> All() {
            List<CreditCard> cards = service.getAll();
            ViewBag.allCards = cards;
            return View();
        }
        
        [HttpGet]
        public ActionResult<Array> Summary() {
            return View();
        }

    }

}