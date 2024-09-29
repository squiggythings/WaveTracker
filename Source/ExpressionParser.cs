using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WaveTracker.Source {
    internal delegate double Function(params double[] parameters);

    internal enum Associativity {
        Left,
        Right
    }

    internal class Operator {
        public Func<double, double, double> func;
        public int priority;
        public Associativity associativity;

        public Operator(Func<double, double, double> func, int priority, Associativity associativity) {
            this.func = func;
            this.priority = priority;
            this.associativity = associativity;
        }
    }

    public class ExpressionParser {
        public const char START_EXPR = '(';
        public const char END_EXPR = ')';
        public const char SEP_EXPR = ',';
        public const char DECIMAL = '.';

        private static readonly Dictionary<string, Operator> operators = new() {
            { "^", new(Math.Pow, 4, Associativity.Right) },
            { "/", new((lhs, rhs) => { return lhs / rhs; }, 3, Associativity.Left)},
            { "*", new((lhs, rhs) => { return lhs * rhs; }, 3, Associativity.Left) },
            { "+", new((lhs, rhs) => { return lhs + rhs; }, 2, Associativity.Left) },
            { "-", new((lhs, rhs) => { return lhs - rhs; }, 2, Associativity.Left) },
        };

        //Name of function -> delegate & expected parameter count
        private static readonly Dictionary<string, (Function, int)> functions = new() {
            { "rand",       ((double[] parameters) => { return Random.Shared.Next(byte.MinValue, byte.MaxValue + 1) / 255.0; }, 0)},
            { "randbyte",   ((double[] parameters) => { return Random.Shared.Next(byte.MinValue, byte.MaxValue + 1); },         0)},
            { "randuc",     ((double[] parameters) => { return Random.Shared.Next(); },                                         0)},

            { "deg2rad",    ((double[] parameters) => { return Math.PI * parameters[0] / 180.0; },          1)},
            { "rad2deg",    ((double[] parameters) => { return parameters[0] * (180.0 / Math.PI); },        1)},
            { "sin",        ((double[] parameters) => { return Math.Sin(parameters[0]); },                  1)},
            { "cos",        ((double[] parameters) => { return Math.Cos(parameters[0]); },                  1) },
            { "tan",        ((double[] parameters) => { return Math.Tan(parameters[0]); },                  1) },
            { "asin",       ((double[] parameters) => { return Math.Asin(parameters[0]); },                 1)},
            { "acos",       ((double[] parameters) => { return Math.Acos(parameters[0]); },                 1) },
            { "atan",       ((double[] parameters) => { return Math.Atan(parameters[0]); },                 1) },

            { "ceil",       ((double[] parameters) => { return Math.Ceiling(parameters[0]); },              1) },
            { "floor",      ((double[] parameters) => { return Math.Floor(parameters[0]); },                1) },
            { "abs",        ((double[] parameters) => { return Math.Abs(parameters[0]); },                  1) },
            { "round",      ((double[] parameters) => { return Math.Round(parameters[0]); },                1) },
            { "frac",       ((double[] parameters) => { return Math.Abs(parameters[0] % 1); },                        1) },
            { "sfrac",       ((double[] parameters) => { return parameters[0] % 1; },                        1) },
            { "sign",       ((double[] parameters) => { return Math.Sign(parameters[0]); },                 1) },
            { "trunc",      ((double[] parameters) => { return Math.Truncate(parameters[0]); },             1) },
            { "sqrt",       ((double[] parameters) => { return Math.Sqrt(parameters[0]); },                 1) },
            { "cbrt",       ((double[] parameters) => { return Math.Cbrt(parameters[0]); },                 1) },
            { "loge",       ((double[] parameters) => { return Math.Log(parameters[0]); },                  1) },
            { "log10",      ((double[] parameters) => { return Math.Log10(parameters[0]); },                1) },
            { "log2",       ((double[] parameters) => { return Math.Log2(parameters[0]); },                 1) },

            { "log",        ((double[] parameters) => { return Math.Log(parameters[0], parameters[1]); },   2) },
            { "pow",        ((double[] parameters) => { return Math.Pow(parameters[0], parameters[1]); },   2) },
            { "min",        ((double[] parameters) => { return Math.Min(parameters[0], parameters[1]); },   2) },
            { "max",        ((double[] parameters) => { return Math.Max(parameters[0], parameters[1]); },   2) },
        };

        private static readonly Dictionary<string, Func<double>> variablesAndConstants = new() {
            { "pi", () => { return Math.PI; } },
            { "e", () => { return Math.E; } },
            { "tau", () => { return Math.Tau; } },
            { "time", () => { return DateTime.UtcNow.Millisecond; } }, //TODO
        };

        //Public Functions
        /// <summary>
        /// Evaluates an infix mathematical expression
        /// </summary>
        /// <param name="infix"></param>
        /// <returns></returns>
        public static double Evaluate(string infix, params (string, double)[] variables) {
            return EvaluateRPNTokens(CompileInfixToRPN(infix), variables);
        }

        //Public Functions
        /// <summary>
        /// Evaluates a list of RPN tokens
        /// </summary>
        /// <param name="infix"></param>
        /// <returns></returns>
        public static double EvaluateRPNTokens(List<string> rpn, params (string, double)[] userVariables) {
            if(rpn.Count  == 0) {
                throw new Exception("Empty token list");
            }

            Stack<double> stack = new();
            Dictionary<string, double> variableDictionary = userVariables.ToDictionary();

            foreach (var token in rpn) {
                if (operators.TryGetValue(token, out Operator op)) {
                    double rhs = stack.Pop();
                    double lhs = stack.Pop(); //lhs must be second pop

                    stack.Push(op.func(lhs, rhs));
                }
                else if (functions.TryGetValue(token, out var function)) {
                    double[] args = new double[function.Item2];

                    //Reversed loop due to ordering of parameters in RPN (also consistency reasons)
                    for (int i = function.Item2 - 1; i >= 0; i--) {
                        args[i] = stack.Pop();
                    }

                    stack.Push(function.Item1(args.ToArray())); //evaluate function
                }
                else if (variablesAndConstants.TryGetValue(token, out var varconst)) {
                    stack.Push(varconst());
                }
                else if (variableDictionary.TryGetValue(token, out double userVariable)) {
                    stack.Push(userVariable);
                }
                else if (double.TryParse(token, out double value)) {
                    stack.Push(value);
                }
                else {
                    throw new Exception($"\"{token}\" is not a valid symbol");
                }
            }

            return stack.Pop();
        }

        /// <summary>
        /// Produces an RPN token list ready to be evaluated. 
        /// Useful for repeatedly evaluating the same expression without the parser overhead
        /// </summary>
        /// <param name="infix"></param>
        /// <returns></returns>
        public static List<string> CompileInfixToRPN(string infix) {
            return InfixTokensToRPN(TokenizeInfix(FormatInfix(infix)));
        }

        //Internal Functions
        internal static string FormatInfix(string infix) {
            if (string.IsNullOrEmpty(infix)) {
                throw new Exception("Empty expression");
            }

            StringBuilder output = new();
            int bracketDifference = 0;

            for (int i = 0; i < infix.Length; i++) {
                if (char.IsWhiteSpace(infix[i])) {
                    continue;
                }
                else if (infix[i] == '(') {
                    bracketDifference++;
                }
                else if (infix[i] ==  ')') {
                    bracketDifference--;
                }
                output.Append(infix[i]);
            }

            if(bracketDifference != 0) {
                throw new Exception("Unequal number of opening and closing parentheses");
            }

            return output.ToString();
        }

        internal static List<string> TokenizeInfix(string infix) {
            List<string> tokens = new();

            int pos = 0;
            while (pos < infix.Length) {
                if (char.IsWhiteSpace(infix[pos]) || infix[pos] == ',') {
                    pos++;
                    continue;
                }

                tokens.Add(ReadNextToken(infix, ref pos));
            }

            return tokens;
        }

        internal static string ReadNextToken(string expr, ref int pos) {
            StringBuilder token = new();
            token.Append(expr[pos]);

            if (operators.ContainsKey(expr[pos].ToString())) {

                if(IsUnaryOperator(expr, pos++)) {
                    token.Append(ReadNumberToken(expr, ref pos));
                }
                return token.ToString();
            }
            else if (expr[pos] == '(' || expr[pos] == ')') {
                pos++;
                return token.ToString();
            }
            else if (char.IsLetter(token[0])
                || functions.ContainsKey(token.ToString())
                || variablesAndConstants.ContainsKey(token.ToString())) { //Is a function/constant/variable name

                while (++pos < expr.Length && char.IsLetter(expr[pos])) {
                    token.Append(expr[pos]);
                }

                if (functions.ContainsKey(token.ToString())) {
                    return token.ToString();
                }

                if (variablesAndConstants.ContainsKey(token.ToString())) {
                    return token.ToString();
                }

                //Token must be a user defined variable
                //If no value is supplied during evaluation, an exception will be thrown
                return token.ToString();
            }
            else if (char.IsDigit(expr[pos]) || IsUnaryOperator(expr, pos) || expr[pos] == DECIMAL) {
                return ReadNumberToken(expr, ref pos);
            }

            throw new ArgumentException("Invalid Token");
        }

        internal static string ReadNumberToken(string expr, ref int pos) {
            StringBuilder number = new();
            number.Append(expr[pos]);

            // Read the whole part of number
            if (char.IsDigit(expr[pos])) {
                while (++pos < expr.Length && char.IsDigit(expr[pos])) {
                    number.Append(expr[pos]);
                }
            }

            // Read the fractional part of number
            if (pos < expr.Length
                && expr[pos] == DECIMAL) {
                // Add current system specific decimal separator
                number.Append(DECIMAL);

                while (++pos < expr.Length && char.IsDigit(expr[pos])) {
                    number.Append(expr[pos]);
                }
            }

            return number.ToString();
        }

        internal static bool IsUnaryOperator(string expr, int pos) {

            if(expr[pos] == '+' || expr[pos] == '-') {
                return pos == 0 || expr[pos - 1] == '(' || operators.ContainsKey(expr[pos - 1].ToString());
            }
            return false;
        }

        //Reference: https://en.wikipedia.org/wiki/Shunting_yard_algorithm#The_algorithm_in_detail
        internal static List<string> InfixTokensToRPN(List<string> infixTokens) {
            Stack<string> stack = new(); //Operator stack
            List<string> rpnTokens = new(); //RPN tokens

            foreach (string token in infixTokens) {
                if (operators.TryGetValue(token, out Operator op)) { // Token is an operator
                    while (stack.Count != 0 && operators.ContainsKey(stack.Peek())) {
                        Operator currentOp = op;
                        Operator topOp = operators[stack.Peek()]; // Null if the top element is a function

                        if (topOp == null
                            || topOp.priority > currentOp.priority
                            || (currentOp.associativity == Associativity.Left && currentOp.priority == topOp.priority)) {
                            rpnTokens.Add(stack.Pop());
                            continue;
                        }
                        break;
                    }

                    // Push operator onto the stack
                    stack.Push(token);
                }
                else if (functions.ContainsKey(token)) { // Token is an function

                    // Push function onto the stack
                    stack.Push(token);
                }
                else if (token == "(") {
                    stack.Push(token);
                }
                else if (token == ")") {
                    while (stack.Count != 0 && stack.Peek() != "(") {
                        // Until the top token on the stack is a left parenthesis,
                        // pop from the stack to the output buffer
                        rpnTokens.Add(stack.Pop());
                    }

                    // Remove the "("
                    stack.Pop();

                    //If the parenthesis belongs to a function, pop it
                    if (functions.ContainsKey(stack.Peek())) {
                        rpnTokens.Add(stack.Pop());
                    }
                }
                else {
                    rpnTokens.Add(token);
                }
            }

            while (stack.Count != 0) {
                // Add remaining tokens on stack to the token list
                rpnTokens.Add(stack.Pop());
            }

            return rpnTokens;
        }
    }
}
