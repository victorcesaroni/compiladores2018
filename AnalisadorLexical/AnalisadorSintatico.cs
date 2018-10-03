﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Compilador
{   
    class AnalisadorSintatico
    {
        class ExceptionEsperado : Exception
        {
            Token token;
            string message;
            Simbolo esperado;

            public ExceptionEsperado(string message, Simbolo esperado, Token token)
            {
                this.token = token;
                this.message = message;
                this.esperado = esperado;
            }

            public override string ToString()
            {
                string lexema = "DESCONHECIDO";
                foreach (var pair in AnalisadorLexico.mapaDeSimbolo)
                {
                    if (pair.Value == esperado)
                    {
                        lexema = pair.Key;
                        break;
                    }
                }

                return String.Format("{2} {0}:{1} (Esperado '{3}' mas recebeu '{4}')", token.linha, token.coluna, message, lexema, token.lexema);
            }
        }

        class ExceptionInesperado : Exception
        {
            Token token;
            string message;

            public ExceptionInesperado(string message, Token token)
            {
                this.token = token;
                this.message = message;
            }

            public override string ToString()
            {
                return String.Format("{2} {0}:{1} ('{3}' inesperado')", token.linha, token.coluna, message, token.lexema);
            }
        }

        public StreamReader arquivo;
        AnalisadorLexico lexico;
        
        Token token = new Token();

        public AnalisadorSintatico(string caminhoArquivo)
        {
            arquivo = new StreamReader(new FileStream(caminhoArquivo, FileMode.Open));
            lexico = new AnalisadorLexico(arquivo);

            /*try
            {
                while (!lexico.FimDeArquivo())
                {
                    var token = lexico.PegaToken();
                    Console.WriteLine("{0} {1} {2}:{3}", token.simbolo, token.lexema, token.linha, token.coluna);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }*/

            Analisa();

            arquivo.Close();
        }

        public void Analisa()
        {
            Lexico();
            if (ChecaSimboloEsperado(Simbolo.S_PROGRAMA))
            {
                Lexico();
                if (ChecaSimboloEsperado(Simbolo.S_IDENTIFICADOR))
                {
                    Lexico();
                    if (ChecaSimboloEsperado(Simbolo.S_PONTO_VIRGULA))
                    {
                        AnalisaBloco();

                        if (ChecaSimboloEsperado(Simbolo.S_PONTO))
                        {
                            try
                            {
                                Lexico();
                                throw new ExceptionEsperado("Token inesperado apos final de programa", Simbolo.S_PONTO, token);
                            }
                            catch
                            {
                                Console.WriteLine("SUCESSO");
                            }
                        }
                    }
                }
            }
        }

        void AnalisaBloco()
        {
            Lexico();
            AnalisaEtapaDeclaracaoVariaveis();
            AnalisaSubRotinas();
            AnalisaComandos();
        }

        void AnalisaEtapaDeclaracaoVariaveis()
        {
            if (ChecaSimboloEsperado(Simbolo.S_VAR, true))
            {
                Lexico();
                if (ChecaSimboloEsperado(Simbolo.S_IDENTIFICADOR))
                {
                    while (token.simbolo == Simbolo.S_IDENTIFICADOR)
                    {
                        AnalisaVariaveis();

                        if (ChecaSimboloEsperado(Simbolo.S_PONTO_VIRGULA))
                        {
                            Lexico();
                        }
                    }
                }
            }
        }

        void AnalisaSubRotinas()
        {
            
        }

        void AnalisaComandos()
        {
            if (ChecaSimboloEsperado(Simbolo.S_INICIO, true))
            {

            }
        }

        void AnalisaVariaveis()
        {
            do
            {
                if (ChecaSimboloEsperado(Simbolo.S_IDENTIFICADOR))
                {
                    Lexico();
                    if (token.simbolo == Simbolo.S_VIRGULA || token.simbolo == Simbolo.S_DOIS_PONTOS)
                    {
                        if (token.simbolo == Simbolo.S_VIRGULA)
                        {
                            Lexico();
                            ChecaSimboloInesperado(Simbolo.S_DOIS_PONTOS);
                        }
                    }
                }

                if (lexico.FimDeArquivo() && token.simbolo != Simbolo.S_DOIS_PONTOS)
                    throw new ExceptionEsperado("Final de arquivo inesperado", Simbolo.S_DOIS_PONTOS, token);
            } while (token.simbolo != Simbolo.S_DOIS_PONTOS);
            
            Lexico();

            if (token.simbolo == Simbolo.S_INTEIRO || token.simbolo == Simbolo.S_BOOLEANO)
            {
                Lexico();
            }
            else throw new ExceptionInesperado("", token);
        }

        bool ChecaSimboloEsperado(Simbolo s, bool opcional = false, string message = "")
        {
            if (token.simbolo == s)
                return true;

            if (!opcional)
                throw new ExceptionEsperado(message, s, token);

            return false;
        }

        void ChecaSimboloInesperado(Simbolo s, string message = "")
        {
            if (token.simbolo == s)
                throw new ExceptionInesperado(message, token);
        }

        public void Lexico()
        {
            token = lexico.PegaToken();
        }
    }
}
