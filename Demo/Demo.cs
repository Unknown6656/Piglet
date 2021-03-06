﻿using System;
using Piglet.Parser.Configuration.Generic;
using Piglet.Parser.Configuration;
using Piglet.Parser.Construction;
using Piglet.Demo.Parser;
using Piglet.Demo.Lexer;
using System.Linq;
using Piglet.Parser;

namespace Piglet.Demo
{
    using f = Func<int, int>;

    public class Demo
    {
        public static void Main(string[] args)
        {
            static int Exp(int a, int x) => Enumerable.Range(0, x).Aggregate(1, (acc, i) => acc * a);
            var configurator = ParserFactory.Configure<int>();

            INonTerminal<int> expr = configurator.CreateNonTerminal();
            INonTerminal<int> term = configurator.CreateNonTerminal();
            INonTerminal<int> factor = configurator.CreateNonTerminal();
            INonTerminal<int> expexpr = configurator.CreateNonTerminal();
            ITerminal<int> number = configurator.CreateTerminal("\\d+", t => int.Parse(t, System.Globalization.CultureInfo.InvariantCulture));
            ITerminal<int> add = configurator.CreateTerminal("\\+");
            ITerminal<int> sub = configurator.CreateTerminal("-");
            ITerminal<int> mul = configurator.CreateTerminal("\\*");
            ITerminal<int> div = configurator.CreateTerminal("/");
            ITerminal<int> pow = configurator.CreateTerminal("[\\^]");

            expr.AddProduction(expr, add, term).SetReduceFunction(s => s[0] + s[2]);
            expr.AddProduction(expr, sub, term).SetReduceFunction(s => s[0] - s[2]);
            expr.AddProduction(term).SetReduceFunction(s => s[0]);

            term.AddProduction(term, mul, expexpr).SetReduceFunction(s => s[0] * s[2]);
            term.AddProduction(term, div, expexpr).SetReduceFunction(s => s[0] / s[2]);
            term.AddProduction(expexpr).SetReduceFunction(s => s[0]);

            expexpr.AddProduction(expexpr, pow, factor).SetReduceFunction(s => Exp(s[0], s[2]));
            expexpr.AddProduction(factor).SetReduceFunction(s => s[0]);

            factor.AddProduction(number).SetReduceFunction(s => s[0]);
            factor.AddProduction("(", expr, ")").SetReduceFunction(s => s[1]);

            var parser = configurator.CreateParser();
            var value = parser.Parse("3^4 + 1");














            var s = @"x + -5 - 3 * -x * x";
            var par = new test_lexer().CreateParser();
            var f = par.Parse(s);

            Console.WriteLine(s);
            Console.WriteLine(f);

            return;

            // Simple demo runner
            WordsAndNumbers.Run();
            Movement.Run();
            JsonParser.Run();
            BlogFormatParser.RunFluent();
            //Console.ReadKey();
        }


        private class test_lexer
            : ParserConstructor<(string, f)>
        {
            /**/
            protected override void Construct(NonTerminalWrapper<(string, f)> nt_func)
            {
                var nt_expr = CreateNonTerminal<(string s, f f)>();
                var nt_unop = CreateNonTerminal<Func<(string s, f f), (string s, f f)>>();
                var nt_binop = CreateNonTerminal<Func<(string s, f f), (string s, f f), (string s, f f)>>();

                var t_lit = CreateTerminal(@"\d+", x => (i: int.Parse(x), x));
                var t_var = CreateTerminal(@"x", x => x);
                var t_add = CreateTerminal(@"\+", x => x);
                var t_mul = CreateTerminal(@"\*", x => x);
                var t_sub = CreateTerminal(@"-", x => x);
                var t_div = CreateTerminal(@"/", x => x);
                var t_mod = CreateTerminal(@"%", x => x);
                var t_oparen = CreateTerminal(@"\(");
                var t_cparen = CreateTerminal(@"\)");

                SetPrecedenceList(
                    (AssociativityDirection.Left, new[] { t_add, t_sub }),
                    (AssociativityDirection.Left, new[] { t_mul, t_div, t_mod })
                 // (AssociativityDirection.Right, new[] { t_pow })
                );

                var prec_b = SetAssociativity(AssociativityDirection.Left);
                var prec_u = SetAssociativity(AssociativityDirection.Right);

                CreateProduction(nt_func, nt_expr);
                CreateProduction(nt_unop, t_add, _ => f1 => ($"(+{f1.s})", f1.f));
                CreateProduction(nt_unop, t_sub, _ => f1 => ($"(-{f1.s})", x => f1.f(x)));
                CreateProduction(nt_binop, t_mod, _ => (f1, f2) => ($"({f1.s} % {f2.s})", x => f1.f(x) % f2.f(x)));
                CreateProduction(nt_binop, t_div, _ => (f1, f2) => ($"({f1.s} / {f2.s})", x => f1.f(x) / f2.f(x)));
                CreateProduction(nt_binop, t_mul, _ => (f1, f2) => ($"({f1.s} * {f2.s})", x => f1.f(x) * f2.f(x)));
                CreateProduction(nt_binop, t_sub, _ => (f1, f2) => ($"({f1.s} - {f2.s})", x => f1.f(x) - f2.f(x)));
                CreateProduction(nt_binop, t_add, _ => (f1, f2) => ($"({f1.s} + {f2.s})", x => f1.f(x) + f2.f(x)));
                CreateProduction(nt_expr, t_var, l => ("X", x => x));
                CreateProduction(nt_expr, t_lit, l => (l.x, _ => l.i));
                CreateProduction(nt_expr, t_oparen, nt_expr, t_cparen, (x, y, z) => ($"({y.s})", u => y.f(u)));
                CreateProduction(nt_expr, nt_unop, nt_expr, (x, y) => x(y)).SetPrecedence(prec_u);
                CreateProduction(nt_expr, nt_expr, nt_binop, nt_expr, (x, y, z) => y(x, z)).SetPrecedence(prec_b);

                Configurator.LexerSettings.Ignore = new[]
                {
                    @"\s+",
                    @"/\*[^(\*/)]*\*/",
                    @"//[^\n]*\n"
                };
            }
        }
    }
}