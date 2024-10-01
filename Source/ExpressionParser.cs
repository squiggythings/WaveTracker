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
            { "^", new(Math.Pow,                            4, Associativity.Right) },
            { "/", new((lhs, rhs) => { return lhs / rhs; }, 3, Associativity.Left)  },
            { "*", new((lhs, rhs) => { return lhs * rhs; }, 3, Associativity.Left)  },
            { "+", new((lhs, rhs) => { return lhs + rhs; }, 2, Associativity.Left)  },
            { "-", new((lhs, rhs) => { return lhs - rhs; }, 2, Associativity.Left)  },
            { "p", new((lhs, _) => { return +lhs; },        5, Associativity.Right) },
            { "n", new((lhs, _) => { return -lhs; },        5, Associativity.Right) },
        };

        //Name of function -> delegate & expected parameter count
        private static readonly Dictionary<string, (Function, int)> functions = new() {
            { "rand",       ((double[] parameters) => { return Random.Shared.Next(byte.MinValue, byte.MaxValue + 1) / 255.0; }, 0)},
            { "randbyte",   ((double[] parameters) => { return Random.Shared.Next(byte.MinValue, byte.MaxValue + 1); },         0)},
            { "randuc",     ((double[] parameters) => { return Random.Shared.Next(); },                                         0)},

            { "deg2rad",    ((double[] parameters) => { return Math.PI * parameters[0] / 180.0; },          1) },
            { "rad2deg",    ((double[] parameters) => { return parameters[0] * (180.0 / Math.PI); },        1) },
            { "sin",        ((double[] parameters) => { return Math.Sin(parameters[0]); },                  1) },
            { "cos",        ((double[] parameters) => { return Math.Cos(parameters[0]); },                  1) },
            { "tan",        ((double[] parameters) => { return Math.Tan(parameters[0]); },                  1) },
            { "asin",       ((double[] parameters) => { return Math.Asin(parameters[0]); },                 1) },
            { "acos",       ((double[] parameters) => { return Math.Acos(parameters[0]); },                 1) },
            { "atan",       ((double[] parameters) => { return Math.Atan(parameters[0]); },                 1) },
            { "ceil",       ((double[] parameters) => { return Math.Ceiling(parameters[0]); },              1) },
            { "floor",      ((double[] parameters) => { return Math.Floor(parameters[0]); },                1) },
            { "abs",        ((double[] parameters) => { return Math.Abs(parameters[0]); },                  1) },
            { "round",      ((double[] parameters) => { return Math.Round(parameters[0]); },                1) },
            { "frac",       ((double[] parameters) => { return Math.Abs(parameters[0] % 1); },              1) },
            { "sfrac",      ((double[] parameters) => { return parameters[0] % 1; },                        1) },
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

        public static readonly Dictionary<string, double> symbols = new() {
            { "pi", Math.PI },
            { "e", Math.E },
            { "tau", Math.Tau },
            { "t", 0 }, //Updated when waveform gets evaluated
        };

        //Public Functions
        /// <summary>
        /// Evaluates an infix mathematical expression
        /// </summary>
        /// <param name="infix"></param>
        /// <returns></returns>
        public static double Evaluate(string infix) {
            if (string.IsNullOrWhiteSpace(infix)) {
                throw new Exception("Empty expression");
            }

            return EvaluateRPNTokens(CompileInfixToRPN(infix));
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

        /// <summary>
        /// Evaluates a list of RPN tokens
        /// </summary>
        /// <param name="infix"></param>
        /// <returns></returns>
        public static double EvaluateRPNTokens(List<string> rpn) {
            if(rpn.Count == 0) {
                throw new Exception("Empty token list");
            }

            Stack<double> stack = new();

            foreach (var token in rpn) {
                if (operators.TryGetValue(token, out Operator op)) {
                    double rhs = stack.Pop();

                    if(token == "p" || token == "n") {
                        stack.Push(op.func(rhs, 0));
                    }
                    else {
                        double lhs = stack.Pop(); //lhs must be second pop
                        stack.Push(op.func(lhs, rhs));
                    }
                }
                else if (functions.TryGetValue(token, out var function)) {
                    double[] args = new double[function.Item2];

                    //Reversed loop due to ordering of parameters in RPN (also consistency reasons)
                    for (int i = function.Item2 - 1; i >= 0; i--) {
                        args[i] = stack.Pop();
                    }

                    stack.Push(function.Item1(args.ToArray())); //evaluate function
                }
                else if (symbols.TryGetValue(token, out var symbol)) {
                    stack.Push(symbol);
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

        //Internal Functions
        internal static string FormatInfix(string infix) {
            if (string.IsNullOrWhiteSpace(infix)) {
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
                bool isUnary = pos == 0 || expr[pos - 1] == '(' || operators.ContainsKey(expr[pos - 1].ToString());

                return expr[pos++] switch {
                    '+' => isUnary ? "p" : "+",
                    '-' => isUnary ? "n" : "-",
                    _ => token.ToString(),
                };
            }
            else if (expr[pos] == '(' || expr[pos] == ')') {
                pos++;
                return token.ToString();
            }
            else if (char.IsLetter(token[0])
                || functions.ContainsKey(token.ToString())
                || symbols.ContainsKey(token.ToString())) { //Is a function/constant/variable name

                while (++pos < expr.Length && char.IsLetter(expr[pos])) {
                    token.Append(expr[pos]);
                }

                if (functions.ContainsKey(token.ToString()) 
                    || symbols.ContainsKey(token.ToString())) {
                    return token.ToString();
                }
            }
            else if (char.IsDigit(expr[pos]) || expr[pos] == DECIMAL) {
                // Read the whole part of number
                if (char.IsDigit(expr[pos])) {
                    while (++pos < expr.Length && char.IsDigit(expr[pos])) {
                        token.Append(expr[pos]);
                    }
                }

                // Read the fractional part of number
                if (pos < expr.Length
                    && expr[pos] == DECIMAL) {
                    // Add current system specific decimal separator
                    token.Append(DECIMAL);

                    while (++pos < expr.Length && char.IsDigit(expr[pos])) {
                        token.Append(expr[pos]);
                    }
                }

                return token.ToString();
            }

            throw new Exception(token.ToString() + " is not a valid token or symbol");
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
                    if (stack.Count != 0 && functions.ContainsKey(stack.Peek())) {
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
