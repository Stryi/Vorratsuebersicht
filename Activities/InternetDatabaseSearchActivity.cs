using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Globalization;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Android.Content.PM;

using Newtonsoft.Json;

namespace VorratsUebersicht
{
    using static Tools;

    [Activity(Label = "https://openfoodfacts.org")]
    public class InternetDatabaseSearchActivity : Activity
    {
        FoodInformation foodInfo = null;
        QuantityAndUnit foodSize = null;
        decimal? kcalPer100 = null;

        public static Bitmap picture = null;
        public string formatedResponseFromServer = string.Empty;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            this.SetContentView(Resource.Layout.InternetDatabaseSearch);

            string eanCode = Intent.GetStringExtra ("EANCode") ?? string.Empty;

            var textView = FindViewById<TextView>(Resource.Id.InternetDatabaseResult_ProgressText);
            textView.Text = this.Resources.GetString(Resource.String.InternetDatabaseSearch_Searching);

            var textInfo = FindViewById<TextView>(Resource.Id.InternetDatabaseResult_Description);
            textInfo.Text = this.Resources.GetString(Resource.String.InternetDatabaseSearch_Info);

            new System.Threading.Thread(new ThreadStart(delegate             
            {
                this.SearchAndShowArticle(eanCode);
            })).Start();

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.InternetDatabaseSearch_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    return true;

                case Resource.Id.InternetDatabaseResult_Save:

                    if (this.foodInfo?.status == 1)
                    {
                        Intent intent = new Intent();
                        intent.PutExtra("Name",       this.foodInfo.product.GetProduktName());
                        intent.PutExtra("Hersteller", this.foodInfo.product.brands);
                        if (this.foodSize != null)
                        {
                            intent.PutExtra("Quantity",   (long)this.foodSize.Quantity);
                            intent.PutExtra("Unit",       this.foodSize.Unit);
                        }
                        if (this.kcalPer100 != null)
                        {
                            intent.PutExtra("KCalPer100",   (long)this.kcalPer100.Value);
                            
                        }

                        this.SetResult(Result.Ok, intent);
                    }
                    else
                    {
                        this.SetResult(Result.Canceled);
                        InternetDatabaseSearchActivity.picture = null;
                    }

                    this.OnBackPressed();
                    return true;

            }

