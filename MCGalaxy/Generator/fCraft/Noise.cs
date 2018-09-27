﻿// Part of fCraft | Copyright 2009-2015 Matvei Stefarov <me@matvei.org> | BSD-3 | See LICENSE.txt //Copyright (c) 2011-2013 Jon Baker, Glenn Marien and Lao Tszy <Jonty800@gmail.com> //Copyright (c) <2012-2014> <LeChosenOne, DingusBungus> | ProCraft Copyright 2014-2016 Joseph Beauvais <123DMWM@gmail.com>
using System;

namespace MCGalaxy.Generator {

    /// <summary> Interpolation mode for perlin noise. </summary>
    public enum NoiseInterpolationMode {
        
        /// <summary> Cosine interpolation (fast). </summary>
        Cosine,

        /// <summary> Bicubic interpolation (slow). </summary>
        Bicubic,
    }


    /// <summary> Class for generating and filtering 2D and 3D noise, extensively used by MapGenerator and Cloudy brush. </summary>
    public sealed class Noise {
        public readonly int Seed;
        public readonly NoiseInterpolationMode InterpolationMode;

        public Noise( int seed, NoiseInterpolationMode interpolationMode ) {
            Seed = seed;
            InterpolationMode = interpolationMode;
        }


        public static float InterpolateCosine( float v0, float v1, float x ) {
            double f = (1 - Math.Cos( x * Math.PI )) * .5;
            return (float)(v0 * (1 - f) + v1 * f);
        }


        public static float InterpolateCosine( float v00, float v01, float v10, float v11, float x, float y ) {
            return InterpolateCosine( InterpolateCosine( v00, v10, x ),
                                      InterpolateCosine( v01, v11, x ),
                                      y );
        }


        // Cubic and Catmull-Rom Spline interpolation methods by Paul Bourke
        // http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/
        public static float InterpolateCubic( float v0, float v1, float v2, float v3, float mu ) {
            float mu2 = mu * mu;
            float a0 = v3 - v2 - v0 + v1;
            float a1 = v0 - v1 - a0;
            float a2 = v2 - v0;
            float a3 = v1;
            return (a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3);
        }


        public float StaticNoise( int x, int y ) {
            int n = Seed + x + y * short.MaxValue;
            n = (n << 13) ^ n;
            return (float)(1.0 - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7FFFFFFF) / 1073741824d);
        }


        readonly float[,] points = new float[4, 4];
        public float InterpolatedNoise( float x, float y ) {
            int xInt = (int)Math.Floor( x );
            float xFloat = x - xInt;

            int yInt = (int)Math.Floor( y );
            float yFloat = y - yInt;

            float p00, p01, p10, p11;

            switch( InterpolationMode ) {

                case NoiseInterpolationMode.Cosine:
                    p00 = StaticNoise( xInt, yInt );
                    p01 = StaticNoise( xInt, yInt + 1 );
                    p10 = StaticNoise( xInt + 1, yInt );
                    p11 = StaticNoise( xInt + 1, yInt + 1 );
                    return InterpolateCosine( InterpolateCosine( p00, p10, xFloat ), InterpolateCosine( p01, p11, xFloat ), yFloat );

                case NoiseInterpolationMode.Bicubic:
                    for( int xOffset = -1; xOffset < 3; xOffset++ ) {
                        for( int yOffset = -1; yOffset < 3; yOffset++ ) {
                            points[xOffset + 1, yOffset + 1] = StaticNoise( xInt + xOffset, yInt + yOffset );
                        }
                    }
                    p00 = InterpolateCubic( points[0, 0], points[1, 0], points[2, 0], points[3, 0], xFloat );
                    p01 = InterpolateCubic( points[0, 1], points[1, 1], points[2, 1], points[3, 1], xFloat );
                    p10 = InterpolateCubic( points[0, 2], points[1, 2], points[2, 2], points[3, 2], xFloat );
                    p11 = InterpolateCubic( points[0, 3], points[1, 3], points[2, 3], points[3, 3], xFloat );
                    return InterpolateCubic( p00, p01, p10, p11, yFloat );

                default:
                    throw new ArgumentException();
            }
        }


        public float PerlinNoise( float x, float y, int startOctave, int endOctave, float decay ) {
            float total = 0;

            float frequency = (float)Math.Pow( 2, startOctave );
            float amplitude = (float)Math.Pow( decay, startOctave );

            for( int n = startOctave; n <= endOctave; n++ ) {
                total += InterpolatedNoise( x * frequency + frequency, y * frequency + frequency ) * amplitude;
                frequency *= 2;
                amplitude *= decay;
            }
            return total;
        }


