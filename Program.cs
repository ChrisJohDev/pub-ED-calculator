using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;

namespace Calculator2
{
    class Program
    {
        static void Main(string[] args)
        {
            bool ok = true;
            string strIn = "", res = "";

            while (ok)
            {
                Console.WriteLine("\nEnter a simple math calculation. Use only one operator type.\nPlus '+' and minus '-' can be multiple.\nEnter 0 to exit.");
                strIn = Console.ReadLine();

                // Checks if we should exit.
                if (strIn.Length == 1 && IsInteger(strIn) && Convert.ToInt32(strIn) == 0)
                {
                    ok = false;
                }
                else if (strIn.Length > 0)
                {
                    res = DataInput(strIn);
                    if (res.Contains("Error:"))
                    {
                        Output(strIn, "", res);
                    }
                    else
                    {
                        Output(strIn, res);
                    }
                }

            }
        }

        /* ***** Controller and routing functions START ***** */

        // Main calculation method router
        static string Calculate(string data)
        {
            string result = "";

            //Check which type of calculation it is
            if (data.IndexOf('^') > 0)
            {
                result = PowerTo(data);
            }
            else if (data.IndexOf("*") > 0)
            {
                result = Multiplication(data);
            }
            else if (data.IndexOf("/") > 0)
            {
                result = Division(data);
            }
            else if (data.IndexOf("%") > 0)
            {
                result = Modulus(data);
            }
            else if (data.IndexOf("-") > -1 && data.Length > 1)
            {
                result = Subtraction(data);
            }
            else if (data.IndexOf("+") > 0)
            {
                result = Addition(data);
            }
            else
            {
                result = data;
                //errorMessage = "No valid calculation was found in expression " + data;
            }

            //Output(data, result, errorMessage);
            return result;
        }

        // Starting point for the calculation
        static string DataInput(string data="")
        {
            string ret = "";

            ret = Evaluate(data);

            return ret;
        }

        static int DecimalsCount(double data)
        {
            string[] tmp = data.ToString().Split(".");
            if (tmp.Length < 2)
            {
                return 0;
            }
            else
            {
                return tmp[1].Length;
            }

        }
        private static string Evaluate(string data)
        {
            string pOpen = @"[\(]";
            string pClose = @"[\)]";
            string ret = data, tmp = data;
            int indxPlus = -1, indxMinus = -1, indxMultiplication = -1, indxDivision = -1, indxModulus = -1, indxPower = -1, indxOpenParanthesis = -1, indxCloseParanthesis = -1;
            Regex rgxOpen = new Regex(pOpen);
            Regex rgxClose = new Regex(pClose);
            MatchCollection matchOpen;
            MatchCollection matchClose;
            bool ok = true, notReady = true, leadingMinus = false;

            while (notReady)
            {
                // Check the input and what operators it contains and where.
                indxDivision = tmp.IndexOf('/');
                indxModulus = tmp.IndexOf('%');
                indxMultiplication = tmp.IndexOf('*');
                indxPlus = tmp.IndexOf('+');
                indxPower = tmp.IndexOf('^');
                indxOpenParanthesis = tmp.IndexOf('(');
                indxCloseParanthesis = tmp.IndexOf(')');
                matchOpen = rgxOpen.Matches(tmp);
                matchClose = rgxClose.Matches(tmp);
                
                if(ret.Substring(0,1)=="-")
                {
                    leadingMinus = true;
                    tmp = tmp.Substring(1, tmp.Length - 1);
                }
                indxMinus = tmp.IndexOf('-');
                if (leadingMinus)
                {
                    tmp = "-" + tmp;
                    leadingMinus = false;
                }

                if (indxOpenParanthesis > -1 && indxCloseParanthesis > -1)
                {
                    if (matchOpen.Count > 0 && matchOpen.Count == matchClose.Count)
                    {
                        tmp = SolveParanthesis(tmp, matchOpen.Count);
                        ret = tmp;
                        if (ret.Contains("Error:")) ok = false;
                    }
                    else if (matchOpen.Count > 0 || matchClose.Count > 0)
                    {
                        ret = "Error: Malformed expression.";
                        ok = false;
                    }
                }
                else if (ok && (indxPower == 0 || indxMultiplication == 0 || indxDivision == 0))
                {
                    ret = "Error: Malformed expression!";
                    if (ret.Contains("Error:")) ok = false;
                }
                else if (ok && indxModulus > 0)
                {
                    tmp = FindExpression(ret, "%");
                    ret = tmp;
                    ok = false; // Modulus cannot be apart of an espression.
                }
                else if (ok && indxPower > 0)
                {
                    tmp = FindExpression(ret, "^");
                    ret = tmp;
                    if (ret.Contains("Error:")) ok = false;
                }
                else if (ok && indxMultiplication > 0)
                {
                    tmp = FindExpression(ret, "*");
                    ret = tmp;
                    if (ret.Contains("Error:")) ok = false;
                }
                else if (ok && indxDivision > 0)
                {
                    tmp = FindExpression(ret, "/");
                    ret = tmp; 
                    if (ret.Contains("Error:")) ok = false;
                }
                else if (ok && indxPlus > 0)
                {
                    tmp = FindExpression(ret, "+");
                    ret = tmp;
                    if (ret.Contains("Error:")) ok = false;
                }
                else if (ok && indxMinus > -1)
                {
                    tmp = Calculate(ret);
                    ret = tmp;
                    if (ret.Contains("Error:")) ok = false;
                }
                else
                {
                    // Check if we are finished and should exit.
                    if (IsNumber(ret) || ret.Contains("Error:")) notReady = false;
                }
            }

            if (ok)
            {
                return ret;
            }
            else
            {
                if (ret == "")
                {
                    return "Error: Expression doesn't contain any supported operators.";
                }
                else
                {
                    return ret;
                }
            }
        }

