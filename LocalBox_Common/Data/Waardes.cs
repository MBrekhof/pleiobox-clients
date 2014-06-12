using System;
using System.Collections.Generic;

namespace LocalBox_Common
{
    public class Waardes
    {
        private static Waardes _instance = null;

        public static Waardes Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Waardes();
                return _instance;
            }
        }

        Dictionary<string,string> _stateDict = null;

        Database CreateDbConnection()
        {
            return DataLayer.Instance.DbInstance();
        }

        public Dictionary<string, string> Dictionary
        {
            get
            {
                if (_stateDict == null)
                {
                        Database db = CreateDbConnection();
                        _stateDict = db.GetAllState();
                }

                return _stateDict;
            }
        }

        public string this[string key] {
            get {
                return Dictionary.ContainsKey(key) ? Dictionary[key] : null;
            } set {
                Dictionary[key] = value;
                Save();
            }
        }

        public void Refresh()
        {
            _stateDict = null;
        }

        public void Save()
        {
            if (_stateDict != null)
            {
                    Database db = CreateDbConnection();
                    db.SetAllState(_stateDict);
            }
        }

        public DateTime DatumTijdTokenExpiratie
        {
            get
            {
                DateTime result;
                if (!Dictionary.ContainsKey("datumtijdtokenexpiratie") || !DateTime.TryParse(Dictionary["datumtijdtokenexpiratie"], out result))
                {
                    result = new DateTime(1000, 1, 1);
                    return result;
                }
                return result;
            }
            set
            {
                Dictionary["datumtijdtokenexpiratie"] = value.ToString();
                Save();
            }
        }

        public string AccessToken
        {
            get
            {
                return Dictionary.ContainsKey("accesstoken") ? Dictionary["accesstoken"] : null;
            }
            set
            {
                Dictionary["accesstoken"] = value;
                Save();
            }
        }

        public string RefreshToken
        {
            get
            {
                return Dictionary.ContainsKey("refreshtoken") ? Dictionary["refreshtoken"] : null;
            }
            set
            {
                Dictionary["refreshtoken"] = value;
                Save();
            }
        }

        public int GeselecteerdeBox
        {
            get
            {
                return Dictionary.ContainsKey("geselecteerdebox") ? Int32.Parse(Dictionary["geselecteerdebox"]) : -1;
            }
            set
            {
                Dictionary["geselecteerdebox"] = value.ToString();
                Save();
            }
        }


        public string LaatsteVersie
        {
            get
            {
                return Dictionary.ContainsKey("laatsteVersie") ? Dictionary["laatsteVersie"] : null;
            }
            set
            {
                Dictionary["laatsteVersie"] = value;
                Save();
            }
        }


    }
}