        public void PerlinNoise( float[,] map, int startOctave, int endOctave, float decay, int offsetX, int offsetY ) {
            if( map == null ) throw new ArgumentNullException( "map" );
            float maxDim = 1f / Math.Max( map.GetLength( 0 ), map.GetLength( 1 ) );
            for( int x = map.GetLength( 0 ) - 1; x >= 0; x-- ) {
                for( int y = map.GetLength( 1 ) - 1; y >= 0; y-- ) {
                    map[x, y] += PerlinNoise( x * maxDim + offsetX, y * maxDim + offsetY, startOctave, endOctave, decay );
                }
            }
        }


        #region Normalization

        public static void Normalize( float[,] map ) { Normalize( map, 0, 1 ); }


        public unsafe static void CalculateNormalizationParams( float* ptr, int length, float low, float high, out float multiplier, out float constant ) {
            float min = float.MaxValue,
                  max = float.MinValue;

            for( int i = 0; i < length; i++ ) {
                min = Math.Min( min, ptr[i] );
                max = Math.Max( max, ptr[i] );
            }

            multiplier = ( high - low ) / ( max - min );
            constant = -min * ( high - low ) / ( max - min ) + low;
        }


        public unsafe static void Normalize( float[,] map, float low, float high ) {
            fixed( float* ptr = map ) {
                float multiplier, constant;
                CalculateNormalizationParams( ptr, map.Length, low, high, out multiplier, out constant );
                for( int i = 0; i < map.Length; i++ ) {
                    ptr[i] = ptr[i] * multiplier + constant;
                }
            }
        }

        #endregion


        // assumes normalized input
        public unsafe static void Marble( float[,] map ) {
            if( map == null ) throw new ArgumentNullException( "map" );
            fixed( float* ptr = map ) {
                for( int i = 0; i < map.Length; i++ ) {
                    ptr[i] = Math.Abs( ptr[i] * 2 - 1 );
                }
            }
        }


        public static void ApplyBias( float[,] data, float c00, float c01, float c10, float c11, float midpoint ) {
            if( data == null ) throw new ArgumentNullException( "data" );
            float maxX = 2f / data.GetLength( 0 );
            float maxY = 2f / data.GetLength( 1 );
            int offsetX = data.GetLength( 0 ) / 2;
            int offsetY = data.GetLength( 1 ) / 2;

            for( int x = offsetX - 1; x >= 0; x-- ) {
                for( int y = offsetY - 1; y >= 0; y-- ) {
                    data[x, y] += InterpolateCosine( c00, (c00 + c01) / 2, (c00 + c10) / 2, midpoint, x * maxX, y * maxY );
                    data[x + offsetX, y] += InterpolateCosine( (c00 + c10) / 2, midpoint, c10, (c11 + c10) / 2, x * maxX, y * maxY );
                    data[x, y + offsetY] += InterpolateCosine( (c00 + c01) / 2, c01, midpoint, (c01 + c11) / 2, x * maxX, y * maxY );
                    data[x + offsetX, y + offsetY] += InterpolateCosine( midpoint, (c01 + c11) / 2, (c11 + c10) / 2, c11, x * maxX, y * maxY );
                }
            }
        }


        // assumes normalized input
        public unsafe static void ScaleAndClip( float[,] data, float steepness ) {
            if( data == null ) throw new ArgumentNullException( "data" );
            fixed( float* ptr = data ) {
                for( int i = 0; i < data.Length; i++ ) {
                    ptr[i] = Math.Min( 1, Math.Max( 0, ptr[i] * steepness * 2 - steepness ) );
                }
            }
        }


        public unsafe static void Invert( float[,] data ) {
            if( data == null ) throw new ArgumentNullException( "data" );
            fixed( float* ptr = data ) {
                for( int i = 0; i < data.Length; i++ ) {
                    ptr[i] = 1 - ptr[i];
                }
            }
        }

