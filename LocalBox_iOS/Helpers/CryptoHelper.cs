using System;
using LocalBox_Common;
using MonoTouch.UIKit;

namespace LocalBox_iOS.Helpers
{
    public static class CryptoHelper
    {
        public static void ValidateKeyPresence(LocalBox localBox, Action onCompletion) {
            if (localBox.HasCryptoKeys)
            {
                if (!localBox.HasPassPhrase)
                {
                    ShowPassphraseForExistingKey(localBox, onCompletion);
                }
            }
            else
            {
                CreateNewKeyPair(localBox, onCompletion);
            }
        }

        private static void ShowPassphraseForExistingKey(LocalBox localBox, Action onCompletion, bool repeat = false) {
            string message = string.Empty;
            if (repeat)
            {
                message = "Verkeerde passphrase opgegeven, probeer het opnieuw";
            }
            else
            {
                message = "Bij deze LocalBox is al een sleutel aanwezig, geef de passphrase op van deze sleutel";
            }

            UIAlertView createPassPhrase = new UIAlertView("Passphrase", message, null, "Annuleer", "Ok");
            createPassPhrase.AlertViewStyle = UIAlertViewStyle.SecureTextInput;
            createPassPhrase.GetTextField(0).Placeholder = "Passphrase";


            createPassPhrase.Clicked += async (object s, UIButtonEventArgs args) => {
                if(args.ButtonIndex == 0) {
                    DataLayer.Instance.DeleteLocalBox(localBox.Id);
                    if(onCompletion != null) {
                        onCompletion();
                    }
                }else if(args.ButtonIndex == 1) {
                    if(!await BusinessLayer.Instance.ValidatePassPhrase(localBox ,createPassPhrase.GetTextField(0).Text)) {
                        ShowPassphraseForExistingKey(localBox, onCompletion, true);
                    } else {
                        if(onCompletion != null) {
                            onCompletion();
                        }
                    }
                }
            };
            createPassPhrase.Show();
        }

        static void CreateNewKeyPair(LocalBox localBox, Action onCompletion, bool repeat = false)
        {
            string message = repeat ? "De opgegeven passphrases komen niet overeen, probeer het nog eens" : "U moet een passphrase instellen voordat u deze LocalBox kunt gebruiken";


            UIAlertView createPassPhrase = new UIAlertView("Passphrase", message, null, "Annuleer", "Ok");
            createPassPhrase.AlertViewStyle = UIAlertViewStyle.LoginAndPasswordInput;
            var firstField = createPassPhrase.GetTextField(0);
            var secondField = createPassPhrase.GetTextField(1);

            firstField.Placeholder = "Passphrase";
            secondField.Placeholder = "Bevestig Passphrase";

            firstField.AutocorrectionType = UITextAutocorrectionType.No;
            firstField.SecureTextEntry = true;


            createPassPhrase.ShouldEnableFirstOtherButton = (av)=> {
                return av.GetTextField(0).Text.Length > 0 && av.GetTextField(0).Text.Equals(av.GetTextField(1).Text);
            };

            createPassPhrase.Clicked += (object s, UIButtonEventArgs args) => {

                if(args.ButtonIndex == 0){
                    DataLayer.Instance.DeleteLocalBox(localBox.Id);
                    if(onCompletion != null) {
                        onCompletion();
                    }
                }else if(args.ButtonIndex == 1) {
                    if(firstField.Text.Equals(secondField.Text)){
                        DialogHelper.ShowProgressDialog("Beveiliging", "Bezig met het creëren van sleutels", async ()=> {
                            bool result = await BusinessLayer.Instance.SetPublicAndPrivateKey(localBox, firstField.Text); 
                            if(!result) {
                                DataLayer.Instance.DeleteLocalBox(localBox.Id);
                                DialogHelper.ShowErrorDialog("Fout", string.Format("Er is een fout opgetreden bij het toevoegen van de LocalBox {0}. De LocalBox is niet toegevoegd", localBox.Name));
                            } 
                            if(onCompletion != null) {
                                onCompletion();
                            }
                        });
                    } else {
                        CreateNewKeyPair(localBox, onCompletion, true);
                    }
					DialogHelper.HideProgressDialog();
                }
            };
            createPassPhrase.Show();

        }
    }
}

