/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// R = 
// |cos a  -sin a|
// |sin a   cos a|
// v1 = Rv0
// 
// matrix multi is rows x columns


public class Geometry
{

    public class GeoPacket
    {
        public GeoPacket(bool b, Vector4 v) { valid = b; vector = v; }
        public GeoPacket() { valid = false; vector = Vector3.zero; }
        public bool valid;
        public Vector4 vector;
        
    }

    static public float magnitude(float x, float y, float z)
    {
        return Mathf.Sqrt(x * x + y * y + z * z);
    }

    //(-cos(x)+1)/2 on the internal 0-pi
    static public float getSinMag(int index, int resolution)
    {
        return ( -Mathf.Cos(((float)index / (float)resolution) * Mathf.PI) + 1 ) / 2;
    }


    static public Vector2 rotate2d(float angle, Vector2 v) {

        Vector2 ret = Vector2.zero;
        ret.x = Mathf.Cos(angle) * v.x + -Mathf.Sin(angle) * v.y;
        ret.y = Mathf.Sin(angle) * v.x + Mathf.Cos(angle) * v.y;


        return ret;

    }


    static public Vector3 rotateAroundXAxis(float angle, Vector3 currPos)
    {

        Vector2 rotPos = rotate2d(angle, new Vector2(currPos.y, currPos.z));

        return new Vector3(currPos.x, rotPos.x, rotPos.y);

    }

    static public Vector3 rotateAroundYAxis(float angle, Vector3 currPos)
    {
        Vector2 rotPos = rotate2d(angle, new Vector2(currPos.x, currPos.z));

        return new Vector3(rotPos.x, currPos.y, rotPos.y);
    }
    static public Vector3 rotateAroundZAxis(float angle, Vector3 currPos)
    {

        Vector2 rotPos = rotate2d(angle, new Vector2(currPos.x, currPos.y));

        return new Vector3(rotPos.x, rotPos.y, currPos.z);
    }

    // Unity Rotation Order!, Make sure I express my Component.offsetrotation in this order
    // Z axis, the X axis, and the Y axis, in that order. 

    // Unity rotation order
    // positive z rotation is counter clockwise looking from behind(-z)
    // positve y rotation is counter clockwise looking from above(+y)
    // positive x rotation is counter clockwise looking from the left(-x)

    static public float angleBetweenVectors(Vector2 u, Vector2 v)
    {
        return Mathf.Acos( Vector2.Dot(u, v) / (u.magnitude * v.magnitude) );
    }
    static public Vector3 rotateZXY(Vector3 angle, Vector3 currPos)
    {
        Vector3 tempPos = currPos;
        tempPos = rotateAroundZAxis(angle.z, tempPos);
        tempPos = rotateAroundXAxis(angle.x, tempPos);
        tempPos = rotateAroundYAxis(angle.y, tempPos);


        return tempPos;

    }

    static public Vector3 rotateYXZ(Vector3 angle, Vector3 currPos)
    {
        Vector3 tempPos = currPos;
        tempPos = rotateAroundYAxis(angle.y, tempPos);
        tempPos = rotateAroundXAxis(angle.x, tempPos);
        tempPos = rotateAroundZAxis(angle.z, tempPos);

        return tempPos;

    }

    static public Vector3 rotateXYZ(Vector3 angle, Vector3 currPos)
    {
        Vector3 tempPos = currPos;
        tempPos = rotateAroundXAxis(angle.x, tempPos);
        tempPos = rotateAroundYAxis(angle.y, tempPos);
        tempPos = rotateAroundZAxis(angle.z, tempPos);

        return tempPos;

    }

    static public Vector3 rotateZYX(Vector3 angle, Vector3 currPos)
    {
        Vector3 tempPos = currPos;
        tempPos = rotateAroundZAxis(angle.z, tempPos);
        tempPos = rotateAroundYAxis(angle.y, tempPos);
        tempPos = rotateAroundXAxis(angle.x, tempPos);

        return tempPos;

    }

    static public Vector3 rotateXZY(Vector3 angle, Vector3 currPos)
    {
        Vector3 tempPos = currPos;
        tempPos = rotateAroundXAxis(angle.x, tempPos);
        tempPos = rotateAroundZAxis(angle.z, tempPos);
        tempPos = rotateAroundYAxis(angle.y, tempPos);


        return tempPos;

    }



