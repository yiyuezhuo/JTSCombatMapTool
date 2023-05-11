

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
// using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Factorization;


public interface IMapUnit
{
    public int Strength { get; }
    public string Name { get; }
    public float X { get; }
    public float Y { get; }
}

public class FormationTransform
{
    public double Rotation;
    public double WidthMain;
    public double WidthSub;
    public double X;
    public double Y;
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
        var coordMat = Matrix<double>.Build.Dense(2, MapUnits.Count, (i, j) => i == 0 ? MapUnits[j].X : MapUnits[j].Y); // We don't do offset in this level.
        var covMat = GetCovarianceMatrix(coordMat.Transpose());
        
        var factorEvd = covMat.Evd(Symmetricity.Symmetric); // eigen values and Vectors are in ascending order.

        var mainDir = factorEvd.EigenVectors.Column(1);
        var mainVal = System.Math.Sqrt(factorEvd.EigenValues[1].Real);
        var subVal = System.Math.Sqrt(factorEvd.EigenValues[0].Real);
        // var subDir = factorEvd.EigenVectors.Column(0);
        var Rotation = System.Math.Atan2(mainDir[1], mainDir[0]);

        var mean = coordMat.RowSums() / coordMat.ColumnCount;

        return new FormationTransform()
        {
            Rotation = Rotation,
            WidthMain = mainVal,
            WidthSub = subVal,
            X = mean[0],
            Y = mean[1]
        };
    }
}