            return false;
        }


        private void SearchAndShowArticle(string eanCode)
        {
            /*
            foodInfo.product = new FoodInformation.Product();
            foodInfo.product.image_url       = "https://static.openfoodfacts.org/images/products/20005016/front_fr.4.400.jpg";
            */

            //eanCode = "22120649";   // Kartoffeleintopf Mit Würstchen Und Rauchspeck, mit Mengenangabe, und KCal/100g
            //eanCode = "88888888888";  // Not Found
            //eanCode = "4000462810052";  // Goldpuder Weizenmehl Type 405 1 KG
            //eanCode = "5410673854001";  // Reis, mit Einheit und Nährstoff

            string title = string.Empty;
            string info = string.Empty;

            InternetDatabaseSearchActivity.picture = null;
            this.formatedResponseFromServer = string.Empty;
            this.foodInfo = null;
            this.foodSize = null;
            this.kcalPer100 = null;

            var eanCodeView  = FindViewById<TextView>(Resource.Id.InternetDatabaseResult_EanCode);
            var progressText = FindViewById<TextView>(Resource.Id.InternetDatabaseResult_ProgressText);
            var progressBar  = FindViewById<ProgressBar>(Resource.Id.InternetDatabaseResult_Progress);
            
            try
            {
                string[] eanCodeList = eanCode.Split(",");
                foreach(string ean in eanCodeList)
                {
                    RunOnUiThread(() => eanCodeView.Text = string.Format(this.Resources.GetString(Resource.String.InternetDatabaseSearch_EANCode), ean));
                    
                    this.foodInfo = this.GetFoodInformation(ean);

                    if (foodInfo.status == 1)       // Artikelangaben gefunden?
                        break;
                }

            }
            catch(Exception ex)
            {
                TRACE(ex);

                title = ex.Message;

                if (ex.InnerException != null)
                {
                    TRACE(ex.InnerException);
                }

            }

            RunOnUiThread(() =>
            {
                eanCodeView.Text = string.Empty;

                if (this.foodInfo != null)
                {
                    title = string.Format(this.Resources.GetString(Resource.String.InternetDatabaseSearch_EANCode), this.foodInfo.code);

                    if (this.foodInfo.status == 1)
                    {
                        info += string.Format("{0}\n{1}\n\n", this.Resources.GetString(Resource.String.ArticleDetails_ArticleName),  this.foodInfo.product.GetProduktName());
                        info += string.Format("{0}\n{1}\n\n", this.Resources.GetString(Resource.String.ArticleDetails_Manufacturer), this.foodInfo.product.brands);

                        this.foodSize = QuantityAndUnit.Parse(this.foodInfo.product.quantity);
                        if (this.foodSize != null)
                        {
                            string content = this.Resources.GetString(Resource.String.InternetDatabaseSearch_Size);
                            info += string.Format(CultureInfo.CurrentUICulture, content,  this.foodSize.Quantity, this.foodSize.Unit);
                        }
                        else
                        {
                            string unknownSize = this.Resources.GetString(Resource.String.InternetDatabaseSearch_UnknownSize);
                            info += string.Format(unknownSize, this.foodInfo.product.quantity);
                        }

                        if (foodInfo.product.nutriments != null)
                        {
                            /*
                            info += string.Format("Nährwert: {0} {1} pro 100g\n", 
                                this.foodInfo.product.nutriments.energy_100g,
                                this.foodInfo.product.nutriments.energy_unit);
                            */

                            string caloriePer100gInfo = this.Resources.GetString(Resource.String.InternetDatabaseSearch_CaloriesPerUnit);

                            if (string.Compare(this.foodInfo.product.nutriments.energy_unit, "kcal", true) == 0)
                            {
                                this.kcalPer100 = this.foodInfo.product.nutriments.energy_value;
                                info += "\n\n";
                                info += string.Format(caloriePer100gInfo, this.kcalPer100);
                            }

                            if (string.Compare(this.foodInfo.product.nutriments.energy_unit, "kJ", true) == 0)
                            {
                                // kcal = kJ / 4,184 dividieren
                                this.kcalPer100 = this.foodInfo.product.nutriments.energy_100g / 4.184m;
                                this.kcalPer100 = Math.Round(this.kcalPer100.Value, 0);

                                info += "\n\n";
                                info += string.Format(caloriePer100gInfo, this.kcalPer100);
                            }
                        }

                        var languageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                        eanCodeView.Text = "Link: https://" + languageCode + ".openfoodfacts.org/product/" + this.foodInfo.code;
                    }
                    else
                    {
                        info += string.Format("Status: {0} - {1}\n\n", this.foodInfo.status, this.foodInfo.status_verbose);
                        info += string.Format("Response: {0}",   this.formatedResponseFromServer);
                    }
                }

                progressText.Text = title;
                progressBar.Visibility = ViewStates.Gone;

                var textView = FindViewById<TextView>(Resource.Id.InternetDatabaseResult_Text);
                if (textView == null)
                    return;

                textView.Text = info;

                if (this.foodInfo?.product != null)
                {
                    InternetDatabaseSearchActivity.picture = this.GetUrlPicture(this.foodInfo.product.image_url);

                    var imageView = FindViewById<ImageView>(Resource.Id.InternetDatabaseResult_Image);
                    imageView.SetImageBitmap(InternetDatabaseSearchActivity.picture);
                }
            });
        }

        private FoodInformation GetFoodInformation(string eanCode)
        {
            string parameter;
            
            parameter = "?fields=product_name,product_name_de,brands,quantity,nutriments,image_url";

            string server = "https://world.openfoodfacts.org/api/v0/product/";
            string request = string.Format("{0}{1}.json", server, eanCode);
            if (!string.IsNullOrEmpty(parameter))
            {
                request += parameter;
            }

            TRACE("WebRequest: {0}", request);
            Context context = this.ApplicationContext;
            PackageInfo info = context.PackageManager.GetPackageInfo(context.PackageName, 0);

            string agentInfo = "Vorratsübersicht - Android - Version " + info.VersionName + " - https://sites.google.com/site/vorratsuebersicht";

            WebRequest webRequest = WebRequest.Create(request);
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            webRequest.Headers.Add("UserAgent", agentInfo);
            webRequest.Timeout = 10000;

            if (ServicePointManager.ServerCertificateValidationCallback == null)
            {
                ServicePointManager.ServerCertificateValidationCallback += 
                    (sender, certificate, chain, sslPolicy) => { return true; };
            }

            WebResponse response = webRequest.GetResponse();

            string webResponse = string.Empty;

            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                webResponse = reader.ReadToEnd();
                TRACE("Response from OpenFoodFacts.org (max 1024 Zeichen):\n{0}", webResponse);
            }
            
            response.Close();

            var foodInfo = JsonConvert.DeserializeObject<FoodInformation>(webResponse);

            // Formatierte Ausgabe
            var allInfo = JsonConvert.DeserializeObject(webResponse);
            this.formatedResponseFromServer = JsonConvert.SerializeObject(allInfo, Formatting.Indented);
            TRACE("JSon Antwort vom Server OpenFoodFacts.org:\n\n{0}", this.formatedResponseFromServer);

            return foodInfo;
        }

        private Bitmap GetUrlPicture(string imageUrl)
        {
            Bitmap bitmap = null;

            if (string.IsNullOrEmpty(imageUrl))
                return bitmap;

            WebRequest webRequest = WebRequest.Create(imageUrl);
            webRequest.Credentials = CredentialCache.DefaultCredentials;

            WebResponse response = webRequest.GetResponse();

            byte[] bytes = null;

            using (Stream dataStream = response.GetResponseStream())
            using( MemoryStream ms = new MemoryStream())
            {
                int count = 0;
                do
                {
                    byte[] buf = new byte[1024];
                    count = dataStream.Read(buf, 0, 1024);
                    ms.Write(buf, 0, count);
                } while(dataStream.CanRead && count > 0);

                bytes = ms.ToArray();
            }
            bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            response.Close();

            return bitmap;
        }
    }

    public class FoodInformation
    {
        public string code;
        public int status;
        public string status_verbose;
        public Product product;

        public class Product
        {
            public string product_name;
            public string product_name_de;
            public string brands;
            public string code;
            public string image_url;

            public string quantity;
            public int product_quantity;

            // Nährstoffe
            public Nutriments nutriments;

            internal string GetProduktName()
            {
                // Vorzugsweise die Deutsche Sprache nehmen. Beispiel: https://world.openfoodfacts.org/api/v0/product/7610200068989.json
                if (!string.IsNullOrEmpty(this.product_name_de))
                    return this.product_name_de;

                return this.product_name;
            }
        }

        public class Nutriments
        {
            public string energy_unit;
            public decimal energy_value;  // kcal pro 100 g oder 100 ml
            public decimal energy_100g;   // {energy_unit} pro 100 g oder 100 ml  
        }
    }
}