        static string FindExpression(string data, string oData)
        {
            int firstIndx = -1, lastIndx = -1;
            string ret = data, tmp = "", tmpRes = "";
            string pattern = @"[\+\-\*\/\%]";
            Regex rgx = new Regex(pattern);
            MatchCollection matches;
            string[] items = ret.Split(oData);

            for(int i = 0; i < 2; i++)
            {
                matches = rgx.Matches(items[i]);
                if (matches.Count > 0)
                {
                    if (i == 0)
                    {
                        for (int j = items[i].Length - 1; j > -1; j--)
                        {
                            if (!IsDigit(items[i].Substring(j, 1)) && firstIndx < 0)
                            {
                                firstIndx = j;
                            }
                        }
                        tmp = items[i].Substring(firstIndx + 1, items[i].Length - firstIndx - 1) + oData;
                    }
                    else if (i == 1)
                    {
                        for (int j = 0; j < items[i].Length; j++)
                        {
                            if (!IsDigit(items[i].Substring(j, 1)) && lastIndx < 0)
                            {
                                lastIndx = j;
                            }
                        }
                        tmp += items[i].Substring(0, lastIndx );
                    }
                }
                else
                {
                    if (i == 0) tmp = items[0] + oData;
                    else
                    {
                        tmp += items[1];
                    }
                }
            }

            tmpRes = Calculate(tmp);

            if (oData == "%")
            {
                ret = tmpRes;
            }
            else
            {
                ret = ret.Replace(tmp, tmpRes);
            }
            
            return ret;
        }

        //Returns the string between paranthesis ( and ) paraNo number of opening paranthesis in data.
        static string SolveParanthesis(string data, int paraNo = 1)
        {
            string ret = "", sub = "";
            int first = 0, last = 0;
            /*string pattern = @"[\+\-\*\/\%]";
            Regex rgx = new Regex(pattern);
            MatchCollection matches = rgx.Matches(data);*/

            if (paraNo == 1)
            {
                first = data.IndexOf('(') + 1;
                last = data.LastIndexOf(')') - first;
                sub = data.Substring(first, last);

                ret = data.Replace("(" + sub + ")", Evaluate(sub));
            }
            else if (paraNo > 1)
            {
                ret = "Error: Implementation for multiple paranthesis not implemented yet.";
            }
            return ret;
        }