    static public Vector3 rotateYZX(Vector3 angle, Vector3 currPos)
    {
        Vector3 tempPos = currPos;
        tempPos = rotateAroundYAxis(angle.y, tempPos);
        tempPos = rotateAroundZAxis(angle.z, tempPos);
        tempPos = rotateAroundXAxis(angle.x, tempPos);


        return tempPos;

    }

    //sin angle = o/h
    //cos angle = a/h
    //tan angle = o/a

    //Law of cosines
    //c^2 = a^2 + b^2 - 2ab(cos C)
    //a, b, and c are lengths of sides
    //A, B, and C are angles of opposite corners
    static public GeoPacket anglesFromSides(float sidea, float sideb, float sidec)
    {

        Vector3 anglesABC = new Vector3(0f, 0f, 0f);
        anglesABC.x = ((sidea * sidea) - (sideb * sideb) - (sidec * sidec)) / (-2 * sideb * sidec);
        if (Mathf.Abs(anglesABC.x) > 1)
            return new Geometry.GeoPacket(false, Vector3.zero);
        anglesABC.x = Mathf.Acos(anglesABC.x); //returns radians

        anglesABC.y = ((sideb * sideb) - (sidea * sidea) - (sidec * sidec)) / (-2 * sidea * sidec);
        if (Mathf.Abs(anglesABC.y) > 1)
            return new Geometry.GeoPacket(false, Vector3.zero);
        anglesABC.y = Mathf.Acos(anglesABC.y); //returns radians

        anglesABC.z = ((sidec * sidec) - (sidea * sidea) - (sideb * sideb)) / (-2 * sidea * sideb);
        if (Mathf.Abs(anglesABC.z) > 1)
            return new Geometry.GeoPacket(false, Vector3.zero);
        anglesABC.z = Mathf.Acos(anglesABC.z); //returns radians

        return new GeoPacket (true, anglesABC);
    }

    static public float yUpperFromXOnCircle(float x, float xOffset, float yOffset, float radius)
    {
        float tempY = radius * radius - x * x + 2 * x * xOffset - xOffset * xOffset;
        tempY = Mathf.Sqrt(tempY);

        tempY += yOffset;

        return tempY;
    }
    static public float yLowerFromXOnCircle(float x, float xOffset, float yOffset, float radius)
    {
        float tempY = radius * radius - x * x + 2 * x * xOffset - xOffset * xOffset;
        tempY = Mathf.Sqrt(tempY);

        //negate here for lower cicle values before the
        tempY = -tempY;

        tempY += yOffset;

        return tempY;
    }
    // xx + yy - r1r1 = xx + yy - r2r2
    // (x-xOffset1)(x-xOffset1) + (y-yOffset1)(y-yOffset1) - r1*r1 = (x-xOffset2)(x-xOffset2) + (y-yOffset2)(y-yOffset2) - r2*r2
    // X*X - 2*x*xOffset1 +xOffset1*xOffset1 + y*y - 2*y*yOffset1 + yOffset1*yOffset1 - r1*r1 = X*X - 2*x*xOffset2 + xOffset2*xOffset2 + y*y - 2*y*yOffset2 + yOffset2*yOffset2 - r2*r2 
    // -2*x*xOffset1 +xOffset1*xOffset1 - 2*y*yOffset1 + yOffset1*yOffset1 - r1*r1 = -2*x*xOffset2 + xOffset2*xOffset2 - 2*y*yOffset2 + yOffset2*yOffset2 - r2*r2
    // -2*x*xOffset1 + 2*x*xOffset2 +xOffset1*xOffset1 - 2*y*yOffset1 + 2*y*yOffset2 + yOffset1*yOffset1 - r1*r1 =  + xOffset2*xOffset2  + yOffset2*yOffset2 - r2*r2
    // -2*x*xOffset1 + 2*x*xOffset2  - 2*y*yOffset1 + 2*y*yOffset2 = + xOffset2*xOffset2  + yOffset2*yOffset2 - r2*r2 -xOffset1*xOffset1 - yOffset1*yOffset1 + r1*r1
    // 2x(xOffset2 - xOffset1) + 2y(yOffset2 - yOffset1) =  + xOffset2*xOffset2  + yOffset2*yOffset2 - r2*r2 -xOffset1*xOffset1 - yOffset1*yOffset1 + r1*r1

