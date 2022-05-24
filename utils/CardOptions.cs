using CCSortApp.Types;
using System;
using System.Text.RegularExpressions;

namespace CCSortApp.Validators {
    public class CardOptions {
        public List<object> PrepCreditCards(string batch_path, int found_ext) {

            // variables to hold valid and invalid credit cards
            List<string>? valid_cards = new List<string>();
            List<string>? invalid_cards = new List<string>();
            List<string> overall_cards = new List<string>();

            // if found extension value is 0 it means its an CSV
            if(found_ext == 0) {
                overall_cards = readCSVCards(batch_path);
            }else {
                // if found extension value is 1 it means its a TXT
                overall_cards = readTXTCards(batch_path);
            }

            // overall retrieved card records from the file
            foreach(string item in overall_cards) {
                // check if current value contains string
                bool valid_nums = CardNumbersPattern(item);
                if (valid_nums) {
                    // identify which type of a card
                    string rawCard = CardIdentifier(item);
                    if ((new [] {"UNKNOWN", "Invalid Card Format"}).Contains(rawCard)){
                        // remove item from list
                        invalid_cards.Add(item);
                    }else {
                        // if yes add to invalid
                        valid_cards.Add(item);
                    }
                }            
                else {
                    // if yes add to invalid
                    invalid_cards.Add(item);
                }    
            }

            /*
            List<string> tempValid = new List<string>();
            List<string> refineValid = new List<string>();

            for(int i = 0; i < valid_cards.Count; i++) {
                // identify which type of a card
                string rawCard = CardIdentifier(valid_cards[i]);
                // if card returned any of the below results
                // remove from the valid cards list
                if ((new [] {"UNKNOWN", "Invalid Card Format"}).Contains(rawCard)){
                    // add to invalid card list
                    invalid_cards.Add(valid_cards[i]);
                    // remove item from list
                    valid_cards.RemoveAt(i);
                }
            } */

            List<object> final_cards = new List<object> { valid_cards, invalid_cards };
            return final_cards;
        }

        private List<string> readCSVCards(string file_path) {
            
            // variable to hold the entire card list, that will be read from the provided file path
            List<string> found_cards = new List<string>();
            
            // delimiters
            string[] delimiters = new string[] {",", ";"};

            // Check the existence of the file based on the file
            if(File.Exists(file_path) == true) {
                // loop through the text file and read
                // line by line until reach the end of line
                foreach(string line in System.IO.File.ReadLines(@file_path)) {

                    // check if the separator character is comma (,)
                    if (line.Contains(delimiters[0])){

                        // split the long string using the delimiter (,)
                        string[] split_items = line.Split(delimiters[0]);
                        foreach(string item in split_items) {
                            // assign the current value to the array
                            found_cards.Add(item);
                        }
                    // check if the separator character is a semi-colon (;)
                    }else if(line.Contains(delimiters[1])) {

                        // split the long string using the delimiter (;)
                        string[] split_items = line.Split(delimiters[1]);
                        foreach(string item in split_items) {
                            // assign the current value to the array
                            found_cards.Add(item);
                        }

                    }else {
                        // assign the current value to the array
                        found_cards.Add(line); 
                    }
                }
            }
            // return the overall found cards
            // else return the text 'No Card(s) found'
            return found_cards.Count > 0 ? found_cards : new List<string>();
        }

        private List<string> readTXTCards(string file_path) {
            
            // variable to hold the entire card list, that will be read from the provided file path
            List<string> found_cards = new List<string>();

            // Check the existence of the file based on the file
            if(File.Exists(file_path) == true) {

                // loop through the text file and read
                // line by line until reach the end of line
                foreach(string line in System.IO.File.ReadLines(@file_path)) {
                    // assign the current value to the array
                    found_cards.Add(line); 
                }
            }
            // return the overall found cards
            // else return the text 'No Card(s) found'
            return found_cards.Count > 0 ? found_cards : new List<string>();
        }

        public string CardIdentifier(string card_item) {
            // check if the card no is ONLY made up of numbers
            bool onlyNums = CardNumbersPattern(card_item);
            if (!onlyNums)
                return "Invalid Card Format";

            // determine the specific type of a card
            int result = CardCategory(card_item);

            if (result == -1) {
                return "UNKNOWN";
            }else {
                // get the name of the card type found or returned
                string? card_type = Enum.GetName(typeof(CardType), result);

                // return the identified card type
                return card_type;
            }
        }

         public Dictionary<string, string> CardsIdentifier(List<string> cards) {

            Dictionary<string, string> finalCards = new Dictionary<string, string>();
            foreach(string card_item in cards!) {
                 // determine the specific type of a card
                int result = CardCategory(card_item);

                // get the name of the card type found or returned
                string? card_type = Enum.GetName(typeof(CardType), result);
                finalCards.Add(Convert.ToString(card_item), card_type!);
            }
            
            return finalCards;
        }

        private int CardCategory(string item) {
            // obtain or retrieve the first 2 characters 
            // of the card item and convert into a number (integer)
            int card_prefix = Int16.Parse(item.Substring(0, 1));
            int card_prox = Int16.Parse(item.Substring(0,2));
            int card_len = item.Length;
            int found_card = -1;
            
            // using the first character identify which type of a card
            // else return 0 (as an unknown card type)
            switch (card_prefix) {
                case 3:
                    // check if the length is the exact AMEX card length
                    if (card_len == (int)(CardLength.AMEX) & card_prox == 34) { found_card = (int)(CardType.AMEX); }
                    if (card_len == (int)(CardLength.AMEX) & card_prox == 37) { found_card = (int)(CardType.AMEX); }
                    break;

                case 4:
                    // check if the length is the exact VISA card length
                    if (card_len == (int) (CardLength.VISA)) { found_card = (int)(CardType.VISA); }
                    break;

                case 5:
                    // check if the length is the exact MASTERCARD length
                    if (card_len == (int) (CardLength.MASTERCARD)) { found_card = (int)(CardType.MASTERCARD);}
                    break;

                case 6:
                    // check if the length is the exact DISCOVER card length
                    if (card_len == (int) (CardLength.DISCOVER)) { found_card = (int)(CardType.DISCOVER); }
                    break;

                default:
                    // 0 represents an unknown card
                    found_card = 0;
                    break;
            }
            // return the found card type
            return found_card;
        }

        private bool CardNumbersPattern(string item) {
            // define the criteria of the expression
            // item must contain numbers only.
            Regex onlyNumbers = new Regex("^\\d+$");
            return onlyNumbers!.IsMatch(item);
        }
    
        public List<string> Regroup(List<string> original_cards, List<string> outcast) {
            var raw = outcast.Except(original_cards).ToList();
            var samelist = new HashSet<string>(original_cards).SetEquals(outcast);
            return raw;
        }
    }

}