        const float GaussianBlurDivisor = 1 / 273f;
        public static float[,] GaussianBlur5X5( float[,] heightmap ) {
            if( heightmap == null ) throw new ArgumentNullException( "heightmap" );
            float[,] output = new float[heightmap.GetLength( 0 ), heightmap.GetLength( 1 )];
            for( int x = heightmap.GetLength( 0 ) - 1; x >= 0; x-- ) {
                for( int y = heightmap.GetLength( 1 ) - 1; y >= 0; y-- ) {
                    if( (x < 2) || (y < 2) || (x > heightmap.GetLength( 0 ) - 3) || (y > heightmap.GetLength( 1 ) - 3) ) {
                        output[x, y] = heightmap[x, y];
                    } else {
                        output[x, y] = (heightmap[x - 2, y - 2] + heightmap[x - 1, y - 2] * 4 + heightmap[x, y - 2] * 7 + heightmap[x + 1, y - 2] * 4 + heightmap[x + 2, y - 2] +
                                        heightmap[x - 1, y - 1] * 4 + heightmap[x - 1, y - 1] * 16 + heightmap[x, y - 1] * 26 + heightmap[x + 1, y - 1] * 16 + heightmap[x + 2, y - 1] * 4 +
                                        heightmap[x - 2, y] * 7 + heightmap[x - 1, y] * 26 + heightmap[x, y] * 41 + heightmap[x + 1, y] * 26 + heightmap[x + 2, y] * 7 +
                                        heightmap[x - 2, y + 1] * 4 + heightmap[x - 1, y + 1] * 16 + heightmap[x, y + 1] * 26 + heightmap[x + 1, y + 1] * 16 + heightmap[x + 2, y + 1] * 4 +
                                        heightmap[x - 2, y + 2] + heightmap[x - 1, y + 2] * 4 + heightmap[x, y + 2] * 7 + heightmap[x + 1, y + 2] * 4 + heightmap[x + 2, y + 2]) * GaussianBlurDivisor;
                    }
                }
            }
            return output;
        }


        public static float[,] CalculateSlope( float[,] heightmap ) {
            if( heightmap == null ) throw new ArgumentNullException( "heightmap" );
            float[,] output = new float[heightmap.GetLength( 0 ), heightmap.GetLength( 1 )];

            for( int x = heightmap.GetLength( 0 ) - 1; x >= 0; x-- ) {
                for( int y = heightmap.GetLength( 1 ) - 1; y >= 0; y-- ) {
                    if( (x == 0) || (y == 0) || (x == heightmap.GetLength( 0 ) - 1) || (y == heightmap.GetLength( 1 ) - 1) ) {
                        output[x, y] = 0;
                    } else {
                        output[x, y] = (Math.Abs( heightmap[x, y - 1] - heightmap[x, y] ) * 3 +
                                        Math.Abs( heightmap[x, y + 1] - heightmap[x, y] ) * 3 +
                                        Math.Abs( heightmap[x - 1, y] - heightmap[x, y] ) * 3 +
                                        Math.Abs( heightmap[x + 1, y] - heightmap[x, y] ) * 3 +
                                        Math.Abs( heightmap[x - 1, y - 1] - heightmap[x, y] ) * 2 +
                                        Math.Abs( heightmap[x + 1, y - 1] - heightmap[x, y] ) * 2 +
                                        Math.Abs( heightmap[x - 1, y + 1] - heightmap[x, y] ) * 2 +
                                        Math.Abs( heightmap[x + 1, y + 1] - heightmap[x, y] ) * 2) / 20f;
                    }
                }
            }

            return output;
        }


        const int ThresholdSearchPasses = 10;

        public unsafe static float FindThreshold( float[,] data, float desiredCoverage ) {
            if( data == null ) throw new ArgumentNullException( "data" );
            if( desiredCoverage == 0 ) return 0;
            if( desiredCoverage == 1 ) return 1;
            float threshold = 0.5f;
            fixed( float* ptr = data ) {
                for( int i = 0; i < ThresholdSearchPasses; i++ ) {
                    float coverage = CalculateCoverage( ptr, data.Length, threshold );
                    if( coverage > desiredCoverage ) {
                        threshold = threshold - 1 / (float)(4 << i);
                    } else {
                        threshold = threshold + 1 / (float)(4 << i);
                    }
                }
            }
            return threshold;
        }


        public unsafe static float CalculateCoverage( float* data, int length, float threshold ) {
            if( data == null ) throw new ArgumentNullException( "data" );
            int coveredVoxels = 0;
            float* end = data + length;
            while( data < end ) {
                if( *data < threshold ) coveredVoxels++;
                data++;
            }
            return coveredVoxels / (float)length;
        }
    }
}