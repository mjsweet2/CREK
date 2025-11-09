/* Copyright (C) 2024 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmdParser : MonoBehaviour
{
    public List<string> errors;


    //directly correspond with planner session values
    public Dictionary<string, Vector3> sessionVector3s;
    public Dictionary<string, float> sessionFloats;
    public Dictionary<string, int> sessionInts;

    public CmdLexer cmdLexer;

    public string outputCmd;
    public List<string> outputTokens;

    public int inputTokenCount;
    public int currTokenIndex;
    public Token currentToken;
    

    // Start is called before the first frame update
    void Start()
    {
        sessionVector3s = new Dictionary<string, Vector3>();
        sessionFloats = new Dictionary<string, float>();
        sessionInts = new Dictionary<string, int>();

        outputTokens = new List<string>();

        cmdLexer = GetComponent<CmdLexer>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void advance()
    {
        currTokenIndex++;

    }
    public bool endOfCmd() { return (currTokenIndex >= inputTokenCount); }
    public void parseCmd()
    {
        outputTokens.Clear();
        errors.Clear();

        sessionInts.Clear();
        sessionFloats.Clear();
        sessionVector3s.Clear();
        currTokenIndex = 0;
        inputTokenCount = cmdLexer.tokens.Count;

        while (!endOfCmd())
        {
            if(cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.IDENTIFIER)
            {
                resolveIdentifierToken(currTokenIndex);
            }
            else if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.MINUS)
            {
                resolveMinusToken(currTokenIndex);
            }
            else if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.INT)
            {
                resolveIntToken(currTokenIndex);
            }
            else if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.FLOAT)
            {
                resolveFloatToken(currTokenIndex);
            }
            else if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.OPENBRACE)
            {
                resolveFloat3Token(currTokenIndex);
            }
            else
            {
                advance();
            }

        }
        outputCmd = "";
        for(int o = 0; o < outputTokens.Count;o++)
        {
            outputCmd = outputCmd + outputTokens[o];
            //don't add space after last token
            if(o < outputTokens.Count -1)
            {
                outputCmd = outputCmd + " ";
            }
        }

    }
    public void resolveIdentifierToken(int tokenIndex)
    {
        if (cmdLexer.tokens[tokenIndex].tokenType != Token.TOKENTYPE.IDENTIFIER)
            return;
        //no typing info as this identifier is already resolved, we are just passing through to the shell
        outputTokens.Add(cmdLexer.tokens[tokenIndex].token);
        advance();


    }
    public void resolveMinusToken(int tokenIndex)
    {
        if (cmdLexer.tokens[tokenIndex].tokenType != Token.TOKENTYPE.MINUS)
            return;

        if (tokenIndex + 1 >= cmdLexer.tokens.Count)
        {
            errors.Add("minus with no value at index: " + tokenIndex.ToString());
            return;
        }

        if (cmdLexer.tokens[tokenIndex + 1].tokenType == Token.TOKENTYPE.INT)
        {
            string valueString = "-" + cmdLexer.tokens[tokenIndex + 1].token;
            string name = "token" + outputTokens.Count.ToString() +".int";
            sessionInts.Add(name, int.Parse(valueString));
            outputTokens.Add(name);

        }
        else if (cmdLexer.tokens[tokenIndex + 1].tokenType == Token.TOKENTYPE.FLOAT)
        {
            string valueString = "-" + cmdLexer.tokens[tokenIndex + 1].token;
            string name = "token" + outputTokens.Count.ToString() + ".float";
            sessionFloats.Add(name, float.Parse(valueString));
            outputTokens.Add(name);
        }
        else
        {
            errors.Add("minus with unexpected token at index: " + tokenIndex.ToString());
            
        }

        //resolved 2 token in inputTokens list
        advance();
        advance();



    }
    public void resolveIntToken(int tokenIndex)
    {
        //this resolve ints that don't have a leading "-"
        if (cmdLexer.tokens[tokenIndex].tokenType != Token.TOKENTYPE.INT)
            return;


        string valueString = cmdLexer.tokens[tokenIndex].token;
        string name = "token" + outputTokens.Count.ToString() + ".int";
        sessionInts.Add(name, int.Parse(valueString));
        outputTokens.Add(name);

        advance();

    }
    public void resolveFloatToken(int tokenIndex)
    {
        if (cmdLexer.tokens[tokenIndex].tokenType != Token.TOKENTYPE.FLOAT)
            return;
        //this resolve floats that don't have a leading "-"

        string valueString = cmdLexer.tokens[tokenIndex].token;
        string name = "token" + outputTokens.Count.ToString() + ".float";
        sessionFloats.Add(name, float.Parse(valueString));
        outputTokens.Add(name);

        advance();

        
        
    }
    //for the floats inside of a float3, can't be used on float
    bool resolveFloat3ComponentToken(int tokenIndex, ref float f)
    {
        if (cmdLexer.tokens[tokenIndex].tokenType == Token.TOKENTYPE.MINUS)
        {
            if ((tokenIndex + 1) < cmdLexer.tokens.Count)
            {
                if (cmdLexer.tokens[tokenIndex + 1].tokenType == Token.TOKENTYPE.FLOAT)
                {
                    string valueString = "-" + cmdLexer.tokens[tokenIndex + 1].token;
                    f = float.Parse(valueString);
                    advance();
                    advance();

                }
                else
                {
                    errors.Add("unexpected token in float3");
                    return false;
                }
            }
            else
            {
                errors.Add("unexpected end of tokens");
                return false; 
            }


        }
        else if (cmdLexer.tokens[tokenIndex].tokenType == Token.TOKENTYPE.FLOAT)
        {
            string valueString = cmdLexer.tokens[tokenIndex].token;
            f = float.Parse(valueString);
            advance();

        }
        else
        {
            errors.Add("unexpected token in float3");
            return false;
        }

        return true;
    }

    void resolveFloat3Token(int tokenIndex)
    {
        //     [ f , f , f ] = 7 tokens
        //     [ - f , - f , - f ] = 10 tokens


        Vector3 tokenValue = Vector3.zero;
        string name = "";

        //currTokenIndex is the same as tokenIndex here, but advance() inc's currTokenIndex
        if (cmdLexer.tokens[currTokenIndex].tokenType != Token.TOKENTYPE.OPENBRACE)
            return;

        advance();


        //////////////////////////////////////////////////////////////////////////////
        //x
        if (!endOfCmd())
        {
            if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.MINUS |
                    cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.FLOAT)
            {
                if (resolveFloat3ComponentToken(currTokenIndex, ref tokenValue.x))
                {
                    ;
                }

            }
            else
            {
                errors.Add("unexpected token in float3");
            }
        }

        //////////////////////////////////////////////////////////////////////////////
        //eat the coma
        if (!endOfCmd())
        {
            if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.COMMA)
            {
                advance();
            }
            else
            {
                errors.Add("comma expected");

            }
        }
        //////////////////////////////////////////////////////////////////////////////
        //y
        if (!endOfCmd())
        {
            if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.MINUS |
                    cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.FLOAT)
            {
                if (resolveFloat3ComponentToken(currTokenIndex, ref tokenValue.y))
                {
                    ;
                }

            }
            else
            {
                errors.Add("unexpected token in float3");
            }
        }

        //////////////////////////////////////////////////////////////////////////////
        //eat the coma
        if (!endOfCmd())
        {
            if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.COMMA)
            {
                advance();
            }
            else
            {
                errors.Add("comma expected");

            }
        }

        //////////////////////////////////////////////////////////////////////////////
        //z
        if (!endOfCmd())
        {
            if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.MINUS |
                    cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.FLOAT)
            {
                if (resolveFloat3ComponentToken(currTokenIndex, ref tokenValue.z))
                {
                    ;
                }

            }
            else
            {
                errors.Add("unexpected token in float3");
            }
        }


        if (!endOfCmd())
        {
            if (cmdLexer.tokens[currTokenIndex].tokenType == Token.TOKENTYPE.CLOSEBRACE)
            {
                name = "token" + outputTokens.Count.ToString() + ".float3";
                outputTokens.Add(name);
                sessionVector3s.Add(name, tokenValue);
                advance();

            }
            else
            {
                errors.Add("close brace expected");
            }
        }


    }
    


}
