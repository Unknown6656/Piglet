﻿using NUnit.Framework;
using Piglet.Lexer;
using Piglet.Lexer.Configuration;

namespace Piglet.Tests.Lexer.Construction
{
    [TestFixture]
    public class TestUnicodeLexing
    {
        [Test]
        public void TestUnicode()
        {
            var lexer = LexerFactory<string>.Configure(c =>
            {
                c.Token("خنزير صغير", f => "arabic");
                c.Token("nasse", f => "swedish");
                c.Ignore(" ");
                c.Runtime = LexerRuntime.Nfa;
            });
            var lexerInstance = lexer.Begin("خنزير صغير" + " nasse");
            
            Assert.AreEqual("arabic", lexerInstance.Next().token.SymbolValue);
            Assert.AreEqual("swedish", lexerInstance.Next().token.SymbolValue);

        }

        [Test]
        public void TestUnicodeDfa()
        {
            var lexer = LexerFactory<string>.Configure(c =>
            {
                c.Token("خنزير صغير", f => "arabic");
                c.Token("nasse", f => "swedish");
                c.Ignore(" ");
                c.Runtime = LexerRuntime.Dfa;
            });
            var lexerInstance = lexer.Begin("خنزير صغير" + " nasse");

            Assert.AreEqual("arabic", lexerInstance.Next().token.SymbolValue);
            Assert.AreEqual("swedish", lexerInstance.Next().token.SymbolValue);

        }
    }
}
