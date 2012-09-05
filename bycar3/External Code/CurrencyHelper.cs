using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bycar;

namespace bycar3.External_Code
{
    class CurrencyHelper
    {
        public static string BasicCurrencyCode
        {
            get
            {
                DataAccess db = new DataAccess();
                return db.getBasicCurrencyCode();
            }
        }

        public static decimal GetBasicPrice(string CurrencyCode, decimal Price)
        {
            decimal BasicPrice = 0;
            DataAccess da = new DataAccess();
            string BasicCurrencyCode = da.getBasicCurrencyCode();
            decimal RateUsd = da.getCurrencyRate("USD").rate;
            decimal RateEur = da.getCurrencyRate("EUR").rate;
            decimal RateRur = da.getCurrencyRate("RUR").rate;

            if (BasicCurrencyCode.Equals(CurrencyCode))
            {
                BasicPrice = Price;
            }
            else
            {
                if (BasicCurrencyCode.Contains("USD"))
                {
                    if (CurrencyCode.Contains("BYR"))
                    {
                        BasicPrice = Price / RateUsd;
                    }
                    if (CurrencyCode.Contains("EUR"))
                    {
                        BasicPrice = Price * RateEur / RateUsd;
                    }
                    if (CurrencyCode.Contains("RUR"))
                    {
                        BasicPrice = Price * RateRur / RateUsd;
                    }
                }
                if (BasicCurrencyCode.Contains("EUR"))
                {
                    if (CurrencyCode.Contains("BYR"))
                    {
                        BasicPrice = Price / RateEur;
                    }
                    if (CurrencyCode.Contains("USD"))
                    {
                        BasicPrice = Price * RateUsd / RateEur;
                    }
                    if (CurrencyCode.Contains("RUR"))
                    {
                        BasicPrice = Price * RateRur / RateEur;
                    }
                }

                if (BasicCurrencyCode.Contains("RUR"))
                {
                    if (CurrencyCode.Contains("BYR"))
                    {
                        BasicPrice = Price / RateRur;
                    }
                    if (CurrencyCode.Contains("EUR"))
                    {
                        BasicPrice = Price * RateEur / RateRur;
                    }
                    if (CurrencyCode.Contains("USD"))
                    {
                        BasicPrice = Price * RateUsd / RateRur;
                    }
                }

                if (BasicCurrencyCode.Contains("BYR"))
                {
                    if (CurrencyCode.Contains("USD"))
                    {
                        BasicPrice = Price * RateUsd;
                    }
                    if (CurrencyCode.Contains("EUR"))
                    {
                        BasicPrice = Price * RateEur;
                    }
                    if (CurrencyCode.Contains("RUR"))
                    {
                        BasicPrice = Price * RateRur;
                    }
                }
            }
            return BasicPrice;
        }

        public static decimal GetPrice(string CurrencyCode, decimal BasicPrice)
        {
            decimal Price = 0;
            DataAccess da = new DataAccess();
            string BasicCurrencyCode = da.getBasicCurrencyCode();
            decimal RateUsd = da.getCurrencyRate("USD").rate;
            decimal RateEur = da.getCurrencyRate("EUR").rate;
            decimal RateRur = da.getCurrencyRate("RUR").rate;

            if (BasicCurrencyCode.Equals(CurrencyCode))
            {
                Price = BasicPrice;
            }
            else
            {
                if (BasicCurrencyCode.Contains("USD"))
                {
                    if (CurrencyCode.Contains("BYR"))
                    {
                        Price = BasicPrice * RateUsd;                        
                    }
                    if (CurrencyCode.Contains("EUR"))
                    {                        
                        Price = BasicPrice * RateUsd / RateEur;
                    }
                    if (CurrencyCode.Contains("RUR"))
                    {                        
                        Price = BasicPrice * RateUsd / RateRur;
                    }
                }
                if (BasicCurrencyCode.Contains("EUR"))
                {
                    if (CurrencyCode.Contains("BYR"))
                    {
                        Price = BasicPrice * RateEur;
                    }
                    if (CurrencyCode.Contains("USD"))
                    {                        
                        Price = BasicPrice * RateEur / RateUsd;
                    }
                    if (CurrencyCode.Contains("RUR"))
                    {                        
                        Price = BasicPrice * RateEur / RateRur;
                    }
                }

                if (BasicCurrencyCode.Contains("RUR"))
                {
                    if (CurrencyCode.Contains("BYR"))
                    {                        
                        Price = BasicPrice * RateRur;
                    }
                    if (CurrencyCode.Contains("EUR"))
                    {                        
                        Price = BasicPrice * RateRur / RateEur;
                    }
                    if (CurrencyCode.Contains("USD"))
                    {                        
                        Price = BasicPrice * RateRur / RateUsd;
                    }
                }

                if (BasicCurrencyCode.Contains("BYR"))
                {
                    if (CurrencyCode.Contains("USD"))
                    {
                        Price = BasicPrice / RateUsd;
                    }
                    if (CurrencyCode.Contains("EUR"))
                    {                        
                        Price = BasicPrice / RateEur;
                    }
                    if (CurrencyCode.Contains("RUR"))
                    {                        
                        Price = BasicPrice / RateRur; 
                    }
                }
            }
            if (CurrencyCode.Contains("BYR"))
                return Math.Round(Price);
            else
                return Math.Round(Price, 2);
        }
    }
}

