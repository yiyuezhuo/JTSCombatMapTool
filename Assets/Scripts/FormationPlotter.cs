

using System.Collections;
using System.Collections.Generic;
// using UnityEngine;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
// using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Factorization;


/*
 *  32---- 1
 * 16/    \2 
 *   \____/
 *  8     4
 */
public enum MapUnitDirection
{
    RightTop = 1,
    Right = 2,
    RightBottom = 4,
    LeftBottom = 8,
    Left = 16,
    LeftTop = 32
}

public interface IMapUnit
{
    public int Strength { get; }
    public string Name { get; }
    public float X { get; }
    public float Y { get; }
    public MapUnitDirection Direction { get; }
}

public class FormationTransform
{
    public double Rotation;
    public double WidthMain;
    public double WidthSub;
    public double X;
    public double Y;
    public double[] UnitDirection;
    public int UnitDirectionModeIndex;
}

public static class PlotterUtilities
{
    static double Deg2Rad(double x) => x / 360 * 2 * System.Math.PI;

    // static double cos30 = System.Math.Cos(Deg2Rad(30));
    static double cos60 = System.Math.Cos(Deg2Rad(60));
    // static double sin30 = System.Math.Sin(Deg2Rad(30));
    static double sin60 = System.Math.Sin(Deg2Rad(60));

    public static double cos45 = System.Math.Cos(Deg2Rad(45));

    static Dictionary<MapUnitDirection, double[]> directionMap = new() // the top/Bottom is relave to a inverted Y axis
    {
        { MapUnitDirection.RightTop, new double[] { cos60, sin60}},
        { MapUnitDirection.Right, new double[] {1,0}},
        { MapUnitDirection.RightBottom, new double[] {cos60, -sin60}},
        { MapUnitDirection.LeftBottom, new double[] {-cos60, -sin60}},
        { MapUnitDirection.Left, new double[] {-1, 0}},
        { MapUnitDirection.LeftTop, new double[] {-cos60, sin60}}
    };

    public static Dictionary<MapUnitDirection, double[]> DirectionMapYInverted = directionMap.ToDictionary(KV=>KV.Key, KV=>new double[] { KV.Value[0], -KV.Value[1] });

    public static IEnumerable<double[]> GetOrthogonal(double[] x)
    {
        yield return x;
        yield return new double[] { -x[1], x[0] };
        yield return new double[] { -x[0], -x[1] };
        yield return new double[] { x[1], -x[0] };
    }

    public static string ListOfArrayToString(List<double[]> l)
    {
        return string.Join(",", l.Select(a => "[" + string.Join(",", a) + "]"));
    }
}

public interface IMapGroup<T> where T : IMapUnit
{
    public List<T> MapUnits { get; }
    // public string Name { get; }
    // public int[] Color { get; }

    public static Matrix<double> GetCovarianceMatrix(Matrix<double> matrix) // https://stackoverflow.com/questions/32256998/find-covariance-of-math-net-matrix
    { // It's transposed version of numpy
        var columnAverages = matrix.ColumnSums() / matrix.RowCount;
        var centeredColumns = matrix.EnumerateColumns().Zip(columnAverages, (col, avg) => col - avg);
        var centered = DenseMatrix.OfColumnVectors(centeredColumns);
        // var normalizationFactor = matrix.RowCount == 1 ? 1 : matrix.RowCount - 1;
        var normalizationFactor = matrix.RowCount;
        return centered.TransposeThisAndMultiply(centered) / normalizationFactor;
    }

    public FormationTransform GetRectTransform()
    {
        // TODO: Weighted by Strength
        var coordMat = Matrix<double>.Build.Dense(2, MapUnits.Count, (i, j) => i == 0 ? MapUnits[j].X : MapUnits[j].Y); // We don't do offset in this level.
        var covMat = GetCovarianceMatrix(coordMat.Transpose());

        if(covMat[0,0] > 0 || covMat[1,1] > 0)
        {
            var factorEvd = covMat.Evd(Symmetricity.Symmetric); // eigen values and Vectors are in ascending order.

            var mainDir = factorEvd.EigenVectors.Column(1);
            var mainVal = System.Math.Sqrt(factorEvd.EigenValues[1].Real);
            var subVal = System.Math.Sqrt(factorEvd.EigenValues[0].Real);
            // var subDir = factorEvd.EigenVectors.Column(0);
            var Rotation = System.Math.Atan2(mainDir[1], mainDir[0]);

            var mean = coordMat.RowSums() / coordMat.ColumnCount;

            // "Vote" sub unit direction

            var referenceDirections = PlotterUtilities.GetOrthogonal(mainDir.ToArray()).ToList();
            var unitDirections = MapUnits.Select(unit => PlotterUtilities.DirectionMapYInverted[unit.Direction]).ToList();

            var votes = referenceDirections.Select(rd => unitDirections.Sum(ud => rd[0] * ud[0] + rd[1] * ud[1] > PlotterUtilities.cos45 ? 1 : 0)).ToList();
            var threshold = MapUnits.Count / 2.0; // TODO: Weighted by Strength

            UnityEngine.Debug.Log($"referenceDirections={PlotterUtilities.ListOfArrayToString(referenceDirections)}, unitDirections={PlotterUtilities.ListOfArrayToString(unitDirections)}");
            UnityEngine.Debug.Log($"MapUnits.Count={MapUnits.Count}, Vote: {string.Join(",", votes)}");

            int i;
            for (i = 0; i < 4; i++)
            {
                if (votes[i] > threshold)
                {
                    break;
                }
            }
            var unitDirection = i == 4 ? null : referenceDirections[i];

            return new FormationTransform()
            {
                Rotation = Rotation,
                WidthMain = mainVal,
                WidthSub = subVal,
                X = mean[0],
                Y = mean[1],
                UnitDirection = unitDirection,
                UnitDirectionModeIndex = i
            };
        }
        else // fallback for group which has units in a hex. Just voting
        {
            var unitDirection = new double[] { 0, 0 };
            foreach(var unit in MapUnits)
            {
                var dxy = PlotterUtilities.DirectionMapYInverted[unit.Direction];
                for (var i= 0;i < 2;i++)
                    unitDirection[i] += dxy[i];
            }
            for (var i = 0; i < 2; i++)
                unitDirection[i] /= MapUnits.Count; // TODO: weighted by strength

            var Rotation = System.Math.Atan2(unitDirection[1], unitDirection[0]);

            return new FormationTransform()
            {
                Rotation = Rotation,
                WidthMain = 0, // Indicate it's a fallback a hex group
                WidthSub = 0,
                X = MapUnits[0].X,
                Y = MapUnits[0].Y,
                UnitDirection = unitDirection,
                UnitDirectionModeIndex = 0
            };
        }
    }
}