    // xa + yb = c // this is the equation for the radical line, now I need to intersect this to the second circle
    // y = (c-xa)/b, now do replacement into 2nd circle equation
    // y  = -xa/b + c/b



    //returns coefficients for radical line
    //works
    static public Vector2 intersect2Circles(float xOffset1, float yOffset1, float r1, float xOffset2, float yOffset2, float r2)
    {
        float a = 2 * (xOffset2 - xOffset1);
        float b = 2 * (yOffset2 - yOffset1);
        float c = xOffset2 * xOffset2 + yOffset2 * yOffset2 - r2 * r2 - xOffset1 * xOffset1 - yOffset1 * yOffset1 + r1 * r1;

        // y = -xab + bc
        Vector2 lineCoeff = Vector2.zero;
        lineCoeff.x = -a / b;
        lineCoeff.y = c / b;

        return lineCoeff;

    }
    static public float yFromXOnCircleComponentSpace(float radius, float x)
    {

        return Mathf.Sqrt(radius * radius - x * x);

    }

    // Quadratic Formula
    // x = ( (-b +/-(Sqrt(bb-4ac)) ) / 2a

    static public Vector2 quadraticFormula(float a, float b, float c)
    {
        Vector2 pair = new Vector2();


        pair.x = (-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
        pair.y = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);


        return pair;

    }

    // y  = x(1st coeff) + (2nd coeff)


    //calculate overall change for other components to follow
    //starting point is left_fk = ZeroMark, right_fk = ZeroMark + (stance,0,0)


    // y = xa + b

    // (x-xOffset)(x-xOffset) + (y-yOffset)(y-yOffset) - r*r = 0
    // (x-xOffset)(x-xOffset) + ((xa + b) - yOffset)((xa + b) - yOffset) - r*r = 0
    // x*x - 2*x*xOffset + xOffset*xOffset + (xa + b)(xa + b) - 2*yOffset*(xa + b) + yOffset*yOffset - r*r = 0
    // x*x - 2*x*xOffset + xOffset*xOffset + x*x*a*a + 2*x*a*b + b*b - 2*yOffset*x*a - 2*yOffset*b + yOffset*yOffset - r*r = 0
    // x*x(1 + a*a)    +    x(-2*xOffset + 2*a*b - 2*yOffset*a)    +    ( xOffset*xOffset + b*b - 2*yOffset*b + yOffset*yOffset - r*r ) = 0

    static public Vector2 intersectLineCircle(float a, float b, float xOffset, float yOffset, float radius)
    {
        Vector3 pointA = Vector3.zero;

        float d = 1 + a * a;
        float e = (-2 * xOffset + 2 * a * b - 2 * yOffset * a);
        float f = (xOffset * xOffset + b * b - 2 * yOffset * b + yOffset * yOffset - radius * radius);



        //send to quadraticFormula
        Vector2 quadratic = quadraticFormula(d, e, f);



        return quadratic;
    }

    static public bool isPointInSphere(Vector3 point, Vector3 sphereOrigin, float sphereRadius)
    {
        Vector3 translatedPoint = point - sphereOrigin;
        return (translatedPoint.magnitude <= sphereRadius);
    }

    static public void generatePointCircle(float radius, ref Vector3[] points)
    {

        float delta = (2*Mathf.PI) / (float)points.Length;

        if (radius < 0f)
            radius = -radius;

        for(int i = 0; i < points.Length ;i++)
        {
            points[i] = new Vector3(0f, 0f, radius);
            points[i] = rotateAroundXAxis(delta * i, points[i]);
        }

    }
    static public void generatePointCircleTop(float radius, ref Vector3[] points)
    {
        //generate the top half the circle only, including the "half-way" points
        // n + 1 points with n spaces/deltas
        //points ordered from front to back

        float delta = (Mathf.PI) / (float)(points.Length-1);
        delta = -delta;

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector3(0f, 0f, radius);
            points[i] = rotateAroundXAxis((delta * i), points[i]);
        }

    }
    static public void generatePointCircleBack(float radius, ref Vector3[] points)
    {
        //generate the top half the circle only, including the "half-way" points
        // n + 1 points with n spaces/deltas
        //points ordered from front to back

        float delta = (Mathf.PI) / (float)(points.Length - 1);
        delta = -delta;

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector3(0f, radius, 0f);
            points[i] = rotateAroundXAxis((delta * i), points[i]);
        }

    }


}