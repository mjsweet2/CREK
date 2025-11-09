/* Copyright (C) 2024 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token
{
    public enum TOKENTYPE { NONE, INT, FLOAT, MINUS, DOT, IDENTIFIER, COMMA, OPENBRACE, CLOSEBRACE };
    public TOKENTYPE tokenType;

    public int startIdx;
    public int endIdx;
    public string token;
    public Token(int s = 0, int e = 0, string t = "", TOKENTYPE tt = TOKENTYPE.NONE) { startIdx = s; endIdx = e; token = t; tokenType = tt; }



}



public class CmdLexer : MonoBehaviour
{
    public List<Token> tokens;
    public List<string> errors;
    public string inputCmd;
    public int inputCmdLength;
    public int currIndex;
    public string currChar;
    public bool endOfCmd;

    public const string lowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";
    public const string upperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string digits = "0123456789";
    public const string specialCharacters = "._-";
    public const string comma = ",";
    public const string space = " ";
    public const string dot = "."; 
    public const string minus = "-";
    public const string openBrace = "[";
    public const string closeBrace = "]";




    // Start is called before the first frame update
    void Start()
    {
        tokens = new List<Token>();
        errors = new List<string>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    void advance()
    {
        currIndex++;
        if (currIndex >= inputCmdLength)
        {
            currChar = "~";//tildi?
            endOfCmd = true;
        }
        else
        {
            currChar = inputCmd[currIndex].ToString();
        }
    }
    public void lexCmd(string cmd)
    {
        inputCmd = cmd.Trim();
        inputCmdLength = inputCmd.Length;
        tokens.Clear();
        errors.Clear();

        endOfCmd = false;
        currIndex = -1;
        advance(); //sets currIndex = 0, and initializes currChar

        while(!endOfCmd)
        {

            if (isLowerCaseChar(currChar) | isUpperCaseChar(currChar))
            {
                makeIdentifier(currIndex);
            }
            else if (isDigit(currChar))
            {
                makeNumber(currIndex);
            }
            else if (isMinus(currChar))
            {
                makeMinus(currIndex);
            }
            else if (isComma(currChar))
            {
                makeComma(currIndex);
            }
            else if (isOpenBrace(currChar))
            {
                makeOpenBrace(currIndex);
            }
            else if (isCloseBrace(currChar))
            {
                makeCloseBrace(currIndex);
            }
            else
            {
                errors.Add("lex: unknown char: " + currChar + " at index: " + currIndex);
                advance();
            }

            //eat whitespace
            while (!endOfCmd)
            {
                if (!isSpace(currChar))
                    break;
                advance();
            }
        }
    }
    void makeIdentifier(int s)
    {
       
        if (!endOfCmd)
        {
            
            //first must be letter
            if (isLowerCaseChar(currChar) | isUpperCaseChar(currChar))
            {
                
                advance();
                while (!endOfCmd)
                {
                    
                    //all others can be letters or numbers or special char  
                    if (!isLowerCaseChar(currChar) & !isUpperCaseChar(currChar) & !isDigit(currChar) & !isSpecialChar(currChar))
                    {
                        break;
                    }

                    advance();

                }
            }
        }

   
        tokens.Add(new Token(s, currIndex-1, inputCmd.Substring(s,(currIndex-s)), Token.TOKENTYPE.IDENTIFIER));

    }
    void makeNumber(int s)
    {
        int dotCount = 0; // used for determint int vs float

        if (!endOfCmd)
        {
            //minus is a sperate token, floats must lead with digit
            if (isDigit(currChar))
            {
                advance();
                while (!endOfCmd)
                {
                    if(isDot(currChar))
                    {
                        dotCount++;
                    }

                    if (!isDigit(currChar) & !isDot(currChar))
                    {
                        break;
                    }
                    advance();
                }
            }
        }

        if(dotCount == 0)
        { 
            tokens.Add(new Token(s, currIndex - 1, inputCmd.Substring(s, (currIndex - s)), Token.TOKENTYPE.INT));
        }
        else if(dotCount == 1)
        {
            tokens.Add(new Token(s, currIndex - 1, inputCmd.Substring(s, (currIndex - s)), Token.TOKENTYPE.FLOAT));
        }
        else
        {
            errors.Add("to many dots in number");
        }
      
    }

    void makeOpenBrace(int s)
    {
        if (!endOfCmd)
        {
            if (isOpenBrace(currChar))
            {
                advance();
                tokens.Add(new Token(s, currIndex - 1, inputCmd.Substring(s, (currIndex - s)), Token.TOKENTYPE.OPENBRACE));
            }

        }
    }

    void makeCloseBrace(int s)
    {
        if (!endOfCmd)
        {
            if (isCloseBrace(currChar))
            {
                advance();
                tokens.Add(new Token(s, currIndex - 1, inputCmd.Substring(s, (currIndex - s)), Token.TOKENTYPE.CLOSEBRACE));
            }

        }
    }

    void makeComma(int s)
    {
        if (!endOfCmd)
        {
            if (isComma(currChar))
            {
                advance();
                tokens.Add(new Token(s, currIndex - 1, inputCmd.Substring(s, (currIndex - s)), Token.TOKENTYPE.COMMA));
            }

        }
    }
    void makeMinus(int s)
    {
        if (!endOfCmd)
        {
            if (isMinus(currChar))
            {
                advance();
                tokens.Add(new Token(s, currIndex - 1, inputCmd.Substring(s, (currIndex - s)), Token.TOKENTYPE.MINUS));
            }

        }
    }
    public bool isLowerCaseChar(string cmdChar) {

        if (cmdChar.Length != 1)
            return false;

        return lowerCaseLetters.Contains(cmdChar);
    }
    public bool isUpperCaseChar(string cmdChar)
    {

        if (cmdChar.Length != 1)
            return false;

        return upperCaseLetters.Contains(cmdChar);
    }
    public bool isDigit(string cmdChar)
    {
        if (cmdChar.Length != 1)
            return false;

        return digits.Contains(cmdChar);
    }
    public bool isSpecialChar(string cmdChar)
    {

        if (cmdChar.Length != 1)
            return false;

        return specialCharacters.Contains(cmdChar);
    }
    public bool isSpace(string cmdChar)
    {
        if (cmdChar.Length != 1)
            return false;

        return (cmdChar == " ");

    }
    public bool isComma(string cmdChar)
    {
        if (cmdChar.Length != 1)
            return false;

        return (cmdChar == ",");

    }
    public bool isMinus(string cmdChar)
    {
        if (cmdChar.Length != 1)
            return false;

        return (cmdChar == "-");

    }
    public bool isDot(string cmdChar)
    {
        if (cmdChar.Length != 1)
            return false;

        return (cmdChar == ".");

    }
    public bool isOpenBrace(string cmdChar)
    {
        if (cmdChar.Length != 1)
            return false;

        return (cmdChar == "[");

    }
    public bool isCloseBrace(string cmdChar)
    {
        if (cmdChar.Length != 1)
            return false;

        return (cmdChar == "]");

    }


}