        static void Output(string origData, string calcData = "", string messageData = "")
        {
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

            if (calcData != "" && IsNumber(calcData))
            {
                double tmp = Convert.ToDouble(calcData);
                if (DecimalsCount(tmp) > 10)
                {
                    tmp = Math.Round(tmp, 10);
                    nfi.NumberDecimalDigits = DecimalsCount(tmp);
                    calcData = tmp.ToString("N", nfi);
                }
            }

            if (messageData == "")
            {
                if (calcData != "")
                {
                    Console.WriteLine("The expression " + origData + "\nyields the result " + calcData);
                }
                else
                {
                    Console.WriteLine("The expression " + origData + "\ndoesn't yield any result.");
                }
            }
            else
            {
                Console.WriteLine("\n\n" + messageData);
            }
            Console.Write("\nEnter any key to return ... ");
            Console.ReadKey(false);
        
        }

        /* ***** Controller and routing functions END ***** */


        /* ***** Calculation functions START ***** */
        static string Addition(string data)
        {
            double sum = 0.0;
            string[] items = data.Split('+');

            for (int i = 0; i < items.Length; i++)
            {
                if (IsNumber(items[i]))
                {
                    sum += Convert.ToDouble(items[i]);
                }
            }

            return sum.ToString();
        }


        static string Division(string data)
        {
            double sum = 1.0;
            string[] items = data.Split('/');
            string ret = "";
            bool ok = true;

            if (items.Length == 2 && IsNumber(items[0]) && IsNumber(items[1]))
            {
                sum = Convert.ToDouble(items[0]) / Convert.ToDouble(items[1]);
            }
            else
            {
                ret = "Error: Division not possible, error in data.";
                ok = false;
            }

            if (ok) ret = sum.ToString();

            return ret;
        }

        static string Modulus(string data)
        {
            double result = 0.0;
            string ret = "";
            string[] items = data.Split('%');

            if (items.Length == 2 && IsNumber(items[0]) && IsNumber(items[1]))
            {
                result = Convert.ToDouble(items[0]) % Convert.ToDouble(items[1]);
                ret = result.ToString();
            }
            else
            {
                ret = "Eror: Illegal expression for modulus operation.";
            }

            return ret;
        }

        static string Multiplication(string data)
        {
            double sum = 1.0;
            string[] items = data.Split('*');
            string ret = "";
            bool ok = true;

            for (int i = 0; i < items.Length; i++)
            {
                if (ok && IsNumber(items[i]))
                {
                    sum *= Convert.ToDouble(items[i]);
                }
                else
                {
                    ret = "Error: Expression contains non-numerical characters";
                    ok = false;
                }
            }

            if (ok) ret = sum.ToString();

            return ret;
        }

        private static string PowerTo(string data)
        {
            string ret = "";
            string[] items = data.Split('^');

            if (items.Length == 2 && IsNumber(items[0]) && IsNumber(items[1]))
            {
                ret = Math.Pow(Convert.ToDouble(items[0]), Convert.ToDouble(items[1])).ToString();
            }
            else
            {
                ret = "Error: Malformed expression. " + data;
            }

            return ret;
        }

        static string Subtraction(string data)
        {
            double sum = 0.0;
            string[] items = data.Split('-');

            for (int i = 0; i < items.Length; i++)
            {
                if (IsNumber(items[i]))
                {
                    if (i == 0 && items[i].Length > 0)
                    {
                        sum = Convert.ToDouble(items[i]);
                    }
                    else
                    {
                        sum -= Convert.ToDouble(items[i]);
                    }

                }
            }

            return sum.ToString();
        }

        /* ***** Calculation functions END ***** */

        /* ***** Helper functions START ***** */

        // Checks if input is numerical fix empty strings.
        static bool IsNumber(string data)
        {
            string pattern = @"[^0-9\.\-]";
            Regex rgx = new Regex(pattern);
            MatchCollection matches = rgx.Matches(data);
            if (matches.Count > 0 || data.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        static bool IsDigit(string data)
        {
            string pattern = @"[^0-9\.]";
            Regex rgx = new Regex(pattern);
            MatchCollection matches = rgx.Matches(data);
            if (matches.Count > 0 || data.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        // Checks if input is integer
        static bool IsInteger(string data)
        {
            string pattern = @"[^0-9\-]";
            Regex rgx = new Regex(pattern);
            MatchCollection matches = rgx.Matches(data);
            if (matches.Count > 0 || data.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }


        /* ***** Helper functions END ***** */
    }
}
