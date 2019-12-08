using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Arbitrage01.Models;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Arbitrage01.Entities;
using VcmDeneme.Entities;

namespace Arbitrage01.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        WebClient c = new WebClient();

        public IActionResult Binance()
        {
            string allData = c.DownloadString("https://www.binance.com/api/v3/ticker/24hr");

            JArray allDataArray = JArray.Parse(allData);
            BRatio ethBtc = JsonConvert.DeserializeObject<BRatio>(allDataArray[0].ToString());
            decimal ethBtcLastPrice = Convert.ToDecimal(ethBtc.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
            BRatio bnbEth = JsonConvert.DeserializeObject<BRatio>(allDataArray[10].ToString());
            decimal bnbEthLastPrice = Convert.ToDecimal(bnbEth.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
            BRatio ethUsdt = JsonConvert.DeserializeObject<BRatio>(allDataArray[12].ToString());
            decimal ethUsdtLastPrice = Convert.ToDecimal(ethUsdt.LastPrice, System.Globalization.CultureInfo.InvariantCulture);


            List<RatioModel> bRatiosOverBTC = new List<RatioModel>();
            List<BRatio> bRatiosOverETH = new List<BRatio>();
            List<RatioModel> bRatiosOverBNB = new List<RatioModel>();
            List<RatioModel> bRatiosOverUSDT = new List<RatioModel>();
            foreach (var item in allDataArray)
            {
                BRatio bRatio = JsonConvert.DeserializeObject<BRatio>(item.ToString());

                if (bRatio.Symbol.Substring(bRatio.Symbol.Length - 3).Contains("BTC"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = bRatio.Symbol,
                        LastPrice = bRatio.LastPrice,
                        QuoteVolume = bRatio.QuoteVolume
                    };

                    bRatiosOverBTC.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value1 = (ratioModelLastPrice / ethBtcLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (bRatio.Symbol.Substring(bRatio.Symbol.Length - 3).Contains("ETH"))
                {
                    bRatiosOverETH.Add(bRatio);
                    //decimal ratioModelLastPrice = Convert.ToDecimal(bRatio.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                }

                if (bRatio.Symbol.Substring(bRatio.Symbol.Length - 3).Contains("BNB"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = bRatio.Symbol,
                        LastPrice = bRatio.LastPrice,
                        QuoteVolume = bRatio.QuoteVolume
                    };
                    bRatiosOverBNB.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value3 = (ratioModelLastPrice * bnbEthLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (bRatio.Symbol.Substring(bRatio.Symbol.Length - 4).Contains("USDT"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = bRatio.Symbol,
                        LastPrice = bRatio.LastPrice,
                        QuoteVolume = bRatio.QuoteVolume
                    };
                    bRatiosOverUSDT.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value4 = (ratioModelLastPrice / ethUsdtLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            IEnumerable<RatioModel> bRatiosOverBtcInOrder = bRatiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.QuoteVolume)).Reverse();

            IDictionary<string, RatioModel> btcDict = new Dictionary<string, RatioModel>();

            foreach (var item in bRatiosOverBtcInOrder)
            {
                btcDict.Add(item.Symbol.Substring(0, item.Symbol.Length - 3), item);
            }

            foreach (var ethItem in bRatiosOverETH)
            {
                if (btcDict.Keys.Contains(ethItem.Symbol.Substring(0, ethItem.Symbol.Length - 3)))
                {
                    btcDict[ethItem.Symbol.Substring(0, ethItem.Symbol.Length - 3)].XxxEth = ethItem.LastPrice;
                }
            }

            foreach (var bnbItem in bRatiosOverBNB)
            {
                if (btcDict.Keys.Contains(bnbItem.Symbol.Substring(0, bnbItem.Symbol.Length - 3)))
                {
                    btcDict[bnbItem.Symbol.Substring(0, bnbItem.Symbol.Length - 3)].Value3 = bnbItem.Value3;
                }
            }

            foreach (var usdtItem in bRatiosOverUSDT)
            {
                if (btcDict.Keys.Contains(usdtItem.Symbol.Substring(0, usdtItem.Symbol.Length - 4)))
                {
                    btcDict[usdtItem.Symbol.Substring(0, usdtItem.Symbol.Length - 4)].Value4 = usdtItem.Value4;
                }
            }

            foreach (var ratioModel in bRatiosOverBtcInOrder)
            {
                if (Convert.ToDecimal(ratioModel.QuoteVolume, System.Globalization.CultureInfo.InvariantCulture) > 0)
                {

                    decimal[] valuesInRow = { Convert.ToDecimal(ratioModel.Value1, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.XxxEth, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value3, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value4, System.Globalization.CultureInfo.InvariantCulture) };

                    decimal[] valuesInRowNoZero = { };
                    int i = 0;

                    foreach (decimal value in valuesInRow)

                        if (value != 0)
                        {
                            Array.Resize(ref valuesInRowNoZero, i + 1);
                            valuesInRowNoZero[i] = value;
                            i++;
                        };

                    int indexOfMin = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Min());
                    int indexOfMax = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Max());
                    int[] rankOfIndex = { indexOfMin, indexOfMax };
                    decimal difference = valuesInRowNoZero[rankOfIndex.Min()] - valuesInRowNoZero[rankOfIndex.Max()];
                    ratioModel.Difference = difference.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.ResultValue = (difference / valuesInRowNoZero[rankOfIndex.Min()]).ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    ratioModel.ResultValue = "0";
                }
            }

            //IEnumerable<RatioModel> bRatiosOverBtcOrderedByResultValue = bRatiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.ResultValue)).Reverse();

            return View(bRatiosOverBtcInOrder);
        }

        public IActionResult Binance30()
        {
            string allData = c.DownloadString("https://www.binance.com/api/v3/ticker/24hr");

            JArray allDataArray = JArray.Parse(allData);
            BRatio ethBtc = JsonConvert.DeserializeObject<BRatio>(allDataArray[0].ToString());
            decimal ethBtcLastPrice = Convert.ToDecimal(ethBtc.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
            BRatio bnbEth = JsonConvert.DeserializeObject<BRatio>(allDataArray[10].ToString());
            decimal bnbEthLastPrice = Convert.ToDecimal(bnbEth.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
            BRatio ethUsdt = JsonConvert.DeserializeObject<BRatio>(allDataArray[12].ToString());
            decimal ethUsdtLastPrice = Convert.ToDecimal(ethUsdt.LastPrice, System.Globalization.CultureInfo.InvariantCulture);


            List<RatioModel> bRatiosOverBTC = new List<RatioModel>();
            List<BRatio> bRatiosOverETH = new List<BRatio>();
            List<RatioModel> bRatiosOverBNB = new List<RatioModel>();
            List<RatioModel> bRatiosOverUSDT = new List<RatioModel>();
            foreach (var item in allDataArray)
            {
                BRatio bRatio = JsonConvert.DeserializeObject<BRatio>(item.ToString());

                if (bRatio.Symbol.Substring(bRatio.Symbol.Length - 3).Contains("BTC"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = bRatio.Symbol,
                        LastPrice = bRatio.LastPrice,
                        QuoteVolume = bRatio.QuoteVolume
                    };

                    bRatiosOverBTC.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value1 = (ratioModelLastPrice / ethBtcLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (bRatio.Symbol.Substring(bRatio.Symbol.Length - 3).Contains("ETH"))
                {
                    bRatiosOverETH.Add(bRatio);
                    //decimal ratioModelLastPrice = Convert.ToDecimal(bRatio.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                }

                if (bRatio.Symbol.Substring(bRatio.Symbol.Length - 3).Contains("BNB"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = bRatio.Symbol,
                        LastPrice = bRatio.LastPrice,
                        QuoteVolume = bRatio.QuoteVolume
                    };
                    bRatiosOverBNB.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value3 = (ratioModelLastPrice * bnbEthLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (bRatio.Symbol.Substring(bRatio.Symbol.Length - 4).Contains("USDT"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = bRatio.Symbol,
                        LastPrice = bRatio.LastPrice,
                        QuoteVolume = bRatio.QuoteVolume
                    };
                    bRatiosOverUSDT.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value4 = (ratioModelLastPrice / ethUsdtLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            IEnumerable<RatioModel> bRatiosOverBtcInOrder = bRatiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.QuoteVolume)).Reverse();

            IDictionary<string, RatioModel> btcDict = new Dictionary<string, RatioModel>();

            foreach (var item in bRatiosOverBtcInOrder)
            {
                btcDict.Add(item.Symbol.Substring(0, item.Symbol.Length - 3), item);
            }

            foreach (var ethItem in bRatiosOverETH)
            {
                if (btcDict.Keys.Contains(ethItem.Symbol.Substring(0, ethItem.Symbol.Length - 3)))
                {
                    btcDict[ethItem.Symbol.Substring(0, ethItem.Symbol.Length - 3)].XxxEth = ethItem.LastPrice;
                }
            }

            foreach (var bnbItem in bRatiosOverBNB)
            {
                if (btcDict.Keys.Contains(bnbItem.Symbol.Substring(0, bnbItem.Symbol.Length - 3)))
                {
                    btcDict[bnbItem.Symbol.Substring(0, bnbItem.Symbol.Length - 3)].Value3 = bnbItem.Value3;
                }
            }

            foreach (var usdtItem in bRatiosOverUSDT)
            {
                if (btcDict.Keys.Contains(usdtItem.Symbol.Substring(0, usdtItem.Symbol.Length - 4)))
                {
                    btcDict[usdtItem.Symbol.Substring(0, usdtItem.Symbol.Length - 4)].Value4 = usdtItem.Value4;
                }
            }

            foreach (var ratioModel in bRatiosOverBtcInOrder)
            {
                if (Convert.ToDecimal(ratioModel.QuoteVolume, System.Globalization.CultureInfo.InvariantCulture) > 0)
                {

                    decimal[] valuesInRow = { Convert.ToDecimal(ratioModel.Value1, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.XxxEth, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value3, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value4, System.Globalization.CultureInfo.InvariantCulture) };

                    decimal[] valuesInRowNoZero = { };
                    int i = 0;

                    foreach (decimal value in valuesInRow)

                        if (value != 0)
                        {
                            Array.Resize(ref valuesInRowNoZero, i + 1);
                            valuesInRowNoZero[i] = value;
                            i++;
                        };

                    int indexOfMin = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Min());
                    int indexOfMax = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Max());
                    int[] rankOfIndex = { indexOfMin, indexOfMax };
                    decimal difference = valuesInRowNoZero[rankOfIndex.Min()] - valuesInRowNoZero[rankOfIndex.Max()];
                    ratioModel.Difference = difference.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.ResultValue = (difference / valuesInRowNoZero[rankOfIndex.Min()]).ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    ratioModel.ResultValue = "0";
                }
            }

            IEnumerable<RatioModel> bRatiosOverBtcOrderedByResultValue = bRatiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.ResultValue));
            IEnumerable<RatioModel> bRatiosOverBtcOrderedByResultValueLast30 = bRatiosOverBtcOrderedByResultValue.Take(30);
            IEnumerable<RatioModel> bRatiosOverBtcOrderedByResultValueTop30 = bRatiosOverBtcOrderedByResultValue.Reverse().Take(30);
            IEnumerable<RatioModel> bRatiosOverBtcOrderedByResultValueTop30Last30 = bRatiosOverBtcOrderedByResultValueTop30.Concat(bRatiosOverBtcOrderedByResultValueLast30.Reverse());

            return View(bRatiosOverBtcOrderedByResultValueTop30Last30);
        }

        public IActionResult Bitz()
        {
            string allData = c.DownloadString("https://api.bitzapi.com/Market/tickerall");

            JArray allDataArray = JArray.Parse("[" + allData + "]");
            BitZJson bitZJson = JsonConvert.DeserializeObject<BitZJson>(allDataArray[0].ToString());

            JArray dataArray = JArray.Parse("[" + bitZJson.data.ToString() + "]");
            Data data = JsonConvert.DeserializeObject<Data>(dataArray[0].ToString());

            List<Ratio> ratiosList = new List<Ratio>();

            foreach (KeyValuePair<string, JToken> item in (JObject)bitZJson.data)
            {
                Ratio nesne = JsonConvert.DeserializeObject<Ratio>(item.Value.ToString());
                ratiosList.Add(nesne);
            }
            // Information about the process above: https://www.newtonsoft.com/json/help/html/JObjectProperties.htm

            Ratio btcDkkt = ratiosList[3];
            decimal btcDkktLastPrice = Convert.ToDecimal(btcDkkt.Now, System.Globalization.CultureInfo.InvariantCulture);
            Ratio ethDkkt = ratiosList[4];
            decimal ethDkktLastPrice = Convert.ToDecimal(ethDkkt.Now, System.Globalization.CultureInfo.InvariantCulture);
            Ratio usdtDkkt = ratiosList[1];
            decimal usdtDkktLastPrice = Convert.ToDecimal(usdtDkkt.Now, System.Globalization.CultureInfo.InvariantCulture);



            List<RatioModel> ratiosOverBTC = new List<RatioModel>();
            List<RatioModel> ratiosOverETH = new List<RatioModel>();
            List<RatioModel> ratiosOverUSDT = new List<RatioModel>();
            List<Ratio> ratiosOverDKKT = new List<Ratio>();
            foreach (var ratio in ratiosList)
            {
                //BRatio bRatio = JsonConvert.DeserializeObject<BRatio>(item.ToString());

                if (ratio.Symbol.Substring(ratio.Symbol.Length - 3).Contains("btc"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = ratio.Symbol,
                        LastPrice = ratio.Now,
                        QuoteVolume = ratio.QuoteVolume
                    };

                    ratiosOverBTC.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value1 = (ratioModelLastPrice * btcDkktLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (ratio.Symbol.Substring(ratio.Symbol.Length - 3).Contains("eth"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = ratio.Symbol,
                        LastPrice = ratio.Now,
                        QuoteVolume = ratio.QuoteVolume
                    };
                    ratiosOverETH.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value2 = (ratioModelLastPrice * ethDkktLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (ratio.Symbol.Substring(ratio.Symbol.Length - 4).Contains("usdt"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = ratio.Symbol,
                        LastPrice = ratio.Now,
                        QuoteVolume = ratio.QuoteVolume
                    };
                    ratiosOverUSDT.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value3 = (ratioModelLastPrice * usdtDkktLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (ratio.Symbol.Substring(ratio.Symbol.Length - 4).Contains("dkkt"))
                {
                    ratiosOverDKKT.Add(ratio);
                    //decimal ratioModelLastPrice = Convert.ToDecimal(bRatio.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            IEnumerable<RatioModel> ratiosOverBtcInOrder = ratiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.QuoteVolume)).Reverse();

            IDictionary<string, RatioModel> btcDict = new Dictionary<string, RatioModel>();

            foreach (var item in ratiosOverBtcInOrder)
            {
                btcDict.Add(item.Symbol.Substring(0, item.Symbol.Length - 4), item);
            }

            foreach (var ethItem in ratiosOverETH)
            {
                if (btcDict.Keys.Contains(ethItem.Symbol.Substring(0, ethItem.Symbol.Length - 4)))
                {
                    btcDict[ethItem.Symbol.Substring(0, ethItem.Symbol.Length - 4)].Value2 = ethItem.Value2;
                }
            }

            foreach (var usdtItem in ratiosOverUSDT)
            {
                if (btcDict.Keys.Contains(usdtItem.Symbol.Substring(0, usdtItem.Symbol.Length - 5)))
                {
                    btcDict[usdtItem.Symbol.Substring(0, usdtItem.Symbol.Length - 5)].Value3 = usdtItem.Value3;
                }
            }

            foreach (var dkktItem in ratiosOverDKKT)
            {
                if (btcDict.Keys.Contains(dkktItem.Symbol.Substring(0, dkktItem.Symbol.Length - 5)))
                {
                    btcDict[dkktItem.Symbol.Substring(0, dkktItem.Symbol.Length - 5)].Value4 = dkktItem.Now;
                }
            }

            foreach (var ratioModel in ratiosOverBtcInOrder)
            {
                if (Convert.ToDecimal(ratioModel.QuoteVolume, System.Globalization.CultureInfo.InvariantCulture) > 0)
                {

                    decimal[] valuesInRow = { Convert.ToDecimal(ratioModel.Value1, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value2, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value3, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value4, System.Globalization.CultureInfo.InvariantCulture) };

                    decimal[] valuesInRowNoZero = { };
                    int i = 0;

                    foreach (decimal value in valuesInRow)

                        if (value != 0)
                        {
                            Array.Resize(ref valuesInRowNoZero, i + 1);
                            valuesInRowNoZero[i] = value;
                            i++;
                        };

                    int indexOfMin = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Min());
                    int indexOfMax = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Max());
                    int[] rankOfIndex = { indexOfMin, indexOfMax };
                    decimal difference = valuesInRowNoZero[rankOfIndex.Min()] - valuesInRowNoZero[rankOfIndex.Max()];
                    ratioModel.Difference = difference.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.ResultValue = (difference / valuesInRowNoZero[rankOfIndex.Min()]).ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    ratioModel.ResultValue = "0";
                }
            }

            return View(ratiosOverBtcInOrder);
        }

        public IActionResult Bitz10()
        {
            string allData = c.DownloadString("https://api.bitzapi.com/Market/tickerall");

            JArray allDataArray = JArray.Parse("[" + allData + "]");
            BitZJson bitZJson = JsonConvert.DeserializeObject<BitZJson>(allDataArray[0].ToString());

            JArray dataArray = JArray.Parse("[" + bitZJson.data.ToString() + "]");
            Data data = JsonConvert.DeserializeObject<Data>(dataArray[0].ToString());

            List<Ratio> ratiosList = new List<Ratio>();

            foreach (KeyValuePair<string, JToken> item in (JObject)bitZJson.data)
            {
                Ratio nesne = JsonConvert.DeserializeObject<Ratio>(item.Value.ToString());
                ratiosList.Add(nesne);
            }
            // Information about the process above: https://www.newtonsoft.com/json/help/html/JObjectProperties.htm

            Ratio btcDkkt = ratiosList[3];
            decimal btcDkktLastPrice = Convert.ToDecimal(btcDkkt.Now, System.Globalization.CultureInfo.InvariantCulture);
            Ratio ethDkkt = ratiosList[4];
            decimal ethDkktLastPrice = Convert.ToDecimal(ethDkkt.Now, System.Globalization.CultureInfo.InvariantCulture);
            Ratio usdtDkkt = ratiosList[1];
            decimal usdtDkktLastPrice = Convert.ToDecimal(usdtDkkt.Now, System.Globalization.CultureInfo.InvariantCulture);



            List<RatioModel> ratiosOverBTC = new List<RatioModel>();
            List<RatioModel> ratiosOverETH = new List<RatioModel>();
            List<RatioModel> ratiosOverUSDT = new List<RatioModel>();
            List<Ratio> ratiosOverDKKT = new List<Ratio>();
            foreach (var ratio in ratiosList)
            {
                //BRatio bRatio = JsonConvert.DeserializeObject<BRatio>(item.ToString());

                if (ratio.Symbol.Substring(ratio.Symbol.Length - 3).Contains("btc"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = ratio.Symbol,
                        LastPrice = ratio.Now,
                        QuoteVolume = ratio.QuoteVolume
                    };

                    ratiosOverBTC.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value1 = (ratioModelLastPrice * btcDkktLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (ratio.Symbol.Substring(ratio.Symbol.Length - 3).Contains("eth"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = ratio.Symbol,
                        LastPrice = ratio.Now,
                        QuoteVolume = ratio.QuoteVolume
                    };
                    ratiosOverETH.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value2 = (ratioModelLastPrice * ethDkktLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (ratio.Symbol.Substring(ratio.Symbol.Length - 4).Contains("usdt"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = ratio.Symbol,
                        LastPrice = ratio.Now,
                        QuoteVolume = ratio.QuoteVolume
                    };
                    ratiosOverUSDT.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.Value3 = (ratioModelLastPrice * usdtDkktLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (ratio.Symbol.Substring(ratio.Symbol.Length - 4).Contains("dkkt"))
                {
                    ratiosOverDKKT.Add(ratio);
                    //decimal ratioModelLastPrice = Convert.ToDecimal(bRatio.LastPrice, System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            IEnumerable<RatioModel> ratiosOverBtcInOrder = ratiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.QuoteVolume)).Reverse();

            IDictionary<string, RatioModel> btcDict = new Dictionary<string, RatioModel>();

            foreach (var item in ratiosOverBtcInOrder)
            {
                btcDict.Add(item.Symbol.Substring(0, item.Symbol.Length - 4), item);
            }

            foreach (var ethItem in ratiosOverETH)
            {
                if (btcDict.Keys.Contains(ethItem.Symbol.Substring(0, ethItem.Symbol.Length - 4)))
                {
                    btcDict[ethItem.Symbol.Substring(0, ethItem.Symbol.Length - 4)].Value2 = ethItem.Value2;
                }
            }

            foreach (var usdtItem in ratiosOverUSDT)
            {
                if (btcDict.Keys.Contains(usdtItem.Symbol.Substring(0, usdtItem.Symbol.Length - 5)))
                {
                    btcDict[usdtItem.Symbol.Substring(0, usdtItem.Symbol.Length - 5)].Value3 = usdtItem.Value3;
                }
            }

            foreach (var dkktItem in ratiosOverDKKT)
            {
                if (btcDict.Keys.Contains(dkktItem.Symbol.Substring(0, dkktItem.Symbol.Length - 5)))
                {
                    btcDict[dkktItem.Symbol.Substring(0, dkktItem.Symbol.Length - 5)].Value4 = dkktItem.Now;
                }
            }

            foreach (var ratioModel in ratiosOverBtcInOrder)
            {
                if (Convert.ToDecimal(ratioModel.QuoteVolume, System.Globalization.CultureInfo.InvariantCulture) > 0)
                {

                    decimal[] valuesInRow = { Convert.ToDecimal(ratioModel.Value1, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value2, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value3, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value4, System.Globalization.CultureInfo.InvariantCulture) };

                    decimal[] valuesInRowNoZero = { };
                    int i = 0;

                    foreach (decimal value in valuesInRow)

                        if (value != 0)
                        {
                            Array.Resize(ref valuesInRowNoZero, i + 1);
                            valuesInRowNoZero[i] = value;
                            i++;
                        };

                    int indexOfMin = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Min());
                    int indexOfMax = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Max());
                    int[] rankOfIndex = { indexOfMin, indexOfMax };
                    decimal difference = valuesInRowNoZero[rankOfIndex.Min()] - valuesInRowNoZero[rankOfIndex.Max()];
                    ratioModel.Difference = difference.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.ResultValue = (difference / valuesInRowNoZero[rankOfIndex.Min()]).ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    ratioModel.ResultValue = "0";
                }
            }

            IEnumerable<RatioModel> ratiosOverBtcOrderedByResultValue = ratiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.ResultValue));
            IEnumerable<RatioModel> ratiosOverBtcOrderedByResultValueLast10 = ratiosOverBtcOrderedByResultValue.Take(10);
            IEnumerable<RatioModel> ratiosOverBtcOrderedByResultValueTop10 = ratiosOverBtcOrderedByResultValue.Reverse().Take(10);
            IEnumerable<RatioModel> ratiosOverBtcOrderedByResultValueTop10Last10 = ratiosOverBtcOrderedByResultValueTop10.Concat(ratiosOverBtcOrderedByResultValueLast10.Reverse());

            return View(ratiosOverBtcOrderedByResultValueTop10Last10);
        }

        public IActionResult Upbit()
        {
            string allMarketsString = c.DownloadString("https://api.upbit.com/v1/market/all");
            JArray allMarketsArray = JArray.Parse(allMarketsString);
            string pairsString = null;
            foreach (JToken item in allMarketsArray)
            {
                UpbitPair pair = JsonConvert.DeserializeObject<UpbitPair>(item.ToString());
                pairsString = pairsString + "," + pair.Market;
            }

            string allTickersString = c.DownloadString($"https://api.upbit.com/v1/ticker?markets= {pairsString.Substring(1)}");
            JArray allTickersArray = JArray.Parse(allTickersString); //Decimals in allTickersString are normal but they turn to scientific notation in allTickersArray over here.

            // !!! Pairs notation is reversed at Upbit API. e.g. BTC/KRW symbolized by KRW-BTC. 
            // But variable naming are not reversed below. e.g btcKrw for BTC/KRW.
            Ticker btcUsdt = JsonConvert.DeserializeObject<Ticker>(allTickersArray[75].ToString());
            decimal btcUsdtLastPrice = Convert.ToDecimal(btcUsdt.Trade_price, System.Globalization.CultureInfo.InvariantCulture);

            List<RatioModel> ratiosOverBTC = new List<RatioModel>();
            List<Ticker> ratiosOverUSDT = new List<Ticker>();

            foreach (var item in allTickersArray)
            {
                Ticker ticker = JsonConvert.DeserializeObject<Ticker>(item.ToString());

                if (ticker.Market.Substring(0, 3).Contains("BTC"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = ticker.Market,
                        LastPrice = decimal.Parse(ticker.Trade_price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture).ToString(),
                        QuoteVolume = decimal.Parse(ticker.Acc_trade_price_24h, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture).ToString(),
                    };

                    ratiosOverBTC.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice);
                    ratioModel.Value1 = (ratioModelLastPrice * btcUsdtLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (ticker.Market.Substring(0, 4).Contains("USDT"))
                {
                    ratiosOverUSDT.Add(ticker);
                }
            }

            IEnumerable<RatioModel> ratiosOverBtcInOrder = ratiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.QuoteVolume)).Reverse();

            IDictionary<string, RatioModel> btcDict = new Dictionary<string, RatioModel>();

            foreach (var item in ratiosOverBtcInOrder)
            {
                btcDict.Add(item.Symbol.Substring(4), item);
            }

            foreach (var usdtItem in ratiosOverUSDT)
            {
                if (btcDict.Keys.Contains(usdtItem.Market.Substring(5)))
                {
                    btcDict[usdtItem.Market.Substring(5)].Value2 = usdtItem.Trade_price;
                }
            }

            foreach (var ratioModel in ratiosOverBtcInOrder)
            {
                if (Convert.ToDecimal(ratioModel.QuoteVolume, System.Globalization.CultureInfo.InvariantCulture) > 0)
                {

                    decimal[] valuesInRow = { Convert.ToDecimal(ratioModel.Value1, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value2, System.Globalization.CultureInfo.InvariantCulture) };

                    decimal[] valuesInRowNoZero = { };
                    int i = 0;

                    foreach (decimal value in valuesInRow)

                        if (value != 0)
                        {
                            Array.Resize(ref valuesInRowNoZero, i + 1);
                            valuesInRowNoZero[i] = value;
                            i++;
                        };

                    int indexOfMin = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Min());
                    int indexOfMax = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Max());
                    int[] rankOfIndex = { indexOfMin, indexOfMax };
                    decimal difference = valuesInRowNoZero[rankOfIndex.Min()] - valuesInRowNoZero[rankOfIndex.Max()];
                    ratioModel.Difference = difference.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.ResultValue = (difference / valuesInRowNoZero[rankOfIndex.Min()]).ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    ratioModel.ResultValue = "0";
                }
            }

            return View(ratiosOverBtcInOrder);
        }

        public IActionResult Upbit10()
        {
            string allMarketsString = c.DownloadString("https://api.upbit.com/v1/market/all");
            JArray allMarketsArray = JArray.Parse(allMarketsString);
            string pairsString = null;
            foreach (JToken item in allMarketsArray)
            {
                UpbitPair pair = JsonConvert.DeserializeObject<UpbitPair>(item.ToString());
                pairsString = pairsString + "," + pair.Market;
            }

            string allTickersString = c.DownloadString($"https://api.upbit.com/v1/ticker?markets= {pairsString.Substring(1)}");
            JArray allTickersArray = JArray.Parse(allTickersString); //Decimals in allTickersString are normal but they turn to scientific notation in allTickersArray over here.

            // !!! Pairs notation is reversed at Upbit API. e.g. BTC/KRW symbolized by KRW-BTC. 
            // But variable naming are not reversed below. e.g btcKrw for BTC/KRW.
            Ticker btcUsdt = JsonConvert.DeserializeObject<Ticker>(allTickersArray[75].ToString());
            decimal btcUsdtLastPrice = Convert.ToDecimal(btcUsdt.Trade_price, System.Globalization.CultureInfo.InvariantCulture);

            List<RatioModel> ratiosOverBTC = new List<RatioModel>();
            List<Ticker> ratiosOverUSDT = new List<Ticker>();

            foreach (var item in allTickersArray)
            {
                Ticker ticker = JsonConvert.DeserializeObject<Ticker>(item.ToString());

                if (ticker.Market.Substring(0, 3).Contains("BTC"))
                {
                    RatioModel ratioModel = new RatioModel
                    {
                        Symbol = ticker.Market,
                        LastPrice = decimal.Parse(ticker.Trade_price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture).ToString(),
                        QuoteVolume = decimal.Parse(ticker.Acc_trade_price_24h, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture).ToString(),
                    };

                    ratiosOverBTC.Add(ratioModel);
                    decimal ratioModelLastPrice = Convert.ToDecimal(ratioModel.LastPrice);
                    ratioModel.Value1 = (ratioModelLastPrice * btcUsdtLastPrice).ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                }

                if (ticker.Market.Substring(0, 4).Contains("USDT"))
                {
                    ratiosOverUSDT.Add(ticker);
                }
            }

            IEnumerable<RatioModel> ratiosOverBtcInOrder = ratiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.QuoteVolume)).Reverse();

            IDictionary<string, RatioModel> btcDict = new Dictionary<string, RatioModel>();

            foreach (var item in ratiosOverBtcInOrder)
            {
                btcDict.Add(item.Symbol.Substring(4), item);
            }

            foreach (var usdtItem in ratiosOverUSDT)
            {
                if (btcDict.Keys.Contains(usdtItem.Market.Substring(5)))
                {
                    btcDict[usdtItem.Market.Substring(5)].Value2 = usdtItem.Trade_price;
                }
            }

            foreach (var ratioModel in ratiosOverBtcInOrder)
            {
                if (Convert.ToDecimal(ratioModel.QuoteVolume, System.Globalization.CultureInfo.InvariantCulture) > 0)
                {

                    decimal[] valuesInRow = { Convert.ToDecimal(ratioModel.Value1, System.Globalization.CultureInfo.InvariantCulture),
                                              Convert.ToDecimal(ratioModel.Value2, System.Globalization.CultureInfo.InvariantCulture) };

                    decimal[] valuesInRowNoZero = { };
                    int i = 0;

                    foreach (decimal value in valuesInRow)

                        if (value != 0)
                        {
                            Array.Resize(ref valuesInRowNoZero, i + 1);
                            valuesInRowNoZero[i] = value;
                            i++;
                        };

                    int indexOfMin = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Min());
                    int indexOfMax = Array.IndexOf(valuesInRowNoZero, valuesInRowNoZero.Max());
                    int[] rankOfIndex = { indexOfMin, indexOfMax };
                    decimal difference = valuesInRowNoZero[rankOfIndex.Min()] - valuesInRowNoZero[rankOfIndex.Max()];
                    ratioModel.Difference = difference.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    ratioModel.ResultValue = (difference / valuesInRowNoZero[rankOfIndex.Min()]).ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    ratioModel.ResultValue = "0";
                }
            }

            IEnumerable<RatioModel> ratiosOverBtcOrderedByResultValue = ratiosOverBTC.OrderBy(ratioModel => decimal.Parse(ratioModel.ResultValue));
            IEnumerable<RatioModel> ratiosOverBtcOrderedByResultValueLast10 = ratiosOverBtcOrderedByResultValue.Take(10);
            IEnumerable<RatioModel> ratiosOverBtcOrderedByResultValueTop10 = ratiosOverBtcOrderedByResultValue.Reverse().Take(10);
            IEnumerable<RatioModel> ratiosOverBtcOrderedByResultValueTop10Last10 = ratiosOverBtcOrderedByResultValueTop10.Concat(ratiosOverBtcOrderedByResultValueLast10.Reverse());

            return View(ratiosOverBtcOrderedByResultValueTop10Last10);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
